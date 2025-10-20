using Microsoft.EntityFrameworkCore;
using SmartECommerce.Data;
using SmartECommerce.Interface;
using SmartECommerce.Models;
using SmartECommerce.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartECommerce.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateOrderAsync(string userId)
        {
            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToListAsync();

            if (!cartItems.Any())
                throw new InvalidOperationException("Cart is empty.");

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity),
                OrderItems = new List<OrderItem>()
            };

            foreach (var cartItem in cartItems)
            {
                if (cartItem.Product.Stock < cartItem.Quantity)
                    throw new InvalidOperationException($"Not enough stock for product {cartItem.Product.Name}.");

                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Product.Price
                };

                order.OrderItems.Add(orderItem);

                // Reduce stock
                cartItem.Product.Stock -= cartItem.Quantity;
            }

            _context.Orders.Add(order);

            // Clear user's cart
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserAsync(string userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> GetOrderDetailsAsync(int orderId, string userId)
        {
            return await _context.Orders
                .Where(o => o.Id == orderId && o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<CartItem>> GetCartPreviewAsync(string userId)
        {
            return await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToListAsync();
        }


        //Admin

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> GetOrderDetailsAdminAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to)
        {
            return await _context.Orders
                .Where(o => o.OrderDate >= from && o.OrderDate <= to)
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<int> GetTotalOrdersAsync(DateTime from, DateTime to)
        {
            return await _context.Orders
                .CountAsync(o => o.OrderDate >= from && o.OrderDate <= to);
        }

        public async Task<IEnumerable<SalesTrendPoint>> GetMonthlySalesTrendAsync(int months)
        {
            var now = DateTime.UtcNow;
            var startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-months + 1);

            var query = _context.Orders
                .Where(o => o.OrderDate >= startDate)
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new SalesTrendPoint
                {
                    Month = $"{g.Key.Month}/{g.Key.Year}",
                    Revenue = g.Sum(x => x.TotalAmount)
                });

            var result = await query.ToListAsync();

            var fullRange = Enumerable.Range(0, months)
                       .Select(i => startDate.AddMonths(i))
                       .Select(d => new SalesTrendPoint { Month = $"{d.Month}/{d.Year}", Revenue = 0m })
                       .ToList();

            foreach (var point in result)
            {
                var match = fullRange.FirstOrDefault(m => m.Month == point.Month);
                if (match != null) match.Revenue = point.Revenue;
            }

            return fullRange;
        }

        public async Task<IEnumerable<CategoryRevenue>> GetRevenueByCategoryAsync()
        {
            var query = _context.OrderItems
                .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .GroupBy(oi => oi.Product.Category.Name)
                .Select(g => new CategoryRevenue
                {
                    CategoryName = g.Key,
                    Percentage = g.Sum(x => x.Price * x.Quantity)
                });

            var results = await query.ToListAsync();

            var totalRevenue = results.Sum(r => r.Percentage);
            foreach (var r in results)
            {
                r.Percentage = totalRevenue == 0 ? 0 : (r.Percentage / totalRevenue) * 100;
            }

            return results;
        }

        public async Task<IEnumerable<RecentOrder>> GetRecentOrdersAsync(int count)
        {
            return await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .Select(o => new RecentOrder
                {
                    OrderId = o.Id,
                    CustomerName = o.User.UserName,
                    Date = o.OrderDate.ToLocalTime().ToString("yyyy-MM-dd"),
                    Total = o.TotalAmount,
                    Status = o.Status.ToString()
                })
                .ToListAsync();
        }

    }
}
