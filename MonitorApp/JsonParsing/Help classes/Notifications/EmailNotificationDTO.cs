namespace MonitorApp.JsonParsing.Help_classes;

public class EmailNotificationDTO : Notification
{
    public string SmtpServer { get; set; } = "smtp.gmail.com";
    public string SmtpPort { get; set; } = "587";
    public string Username { get; set; } = "your-email@gmail.com";
    public string Password { get; set; } = "your-password";
    public string FromEmail { get; set; } = "your-email@gmail.com";
    public string UseSsl { get; set; } = "true";
}