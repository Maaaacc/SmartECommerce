using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartECommerce.Models
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        Completed
    }

    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public DateTime OrderDate { get; set; }

        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
