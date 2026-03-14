using MonitorApp.JsonParsing.DTO.Notifications;

namespace MonitorApp.NotificationServices;

public class TeamsNotificationService : INotificationService
{
    private readonly TeamsNotificationsDto config;
    private readonly JsonApiSenderService sender;
    public string Name { get; set; }

    public TeamsNotificationService(TeamsNotificationsDto config, JsonApiSenderService sender)
    {
        this.config = config;
        this.sender = sender;
        Name = config.name;
    }

    public async Task Notify(string message)
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