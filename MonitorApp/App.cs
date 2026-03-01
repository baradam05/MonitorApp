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
    private Config config;

    public async Task Run()
    {
        config = JsonParser.Load();
        if (config == null || config.QueriesObjects == null || config.QueriesObjects.Count == 0)
        {
            Console.WriteLine("No queries found");
            return;
        }

        List<Connection> connections = new();
        List<Notification> notifications = new();

        LoadConnections(connections);
        LoadNotifications(notifications);

        await RunQueries(config.QueriesObjects, connections, notifications);
    }

    private async Task RunQueries(List<DbQueryDto> queries, List<Connection> connections, List<Notification> notifications)
    {
        foreach (DbQueryDto q in queries)
        {
            Connection c = connections.FirstOrDefault(c => c.Name == q.ConnectionDto.name);
            List<Notification> ns = notifications.Where(n => q.notifications.Any(n2 => n2.name == n.Name)).ToList();
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

            if (c.ExecuteQuery(q))
            {
                try
                {
                    foreach (var n in ns)
                    {
                        await n.Notify(q.notificationText);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
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
        foreach (JsonParsing.Help_classes.NotificationDto notification in config.Notifications)
        {
            if (notification is EmailNotificationDto emailNotificationDto)
            {
                notifications.Add(new EmailNotification(emailNotificationDto) { Name = notification.name });
            }
            else if (notification is SmsNotificationDto smsNotificationDto)
            {
                notifications.Add(new SMSNotification(smsNotificationDto) { Name = notification.name });
            }
            else if (notification is TeamsNotificationsDto teamsNotificationDto)
            {
                notifications.Add(new TeamsNotification(teamsNotificationDto) { Name = notification.name });
            }
        }
    }
}