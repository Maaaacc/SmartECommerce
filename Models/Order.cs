using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartECommerce.Models
{
    public enum OrderStatus
    {
        OrderPlaced,
        Processing,
        Completed,
        Cancelled
    }

    public enum PaymentMethod
    {
        CashOnDelivery
    }

    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string? UserId { get; set; }

        public ApplicationUser? User { get; set; }


        [Precision(18, 2)]
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.OrderPlaced;

        public DateTime OrderPlacedAt { get; set; } = DateTime.Now;
        public DateTime? ProcessingAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }


        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
