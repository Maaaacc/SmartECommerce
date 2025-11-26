using Microsoft.EntityFrameworkCore;
using SmartECommerce.Data;
using SmartECommerce.Interface;
using SmartECommerce.Models;

namespace SmartECommerce.Services
{
    public class ShippingInfoService : IShippingInfoService
    {
        private readonly AppDbContext _context;

        public ShippingInfoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ShippingInfo?> GetByUserIdAsync(string userId)
        {
            return await _context.ShippingInfos.FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task AddOrUpdateAsync(ShippingInfo shippingInfo)
        {
            var existing = await _context.ShippingInfos
                .FirstOrDefaultAsync(s => s.UserId == shippingInfo.UserId);

            if (existing != null)
            {
                existing.RecipientName = shippingInfo.RecipientName;
                existing.AddressLine1 = shippingInfo.AddressLine1;
                existing.AddressLine2 = shippingInfo.AddressLine2;
                existing.City = shippingInfo.City;
                existing.State = shippingInfo.State;
                existing.ZipCode = shippingInfo.ZipCode;
                existing.Country = shippingInfo.Country;
                existing.PhoneNumber = shippingInfo.PhoneNumber;

                _context.ShippingInfos.Update(existing);
            }
            else
            {
                _context.ShippingInfos.Add(shippingInfo);
            }

            await _context.SaveChangesAsync();
        }
    }
}
