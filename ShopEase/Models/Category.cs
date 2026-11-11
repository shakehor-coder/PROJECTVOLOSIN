namespace Ecommerce.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;

        public int? ParentCategoryId { get; set; }//self referencing

        //navigation properties
        public Category? ParentCategory { get; set; }//get the parent category
        public ICollection<Product>Products { get; set; }= new List<Product>();
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    }
}
