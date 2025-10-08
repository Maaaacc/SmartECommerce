using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartECommerce.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be greater than 0.")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Stock is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int? Stock { get; set; }

        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        public int? CategoryId { get; set; }

        public Category? Category { get; set; }
    }
}
