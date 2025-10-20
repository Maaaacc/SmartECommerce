using Microsoft.AspNetCore.Mvc;
using SmartECommerce.Interface;
using System.Threading.Tasks;

namespace SmartECommerce.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: /Product
        // Displays product list with optional search and filter by category
        public async Task<IActionResult> Index(string search, int? category)
        {
            var products = await _productService.GetAllProductsAsync(search, category);
            return View(products);
        }

        // GET: /Product/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }
    }
}
