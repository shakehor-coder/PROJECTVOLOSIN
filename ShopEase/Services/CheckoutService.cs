using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Services;

/// <summary>Creates an order and reserves its stock in one database transaction.</summary>
public sealed class CheckoutService(ApplicationDbContext context)
{
    public async Task<CheckoutResult> PlaceOrderAsync(string userId, CheckoutVM address,
        IReadOnlyCollection<CartItemVM> cart, CancellationToken cancellationToken = default)
    {
        if (cart.Count == 0 || cart.Any(item => item.Quantity <= 0) ||
            cart.Select(item => item.ProductId).Distinct().Count() != cart.Count)
            return new(null, "Invalid cart.");

        if (address.SelectedAddressId.HasValue)
        {
            if (!await context.Addresses.AnyAsync(a => a.AddressId == address.SelectedAddressId &&
                a.UserId == userId, cancellationToken))
                return new(null, "Invalid shipping address.", true);
        }
        else if (string.IsNullOrWhiteSpace(address.Country) || string.IsNullOrWhiteSpace(address.City) ||
                 string.IsNullOrWhiteSpace(address.Street) || string.IsNullOrWhiteSpace(address.Zip))
            return new(null, "Please complete the shipping address.");

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var ids = cart.Select(item => item.ProductId).ToArray();
        // One query for all products; prices come from the database rather than the session.
        var products = await context.Products.AsNoTracking().Where(p => ids.Contains(p.ProductId))
            .ToDictionaryAsync(p => p.ProductId, cancellationToken);
        var order = new Order
        {
            UserId = userId,
            OrderNumber = "ORD-" + Guid.NewGuid().ToString("N"),
            OrderDate = DateTime.UtcNow,
            Status = Order.OrderStatus.Pending
        };
        if (address.SelectedAddressId.HasValue)
            order.ShippingAddressId = address.SelectedAddressId.Value;
        else
            order.ShippingAddress = new Address
            {
                UserId = userId, Country = address.Country!, City = address.City!,
                Street = address.Street!, Zip = address.Zip!
            };

        // Stable update order reduces deadlock risk when carts overlap.
        foreach (var item in cart.OrderBy(item => item.ProductId))
        {
            if (!products.TryGetValue(item.ProductId, out var product) || !product.IsActive)
                return new(null, $"Product unavailable: {item.ProductName}");

            // Conditional UPDATE prevents two checkouts from spending the same stock.
            var updated = await context.Products
                .Where(p => p.ProductId == item.ProductId && p.IsActive && p.StockQuantity >= item.Quantity)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.StockQuantity, p => p.StockQuantity - item.Quantity), cancellationToken);
            if (updated != 1)
                return new(null, $"Not enough stock for {product.Name}");

            var lineTotal = product.Price * item.Quantity;
            order.OrderItems.Add(new OrderItem
            {
                ProductId = product.ProductId, UnitPrice = product.Price,
                Quantity = item.Quantity, LineTotal = lineTotal
            });
            order.TotalAmount += lineTotal;
        }

        context.Orders.Add(order);
        // EF saves the address, order and items together using navigation properties.
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(order.OrderId, null);
        // Early returns and exceptions dispose the uncommitted transaction, rolling back stock.
    }
}
