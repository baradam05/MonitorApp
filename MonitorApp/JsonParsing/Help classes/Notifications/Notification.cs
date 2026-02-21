using System.Text.Json.Serialization;

namespace MonitorApp.JsonParsing.Help_classes;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EmailNotificationDTO), "email")]
[JsonDerivedType(typeof(SMSNotificationDTO), "sms")]
[JsonDerivedType(typeof(TeamsNotificationsDTO), "teams")]
public abstract class Notification
{
    public string name { get; set; }
}