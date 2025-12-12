using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartECommerce.Data;
using SmartECommerce.Interface;
using SmartECommerce.Models.ViewModels;
using System.Threading.Tasks;

namespace SmartECommerce.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly AppDbContext _context;
        public ProductController(IProductService productService, ICategoryService categoryService, AppDbContext context)
        {
            _productService = productService;
            _categoryService = categoryService;
            _context = context;
        }


        // GET: /Product
        // Displays product list with optional search and filter by category
        public async Task<IActionResult> Index(
            string search, 
            List<int>? categories,
            decimal? minPrice,
            decimal? maxPrice,
            string sortOrder,
            string viewMode = "grid")
        {
            var products = await _productService.GetAllProductsAsync(search, categories, minPrice, maxPrice, sortOrder);

            var allCategories = await _categoryService.GetCategoriesWithProductsAsync();

            ViewBag.Categories = new SelectList(allCategories, "Id", "Name");
            ViewBag.SelectedCategories = categories ?? new List<int>();
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortOrder = sortOrder;
            ViewBag.ViewMode = viewMode;

            var priceRange = await _context.Products
                .GroupBy(p => 1)
                .Select(g => new { MinPrice = g.Min(p => p.Price), MaxPrice = g.Max(p => p.Price) })
                .FirstOrDefaultAsync();

            ViewBag.MinPriceRange = priceRange?.MinPrice ?? 0;
            ViewBag.MaxPriceRange = priceRange?.MaxPrice ?? 1000;

            return View(products);
        }

        // GET: /Product/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();


            var moreProducts = await _productService.GetMoreProductAsync(product, id);

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                MoreProducts = moreProducts
            };

            return View(viewModel);
        }

    }
}
