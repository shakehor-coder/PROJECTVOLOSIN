using Ecommerce.Repositories;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class CatalogController : Controller
    {
        private readonly IProductRepository _productRepo;
        private readonly ICategoryRepository _categoryRepo;
        private const int PageSize = 8;

        public CatalogController(IProductRepository productRepo, ICategoryRepository categoryRepo)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
        }

        //browse products + filter by category + search + pagination
        public IActionResult Index(int? categoryId, string? q, string? sort, int page = 1)
        {
            page = Math.Max(1, page);
            var products = _productRepo.GetFiltered(categoryId, q, sort, page, PageSize);
            var totalCount = _productRepo.GetFilteredCount(categoryId, q);

            var vm = new ProductListVM
            {
                Products = products,
                CategoryId = categoryId,
                SearchQuery = q,
                Sort = sort,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize),
                Categories = _categoryRepo.GetAll()
            };

            return View(vm);
        }

        //product details page
        public IActionResult Details(int id)
        {
            var product = _productRepo.GetById(id);
            if (product == null)
                return NotFound();

            var vm = new ProductDetailsVM
            {
                Product = product,
                CategoryName = product.Category?.Name ?? ""
            };

            return View(vm);
        }
    }
}
