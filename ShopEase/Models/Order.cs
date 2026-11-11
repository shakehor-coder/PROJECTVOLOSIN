namespace Ecommerce.Models
{
    public class Order
    {
        public enum OrderStatus
        {
            Pending = 0,
            Processing = 1,
            Shipped = 2,
            Delivered = 3,
            Cancelled = 4
        }
        public int OrderId { get; set; }
        public string UserId { get; set; } = string.Empty;//fk to appuser
        public int ShippingAddressId { get; set; }//fk to address
        public string OrderNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }= OrderStatus.Pending;
        public DateTime OrderDate { get; set; }= DateTime.UtcNow;
        public decimal TotalAmount { get; set; }

        //navigation properties
        public Address ShippingAddress { get; set; } = null!;
        public AppUser User { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; }= new List<OrderItem>();
    }
}
