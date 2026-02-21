using System;
using Elasticsearch.Net;
using MonitorApp.JsonParsing.Help_classes;
using MonitorApp.Queries;
using Nest;

public class ESConnection : MonitorApp.Queries.IConnection
{
    private readonly ElasticClient client;

    public ESConnection(ESConnectionDTO esc)
    {
        var settings = new ConnectionSettings(new Uri(esc.uri))
            .BasicAuthentication(esc.username, esc.password)
            .DefaultIndex(esc.deafultIndex);

        client = new ElasticClient(settings);
    }

    public bool ExecuteQuery(string queryText)
    {
        ISearchResponse<object> response = client.Search<object>(s => s
            .Query(q => q
                .QueryString(qs => qs.Query(queryText))
            )
        );

        return response.IsValid && response.Documents.Any();
    }
}