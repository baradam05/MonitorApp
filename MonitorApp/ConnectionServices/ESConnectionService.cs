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

    public bool ExecuteQuery(QueryDTO query)
    {
        if (query is not EsQueryDto esQuery)
        {
            Console.WriteLine($"Error: ESConnectionService received a non-ES query named '{query.name}'.");
            return false;
        }

        // Handle SQL-style queries
        if (esQuery.queryLang == "sql")
        {
            QuerySqlResponse response = client.Sql.Query(q => q.Query(esQuery.queryText));
            return response.IsValid && response.Rows.Any();
        }

        // Handle native JSON queries
        if (string.IsNullOrEmpty(esQuery.index))
        {
            Console.WriteLine($"Error: No index specified for ES JSON query '{esQuery.name}'.");
            return false;
        }

        StringResponse jsonQueryResponse = client.LowLevel.Search<StringResponse>(esQuery.index, esQuery.queryText);

        if (!jsonQueryResponse.Success)
        {
            Console.WriteLine($"Error executing ES JSON query {esQuery.name}:\n\n {jsonQueryResponse.DebugInformation}");
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
            Console.WriteLine($"Error parsing Elasticsearch response for query '{esQuery.name}'\n\n: {ex.Message}");
            return false;
        }

        return false;
    }
}