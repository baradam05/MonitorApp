namespace MonitorApp.JsonParsing.Help_classes;

public class SMSNotificationDTO : Notification
{
    public string AccountSid { get; set; } = "your-account-sid";
    public string AuthToken { get; set; } = "your-auth-token";
    public string FromNumber { get; set; } = "+1234567890";
}