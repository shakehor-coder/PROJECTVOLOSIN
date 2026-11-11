using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Models
{
    public class AppUser:IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        //navigation properties
        public ICollection<Address> Addresses { get; set; }=new List<Address>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();

    }
}
