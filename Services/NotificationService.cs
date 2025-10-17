using System.Collections.Generic;

namespace SmartECommerce.Services
{
    public class NotificationService : INotificationService
    {
        private readonly List<NotificationMessage> _notifications = new();

        public void AddNotification(string message, NotificationType type = NotificationType.Info)
        {
            _notifications.Add(new NotificationMessage { Message = message, Type = type });
        }

        public List<NotificationMessage> GetNotifications()
        {
            return new List<NotificationMessage>(_notifications);
        }

        public void ClearNotifications()
        {
            _notifications.Clear();
        }
    }
}
