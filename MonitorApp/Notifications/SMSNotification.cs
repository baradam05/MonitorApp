using System.Net.Http.Headers;
using System.Text;
using MonitorApp.JsonParsing.Help_classes;
using MonitorApp.Notifications;
using Notification = MonitorApp.Notifications.Notification;

public class SMSNotification : Notification
{
    private readonly HttpClient httpClient;
    private readonly SmsNotificationDto config;

    public SMSNotification(SmsNotificationDto config)
    {
        this.httpClient = new HttpClient();
        this.config = config;
    }

    public override async Task Notify(string message)
    {
        Console.WriteLine($" - SMS Notification: {message}");
        
        
        var authBytes = Encoding.ASCII.GetBytes(
            $"{config.AccountSid}:{config.AuthToken}"
        );

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(authBytes)
            );

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string,string>("From", config.FromNumber),
            new KeyValuePair<string,string>("To", config.ToNumber),
            new KeyValuePair<string,string>("Body", message)
        });

        var response = await httpClient.PostAsync(config.ApiUrl, content);
        response.EnsureSuccessStatusCode();
    }
}