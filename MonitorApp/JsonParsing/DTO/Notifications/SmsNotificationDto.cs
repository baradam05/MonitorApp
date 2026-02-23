namespace MonitorApp.JsonParsing.Help_classes;

public class SmsNotificationDto : NotificationDto
{
    public string AccountSid { get; set; } = "your-account-sid";
    public string AuthToken { get; set; } = "your-auth-token";
    public string FromNumber { get; set; } = "+1234567890";
    public string ToNumber { get; set; } = "+0987654321";
}