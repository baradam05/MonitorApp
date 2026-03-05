using System.Net.Http.Headers;
using System.Text;
using MonitorApp.JsonParsing.Help_classes;
using MonitorApp.Notifications;
using Notification = MonitorApp.Notifications.Notification;

public class SMSNotification : Notification
{
    private readonly HttpClient httpClient;
    private readonly SmsNotificationDto config;
    private readonly JsonApiSender sender;

    public SMSNotification(SmsNotificationDto config,JsonApiSender sender)
    {
        this.httpClient = new HttpClient();
        this.config = config;
        this.sender = sender;
    }
    
    //REST API
    public override async Task Notify(string message)
    {
        try
        {
            object payload = new
            {
                from = config.fromNumber,
                to = config.toNumber,
                message = message
            };

            await sender.PostJsonAsync(config.apiUrl, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred while sending SMS notification for '{Name}':\n\n {ex.Message}");
        }
    }
    
    //HttpClient directly
    public async Task NotifyOLD(string message)
    {
        byte[] authBytes = Encoding.ASCII.GetBytes(
            $"{config.accountSid}:{config.authToken}"
        );

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(authBytes)
            );

        FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string,string>("From", config.fromNumber),
            new KeyValuePair<string,string>("To", config.toNumber),
            new KeyValuePair<string,string>("Body", message)
        });

        HttpResponseMessage response = await httpClient.PostAsync(config.apiUrl, content);
        response.EnsureSuccessStatusCode();
    }


}