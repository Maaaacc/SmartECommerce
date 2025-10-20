using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SmartECommerce.Models;

namespace SmartECommerce.Interface
{
    public interface IUserService
    {
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync(string search = null);
        Task<ApplicationUser> GetUserByIdAsync(string id);
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
        Task<IEnumerable<string>> GetAllRolesAsync();
        Task UpdateUserRolesAsync(ApplicationUser user, IList<string> rolesToAdd, IList<string> rolesToRemove);
        Task ToggleUserLockoutAsync(ApplicationUser user, bool lockUser);

        //Admin
        Task<int> GetActiveCustomersCountAsync();
    }
}
