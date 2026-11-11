namespace Ecommerce.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; }= string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //navigation properties
        public Category Category { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; }= new List<OrderItem>();
    }
}
