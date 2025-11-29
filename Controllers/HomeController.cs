using Microsoft.AspNetCore.Mvc;
using SmartECommerce.Interface;
using SmartECommerce.Models.ViewModels;
using System.Threading.Tasks;

namespace SmartECommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public HomeController(ILogger<HomeController> logger, IProductService productService, ICategoryService categoryService)
        {
            _logger = logger;
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetCategoriesWithProductsAsync();

            var featuredProducts = (await _productService.GetAllProductsAsync())
                .Take(8)
                .ToList();

            var viewModel = new HomeViewModel
            {
                Categories = categories,
                FeaturedProducts = featuredProducts
            };

            return View(viewModel);
        }
    }
}
