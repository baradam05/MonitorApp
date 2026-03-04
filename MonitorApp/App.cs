using System.Globalization;
using Elasticsearch.Net;
using MonitorApp.JsonParsing;
using MonitorApp.JsonParsing.Help_classes;
using MonitorApp.Notifications;
using Notification = MonitorApp.Notifications.Notification;

namespace MonitorApp;

using Connection = Queries.Connection;

public class App
{
    private Config? config;
    private List<Connection> connections = new();
    private List<Notification> notifications = new();

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
        Connection? c = connections.FirstOrDefault(c => c.Name == q.ConnectionDto.name);
        List<Notification> ns = notifications.Where(n => q.notifications.Any(n2 => n2.name == n.Name)).ToList();
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
                foreach (Notification n in ns)
                {
                    await n.Notify(q.notificationText);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while sending a notification for query '{q.name}'. Processing will continue.");
                Console.WriteLine(e.Message);
            }
        } 
    }
    
    private void LoadConnections(List<Queries.Connection> connections)
    {
        foreach (ConnectionDTO connection in config.Connections)
        {
            if (connection is SqlConnectionDto sqlConnectionDto)
            {
                connections.Add(new SQLConnection(sqlConnectionDto.connectionString) { Name = connection.name });
            }
            else if (connection is EsConnectionDto esConnectionDto)
            {
                connections.Add(new ESConnection(esConnectionDto) { Name = connection.name });
            }
        }
    }

    private void LoadNotifications(List<Notification> notifications)
    {
        JsonApiSender jas = new(new HttpClient());
        foreach (JsonParsing.Help_classes.NotificationDto notification in config.Notifications)
        {
            if (notification is EmailNotificationDto emailNotificationDto)
            {
                notifications.Add(new EmailNotification(emailNotificationDto) { Name = notification.name });
            }
            else if (notification is SmsNotificationDto smsNotificationDto)
            {
                notifications.Add(new SMSNotification(smsNotificationDto,jas) { Name = notification.name });
            }
            else if (notification is TeamsNotificationsDto teamsNotificationDto)
            {
                notifications.Add(new TeamsNotification(teamsNotificationDto,jas) { Name = notification.name });
            }
        }
    }
}