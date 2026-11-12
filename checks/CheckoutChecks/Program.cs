using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.Repositories;
using Ecommerce.Services;
using Ecommerce.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

await using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();
var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
await using (var setup = new ApplicationDbContext(options))
{
    await setup.Database.EnsureCreatedAsync();
    setup.Users.Add(new AppUser { Id = "buyer", UserName = "buyer", FullName = "Buyer" });
    setup.Categories.Add(new Category { CategoryId = 1, Name = "Tests" });
    setup.Products.AddRange(
        new Product { ProductId = 1, Name = "First", SKU = "A", CategoryId = 1, Price = 10m, StockQuantity = 5, IsActive = true },
        new Product { ProductId = 2, Name = "Second", SKU = "B", CategoryId = 1, Price = 20m, StockQuantity = 0, IsActive = true });
    await setup.SaveChangesAsync();
}

static CartItemVM Item(int id, int quantity) => new()
{
    ProductId = id, ProductName = "Test", Quantity = quantity, UnitPrice = 0.01m
};
static CheckoutVM Shipping() => new() { Country = "RU", City = "Moscow", Street = "Test", Zip = "123456" };

await using (var db = new ApplicationDbContext(options))
{
    var result = await new CheckoutService(db).PlaceOrderAsync("buyer", Shipping(), new[] { Item(1, 2) });
    Check(result.OrderId.HasValue && result.Error == null, "Order should succeed");
    var order = await db.Orders.Include(o => o.OrderItems).SingleAsync();
    Check(order.TotalAmount == 20m && order.OrderItems.Single().UnitPrice == 10m, "Use database prices");
    Check(await db.Products.Where(p => p.ProductId == 1).Select(p => p.StockQuantity).SingleAsync() == 3, "Stock decreases");
}

// The first product update must roll back when the second product has no stock.
await using (var db = new ApplicationDbContext(options))
{
    var result = await new CheckoutService(db).PlaceOrderAsync("buyer", Shipping(), new[] { Item(1, 1), Item(2, 1) });
    Check(result.Error != null && result.OrderId == null, "Out of stock fails");
}
await using (var db = new ApplicationDbContext(options))
{
    Check(await db.Products.Where(p => p.ProductId == 1).Select(p => p.StockQuantity).SingleAsync() == 3, "Rollback restores earlier updates");
    Check(await db.Orders.CountAsync() == 1 && await db.Addresses.CountAsync() == 1, "Failed checkout creates no order or address");
    var service = new CheckoutService(db);
    Check((await service.PlaceOrderAsync("buyer", Shipping(), new[] { Item(1, -1) })).Error != null, "Reject negative quantity");
    Check((await service.PlaceOrderAsync("buyer", Shipping(), new[] { Item(1, 1), Item(1, 1) })).Error != null, "Reject duplicate items");
    Check((await service.PlaceOrderAsync("buyer", new CheckoutVM { SelectedAddressId = 999 }, new[] { Item(1, 1) })).InvalidAddress, "Check address ownership");
    var repository = new ProductRepository(db);
    Check(repository.GetFiltered(null, null, null, -1, 8).Count == 2, "Normalize negative page");
    Check(repository.GetFiltered(1, "First", null, 1, 8).Count == repository.GetFilteredCount(1, "First"), "Page and count share filters");
    Check(repository.GetFiltered(null, null, null, int.MaxValue, 8).Count == 0, "Large page does not overflow");
}
Console.WriteLine("Checkout and catalog checks passed.");
