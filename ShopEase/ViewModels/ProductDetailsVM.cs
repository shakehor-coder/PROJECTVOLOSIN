using Ecommerce.Models;

namespace Ecommerce.ViewModels
{
    public class ProductDetailsVM
    {
        public Product Product { get; set; } = null!;
        public string CategoryName { get; set; } = string.Empty;
    }
}
