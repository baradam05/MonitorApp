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
    private List<ConnectionService> connections = new();
    private List<NotificationService> notifications = new();

    public async Task Run(string singleQueryName = "")
    {
        config = JsonParser.Load();
        if (config == null || config.QueriesObjects == null || config.QueriesObjects.Count == 0)
            return;

        LoadConnections(connections);
        LoadNotifications(notifications);
        
        if(singleQueryName != "")
        {
            List<DbQueryDto> q = config.QueriesObjects.Where(q => q.name == singleQueryName).ToList();
            if (q.Count == 0)
            {
                Console.WriteLine($"Query with name '{singleQueryName}' not found.");
                return;
            }

            config.QueriesObjects = q;
        }
        
        foreach (DbQueryDto q in config.QueriesObjects)
        {
            await RunQuery(q);
        }
    }

    private async Task RunQuery(DbQueryDto q)
    {
        ConnectionService? c = connections.FirstOrDefault(c => c.name == q.ConnectionDto.name);
        List<NotificationService> ns = notifications.Where(n => q.notifications.Any(n2 => n2.name == n.Name)).ToList();
        if (c == null)
        {
            Console.WriteLine($"Connection {q.ConnectionDto.name} not found for query {q.name}");
            return;
        }

        if (ns.Count == 0)
        {
            Console.WriteLine($"No notifications found for query {q.name}");
            return;
        }

        if (c.ExecuteQuery(q))
        {
            try
            {
                foreach (NotificationService n in ns)
                {
                    await n.Notify(q.notificationText);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while sending a notification for query '{q.name}'. Processing will continue.\n\n {e.Message}");
                Console.WriteLine();
            }
        } 
    }
    
    private void LoadConnections(List<ConnectionService> connections)
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

    private void LoadNotifications(List<NotificationService> notifications)
    {
        JsonApiSenderService jas = new(new HttpClient());
        foreach (NotificationDto notification in config.Notifications)
        {
            if (notification is EmailNotificationDto emailNotificationDto)
            {
                notifications.Add(new EmailNotificationService(emailNotificationDto) { Name = notification.name });
            }
            else if (notification is SmsNotificationDto smsNotificationDto)
            {
                notifications.Add(new SmsNotificationService(smsNotificationDto,jas) { Name = notification.name });
            }
            else if (notification is TeamsNotificationsDto teamsNotificationDto)
            {
                notifications.Add(new TeamsNotificationService(teamsNotificationDto,jas) { Name = notification.name });
            }
        }
    }
}