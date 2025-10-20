using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartECommerce.Interface;
using SmartECommerce.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public DashboardController(IProductService productService, IOrderService orderService, IUserService userService)
        {
            _productService = productService;
            _orderService = orderService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow;
            var currentMonthStart = new DateTime(today.Year, today.Month, 1);
            var lastMonthStart = currentMonthStart.AddMonths(-1);
            var lastMonthEnd = currentMonthStart.AddDays(-1);

            // Calculate revenue this and last month
            var totalRevenue = await _orderService.GetTotalRevenueAsync(currentMonthStart, today);
            var lastMonthRevenue = await _orderService.GetTotalRevenueAsync(lastMonthStart, lastMonthEnd);
            decimal revenueChange = lastMonthRevenue == 0 ? 100 : ((totalRevenue - lastMonthRevenue) / lastMonthRevenue) * 100;

            // Calculate total orders this and last month
            var totalOrders = await _orderService.GetTotalOrdersAsync(currentMonthStart, today);
            var lastMonthOrders = await _orderService.GetTotalOrdersAsync(lastMonthStart, lastMonthEnd);
            decimal ordersChange = lastMonthOrders == 0 ? 100 : ((totalOrders - lastMonthOrders) / lastMonthOrders) * 100;

            // Active customers (users with orders in last 3 months)
            var activeCustomers = await _userService.GetActiveCustomersCountAsync();

            // Calculate customer change % similarly or set 0 for demo
            decimal customersChange = 0m;

            // Products in stock and low stock count
            var productsInStock = await _productService.GetProductsInStockCountAsync();
            var lowStockCount = await _productService.GetLowStockCountAsync();

            // Sales trend, revenue by category, top products, recent orders, low stock alerts
            var salesTrend = await _orderService.GetMonthlySalesTrendAsync(6); // last 6 months
            var revenueByCategory = await _orderService.GetRevenueByCategoryAsync();
            var topSellingProducts = await _productService.GetTopSellingProductsAsync(5);
            var recentOrders = await _orderService.GetRecentOrdersAsync(5);
            var lowStockAlerts = await _productService.GetLowStockAlertsAsync();

            var model = new AdminDashboardViewModel
            {
                TotalRevenue = totalRevenue,
                RevenueChangePercentage = revenueChange,
                TotalOrders = totalOrders,
                OrdersChangePercentage = ordersChange,
                ActiveCustomers = activeCustomers,
                CustomersChangePercentage = customersChange,
                ProductsInStock = productsInStock,
                LowStockCount = lowStockCount,
                SalesTrend = salesTrend.ToList(),
                RevenueByCategory = revenueByCategory.ToList(),
                TopSellingProducts = topSellingProducts.ToList(),
                RecentOrders = recentOrders.ToList(),
                LowStockAlerts = lowStockAlerts.ToList()
            };

            return View(model);
        }
    }
}
