using System;
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
            .DefaultIndex(esc.deafultIndex);

        client = new ElasticClient(settings);
    }

    public override bool ExecuteQuery(string queryText)
    {
        ISearchResponse<object> response = client.Search<object>(s => s
            .Query(q => q
                .QueryString(qs => qs.Query(queryText))
            )
        );

        return response.IsValid && response.Documents.Any();
    }
}