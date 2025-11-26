using SmartECommerce.Models;

namespace SmartECommerce.Interface
{
    public interface ICheckoutService
    {
        Task CreateOrderAsync(string userId);

        Task<IEnumerable<CartItem>> GetCartPreviewAsync(string userId);

    }
}
