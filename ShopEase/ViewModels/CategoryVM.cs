namespace Ecommerce.ViewModels
{
    public class CategoryVM
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }

        //for the parent dropdown
        public string? ParentCategoryName { get; set; }
    }
}
