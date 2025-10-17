using System.Collections.Generic;
using System.Threading.Tasks;
using SmartECommerce.Models;

namespace SmartECommerce.Services
{
    public interface IOrderService
    {
        Task CreateOrderAsync(string userId);
        Task<IEnumerable<Order>> GetOrdersByUserAsync(string userId);
        Task<Order> GetOrderDetailsAsync(int orderId, string userId);
        Task UpdateOrderStatusAsync(int orderId, OrderStatus status);

        //Admin
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderDetailsAdminAsync(int orderId);
    }
}
