using System.Collections.Generic;

namespace SmartECommerce.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public ShippingInfo? ShippingInfo { get; set; }
        public IEnumerable<CartItem> CartItems { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
