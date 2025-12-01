using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartECommerce.Interface;
using SmartECommerce.Models;
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
        public async Task<IActionResult> Index(
            string searchString,
            string statusFilter,
            string paymentMethod,
            DateTime? fromDate,
            DateTime? toDate,
            string sortOrder = "date_desc")
        {
            var orders = await _orderService.GetFilteredOrdersAsync(
                searchString, statusFilter, paymentMethod, fromDate, toDate, sortOrder);

            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;
            ViewData["PaymentMethod"] = paymentMethod;
            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");

            // Sorting parameters
            ViewData["DateSortParm"] = sortOrder == "date_desc" ? "date_asc" : "date_desc";
            ViewData["CustomerSortParm"] = "customer";
            ViewData["TotalSortParm"] = "total";

            return View(orders);
        }


        // Details of an order
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderDetailsAdminAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // GET: Show update status form
        [HttpGet]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var order = await _orderService.GetOrderDetailsAdminAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var allowedTransitions = Helpers.OrderStatusFlow.AllowedTransitions[order.Status];

            var statusList = allowedTransitions
                .Select(s => new SelectListItem
                {
                    Value = s.ToString(),
                    Text = s.ToString()
                })
                .ToList();

            statusList.Insert(0, new SelectListItem
            {
                Value = order.Status.ToString(),
                Text = $"{order.Status} (current)",
                Disabled = true
            });

            ViewBag.StatusList = statusList;

            return View(order);
        }

        // POST: Update order status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            try
            {
                await _orderService.UpdateOrderStatusAsync(id, status);
                TempData["SuccessMessage"] = "Order status updated successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
