using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repositories
{
    public class CategoryRepository : EntityRepository<Category, int>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public override List<Category> GetAll()
        {
            return _context.Categories
                .Include(c => c.ParentCategory)
                .ToList();
        }

        public override Category? GetById(int id)
        {
            return _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.Products)
                .FirstOrDefault(c => c.CategoryId == id);
        }
    }
}
