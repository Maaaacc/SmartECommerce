using SmartECommerce.Models;

namespace SmartECommerce.Interface
{
    public interface ICartService
    {
        Task AddItemToCartAsync(string userId, int productId, int quantity);
        Task<IEnumerable<CartItem>> GetCartItemsAsync(string userId);
        Task UpdateQuantityAsync(int cartItemId, int quantity);
        Task RemoveItemAsync(int cartItemId);
    }
}
