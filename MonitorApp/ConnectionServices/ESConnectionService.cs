using System.Text.Json;
using Elasticsearch.Net;
using MonitorApp.JsonParsing.DTO.Connections;
using MonitorApp.JsonParsing.DTO.Queries;
using Nest;

namespace MonitorApp.ConnectionServices;

public class ESConnectionService : IConnectionService
{
    private readonly ElasticClient client;
    public string Name { get; set; }

    public ESConnectionService(EsConnectionDto esc)
    {
        ConnectionSettings settings = new ConnectionSettings(new Uri(esc.uri))
            .BasicAuthentication(esc.username, esc.password)
            .DefaultIndex(esc.defaultIndex);

        client = new ElasticClient(settings);
    }

    public bool ExecuteQuery(DbQueryDto query)
    {
        if (query.queryLang == "sql")
        {
            QuerySqlResponse response = client.Sql.Query(q => q.Query(query.queryText));
            return response.IsValid && response.Rows.Any();
        }
        
        string defaultIndex = client.ConnectionSettings.DefaultIndex;
        StringResponse jsonQueryResponse = client.LowLevel.Search<StringResponse>(defaultIndex, query.queryText);

        if (!jsonQueryResponse.Success)
        {
            Console.WriteLine($"Error executing query {query.name}:\n\n {jsonQueryResponse.DebugInformation}");
            return false;
        }

        try
        {
            using (JsonDocument doc = JsonDocument.Parse(jsonQueryResponse.Body))
            {
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("hits", out JsonElement hitsElement))
                {
                    if (hitsElement.TryGetProperty("total", out JsonElement totalElement))
                    {
                        if (totalElement.TryGetProperty("value", out JsonElement valueElement))
                        {
                            if (valueElement.TryGetInt32(out int totalValue) && totalValue > 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error parsing Elasticsearch response for query '{query.name}'\n\n: {ex.Message}");
            return false;
        }

        return false;
    }
}