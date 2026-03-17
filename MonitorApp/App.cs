using MonitorApp.JsonParsing;
using MonitorApp.JsonParsing.DTO;
using MonitorApp.JsonParsing.DTO.Connections;
using MonitorApp.JsonParsing.DTO.Notifications;
using MonitorApp.JsonParsing.DTO.Queries;
using MonitorApp.ConnectionServices;
using MonitorApp.NotificationServices;
using System.Text.RegularExpressions;

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

        QueryResult result = c.ExecuteQuery(q);

        if (result.HasResults)
        {
            if (q.notifications == null || !q.notifications.Any())
            {
                Console.WriteLine($"Query '{q.name}' succeeded but has no notifications defined.");
                return;
            }

            string finalMessage;
            string xmlLikePattern = @"<\s*(head|body|footer|group)";

            if (!string.IsNullOrEmpty(q.notificationText) && Regex.IsMatch(q.notificationText, xmlLikePattern, RegexOptions.Singleline))
            {
                try
                {
                    var generator = new MessageGenerator();
                    finalMessage = generator.Generate(q.notificationText, result.Data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error generating message from template for query '{q.name}': {ex.Message}");
                    finalMessage = "Error: Failed to process notification template.";
                }
            }
            else
            {
                finalMessage = q.notificationText;
            }
            
            if (string.IsNullOrEmpty(finalMessage))
            {
                Console.WriteLine($"Warning: Generated message for query '{q.name}' is empty. Skipping notification.");
                return;
            }

            List<INotificationService> notificationServices = CreateNotificationServices(q.notifications);
            foreach (var n in notificationServices)
            {
                try
                {
                    await n.Notify(finalMessage);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error occurred while sending a notification via '{n.name}' for query '{q.name}'. Processing will continue.\n\n {e.Message}");
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
        var jas = new JsonApiSenderService(new HttpClient());

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