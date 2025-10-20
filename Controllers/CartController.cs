using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartECommerce.Interface;
using System.Drawing.Printing;
using System.Security.Claims;

namespace SmartECommerce.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId == null)
            {
                return Unauthorized();
            }

            await _cartService.AddItemToCartAsync(userId, productId, 1);
            TempData["SuccessMessage"] = "Product added to cart";
            return RedirectToAction("Index", "Product");
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = await _cartService.GetCartItemsAsync(userId);
            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            if (quantity < 1)
                quantity = 1;

            await _cartService.UpdateQuantityAsync(cartItemId, quantity);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = await _cartService.GetCartItemsAsync(userId);

            var item = cartItems.FirstOrDefault(c => c.Id == cartItemId);
            var itemTotal = item != null ? item.Product.Price * item.Quantity : 0;
            var cartTotal = cartItems.Sum(c => c.Product.Price * c.Quantity);

            // Return updated totals
            return Json(new
            {
                success = true,
                itemTotal = $"{itemTotal:0.00}",
                cartTotal = $"{cartTotal:0.00}"
            });

        }


        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            await _cartService.RemoveItemAsync(cartItemId);
            return RedirectToAction(nameof(Index));
        }
    }
}
