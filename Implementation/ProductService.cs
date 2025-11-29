using Microsoft.EntityFrameworkCore;
using SmartECommerce.Data;
using SmartECommerce.Interface;
using SmartECommerce.Models;
using SmartECommerce.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Net.Quic;
using System.Threading.Tasks;

namespace SmartECommerce.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync(
            string search = null, 
            List<int>? categoryIds = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string sortOrder = null)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search));

            if (categoryIds?.Any() == true)
                query = query.Where(p => p.CategoryId.HasValue && categoryIds.Contains(p.CategoryId.Value));


            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Name),
            };

            return await query.ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task AddProductAsync(Product product)
        {
            product.CreatedAt = DateTime.Now;
            product.UpdatedAt = DateTime.Now;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateProductAsync(Product product)
        {
            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);

            if (existingProduct != null)
            {
                // Update only the necessary fields
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.UpdatedAt = DateTime.Now;


                await _context.SaveChangesAsync();
            }
        }
        public async Task DeleteProductAsync(int id)
        {

            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {

                product.DeletedAt = DateTime.Now;

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        //Admin

        public async Task<int> GetProductsInStockCountAsync()
        {
            return await _context.Products.CountAsync(p => p.Stock > 0);
        }

        public async Task<int> GetLowStockCountAsync()
        {
            return await _context.Products.CountAsync(p => p.Stock <= 10);
        }

        public async Task<IEnumerable<ProductSales>> GetTopSellingProductsAsync(int count)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.Order.Status == OrderStatus.Completed)
                .GroupBy(oi => oi.Product.Name)
                .Select(g => new ProductSales
                {
                    ProductName = g.Key,
                    QuantitySold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(ps => ps.QuantitySold)
                .Take(count);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<LowStockAlert>> GetLowStockAlertsAsync()
        {
            return await _context.Products
                .Where(p => p.Stock <= 10)
                .Select(p => new LowStockAlert
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    QuantityLeft = p.Stock,
                    ReorderThreshold = 10
                })
                .ToListAsync();
        }
    }
}
