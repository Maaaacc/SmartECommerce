using System.ComponentModel.DataAnnotations;

namespace SmartECommerce.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password), Compare("Password")]
        public string ConfirmPassword { get; set; }

        // Shipping info
        [Required, StringLength(100)]
        public string RecipientName { get; set; }

        [Required, StringLength(200)]
        public string AddressLine1 { get; set; }

        [StringLength(200)]
        public string? AddressLine2 { get; set; }

        [Required, StringLength(100)]
        public string City { get; set; }

        [Required, StringLength(50)]
        public string State { get; set; }

        [Required, StringLength(20)]
        public string ZipCode { get; set; }

        [Required, StringLength(50)]
        public string Country { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }
    }
}
