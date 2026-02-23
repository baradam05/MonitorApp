using Elasticsearch.Net;
using MonitorApp.JsonParsing;
using MonitorApp.JsonParsing.Help_classes;
using MonitorApp.Notifications;
using Notification = MonitorApp.Notifications.Notification;

namespace MonitorApp;

using Connection = Queries.Connection;

public class App
{
    private Config config;

    public void Run()
    {
        config = JsonParser.Load();
        if (config == null || config.Queries == null || config.Queries.Count == 0)
        {
            Console.WriteLine("No queries found");
            return;
        }

        List<Connection> connections = new();
        List<Notification> notifications = new();

        LoadConnections(connections);
        LoadNotifications(notifications);

        RunQueries(config.Queries, connections, notifications);
    }

    private void RunQueries(List<DbQueryDto> queries, List<Connection> connections, List<Notification> notifications)
    {
        foreach (DbQueryDto q in queries)
        {
            Connection c = connections.FirstOrDefault(c => c.Name == q.ConnectionDto.name);
            List<Notification> ns = notifications.Where(n => q.notifications.Any(n2 => n2.name == n.name)).ToList();
            if (c == null)
            {
                Console.WriteLine($"Connection {q.ConnectionDto.name} not found for query {q.name}");
                continue;
            }

            if (ns.Count == 0)
            {
                Console.WriteLine($"No notifications found for query {q.name}");
                continue;
            }

            if (c.ExecuteQuery(q.queryText))
            {
                foreach (Notification notification in ns)
                {
                    notifications.ForEach(n => n.Notify(q.notificationText));
                }
            }
        }
    }
    
    private void LoadConnections(List<Queries.Connection> connections)
    {
        foreach (ConnectionDTO connection in config.Connections)
        {
            if (connection is SqlConnectionDto sqlConnectionDto)
            {
                connections.Add(new SQLConnection(sqlConnectionDto.connectionString));
            }
            else if (connection is EsConnectionDto esConnectionDto)
            {
                connections.Add(new ESConnection(esConnectionDto));
            }
        }
    }

    private void LoadNotifications(List<Notification> notifications)
    {
        foreach (JsonParsing.Help_classes.NotificationDto notification in config.Notifications)
        {
            if (notification is EmailNotificationDto emailNotificationDto)
            {
                notifications.Add(new EmailNotification(emailNotificationDto));
            }
            else if (notification is SmsNotificationDto smsNotificationDto)
            {
                notifications.Add(new SMSNotification(smsNotificationDto));
            }
            else if (notification is TeamsNotificationsDto teamsNotificationDto)
            {
                notifications.Add(new TeamsNotification(teamsNotificationDto));
            }
        }
    }
}