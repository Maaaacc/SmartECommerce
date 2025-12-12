using System.Collections.Generic;
using System.Threading.Tasks;
using SmartECommerce.Models;
using SmartECommerce.Models.ViewModels;

namespace SmartECommerce.Interface
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync(
            string search = null,
            List<int>? categoryIds = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string sortOrder = null,
            string status = "active");

        Task<Product> GetProductByIdAsync(int id);
        Task<List<Product>> GetMoreProductAsync(Product product, int id);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product, IFormFile imageFile);
        Task DeleteProductAsync(int id);

        //Admin
        Task<int> GetProductsInStockCountAsync();
        Task<int> GetLowStockCountAsync();
        Task<IEnumerable<ProductSales>> GetTopSellingProductsAsync(int count);
        Task<IEnumerable<LowStockAlert>> GetLowStockAlertsAsync();
    }
}
