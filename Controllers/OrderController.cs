using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartECommerce.Interface;
using SmartECommerce.Models;
using SmartECommerce.Models.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartECommerce.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IShippingInfoService _shippingInfoService;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IOrderService orderService, UserManager<ApplicationUser> userManager, IShippingInfoService shippingInfoService)
        {
            _orderService = orderService;
            _userManager = userManager;
            _shippingInfoService = shippingInfoService;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            // Get shipping info
            var shippingInfo = await _shippingInfoService.GetByUserIdAsync(userId);

            // Get cart items
            var cartItems = await _orderService.GetCartPreviewAsync(userId);
            // ✅ (we’ll define this method next if needed)

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
                return RedirectToAction("ShippingInfo", "Account", new { fromCheckout = true });

            try
            {
                await _orderService.CreateOrderAsync(userId);
                TempData["SuccessMessage"] = "Order placed successfully!";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }


        // Order history page
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _orderService.GetOrdersByUserAsync(userId);
            return View(orders);
        }

        // Details for a specific order
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _orderService.GetOrderDetailsAsync(id, userId);
            if (order == null) return NotFound();

            return View(order);
        }
    }
}
