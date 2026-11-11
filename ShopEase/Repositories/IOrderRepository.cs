using Ecommerce.Models;

namespace Ecommerce.Repositories
{
    public interface IOrderRepository : IEntityRepository<Order, int>
    {
        List<Order> GetByUserId(string userId);
    }
}
