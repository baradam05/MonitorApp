using Elasticsearch.Net;
using MonitorApp.JsonParsing;
using MonitorApp.JsonParsing.Help_classes;
using IConnection = MonitorApp.Queries.IConnection;
 
namespace MonitorApp;

public class App
{
    private Config config;
    
    public void Run()
    {
        config = JsonParser.Load();
        List<IConnection> connections = new();

        foreach (Connection connection in config.Connections)
        {
            if (connection is SQLConnectionDTO sqlConnectionDto)
            {
                connections.Add(new SQLConnection(sqlConnectionDto.connectionString));
            }
            else if (connection is ESConnectionDTO esConnectionDto)
            {
                connections.Add(new ESConnection(esConnectionDto));
            }
        }
        
    }
}