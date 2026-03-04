using System;
using System.Text.Json;
using Elasticsearch.Net;
using MonitorApp.JsonParsing.Help_classes;
using MonitorApp.Queries;
using Nest;

public class ESConnection : MonitorApp.Queries.Connection
{
    private readonly ElasticClient client;

    public ESConnection(EsConnectionDto esc)
    {
        ConnectionSettings settings = new ConnectionSettings(new Uri(esc.uri))
            .BasicAuthentication(esc.username, esc.password)
            .DefaultIndex(esc.defaultIndex);

        client = new ElasticClient(settings);
    }

    public override bool ExecuteQuery(DbQueryDto query)
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
            Console.WriteLine(jsonQueryResponse.OriginalException?.Message);
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
            Console.WriteLine($"Error parsing Elasticsearch response for query '{query.name}': {ex.Message}");
            return false;
        }

        return false;
    }
}