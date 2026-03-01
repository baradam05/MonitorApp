using System.Text;
using System.Text.Json;
using MonitorApp.JsonParsing.Help_classes;
using MonitorApp.Notifications;
using Notification = MonitorApp.Notifications.Notification;

public class TeamsNotification : Notification
{
    private readonly TeamsNotificationsDto config;
    private readonly HttpClient httpClient;

    public TeamsNotification(TeamsNotificationsDto notification)
    {
        config = notification;
        httpClient = new HttpClient();
    }

    public override async Task Notify(string message)
    {
        var payload = new
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
                            new { 
                                type = "TextBlock", 
                                text = message,
                                wrap = true 
                            }
                        }
                    }
                }
            }
        };

        string json = JsonSerializer.Serialize(payload);
        using StringContent content = new(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.PostAsync(config.WebhookUrl, content);
    
        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Teams notification failed: {response.StatusCode}, Details: {error}");
        }
    }
}