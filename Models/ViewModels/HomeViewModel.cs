using System.Collections.Generic;
using SmartECommerce.Models;

namespace SmartECommerce.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Product> FeaturedProducts { get; set; }
    }
}
