using Ecommerce.Helpers;
using Ecommerce.Repositories;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepo;

        public CartController(IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        //show cart
        public IActionResult Index()
        {
            var cart = CartHelper.GetCart(HttpContext.Session);
            var vm = new CartVM
            {
                Items = cart,
                Total = cart.Sum(i => i.LineTotal)
            };
            return View(vm);
        }

        //add item to cart
        public IActionResult Add(int productId)
        {
            var product = _productRepo.GetById(productId);
            if (product == null)
                return NotFound();

            var cart = CartHelper.GetCart(HttpContext.Session);

            //check if already in cart
            var existing = cart.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                cart.Add(new CartItemVM
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = 1
                });
            }

            CartHelper.SaveCart(HttpContext.Session, cart);
            return RedirectToAction("Index");
        }

        //update cart item quantity
        [HttpPost]
        public IActionResult Update(int productId, int qty)
        {
            var cart = CartHelper.GetCart(HttpContext.Session);
            var item = cart.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                if (qty <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = qty;
            }

            CartHelper.SaveCart(HttpContext.Session, cart);
            return RedirectToAction("Index");
        }

        //remove item from cart
        public IActionResult Remove(int productId)
        {
            var cart = CartHelper.GetCart(HttpContext.Session);
            var item = cart.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
                cart.Remove(item);

            CartHelper.SaveCart(HttpContext.Session, cart);
            return RedirectToAction("Index");
        }
    }
}
