using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SmartECommerce.Services;
using System.Collections.Generic;
using System.Text.Json;

public static class TempDataExtensions
{
    private const string NotificationsKey = "Notifications";

    public static void PutNotifications(this ITempDataDictionary tempData, List<NotificationMessage> notifications)
    {
        tempData[NotificationsKey] = JsonSerializer.Serialize(notifications);
    }

    public static List<NotificationMessage> GetNotifications(this ITempDataDictionary tempData)
    {
        if (tempData.TryGetValue(NotificationsKey, out var obj) && obj is string json)
        {
            return JsonSerializer.Deserialize<List<NotificationMessage>>(json) ?? new List<NotificationMessage>();
        }
        return new List<NotificationMessage>();
    }
}
