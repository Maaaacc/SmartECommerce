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

        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
    }
}
