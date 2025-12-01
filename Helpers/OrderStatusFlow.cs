using SmartECommerce.Models;

namespace SmartECommerce.Helpers
{
    public class OrderStatusFlow
    {
        public static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions =
            new()
            {
                { OrderStatus.OrderPlaced, new[] { OrderStatus.Processing, OrderStatus.Cancelled } },
                { OrderStatus.Processing, new[] { OrderStatus.Completed, OrderStatus.Cancelled, OrderStatus.OrderPlaced } },
                { OrderStatus.Completed,   Array.Empty<OrderStatus>() },
                { OrderStatus.Cancelled,   Array.Empty<OrderStatus>() }
            };
        public bool CanChangeStatus(OrderStatus current, OrderStatus next)
        {
            return AllowedTransitions[current].Contains(next);
        }
    }
}
