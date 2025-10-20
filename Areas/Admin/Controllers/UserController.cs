using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartECommerce.Interface;
using SmartECommerce.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index(string search)
        {
            var users = await _userService.GetAllUsersAsync(search);
            ViewData["Search"] = search;
            return View(users);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userService.GetUserRolesAsync(user);
            var allRoles = await _userService.GetAllRolesAsync();

            var model = new UserDetailsViewModel
            {
                User = user,
                UserRoles = userRoles,
                AllRoles = allRoles.ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoles(string userId, List<string> selectedRoles)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userService.GetUserRolesAsync(user);
            var rolesToAdd = selectedRoles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(selectedRoles).ToList();

            await _userService.UpdateUserRolesAsync(user, rolesToAdd, rolesToRemove);
            TempData["SuccessMessage"] = "Roles updated successfully.";
            return RedirectToAction(nameof(Details), new { id = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLockout(string userId, bool lockUser)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            await _userService.ToggleUserLockoutAsync(user, lockUser);
            TempData["SuccessMessage"] = lockUser ? "User locked." : "User unlocked.";
            return RedirectToAction(nameof(Details), new { id = userId });
        }
    }
}
