using Ecommerce.Models;

namespace Ecommerce.Repositories
{
    public interface IProductRepository : IEntityRepository<Product, int>
    {
        //for catalog filtering and paging
        List<Product> GetFiltered(int? categoryId, string? search, string? sort, int page, int pageSize);
        int GetFilteredCount(int? categoryId, string? search);
    }
}
