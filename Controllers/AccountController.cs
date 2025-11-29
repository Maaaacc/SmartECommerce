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
                    UserId = userId 
                };
            }

            return View(shipping);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShippingInfo(ShippingInfo model)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            // Always set UserId from authenticated user
            model.UserId = userId;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _shippingInfoService.AddOrUpdateAsync(model);

            return RedirectToAction("Index", "Checkout");
        }


    }
}
