using System.Text;
using System.Text.Json;
using MonitorApp.JsonParsing.Help_classes;
using MonitorApp.Notifications;
using Notification = MonitorApp.Notifications.Notification;

public class TeamsNotification : Notification
{
    private readonly TeamsNotificationsDto config;
    private readonly JsonApiSender sender;

    public TeamsNotification(TeamsNotificationsDto config, JsonApiSender sender)
    {
        this.config = config;
        this.sender = sender;
    }

    public override async Task Notify(string message)
    {
        try
        {
            object payload = new
            {
                type = "message",
                attachments = new[]
                {
                    new
                    {
                        contentType = "application/vnd.microsoft.card.adaptive",
                        content = new
                        {
                            type = "AdaptiveCard",
                            version = "1.4",
                            body = new[]
                            {
                                new
                                {
                                    type = "TextBlock",
                                    text = message,
                                    wrap = true
                                }
                            }
                        }
                    }
                }
            };

            await sender.PostJsonAsync(config.webhookUrl, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred while sending Teams notification for '{Name}':\n\n {ex.Message}");
        }
    }
}