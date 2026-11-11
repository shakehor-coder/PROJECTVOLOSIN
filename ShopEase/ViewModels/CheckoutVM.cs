using Ecommerce.Models;

namespace Ecommerce.ViewModels
{
    public class CheckoutVM
    {
        //cart summary
        public List<CartItemVM> CartItems { get; set; } = new List<CartItemVM>();
        public decimal CartTotal { get; set; }

        //existing addresses
        public List<Address> Addresses { get; set; } = new List<Address>();
        public int? SelectedAddressId { get; set; }

        //new address fields
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? Zip { get; set; }
    }
}
