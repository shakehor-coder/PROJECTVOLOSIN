using Ecommerce.Models;

namespace Ecommerce.ViewModels
{
    public class ProductListVM
    {
        public List<Product> Products { get; set; } = new List<Product>();

        //filter and search state
        public int? CategoryId { get; set; }
        public string? SearchQuery { get; set; }
        public string? Sort { get; set; }

        //paging
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        //for the category dropdown
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}
