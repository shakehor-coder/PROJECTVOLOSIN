using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repositories
{
    public class ProductRepository : EntityRepository<Product, int>, IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public override List<Product> GetAll()
        {
            return _context.Products
                .Include(p => p.Category)
                .ToList();
        }

        public override Product? GetById(int id)
        {
            return _context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductId == id);
        }

        public List<Product> GetFiltered(int? categoryId, string? search, string? sort, int page, int pageSize)
        {
            var query = Filter(categoryId, search).AsNoTracking().Include(p => p.Category).AsQueryable();

            //sorting
            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price).ThenBy(p => p.ProductId),
                "price_desc" => query.OrderByDescending(p => p.Price).ThenBy(p => p.ProductId),
                "name" => query.OrderBy(p => p.Name).ThenBy(p => p.ProductId),
                _ => query.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.ProductId)
            };

            //paging
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            return query
                .Skip((int)Math.Min((long)(page - 1) * pageSize, int.MaxValue))
                .Take(pageSize)
                .ToList();
        }

        public int GetFilteredCount(int? categoryId, string? search)
        {
            return Filter(categoryId, search).Count();
        }

        private IQueryable<Product> Filter(int? categoryId, string? search)
        {
            var query = _context.Products.Where(p => p.IsActive);
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search));
            return query;
        }
    }
}
