using Ecommerce.Models;

namespace Ecommerce.Repositories
{
    public interface IAddressRepository : IEntityRepository<Address, int>
    {
        List<Address> GetByUserId(string userId);
    }
}
