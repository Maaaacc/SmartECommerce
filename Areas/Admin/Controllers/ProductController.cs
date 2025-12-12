using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartECommerce.Data;
using SmartECommerce.Interface;
using SmartECommerce.Models;
using SmartECommerce.Models.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SmartECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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

        public async Task<IActionResult> Index(
            string search,
            List<int>? categories,
            decimal? minPrice,
            decimal? maxPrice,
            string sortOrder,
            string status = "active")
        {
            var products = await _productService.GetAllProductsAsync(search, categories, minPrice, maxPrice, sortOrder, status);

            // Load categories for filters dropdown
            ViewBag.Categories = new SelectList(
                await _categoryService.GetCategoriesWithProductsAsync(), "Id", "Name");

            // Filter state
            ViewBag.SelectedCategories = categories ?? new List<int>();
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Search = search;
            ViewBag.Status = status;

            var priceRange = await _context.Products
                .GroupBy(p => 1)
                .Select(g => new { MinPrice = g.Min(p => p.Price), MaxPrice = g.Max(p => p.Price) })
                .FirstOrDefaultAsync();

            ViewBag.MinPriceRange = priceRange?.MinPrice ?? 0;
            ViewBag.MaxPriceRange = priceRange?.MaxPrice ?? 1000;

            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.CategoryList = new SelectList(await _categoryService.GetAllCategoriesAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Generate a unique filename
                    var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", fileName);

                    // Ensure directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    product.ImageUrl = fileName;
                }

                await _productService.AddProductAsync(product);
                TempData["SuccessMessage"] = "Product created successfully";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryList = new SelectList(await _categoryService.GetAllCategoriesAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            ViewBag.CategoryList = new SelectList(await _categoryService.GetAllCategoriesAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? ImageFile)
        {
            if (id != product.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.CategoryList = new SelectList(await _categoryService.GetAllCategoriesAsync(), "Id", "Name", product.CategoryId);
                return View(product);
            }

            try
            {
                await _productService.UpdateProductAsync(product, ImageFile);
                TempData["SuccessMessage"] = "Product updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the product.";
                ViewBag.CategoryList = new SelectList(await _categoryService.GetAllCategoriesAsync(), "Id", "Name", product.CategoryId);
                return View(product);
            }

        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productService.DeleteProductAsync(id);
            TempData["SuccessMessage"] = "Product deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        // GET: Amin/Product/Details/{id}
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
