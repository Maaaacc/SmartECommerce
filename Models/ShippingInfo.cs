using System.ComponentModel.DataAnnotations;

namespace SmartECommerce.Models
{
    public class ShippingInfo
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string RecipientName { get; set; }

        [Required]
        [StringLength(200)]
        public string AddressLine1 { get; set; }

        [StringLength(200)]
        public string? AddressLine2 { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [StringLength(50)]
        public string State { get; set; }

        [Required]
        [StringLength(20)]
        public string ZipCode { get; set; }

        [Required]
        [StringLength(50)]
        public string Country { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        // Link to user
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }

}
