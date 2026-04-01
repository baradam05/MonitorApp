using System.Net.Http.Headers;
using System.Text;
using MonitorApp.JsonParsing.DTO.Notifications;

namespace MonitorApp.NotificationServices;

/// <summary>
/// Sends SMS notifications via an external API.
/// </summary>
public class SmsNotificationService : INotificationService
{
    private readonly HttpClient httpClient;
    private readonly SmsNotificationDto config;
    private readonly JsonApiSenderService sender;

    public string name { get; set; }

    public SmsNotificationService(SmsNotificationDto config, JsonApiSenderService sender)
    {
        this.httpClient = new HttpClient();
        this.config = config;
        this.sender = sender;
        name = config.name;
    }
    
    /// <summary>
    /// Sends an SMS notification using a JSON payload.
    /// </summary>
    public async Task Notify(string message)
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
            Console.WriteLine($"An unexpected error occurred while sending SMS notification for '{name}':\n\n {ex.Message}");
        }
    }
    
    /// <summary>
    /// DEPRECATED: Sends an SMS notification using form-urlencoded content.
    /// </summary>
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