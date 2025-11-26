using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartECommerce.Models;
using SmartECommerce.Interface;
using SmartECommerce.Services;
using System.Threading.Tasks;

namespace SmartECommerce.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IShippingInfoService _shippingInfoService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(IShippingInfoService shippingInfoService, UserManager<ApplicationUser> userManager)
        {
            _shippingInfoService = shippingInfoService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> ShippingInfo()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var shipping = await _shippingInfoService.GetByUserIdAsync(userId);
            if (shipping == null)
            {
                shipping = new ShippingInfo
                {
                    UserId = userId  // Set it here for new records
                };
            }

            return View(shipping);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShippingInfo(ShippingInfo model)
        {
            System.Diagnostics.Debug.WriteLine("=== POST ShippingInfo START ===");

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            System.Diagnostics.Debug.WriteLine($"UserId: {userId}");
            System.Diagnostics.Debug.WriteLine($"Model.Id: {model.Id}");
            System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
                return View(model);
            }

            // Always set UserId from authenticated user
            model.UserId = userId;

            await _shippingInfoService.AddOrUpdateAsync(model);

            TempData["Success"] = "Shipping information saved successfully.";
            return RedirectToAction("Index", "Checkout");
        }

    }
}
