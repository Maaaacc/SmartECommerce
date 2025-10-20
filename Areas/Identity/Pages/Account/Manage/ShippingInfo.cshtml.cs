using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartECommerce.Interface;
using SmartECommerce.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SmartECommerce.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class ShippingInfoModel : PageModel
    {
        private readonly IShippingInfoService _shippingInfoService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShippingInfoModel(IShippingInfoService shippingInfoService, UserManager<ApplicationUser> userManager)
        {
            _shippingInfoService = shippingInfoService;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required, StringLength(100)]
            [Display(Name = "Recipient Name")]
            public string RecipientName { get; set; }

            [Required, StringLength(200)]
            [Display(Name = "Address Line 1")]
            public string AddressLine1 { get; set; }

            [StringLength(200)]
            [Display(Name = "Address Line 2")]
            public string AddressLine2 { get; set; }

            [Required, StringLength(100)]
            public string City { get; set; }

            [Required, StringLength(50)]
            public string State { get; set; }

            [Required, StringLength(20)]
            [Display(Name = "Zip Code")]
            public string ZipCode { get; set; }

            [Required, StringLength(50)]
            public string Country { get; set; }

            [Phone]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("User not found.");

            var shipping = await _shippingInfoService.GetByUserIdAsync(user.Id);

            if (shipping != null)
            {
                Input = new InputModel
                {
                    RecipientName = shipping.RecipientName,
                    AddressLine1 = shipping.AddressLine1,
                    AddressLine2 = shipping.AddressLine2,
                    City = shipping.City,
                    State = shipping.State,
                    ZipCode = shipping.ZipCode,
                    Country = shipping.Country,
                    PhoneNumber = shipping.PhoneNumber
                };
            }
            else
            {
                Input = new InputModel();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("User not found.");

            var shipping = new ShippingInfo
            {
                UserId = user.Id,
                RecipientName = Input.RecipientName,
                AddressLine1 = Input.AddressLine1,
                AddressLine2 = Input.AddressLine2,
                City = Input.City,
                State = Input.State,
                ZipCode = Input.ZipCode,
                Country = Input.Country,
                PhoneNumber = Input.PhoneNumber
            };

            await _shippingInfoService.AddOrUpdateAsync(shipping);

            StatusMessage = "Your shipping information has been updated.";
            return RedirectToPage();
        }
    }
}
