using Microsoft.EntityFrameworkCore;
using SmartECommerce.Data;
using SmartECommerce.Helpers;
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
        private readonly OrderStatusFlow _statusFlow;


        public OrderService(AppDbContext context)
        {
            _context = context;
            _statusFlow = new OrderStatusFlow();
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserAsync(string userId, OrderStatus? status = null)
        {
            var query = _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderPlacedAt)
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

        //Admin

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string changedBy, bool isAdminOverride = false)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return;

            var previousStatus = order.Status;

            if (previousStatus == newStatus)
                throw new InvalidOperationException($"Order is already in status: {newStatus}");

            if (!_statusFlow.CanChangeStatus(previousStatus, newStatus))
                throw new InvalidOperationException($"Invalid transition: {previousStatus} → {newStatus}");

            order.Status = newStatus;

            switch (newStatus)
            {
                case OrderStatus.OrderPlaced:
                    order.OrderPlacedAt = DateTime.UtcNow; 
                    order.ProcessingAt = null;
                    order.CompletedAt = null;
                    order.CancelledAt = null;
                    break;

                case OrderStatus.Processing:
                    order.ProcessingAt = DateTime.UtcNow;
                    order.CompletedAt = null;
                    order.CancelledAt = null;
                    break;


                case OrderStatus.Completed:
                    order.CompletedAt = DateTime.UtcNow;
                    order.CancelledAt = null;
                    break;

                case OrderStatus.Cancelled:
                    order.CancelledAt = DateTime.UtcNow;
                    order.ProcessingAt = null;
                    order.CompletedAt = null;
                    break;
            }

            // log changes
            _context.OrderStatusLogs.Add(new OrderStatusLog
            {
                OrderId = orderId,
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                ChangedBy = changedBy,
                ChangedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Order>> GetFilteredOrdersAsync(
            string searchString,
            string statusFilter,
            string paymentMethod,
            DateTime? fromDate,
            DateTime? toDate,
            string sortOrder)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User.ShippingInfo)
                .AsQueryable();

            // Search by Name or order ID
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(o => o.User.UserName.Contains(searchString) ||
                    o.Id.ToString().Contains(searchString));
            }

            // Status filter
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                if (Enum.TryParse<OrderStatus>(statusFilter, true, out var statusEnum))
                {
                    query = query.Where(o => o.Status == statusEnum);
                }
            }

            // Payment method filter
            if (!string.IsNullOrEmpty(paymentMethod) && paymentMethod != "All")
            {
                if (Enum.TryParse<PaymentMethod>(paymentMethod, true, out var paymentEnum))
                {
                    query = query.Where(o => o.PaymentMethod == paymentEnum);
                }
            }

            // Date range filter
            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderPlacedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(o => o.OrderPlacedAt <= toDate.Value.Date.AddDays(1));
            }

            // Sorting with Pending time priority

            query = sortOrder switch
            {
                "date_asc" => query.OrderBy(o => o.OrderPlacedAt),
                "date_desc" => query.OrderByDescending(o => o.OrderPlacedAt),
                "customer" => query.OrderBy(o => o.User.UserName),
                "total" => query.OrderByDescending(o => o.TotalAmount),
                _ => query.OrderByDescending(o => o.Status == OrderStatus.OrderPlaced ?
                            (o.OrderPlacedAt.AddDays(30) - DateTime.UtcNow)
                            : TimeSpan.Zero)
                        .ThenByDescending(o => o.OrderPlacedAt)
            };


            return await query.Take(100).ToListAsync();
        }

        public async Task<Order> GetOrderDetailsAdminAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                    .ThenInclude(u => u.ShippingInfo)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<OrderStatusLog>> GetOrderStatusLogsAsync(int orderId)
        {
            return await _context.OrderStatusLogs
                .Include(l => l.Order)
                .Where(l => l.OrderId == orderId)
                .OrderByDescending(l => l.ChangedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to)
        {
            return await _context.Orders
                .Where(o => o.OrderPlacedAt >= from
                    && o.OrderPlacedAt <= to
                    && o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<int> GetTotalOrdersAsync(DateTime from, DateTime to)
        {
            return await _context.Orders
                .CountAsync(o => o.OrderPlacedAt >= from
                    && o.OrderPlacedAt <= to
                    && (o.Status == OrderStatus.OrderPlaced
                        || o.Status == OrderStatus.Processing
                        || o.Status == OrderStatus.Completed));
        }

        public async Task<IEnumerable<SalesTrendPoint>> GetMonthlySalesTrendAsync(int months)
        {
            var now = DateTime.UtcNow;
            var startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-months + 1);

            var query = _context.Orders
                .Where(o => o.OrderPlacedAt >= startDate
                         && o.Status == OrderStatus.Completed)
                .GroupBy(o => new
                {
                    Year = o.OrderPlacedAt.Year,
                    Month = o.OrderPlacedAt.Month
                })
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
                .OrderByDescending(o => o.OrderPlacedAt)
                .Take(count)
                .Select(o => new RecentOrder
                {
                    OrderId = o.Id,
                    CustomerName = o.User.UserName,
                    Date = o.OrderPlacedAt.ToLocalTime().ToString("yyyy-MM-dd"),
                    Total = o.TotalAmount,
                    Status = o.Status.ToString()
                })
                .ToListAsync();
        }

    }
}
