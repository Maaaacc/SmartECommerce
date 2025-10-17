using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartECommerce.Models;
using SmartECommerce.Services;
using System.Threading.Tasks;

namespace SmartECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // List all orders
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync(); // fetch all for admin
            return View(orders);
        }

        // Details of an order
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderDetailsAdminAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        // GET: Show update status form
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var order = await _orderService.GetOrderDetailsAdminAsync(id);
            if (order == null) return NotFound();

            ViewBag.StatusList = new SelectList(Enum.GetValues(typeof(OrderStatus)));
            return View(order);
        }

        // POST: Update order status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            await _orderService.UpdateOrderStatusAsync(id, status);
            TempData["SuccessMessage"] = "Order status updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
