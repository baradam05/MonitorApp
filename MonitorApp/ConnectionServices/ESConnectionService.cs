using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Elasticsearch.Net;
using MonitorApp.JsonParsing.DTO.Connections;
using MonitorApp.JsonParsing.DTO.Queries;
using Nest;

namespace MonitorApp.ConnectionServices;

public class ESConnectionService : IConnectionService
{
    private readonly ElasticClient client;
    public string name { get; set; }

    public ESConnectionService(EsConnectionDto esc)
    {
        ConnectionSettings settings = (esc.username == null || esc.password == null) ?
            new ConnectionSettings(new Uri(esc.uri)) :
            new ConnectionSettings(new Uri(esc.uri)).BasicAuthentication(esc.username, esc.password);

        client = new ElasticClient(settings);
    }

    public QueryResult ExecuteQuery(QueryDTO query)
    {
        if (query is not EsQueryDto esQuery)
        {
            Console.WriteLine($"Error: ESConnectionService received a non-ES query named '{query.name}'.");
            return new QueryResult { HasResults = false };
        }

        if (esQuery.queryLang == "sql")
        {
            var response = client.Sql.Query(q => q.Query(esQuery.queryText));
            var data = new List<Dictionary<string, object>>();

            if (response.IsValid && response.Rows.Any())
            {
                var columns = response.Columns.Select(c => c.Name).ToList();
                foreach (var row in response.Rows)
                {
                    var rowData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < columns.Count; i++)
                    {
                        // Convert SqlValue to a standard C# object using System.Text.Json
                        var jsonValue = JsonSerializer.Serialize(row[i]);
                        var value = JsonSerializer.Deserialize<JsonElement>(jsonValue);
                        rowData[columns[i]] = GetValueFromJsonElement(value);
                    }
                    data.Add(rowData);
                }
            }

            return new QueryResult
            {
                HasResults = data.Any(),
                Data = data
            };
        }

        if (string.IsNullOrEmpty(esQuery.index))
        {
            Console.WriteLine($"Error: No index specified for ES JSON query '{esQuery.name}'.");
            return new QueryResult { HasResults = false };
        }

        var jsonResponse = client.LowLevel.Search<StringResponse>(esQuery.index, esQuery.queryText);

        if (!jsonResponse.Success)
        {
            Console.WriteLine($"Error executing ES JSON query {esQuery.name}:\n\n {jsonResponse.DebugInformation}");
            return new QueryResult { HasResults = false };
        }

        try
        {
            var data = new List<Dictionary<string, object>>();
            using (var jsonDoc = JsonDocument.Parse(jsonResponse.Body))
            {
                if (jsonDoc.RootElement.TryGetProperty("hits", out var hitsElement) &&
                    hitsElement.TryGetProperty("hits", out var innerHits))
                {
                    foreach (var hit in innerHits.EnumerateArray())
                    {
                        if (hit.TryGetProperty("_source", out var sourceElement))
                        {
                            var row = JsonSerializer.Deserialize<Dictionary<string, object>>(sourceElement.GetRawText());
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
     private object GetValueFromJsonElement(JsonElement element)
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
                return element.GetDouble();
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
}