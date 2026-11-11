using Ecommerce.Models;
using Ecommerce.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ecommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepo;

        public CategoriesController(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        //list all categories
        public IActionResult Index()
        {
            var categories = _categoryRepo.GetAll();
            return View(categories);
        }

        //create category - get
        public IActionResult Create()
        {
            ViewBag.ParentCategories = new SelectList(_categoryRepo.GetAll(), "CategoryId", "Name");
            return View();
        }

        //create category - post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryRepo.Add(category);
                _categoryRepo.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ParentCategories = new SelectList(_categoryRepo.GetAll(), "CategoryId", "Name");
            return View(category);
        }

        //edit category - get
        public IActionResult Edit(int id)
        {
            var category = _categoryRepo.GetById(id);
            if (category == null)
                return NotFound();

            //exclude self from parent dropdown
            var parents = _categoryRepo.GetAll().Where(c => c.CategoryId != id).ToList();
            ViewBag.ParentCategories = new SelectList(parents, "CategoryId", "Name", category.ParentCategoryId);
            return View(category);
        }

        //edit category - post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryRepo.Update(category);
                _categoryRepo.SaveChanges();
                return RedirectToAction("Index");
            }
            var parents = _categoryRepo.GetAll().Where(c => c.CategoryId != category.CategoryId).ToList();
            ViewBag.ParentCategories = new SelectList(parents, "CategoryId", "Name", category.ParentCategoryId);
            return View(category);
        }

        //delete category - get
        public IActionResult Delete(int id)
        {
            var category = _categoryRepo.GetById(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        //delete category - post
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _categoryRepo.Delete(id);
            _categoryRepo.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
