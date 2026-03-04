namespace MonitorApp.JsonParsing.Help_classes;

public class SmsNotificationDto : NotificationDto
{
    public required string apiUrl { get; set; }
    public required string accountSid { get; set; }
    public required string authToken { get; set; }
    
    public required string fromNumber { get; set; }
    public required string toNumber { get; set; }
}