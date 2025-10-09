using Microsoft.AspNetCore.Connections.Features;
using Microsoft.EntityFrameworkCore;
using SmartECommerce.Data;
using SmartECommerce.Models;

namespace SmartECommerce.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddItemToCartAsync(string userId, int productId, int quantity)
        {
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var newItem = new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
        }
    
        public async Task<IEnumerable<CartItem>> GetCartItemsAsync(string userId)
        {
            return await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToListAsync();
        }
        
        public async Task UpdateQuantityAsync(int cartItemId, int quantity)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if(item != null)
            {
                item.Quantity = quantity;
                await _context.SaveChangesAsync();
            }
        }
    
        public async Task RemoveItemAsync(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if(item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}
