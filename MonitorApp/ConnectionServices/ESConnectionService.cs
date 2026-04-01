using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Elasticsearch.Net;
using MonitorApp.JsonParsing.DTO.Connections;
using MonitorApp.JsonParsing.DTO.Queries;
using Nest;

namespace MonitorApp.ConnectionServices;

/// <summary>
/// Connection service for Elasticsearch.
/// </summary>
public class ESConnectionService : IConnectionService
{
    private readonly ElasticClient client;
    private EsConnectionDto connectionDto;
    public string name { get; set; }

    public ESConnectionService(EsConnectionDto esc)
    {
        connectionDto = esc;
        
        ConnectionSettings settings = (esc.username == null || esc.password == null)
            ? new ConnectionSettings(new Uri(esc.uri))
            : new ConnectionSettings(new Uri(esc.uri)).BasicAuthentication(esc.username, esc.password);

        client = new ElasticClient(settings);
    }

    /// <summary>
    /// Executes an Elasticsearch query by dispatching to the appropriate handler based on query language.
    /// </summary>
    public async Task<QueryResult> ExecuteQuery(QueryDTO query)
    {
        if (query is not EsQueryDto esQuery)
        {
            Console.WriteLine($"Error: ESConnectionService received a non-ES query named '{query.name}'.");
            return new QueryResult { HasResults = false };
        }

        if (string.Equals(esQuery.queryLang, "sql", StringComparison.OrdinalIgnoreCase))
        {
            return await ExecuteSqlFormatQuery(esQuery);
        }

        return ExecuteJsonFormatQuery(esQuery);
    }

    /// <summary>
    /// Executes a query using the Elasticsearch SQL dialect.
    /// </summary>
    private async Task<QueryResult> ExecuteSqlFormatQuery(EsQueryDto esQuery)
    {
        List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

        using HttpClient client = new HttpClient();

        if (!string.IsNullOrEmpty(connectionDto.username) && !string.IsNullOrEmpty(connectionDto.password))
        {
            byte[] byteArray = Encoding.ASCII.GetBytes($"{connectionDto.username}:{connectionDto.password}");
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        var requestBody = new { query = esQuery.queryText };
        string jsonPayload = JsonSerializer.Serialize(requestBody);
        StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await client.PostAsync($"{connectionDto.uri}/_sql?format=json", content);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(responseBody);
            JsonElement root = doc.RootElement;

            List<string> columns = new List<string>();
            foreach (JsonElement columnElement in root.GetProperty("columns").EnumerateArray())
            {
                columns.Add(columnElement.GetProperty("name").GetString());
            }

            foreach (JsonElement rowElement in root.GetProperty("rows").EnumerateArray())
            {
                Dictionary<string, object> rowData = new Dictionary<string, object>();
                List<JsonElement> rowValues = rowElement.EnumerateArray().ToList();

                for (int i = 0; i < columns.Count; i++)
                {
                    JsonElement value = rowValues[i];
                    rowData[columns[i]] = GetValue(value);
                }

                data.Add(rowData);
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Error connecting to Elasticsearch: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred: {e.Message}");
        }

        return new QueryResult { HasResults = data.Any(), Data = data.Count == 0 ? null : data };
    }

    private static object GetValue(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
                if (element.TryGetInt64(out long l))
                {
                    return l;
                }
                else
                {
                    return element.GetDouble();
                }
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Null:
                return null;
            default:
                return element.ToString();
        }
    }

    /// <summary>
    /// Executes a query using the Elasticsearch JSON query DSL.
    /// </summary>
    private QueryResult ExecuteJsonFormatQuery(EsQueryDto esQuery)
    {
        if (string.IsNullOrEmpty(esQuery.index))
        {
            Console.WriteLine($"Error: No index specified for ES JSON query '{esQuery.name}'.");
            return new QueryResult { HasResults = false };
        }

        StringResponse jsonResponse = client.LowLevel.Search<StringResponse>(esQuery.index, esQuery.queryText);

        if (!jsonResponse.Success)
        {
            Console.WriteLine($"Error executing ES JSON query {esQuery.name}:\n\n {jsonResponse.DebugInformation}");
            return new QueryResult { HasResults = false };
        }

        try
        {
            List<Dictionary<string, object>> data = new();
            using (JsonDocument jsonDoc = JsonDocument.Parse(jsonResponse.Body))
            {
                if (jsonDoc.RootElement.TryGetProperty("hits", out var hitsElement) &&
                    hitsElement.TryGetProperty("hits", out var innerHits))
                {
                    foreach (JsonElement hit in innerHits.EnumerateArray())
                    {
                        if (hit.TryGetProperty("_source", out var sourceElement))
                        {
                            Dictionary<string, object>? row =
                                JsonSerializer.Deserialize<Dictionary<string, object>>(sourceElement.GetRawText());
                            if (row != null)
                            {
                                data.Add(row);
                            }
                        }
                    }
                }
            }

            return new QueryResult { HasResults = data.Any(), Data = data };
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error parsing Elasticsearch response for query '{esQuery.name}'\n\n: {ex.Message}");
            return new QueryResult { HasResults = false };
        }
    }

    /// <summary>
    /// Converts a JsonElement to its corresponding .NET object representation.
    /// </summary>
    private object GetValueFromJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out long l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }
}