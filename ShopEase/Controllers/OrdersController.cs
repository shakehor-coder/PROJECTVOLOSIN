using Ecommerce.Models;
using Ecommerce.Services;
using Ecommerce.Repositories;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderRepository _orderRepo;
        private readonly GuestCustomerService _customers;

        public OrdersController(IOrderRepository orderRepo, GuestCustomerService customers)
        {
            _orderRepo = orderRepo;
            _customers = customers;
        }

        //my orders list
        public IActionResult Index()
        {
            var userId = _customers.CurrentId(HttpContext);
            if (userId == null)
                return View(new List<Order>());

            var orders = _orderRepo.GetByUserId(userId);
            return View(orders);
        }

        //order details
        public IActionResult Details(int id)
        {
            var order = _orderRepo.GetById(id);
            if (order == null || order.UserId != _customers.CurrentId(HttpContext))
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
