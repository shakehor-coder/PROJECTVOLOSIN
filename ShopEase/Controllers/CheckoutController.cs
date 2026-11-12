using Ecommerce.Services;
using Ecommerce.Helpers;
using Ecommerce.Models;
using Ecommerce.Repositories;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly CheckoutService _checkout;
        private readonly ILogger<CheckoutController> _logger;
        private readonly GuestCustomerService _customers;
        private readonly IAddressRepository _addressRepo;

        public CheckoutController(
            CheckoutService checkout,
            ILogger<CheckoutController> logger,
            IAddressRepository addressRepo,
            GuestCustomerService customers)
        {
            _checkout = checkout;
            _logger = logger;
            _customers = customers;
            _addressRepo = addressRepo;
        }

        //show checkout page
        public async Task<IActionResult> Index()
        {
            var cart = CartHelper.GetCart(HttpContext.Session);
            if (!cart.Any())
                return RedirectToAction("Index", "Cart");

            var userId = await _customers.GetOrCreateIdAsync(HttpContext);
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
        public async Task<IActionResult> Index(CheckoutVM vm)
        {
            var userId = await _customers.GetOrCreateIdAsync(HttpContext);
            if (userId == null)
                return RedirectToAction("Index", "Home");

            var cart = CartHelper.GetCart(HttpContext.Session);
            if (!cart.Any())
                return RedirectToAction("Index", "Cart");

            try
            {
                var result = await _checkout.PlaceOrderAsync(userId, vm, cart, HttpContext.RequestAborted);
                if (result.InvalidAddress)
                    return BadRequest(result.Error);
                if (result.Error != null)
                {
                    TempData["Error"] = result.Error;
                    return RedirectToAction("Index");
                }

                CartHelper.ClearCart(HttpContext.Session);
                return RedirectToAction("Details", "Orders", new { id = result.OrderId });
            }
            catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Checkout failed");
                TempData["Error"] = "Something went wrong during checkout.";
                return RedirectToAction("Index");
            }
        }
    }
}
