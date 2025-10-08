using System.ComponentModel.DataAnnotations;

namespace SmartECommerce.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        [Required]
        public int ProductId { get; set; }

        public Product Product { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
