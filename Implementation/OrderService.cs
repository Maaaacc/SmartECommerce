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


        public async Task<IEnumerable<Order>> GetOrdersByUserAsync(string userId, OrderStatus? status = null)
        {
            var query =  _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status);
            }

            return await query.ToListAsync();
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


        //Admin
        public async Task<IEnumerable<Order>> GetAllOrdersAsync() { return await _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).Include(o => o.User).OrderByDescending(o => o.OrderDate).ToListAsync(); }


        public async Task<Order> GetOrderDetailsAdminAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                    .ThenInclude(u => u.ShippingInfo)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to)
        {
            return await _context.Orders
                .Where(o => o.OrderDate >= from
                    && o.OrderDate <= to
                    && o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<int> GetTotalOrdersAsync(DateTime from, DateTime to)
        {
            return await _context.Orders
                .CountAsync(o => o.OrderDate >= from
                    && o.OrderDate <= to
                    && (o.Status == OrderStatus.Pending 
                        || o.Status == OrderStatus.Processing 
                        || o.Status ==OrderStatus.Completed));
        }

        public async Task<IEnumerable<SalesTrendPoint>> GetMonthlySalesTrendAsync(int months)
        {
            var now = DateTime.UtcNow;
            var startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-months + 1);

            var query = _context.Orders
                .Where(o => o.OrderDate >= startDate
                         && o.Status == OrderStatus.Completed) 
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new SalesTrendPoint
                {
                    Month = $"{g.Key.Month}/{g.Key.Year}",
                    Revenue = g.Sum(x => x.TotalAmount)
                });

            var result = await query.ToListAsync();

            var fullRange = Enumerable.Range(0, months)
                           .Select(i => startDate.AddMonths(i))
                           .Select(d => new SalesTrendPoint
                           {
                               Month = $"{d.Month}/{d.Year}",
                               Revenue = 0m
                           })
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
                .Where(oi => oi.Order.Status == OrderStatus.Completed) 
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
