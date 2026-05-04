using MonitorApp.JsonParsing;
using System.Diagnostics;
using MonitorApp.JsonParsing.DTO;
using MonitorApp.JsonParsing.DTO.Connections;
using MonitorApp.JsonParsing.DTO.Notifications;
using MonitorApp.JsonParsing.DTO.Queries;

namespace MonitorApp.UI
{
    public static class UI
    {
        private static Config? config = ConfigParser.Load();        
        public static UIMenu CreateMainMenu()
        {
            UIMenu menu = new();
            
            menu.AddItem(new UIComponent("Show Connections", () => ShowConnections()));
            menu.AddItem(new UIComponent("Show Queries", () => ShowQueries()));
            menu.AddItem(new UIComponent("Edit config.json", () => EditConfig()));
            menu.AddItem(new UIComponent("Test Query", () => TestQuery()));
            menu.AddItem(new UIComponent("Run All Queries", () => "RUN_ALL"));
            menu.AddItem(new UIComponent("Exit", () => "EXIT"));

            return menu;
        }

        private static string? ShowConnections()
        {
            Console.Clear();
            Console.WriteLine("CONNECTIONS\n ___________");
            
            foreach (ConnectionDTO conn in config.Connections)
            {
                Console.Write($"{conn.name}");
                if (conn is EsConnectionDto es)
                {
                    Console.WriteLine(" (Elasticsearch)");
                    Console.WriteLine($" - {es.uri}");
                }
                else if (conn is SqlConnectionDto sql)
                {
                    Console.WriteLine(" (SQL)");
                    Console.WriteLine($" - {sql.connectionString}");
                }
            }
            
            Console.WriteLine("\nPress any key to return to the menu.");
            Console.ReadKey();
            return "CONTINUE_MENU";
        }

        private static string? ShowQueries()
        {
            Console.Clear();
            Console.WriteLine("QUERIES\n___________");

            foreach (QueryDTO query in config.QueriesObjects)
            {
                Console.Write($"{query.name}");
                if (query is EsQueryDto esQuery)
                {
                    Console.WriteLine($" ({esQuery.ConnectionDto.name})");
                    if(esQuery.index != null)
                        Console.WriteLine($" - index: {esQuery.index}");
                    Console.WriteLine($" - queryText: {esQuery.queryText}");
                    
                }
                else if (query is SqlQueryDto sqlQuery)
                {
                    Console.WriteLine($" ({sqlQuery.ConnectionDto.name})");
                    Console.WriteLine($" - queryText: {sqlQuery.queryText}");

                }
                else if(query is InFileDTO inFile)
                {
                    Console.WriteLine(" (InFile)");
                    Console.WriteLine($" - file path: {inFile.path}");
                    continue;
                }

                foreach (NotificationDto notification in query.notifications)
                {
                    Console.Write($" - {notification.name}");
                    if(notification is SmsNotificationDto sms)
                    {
                        Console.WriteLine(" (SMS)");
                        Console.WriteLine($"   * apiURL: {sms.apiUrl}");
                        Console.WriteLine($"   * accountSid: {sms.accountSid}");
                        Console.WriteLine($"   * authToken: {sms.authToken}");
                        Console.WriteLine($"   * fromNumber: {sms.fromNumber}");
                        Console.WriteLine($"   * toNumber: {sms.toNumber}");
                    }
                    else if(notification is EmailNotificationDto email)
                    {
                        Console.WriteLine(" (Email)");
                        Console.WriteLine($"   * smtpServer: {email.smtpServer}");
                        Console.WriteLine($"   * smtpPort: {email.smtpPort}");
                        if (!string.IsNullOrEmpty(email.username) || !string.IsNullOrEmpty(email.password))
                        {
                            Console.WriteLine($"   * username: {email.username}");
                            Console.WriteLine($"   * password: {email.password}");
                        }

                        Console.WriteLine($"   * fromEmail: {email.fromEmail}");
                        Console.WriteLine($"   * toEmail: {email.toEmail}");
                        Console.WriteLine($"   * subject: {email.subject}");
                        Console.WriteLine($"   * useSsl: {email.useSsl}");
                    }
                    else if(notification is TeamsNotificationsDto teams)
                    {
                        Console.WriteLine(" (Teams)");
                        Console.WriteLine($"   * webhook url: {teams.webhookUrl}");
                    }

                    Console.WriteLine();
                }
            }
            
            Console.WriteLine("\nPress any key to return to the menu.");
            Console.ReadKey();
            return "CONTINUE_MENU";
        }
        
        private static string? EditConfig()
        {
            Console.Clear();
            Console.WriteLine("EDIT CONFIG\n ___________");
            
            string configDir = Path.Combine(AppContext.BaseDirectory, "_Config");
            string configPath = Path.Combine(configDir, "config.json");
            
            Process p = new() { StartInfo = new ProcessStartInfo(configPath) { UseShellExecute = true } };
            p.Start();
            Console.WriteLine("Opening 'config.json' in your default editor ...");
            
            Console.WriteLine("\nPress any key to return to the menu.");
            Console.ReadKey();
            return "CONTINUE_MENU";
        }

        private static string? TestQuery()
        {
            Console.Clear();
            Console.WriteLine("TEST QUERY\n ___________");
            Console.Write("Enter query name to test: ");
            Console.CursorVisible = true;
            string? queryName = Console.ReadLine();
            Console.CursorVisible = false;
            
            if (string.IsNullOrWhiteSpace(queryName))
                return "CONTINUE_MENU";
            
            return $"/test:\"{queryName}\"";
        }
    }
}