using SmartECommerce.Models;

namespace SmartECommerce.Interface
{
    public interface IShippingInfoService
    {
        Task<ShippingInfo?> GetByUserIdAsync(string userId);
        Task AddOrUpdateAsync(ShippingInfo shippingInfo);
    }
}
