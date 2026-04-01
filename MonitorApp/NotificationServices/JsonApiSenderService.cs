using System.Text;
using System.Text.Json;

namespace MonitorApp.NotificationServices;

/// <summary>
/// A service for sending JSON payloads to an API endpoint.
/// </summary>
public class JsonApiSenderService
{
    private readonly HttpClient httpClient;

    public JsonApiSenderService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <summary>
    /// Sends a JSON payload to the specified URL via POST.
    /// </summary>
    public async Task<string> PostJsonAsync(string url, object payload)
    {
        string json = JsonSerializer.Serialize(payload);

        using StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new Exception($"API request failed: {response.StatusCode} {error}");
        }

        return await response.Content.ReadAsStringAsync();
    }
}