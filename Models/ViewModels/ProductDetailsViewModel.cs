namespace SmartECommerce.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public IEnumerable<Product> MoreProducts { get; set; } = new List<Product>();
    }
}
