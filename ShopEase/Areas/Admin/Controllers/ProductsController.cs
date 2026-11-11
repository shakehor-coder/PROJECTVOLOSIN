using Ecommerce.Models;
using Ecommerce.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ecommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepo;
        private readonly ICategoryRepository _categoryRepo;

        public ProductsController(IProductRepository productRepo, ICategoryRepository categoryRepo)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
        }

        //list all products
        public IActionResult Index()
        {
            var products = _productRepo.GetAll();
            return View(products);
        }

        //create product - get
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_categoryRepo.GetAll(), "CategoryId", "Name");
            return View();
        }

        //create product - post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                _productRepo.Add(product);
                _productRepo.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Categories = new SelectList(_categoryRepo.GetAll(), "CategoryId", "Name");
            return View(product);
        }

        //edit product - get
        public IActionResult Edit(int id)
        {
            var product = _productRepo.GetById(id);
            if (product == null)
                return NotFound();

            ViewBag.Categories = new SelectList(_categoryRepo.GetAll(), "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        //edit product - post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                _productRepo.Update(product);
                _productRepo.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Categories = new SelectList(_categoryRepo.GetAll(), "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        //delete product - get
        public IActionResult Delete(int id)
        {
            var product = _productRepo.GetById(id);
            if (product == null)
                return NotFound();
            return View(product);
        }

        //delete product - post
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _productRepo.Delete(id);
            _productRepo.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
