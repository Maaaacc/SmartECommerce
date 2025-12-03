namespace SmartECommerce.Models
{
    public class OrderStatusLog
    {
        public int Id {  get; set; }
        public int OrderId { get; set; }
        public OrderStatus PreviousStatus {  get; set; }
        public OrderStatus NewStatus {  get; set; }
        public string ChangedBy { get; set; } // username or user id
        public DateTime ChangedAt {  get; set; }

        public Order Order {  get; set; }
    }
}
