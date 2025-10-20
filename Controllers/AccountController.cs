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
        public async Task<IActionResult> ShippingInfo(bool fromCheckout = false)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var shipping = await _shippingInfoService.GetByUserIdAsync(userId);

            if (shipping == null)
            {
                shipping = new ShippingInfo();
            }

            ViewBag.FromCheckout = fromCheckout;
            return View(shipping);
        }

        [HttpPost]
        public async Task<IActionResult> ShippingInfo(ShippingInfo model, bool fromCheckout = false)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.UserId = _userManager.GetUserId(User);
            await _shippingInfoService.AddOrUpdateAsync(model);

            if (fromCheckout)
                return RedirectToAction("Checkout", "Order");

            TempData["Success"] = "Shipping information saved successfully.";
            return RedirectToAction("Profile", "Account");
        }
    }
}
