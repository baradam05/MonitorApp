using System.Text.Json.Serialization;

namespace MonitorApp.JsonParsing.DTO.Notifications;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EmailNotificationDto), "email")]
[JsonDerivedType(typeof(SmsNotificationDto), "sms")]
[JsonDerivedType(typeof(TeamsNotificationsDto), "teams")]
public abstract class NotificationDto
{
    public required string name { get; set; }
    public required string notificationText { get; set; }
}