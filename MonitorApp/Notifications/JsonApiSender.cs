using System.Text;
using System.Text.Json;

namespace MonitorApp.Notifications;

public class JsonApiSender
{
    private readonly HttpClient httpClient;

    public JsonApiSender(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

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