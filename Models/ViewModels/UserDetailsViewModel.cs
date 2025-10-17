using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace SmartECommerce.Models
{
    public class UserDetailsViewModel
    {
        public ApplicationUser User { get; set; }
        public IList<string> UserRoles { get; set; }
        public List<string> AllRoles { get; set; }
    }
}
