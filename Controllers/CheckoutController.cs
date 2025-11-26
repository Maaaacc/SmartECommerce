using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartECommerce.Interface;
using SmartECommerce.Models;
using SmartECommerce.Models.ViewModels;
using System.Security.Claims;

namespace SmartECommerce.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly IShippingInfoService _shippingInfoService;
        private readonly ICheckoutService _checkoutService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(IShippingInfoService shippingInfoService, ICheckoutService checkoutService, UserManager<ApplicationUser> userManager)
        {
            _shippingInfoService = shippingInfoService;
            _checkoutService = checkoutService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            // Get shipping info
            var shippingInfo = await _shippingInfoService.GetByUserIdAsync(userId);

            // Get cart items
            var cartItems = await _checkoutService.GetCartPreviewAsync(userId);

            if (cartItems == null || !cartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var viewModel = new CheckoutViewModel
            {
                ShippingInfo = shippingInfo,
                CartItems = cartItems,
                TotalAmount = cartItems.Sum(i => i.Product.Price * i.Quantity)
            };

            return View(viewModel);
        }


        // Place order from current user's cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var shippingInfo = await _shippingInfoService.GetByUserIdAsync(userId);
            if (shippingInfo == null)
                return RedirectToAction("ShippingInfo", "Account");

            try
            {
                await _checkoutService.CreateOrderAsync(userId);
                TempData["SuccessMessage"] = "Order placed successfully!";
                return RedirectToAction("Index", "Order");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }
    }
}
