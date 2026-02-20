using System.Text.Json.Serialization;

namespace MonitorApp.JsonParsing.Help_classes;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EmailNotification), "email")]
[JsonDerivedType(typeof(SMSNotification), "sms")]
[JsonDerivedType(typeof(TeamsNotifications), "teams")]
public abstract class Notification
{
    public string name { get; set; }
}