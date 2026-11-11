using Ecommerce.Data;
using Ecommerce.Helpers;
using Ecommerce.Models;
using Ecommerce.Repositories;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAddressRepository _addressRepo;
        private readonly IProductRepository _productRepo;
        private readonly UserManager<AppUser> _userManager;

        public CheckoutController(
            ApplicationDbContext context,
            IAddressRepository addressRepo,
            IProductRepository productRepo,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _addressRepo = addressRepo;
            _productRepo = productRepo;
            _userManager = userManager;
        }

        //show checkout page
        public IActionResult Index()
        {
            var cart = CartHelper.GetCart(HttpContext.Session);
            if (!cart.Any())
                return RedirectToAction("Index", "Cart");

            var userId = _userManager.GetUserId(User);
            var addresses = userId != null ? _addressRepo.GetByUserId(userId) : new List<Address>();

            var vm = new CheckoutVM
            {
                CartItems = cart,
                CartTotal = cart.Sum(i => i.LineTotal),
                Addresses = addresses
            };

            return View(vm);
        }

        //process checkout - atomic transaction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(CheckoutVM vm)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return RedirectToAction("Index", "Home");

            var cart = CartHelper.GetCart(HttpContext.Session);
            if (!cart.Any())
                return RedirectToAction("Index", "Cart");

            if (vm.SelectedAddressId.HasValue &&
                !_context.Addresses.Any(a => a.AddressId == vm.SelectedAddressId.Value && a.UserId == userId))
                return BadRequest("Invalid shipping address.");
            if (!vm.SelectedAddressId.HasValue &&
                (string.IsNullOrWhiteSpace(vm.Country) || string.IsNullOrWhiteSpace(vm.City) ||
                 string.IsNullOrWhiteSpace(vm.Street) || string.IsNullOrWhiteSpace(vm.Zip)))
            {
                TempData["Error"] = "Please complete the shipping address.";
                return RedirectToAction("Index");
            }

            //use transaction for atomicity
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                //resolve or create address
                int addressId;
                if (vm.SelectedAddressId.HasValue)
                {
                    addressId = vm.SelectedAddressId.Value;
                }
                else
                {
                    var newAddress = new Address
                    {
                        UserId = userId,
                        Country = vm.Country ?? "",
                        City = vm.City ?? "",
                        Street = vm.Street ?? "",
                        Zip = vm.Zip ?? ""
                    };
                    _context.Addresses.Add(newAddress);
                    _context.SaveChanges();
                    addressId = newAddress.AddressId;
                }

                //1. validate stock for all items
                foreach (var item in cart)
                {
                    var product = _productRepo.GetById(item.ProductId);
                    if (product == null || product.StockQuantity < item.Quantity)
                    {
                        TempData["Error"] = $"Not enough stock for {item.ProductName}";
                        transaction.Rollback();
                        return RedirectToAction("Index");
                    }
                }

                //2. create order
                var order = new Order
                {
                    UserId = userId,
                    ShippingAddressId = addressId,
                    OrderNumber = "ORD-" + DateTime.UtcNow.Ticks,
                    Status = Order.OrderStatus.Pending,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = cart.Sum(i => i.LineTotal)
                };
                _context.Orders.Add(order);
                _context.SaveChanges();

                //3. create order items + 4. decrease stock
                foreach (var item in cart)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity,
                        LineTotal = item.LineTotal
                    };
                    _context.OrderItems.Add(orderItem);

                    //decrease product stock
                    var product = _context.Products.Find(item.ProductId);
                    if (product != null)
                        product.StockQuantity -= item.Quantity;
                }

                //5. save all changes inside one transaction
                _context.SaveChanges();
                transaction.Commit();

                //clear cart after successful order
                CartHelper.ClearCart(HttpContext.Session);

                return RedirectToAction("Details", "Orders", new { id = order.OrderId });
            }
            catch
            {
                transaction.Rollback();
                TempData["Error"] = "Something went wrong during checkout.";
                return RedirectToAction("Index");
            }
        }
    }
}
