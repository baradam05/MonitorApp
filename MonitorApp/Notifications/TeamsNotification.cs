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
        Object payload = new
        {
            text = message
        };

        string json = JsonSerializer.Serialize(payload);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.PostAsync(config.WebhookUrl, content);
        response.EnsureSuccessStatusCode();
    }
}