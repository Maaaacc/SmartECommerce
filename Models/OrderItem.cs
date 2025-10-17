using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SmartECommerce.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order Order { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Precision(18, 2)]
        public decimal Price { get; set; }
    }
}
