using System.Net;
using System.Net.Mail;
using MonitorApp.JsonParsing.Help_classes;

namespace MonitorApp.Notifications;

public class EmailNotification : Notification
{
    private readonly EmailNotificationDto config;

    public EmailNotification(EmailNotificationDto notificationDto)
    {
        config = notificationDto;
    }

    public override async Task Notify(string message)
    {
        Console.WriteLine($" - EMAIL Notification: {message}");
        
        using SmtpClient smtpClient = new(config.SmtpServer, int.Parse(config.SmtpPort))
        {
            Credentials = new NetworkCredential(config.Username, config.Password),
            EnableSsl = Convert.ToBoolean(config.UseSsl)
        };

        using MailMessage mailMessage = new()
        {
            From = new MailAddress(config.FromEmail),
            Subject = config.Subject,
            Body = message,
            IsBodyHtml = false
        };

        mailMessage.To.Add(config.ToEmail);

        await smtpClient.SendMailAsync(mailMessage);
    }
}