using Microsoft.EntityFrameworkCore;
using SmartECommerce.Data;
using SmartECommerce.Models;
using SmartECommerce.Interface;

namespace SmartECommerce.Implementation
{
    public class CheckoutService : ICheckoutService
    {
        private readonly AppDbContext _context;

        public CheckoutService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CartItem>> GetCartPreviewAsync(string userId)
        {
            return await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToListAsync();
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

    }
}
