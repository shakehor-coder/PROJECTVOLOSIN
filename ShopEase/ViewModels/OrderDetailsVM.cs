using Ecommerce.Models;
using static Ecommerce.Models.Order;

namespace Ecommerce.ViewModels
{
    public class OrderDetailsVM
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }

        //shipping info
        public string ShippingAddress { get; set; } = string.Empty;

        //order items
        public List<OrderItemVM> Items { get; set; } = new List<OrderItemVM>();
    }

    public class OrderItemVM
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
