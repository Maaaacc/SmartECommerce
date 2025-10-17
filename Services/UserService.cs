using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartECommerce.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartECommerce.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync(string search = null)
        {
            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => u.UserName.Contains(search) || u.Email.Contains(search));
            }

            return await users.ToListAsync();
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<IEnumerable<string>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        }

        public async Task UpdateUserRolesAsync(ApplicationUser user, IList<string> rolesToAdd, IList<string> rolesToRemove)
        {
            if (rolesToAdd != null && rolesToAdd.Count > 0)
            {
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            }
            if (rolesToRemove != null && rolesToRemove.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }
        }

        public async Task ToggleUserLockoutAsync(ApplicationUser user, bool lockUser)
        {
            if (lockUser)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
        }
    }
}
