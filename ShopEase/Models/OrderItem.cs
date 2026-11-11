namespace Ecommerce.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }//fk order
        public int ProductId { get; set; }//fk product
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }

        //navigation properties
        public Order Order { get; set; } = null!;
        public Product Product { get; set; }=null!;
    }
}
