using Ecommerce.Data;
using Ecommerce.Models;

namespace Ecommerce.Repositories
{
    public class AddressRepository : EntityRepository<Address, int>, IAddressRepository
    {
        private readonly ApplicationDbContext _context;

        public AddressRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<Address> GetByUserId(string userId)
        {
            return _context.Addresses
                .Where(a => a.UserId == userId)
                .ToList();
        }
    }
}
