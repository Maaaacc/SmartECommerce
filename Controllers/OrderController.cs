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

       


        // Order history page
        public async Task<IActionResult> Index(OrderStatus? status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _orderService.GetOrdersByUserAsync(userId, status);

            ViewBag.SelectedStatus = status?.ToString();

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
