using System.Collections.Generic;
using System.Threading.Tasks;
using SmartECommerce.Models;
using SmartECommerce.Models.ViewModels;

namespace SmartECommerce.Interface
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetOrdersByUserAsync(string userId, OrderStatus? status);
        Task<Order> GetOrderDetailsAsync(int orderId, string userId);
        Task UpdateOrderStatusAsync(int orderId, OrderStatus status, string changedBy, bool isAdminOverride = false);

        //Admin
        Task<IEnumerable<Order>> GetFilteredOrdersAsync(
            string searchString, 
            string statusFilter,
            string paymentMethod,
            DateTime? fromDate,
            DateTime? toDate,
            string sortOrder);

        Task<List<OrderStatusLog>> GetOrderStatusLogsAsync(int orderId);
        Task<Order> GetOrderDetailsAdminAsync(int orderId);

        //Admin Dashboard
        Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to);
        Task<int> GetTotalOrdersAsync(DateTime from, DateTime to);
        Task<IEnumerable<SalesTrendPoint>> GetMonthlySalesTrendAsync(int months);
        Task<IEnumerable<CategoryRevenue>> GetRevenueByCategoryAsync();
        Task<IEnumerable<RecentOrder>> GetRecentOrdersAsync(int count);

    }
}
