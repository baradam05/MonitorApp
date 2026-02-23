using System.Net.Http.Headers;
using System.Text;
using MonitorApp.JsonParsing.Help_classes;
using MonitorApp.Notifications;
using Notification = MonitorApp.Notifications.Notification;

public class SMSNotification : Notification
{
    private readonly SmsNotificationDto config;
    private readonly HttpClient httpClient;

    public SMSNotification(SmsNotificationDto smsNotificationDto)
    {
        config = smsNotificationDto;
        httpClient = new HttpClient();
    }

    public override async Task Notify(string message)
    {
        string url = $"https://api.twilio.com/2010-04-01/Accounts/{config.AccountSid}/Messages.json";

        byte[] byteArray = Encoding.ASCII.GetBytes($"{config.AccountSid}:{config.AuthToken}");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        FormUrlEncodedContent content = new(new[]
        {
            new KeyValuePair<string, string>("From", config.FromNumber),
            new KeyValuePair<string, string>("To", config.ToNumber),
            new KeyValuePair<string, string>("Body", message)
        });

        HttpResponseMessage response = await httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
    }
}