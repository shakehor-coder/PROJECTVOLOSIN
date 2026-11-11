using Ecommerce.ViewModels;
using System.Text.Json;

namespace Ecommerce.Helpers
{
    //session-based cart helper
    public static class CartHelper
    {
        private const string CartKey = "ShoppingCart";

        public static List<CartItemVM> GetCart(ISession session)
        {
            var json = session.GetString(CartKey);
            if (string.IsNullOrEmpty(json))
                return new List<CartItemVM>();

            return JsonSerializer.Deserialize<List<CartItemVM>>(json) ?? new List<CartItemVM>();
        }

        public static void SaveCart(ISession session, List<CartItemVM> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            session.SetString(CartKey, json);
        }

        public static void ClearCart(ISession session)
        {
            session.Remove(CartKey);
        }
    }
}
