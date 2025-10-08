using System.Collections.Generic;
using System.Threading.Tasks;
using SmartECommerce.Models;

namespace SmartECommerce.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync(string search = null, int? categoryId = null);
        Task<Product> GetProductByIdAsync(int id);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
    }
}
