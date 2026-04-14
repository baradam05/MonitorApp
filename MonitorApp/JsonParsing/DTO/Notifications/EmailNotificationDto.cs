namespace MonitorApp.JsonParsing.DTO.Notifications;

public class EmailNotificationDto : NotificationDto
{
    public required string smtpServer { get; set; }
    public required string smtpPort { get; set; }
    public string? username { get; set; }
    public string? password { get; set; }
    public required string fromEmail { get; set; }
    public required string toEmail { get; set; }
    public required string subject { get; set; }
    public required string useSsl { get; set; }
}