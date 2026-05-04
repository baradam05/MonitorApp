using MonitorApp.JsonParsing;
using MonitorApp.JsonParsing.DTO;
using MonitorApp.JsonParsing.DTO.Connections;
using MonitorApp.JsonParsing.DTO.Notifications;
using MonitorApp.JsonParsing.DTO.Queries;
using MonitorApp.ConnectionServices;
using MonitorApp.MessageBuilders;
using MonitorApp.NotificationServices;
using System.Text.RegularExpressions;

namespace MonitorApp;

/// <summary>
/// Main application class.
/// </summary>
public class App
{
    private Config? config;
    private List<IConnectionService> connections = new();

    /// <summary>
    /// Runs the app, executing queries and dispatching notifications.
    /// </summary>
    public async Task Run(string singleQueryName = "")
    {
        config = ConfigParser.Load();
        if (config == null || config.QueriesObjects == null || config.QueriesObjects.Count == 0)
            return;

        LoadConnections(connections);

        List<QueryDTO> queriesToRun = config.QueriesObjects;

        //If Run() is called with a query name, filter the queries to run only that one - for /test:"" command line usage
        if (!string.IsNullOrEmpty(singleQueryName))
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

    // Executes a single query and sends notifications.
    private async Task RunQuery(QueryDTO q)
    {
        ConnectionDTO? connectionDto = q switch
        {
            SqlQueryDto sqlQuery => sqlQuery.ConnectionDto,
            EsQueryDto esQuery => esQuery.ConnectionDto,
            _ => null
        };

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

        QueryResult result = await c.ExecuteQuery(q);

        if (result.HasResults)
        {
            if (q.notifications == null || !q.notifications.Any())
            {
                Console.WriteLine($"Query '{q.name}' succeeded but has no notifications defined.");
                return;
            }

            foreach (NotificationDto notification in q.notifications)
            {
                MessageBuilder messageBuilder = new();
                string finalMessage = messageBuilder.Build(result, notification);

                if (string.IsNullOrEmpty(finalMessage))
                {
                    Console.WriteLine($"Warning: Generated message for query '{q.name}' is empty. Skipping notification.");
                    continue;
                }

                //Notification sending
                List<INotificationService> services = CreateNotificationServices(new List<NotificationDto> { notification });
                INotificationService n = services.First();

                try
                {
                    Console.WriteLine($"Query '{q.name}' is sending notification '{notification.name}' (Type: {notification.GetType().Name.Replace("NotificationDto", "")}).");
                    await n.Notify(finalMessage);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error occurred while sending a notification via '{n.name}' for query '{q.name}'. Processing will continue.\n\n {e.Message}");
                }
            }
        }
    }

    // Initializes connection services based on the configuration.
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

    // Creates notification services.
    private List<INotificationService> CreateNotificationServices(List<NotificationDto> notificationDtos)
    {
        List<INotificationService> services = new();
        JsonApiSenderService jas = new(new HttpClient());

        foreach (NotificationDto notification in notificationDtos)
        {
            if (notification is EmailNotificationDto emailDto)
            {
                services.Add(new EmailNotificationService(emailDto) { name = notification.name });
            }
            else if (notification is SmsNotificationDto smsDto)
            {
                services.Add(new SmsNotificationService(smsDto, jas) { name = notification.name });
            }
            else if (notification is TeamsNotificationsDto teamsDto)
            {
                services.Add(new TeamsNotificationService(teamsDto, jas) { name = notification.name });
            }
        }
        return services;
    }
}