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
        try
        {
            if (!int.TryParse(config.smtpPort, out int port))
            {
                Console.WriteLine($"Error: Invalid SMTP port configured for notification '{Name}': {config.smtpPort}");
                return;
            }

            Console.WriteLine($" - Sending EMAIL notification for '{Name}': {message}");
        
            using SmtpClient smtpClient = new(config.smtpServer, port)
            {
                Credentials = new NetworkCredential(config.username, config.password),
                EnableSsl = Convert.ToBoolean(config.useSsl)
            };

            using MailMessage mailMessage = new()
            {
                From = new MailAddress(config.fromEmail),
                Subject = config.subject,
                Body = message,
                IsBodyHtml = false
            };

            mailMessage.To.Add(config.toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (SmtpException ex)
        {
            Console.WriteLine($"Error sending email for notification '{Name}'. Please check your SMTP settings. Details: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred while sending email for notification '{Name}'. Details: {ex.Message}");
        }
    }
}