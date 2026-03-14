using MonitorApp.JsonParsing;
using MonitorApp.JsonParsing.DTO;
using MonitorApp.JsonParsing.DTO.Connections;
using MonitorApp.JsonParsing.DTO.Notifications;
using MonitorApp.JsonParsing.DTO.Queries;
using MonitorApp.ConnectionServices;
using MonitorApp.NotificationServices;

namespace MonitorApp;

public class App
{
    private Config? config;
    private List<IConnectionService> connections = new();

    public async Task Run(string singleQueryName = "")
    {
        config = JsonParser.Load();
        if (config == null || config.QueriesObjects == null || config.QueriesObjects.Count == 0)
            return;

        LoadConnections(connections);
        
        List<QueryDTO> queriesToRun = config.QueriesObjects;

        if(!string.IsNullOrEmpty(singleQueryName))
        {
            queriesToRun = config.QueriesObjects.Where(q => q.name == singleQueryName).ToList();
            if (queriesToRun.Count == 0)
            {
                Console.WriteLine($"Query with name '{singleQueryName}' not found.");
                return;
            }
        }
        
        foreach (QueryDTO q in queriesToRun)
        {
            await RunQuery(q);
        }
    }

    private async Task RunQuery(QueryDTO q)
    {
        // 1. Find Connection
        ConnectionDTO? connectionDto = null;
        if (q is SqlQueryDto sqlQuery)
        {
            connectionDto = sqlQuery.ConnectionDto;
        }
        else if (q is EsQueryDto esQuery)
        {
            connectionDto = esQuery.ConnectionDto;
        }

        if (connectionDto == null)
        {
            Console.WriteLine($"Could not determine connection for query '{q.name}'.");
            return;
        }

        IConnectionService? c = connections.FirstOrDefault(c => c.name == connectionDto.name);
        if (c == null)
        {
            Console.WriteLine($"Connection '{connectionDto.name}' not found for query '{q.name}'.");
            return;
        }

        // 2. Execute Query
        if (c.ExecuteQuery(q))
        {
            // 3. Create Notification Services and Notify
            if (q.notifications == null || q.notifications.Count == 0)
            {
                Console.WriteLine($"Query '{q.name}' succeeded but has no notifications defined.");
                return;
            }

            List<INotificationService> notificationServices = CreateNotificationServices(q.notifications);

            foreach (INotificationService n in notificationServices)
            {
                try
                {
                    await n.Notify(q.notificationText);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error occurred while sending a notification via '{n.Name}' for query '{q.name}'. Processing will continue.\n\n {e.Message}");
                }
            }
        } 
    }
    
    private void LoadConnections(List<IConnectionService> connections)
    {
        foreach (ConnectionDTO connection in config.Connections)
        {
            if (connection is SqlConnectionDto sqlConnectionDto)
            {
                connections.Add(new SQLConnectionService(sqlConnectionDto.connectionString) { name = connection.name });
            }
            else if (connection is EsConnectionDto esConnectionDto)
            {
                connections.Add(new ESConnectionService(esConnectionDto) { name = connection.name });
            }
        }
    }

    private List<INotificationService> CreateNotificationServices(List<NotificationDto> notificationDtos)
    {
        var services = new List<INotificationService>();
        var jas = new JsonApiSenderService(new HttpClient()); // Create one instance to share

        foreach (NotificationDto notification in notificationDtos)
        {
            if (notification is EmailNotificationDto emailDto)
            {
                services.Add(new EmailNotificationService(emailDto) { Name = notification.name });
            }
            else if (notification is SmsNotificationDto smsDto)
            {
                services.Add(new SmsNotificationService(smsDto, jas) { Name = notification.name });
            }
            else if (notification is TeamsNotificationsDto teamsDto)
            {
                services.Add(new TeamsNotificationService(teamsDto, jas) { Name = notification.name });
            }
        }
        return services;
    }
}