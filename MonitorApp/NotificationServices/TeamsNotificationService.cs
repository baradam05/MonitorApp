using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MonitorApp.JsonParsing.DTO.Notifications;

namespace MonitorApp.NotificationServices;

/// <summary>
/// Sends notifications to Microsoft Teams via webhooks.
/// </summary>
public class TeamsNotificationService : INotificationService
{
    private readonly TeamsNotificationsDto config;
    private readonly JsonApiSenderService sender;

    public string name { get; set; }

    public TeamsNotificationService(TeamsNotificationsDto config, JsonApiSenderService sender)
    {
        this.config = config;
        this.sender = sender;
        name = config.name;
    }

    /// <summary>
    /// Sends a notification to the Teams webhook.
    /// </summary>
    public async Task Notify(string message)
    {
        try
        {
            List<object> adaptiveCardBody = CreateAdaptiveCardBody(message);

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
                            version = "1.5",
                            body = adaptiveCardBody
                        }
                    }
                }
            };

            string tmp = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await sender.PostJsonAsync(config.webhookUrl, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred while sending Teams notification for '{name}':\n\n {ex.Message}");
        }
    }

    // Creates an Adaptive Card body from a Markdown message.
    private List<object> CreateAdaptiveCardBody(string message)
    {
        List<object> bodyElements = new();
        string[] lines = message.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("## "))
            {
                bodyElements.Add(new { type = "TextBlock", text = trimmedLine.Substring(3), size = "Medium", weight = "Bolder", wrap = true });
            }
            else if (trimmedLine.StartsWith("# "))
            {
                bodyElements.Add(new { type = "TextBlock", text = trimmedLine.Substring(2), size = "Large", weight = "Bolder", wrap = true });
            }
            else
            {

                bodyElements.Add(new { type = "TextBlock", text = line, wrap = true });
            }
        }
        return bodyElements;
    }
}