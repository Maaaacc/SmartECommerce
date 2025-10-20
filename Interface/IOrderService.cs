using System.Collections.Generic;
using System.Threading.Tasks;
using SmartECommerce.Models;
using SmartECommerce.Models.ViewModels;

namespace SmartECommerce.Interface
{
    public interface IOrderService
    {
        Task CreateOrderAsync(string userId);
        Task<IEnumerable<Order>> GetOrdersByUserAsync(string userId);
        Task<Order> GetOrderDetailsAsync(int orderId, string userId);
        Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<IEnumerable<CartItem>> GetCartPreviewAsync(string userId);

        //Admin
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderDetailsAdminAsync(int orderId);

        //Admin Dashboard
        Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to);
        Task<int> GetTotalOrdersAsync(DateTime from, DateTime to);
        Task<IEnumerable<SalesTrendPoint>> GetMonthlySalesTrendAsync(int months);
        Task<IEnumerable<CategoryRevenue>> GetRevenueByCategoryAsync();
        Task<IEnumerable<RecentOrder>> GetRecentOrdersAsync(int count);

    }
}
