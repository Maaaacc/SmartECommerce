using System.Collections.Generic;

namespace SmartECommerce.Interface
{
    public interface INotificationService
    {
        void AddNotification(string message, NotificationType type = NotificationType.Info);
        List<NotificationMessage> GetNotifications();
        void ClearNotifications();
    }

    public enum NotificationType
    {
        Success,
        Info,
        Warning,
        Error
    }

    public class NotificationMessage
    {
        public string Message { get; set; }
        public NotificationType Type { get; set; }
    }
}
