using Ecommerce.Models;
using Ecommerce.Repositories;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderRepository _orderRepo;
        private readonly UserManager<AppUser> _userManager;

        public OrdersController(IOrderRepository orderRepo, UserManager<AppUser> userManager)
        {
            _orderRepo = orderRepo;
            _userManager = userManager;
        }

        //my orders list
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return RedirectToAction("Index", "Home");

            var orders = _orderRepo.GetByUserId(userId);
            return View(orders);
        }

        //order details
        public IActionResult Details(int id)
        {
            var order = _orderRepo.GetById(id);
            if (order == null)
                return NotFound();

            var vm = new OrderDetailsVM
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                ShippingAddress = $"{order.ShippingAddress.Street}, {order.ShippingAddress.City}, {order.ShippingAddress.Country} {order.ShippingAddress.Zip}",
                Items = order.OrderItems.Select(oi => new OrderItemVM
                {
                    ProductName = oi.Product?.Name ?? "",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    LineTotal = oi.LineTotal
                }).ToList()
            };

            return View(vm);
        }
    }
}
