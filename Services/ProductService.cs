using Microsoft.EntityFrameworkCore;
using SmartECommerce.Data;
using SmartECommerce.Interface;
using SmartECommerce.Models;
using SmartECommerce.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartECommerce.Services
{
    /// <summary>
    /// Service for managing product operations such as CRUD,
    /// filtering, and admin-specific statistics.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IImageService _imageService;

        /// <summary>
        /// Constructs the ProductService with the specified DbContext.
        /// </summary>
        /// <param name="context">Database context</param>
        public ProductService(AppDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        /// <summary>
        /// Gets all products with optional filtering by search term, category IDs,
        /// price range, and sorting order. Only non-deleted products are returned.
        /// </summary>
        /// <param name="search">Optional search term for product name or description</param>
        /// <param name="categoryIds">Optional list of category IDs to filter</param>
        /// <param name="minPrice">Optional minimum price filter</param>
        /// <param name="maxPrice">Optional maximum price filter</param>
        /// <param name="sortOrder">Optional sort order: "price_asc", "price_desc", or default by name</param>
        /// <returns>Filtered list of products asynchronously</returns>
        public async Task<IEnumerable<Product>> GetAllProductsAsync(
            string search = null,
            List<int>? categoryIds = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string sortOrder = null,
            string status = "active")
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            query = status switch
            {
                "inactive" => query.Where(p => !p.IsActive),
                "all" => query,
                _ => query.Where(p => p.IsActive)
            };

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));

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

        /// <summary>
        /// Retrieves a product by its ID, including its category.
        /// Returns null if no such product or if the product is soft-deleted.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product or null asynchronously</returns>
        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<List<Product>> GetMoreProductAsync(Product product, int id)
        {
            if(product == null)
            {
                return new List<Product>();
            }
            var sameCategoryProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsActive)
                .Take(4)
                .ToListAsync();

            var otherProducts = await _context.Products
                .Where(p => p.CategoryId != product.CategoryId && p.Id != product.Id && p.IsActive)
                .OrderBy(p => Guid.NewGuid())
                .Take(8 - sameCategoryProducts.Count)
                .ToListAsync();

            return sameCategoryProducts.Concat(otherProducts).ToList();
        }

        /// <summary>
        /// Adds a new product to the database with current timestamps.
        /// </summary>
        /// <param name="product">Product entity to add</param>
        /// <returns>Task representing asynchronous operation</returns>
        public async Task AddProductAsync(Product product)
        {
            product.CreatedAt = DateTime.Now;
            product.UpdatedAt = DateTime.Now;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing product's fields and timestamp.
        /// Does nothing if the product does not exist.
        /// </summary>
        /// <param name="product">Product entity with updated values</param>
        /// <returns>Task representing asynchronous operation</returns>
        public async Task UpdateProductAsync(Product product, IFormFile? imageFile = null)
        {
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (existingProduct == null)
                throw new KeyNotFoundException($"Product with ID {product.Id} not found");

            // Update main fields
            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Stock = product.Stock;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.IsActive = product.IsActive;
            existingProduct.UpdatedAt = DateTime.Now;

            // Handle image upload if provided
            if (imageFile?.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    await _imageService.DeleteImageAsync(existingProduct.ImageUrl, "images/products");
                }

                var fileName = await _imageService.UploadImageAsync(imageFile, "images/products");
                if (!string.IsNullOrEmpty(fileName))
                {
                    existingProduct.ImageUrl = fileName;
                }
            }

            await _context.SaveChangesAsync();
        }


        /// <summary>
        /// Performs a soft delete on a product by setting DeletedAt timestamp.
        /// Does not remove the product from the database.
        /// </summary>
        /// <param name="id">ID of the product to delete</param>
        /// <returns>Task representing asynchronous operation</returns>
        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.DeletedAt = DateTime.Now;
                product.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        // == Admin specific methods ==

        /// <summary>
        /// Counts the number of products currently in stock (stock > 0).
        /// Only counts non-deleted products.
        /// </summary>
        /// <returns>Count of in-stock products asynchronously</returns>
        public async Task<int> GetProductsInStockCountAsync()
        {
            return await _context.Products.CountAsync(p => p.Stock > 0 && p.DeletedAt == null);
        }

        /// <summary>
        /// Counts the number of products with low stock (stock <= 10).
        /// Only counts non-deleted products.
        /// </summary>
        /// <returns>Count of low stock products asynchronously</returns>
        public async Task<int> GetLowStockCountAsync()
        {
            return await _context.Products.CountAsync(p => p.Stock <= 10 && p.DeletedAt == null);
        }

        /// <summary>
        /// Gets the top selling products by quantity sold for completed orders.
        /// </summary>
        /// <param name="count">Maximum number of products to return</param>
        /// <returns>List of ProductSales objects sorted by quantity sold</returns>
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

        /// <summary>
        /// Gets alerts for products that have low stock (<= 10).
        /// </summary>
        /// <returns>List of LowStockAlert objects</returns>
        public async Task<IEnumerable<LowStockAlert>> GetLowStockAlertsAsync()
        {
            return await _context.Products
                .Where(p => p.Stock <= 10 && p.DeletedAt == null)
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
