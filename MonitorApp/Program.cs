using System.Text.RegularExpressions;
using MonitorApp.UI;

namespace MonitorApp;

class Program
{
    static async Task Main(string[] args)
    {
        //Creates a default config file on first run if it doesn't exist, then exits so the user can edit it with their settings before running the app again.
        #region FirstRun

        string configDir = Path.Combine(AppContext.BaseDirectory, "_Config");
        string configPath = Path.Combine(configDir, "config.json");

        if (!File.Exists(configPath))
        {
            Console.WriteLine($"Configuration file not found at '{configPath}'.");

            Directory.CreateDirectory(configDir);

            string defaultConfig = """
                {
                  "Connections": [
                    {
                      "type": "sql",
                      "name": "MockSQLConnection",
                      "connectionString": "Server=YOUR_SERVER;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
                    },
                    {
                      "type": "elastic",
                      "name": "MockElasticConnection",
                      "uri": "http://YOUR_ELASTICSEARCH_URI",
                      "username": "YOUR_USERNAME",
                      "password": "YOUR_PASSWORD"
                    }
                  ],
                  "Queries": [
                    {
                      "type": "sql",
                      "name": "MockQuery",
                      "connection": "MockSQLConnection",
                      "queryText": "SELECT * FROM MockTable",
                      "notifications": [
                        {
                          "type": "email",
                          "name": "MockEmailNotification",
                          "smtpServer": "YOUR_SMTP_SERVER",
                          "smtpPort": "YOUR_SMTP_PORT",
                          "username": "YOUR_EMAIL_USERNAME",
                          "password": "YOUR_EMAIL_PASSWORD",
                          "fromEmail": "FROM_EMAIL@example.com",
                          "toEmail": "TO_EMAIL@example.com",
                          "subject": "Mock Notification",
                          "useSsl": "true",
                          "notificationBody": {
                            "body": "This is a mock notification."
                          }
                        },
                        {
                          "type": "teams",
                          "name": "MockTeamsNotification",
                          "webhookUrl": "YOUR_TEAMS_WEBHOOK_URL",
                          "notificationBody": {
                            "body": "This is another mock notification."
                          }
                        },
                        {
                          "type": "sms",
                          "name": "MockSmsNotification",
                          "apiUrl": "https://api.sms_provider.com/send",
                          "accountSid": "YOUR_SID",
                          "authToken": "YOUR_TOKEN",
                          "fromNumber": "+1234567890",
                          "toNumber": "+1987654321",
                          "notificationBody": {
                            "body": "This is a mock SMS notification."
                          }
                        }
                      ]
                    },
                    {
                      "type": "inFile",
                      "name": "MockInFile",
                      "path": "mock_sql_selects.json"
                    },
                    {
                      "type": "elastic",
                      "name": "MockESQueryNoNotifications",
                      "connection": "MockElasticConnection",
                      "index": "mock-index",
                      "queryText": "{\\\"query\\\":{\\\"match_all\\\":{}}}",
                      "notifications": [
                        {
                          "type": "teams",
                          "name": "MockESTeamsNotification",
                          "webhookUrl": "YOUR_TEAMS_WEBHOOK_URL",
                          "notificationBody": {
                            "body": "This is a mock ES notification."
                          }
                        }
                      ]
                    }
                  ]
                }
                """;

            File.WriteAllText(configPath, defaultConfig);

            Console.WriteLine($"Default configuration file created at '{configPath}'.");
            Console.WriteLine("Please edit the file with your settings and restart the application.");
            return;
        }

        #endregion
        
        UIMenu menu = UI.UI.CreateMainMenu();
        string? command = menu.Run();
        
        App app = new App();

        if (command == null || command == "EXIT")
        {
            Console.WriteLine("Exiting application.");
            return;
        }

        if (command == "RUN_ALL")
        {
            try
            {
                await app.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL: An unhandled error occurred and the application must close.");
                Console.WriteLine("--- ERROR DETAILS ---");
                Console.WriteLine(e);
                Console.WriteLine("--- END ERROR DETAILS ---");
            }

            Console.WriteLine("\nAll queries have been processed. Press any key to exit.");
            Console.ReadKey();
            return;
        }

        Match m = Regex.Match(command, @"^/test:(?:""([^""]+)""|(.+))$", RegexOptions.IgnoreCase);
        if (m.Success)
        {
            string name = m.Groups[1].Success
                ? m.Groups[1].Value
                : m.Groups[2].Value;

            await app.Run(name);
            return;
        }

        Console.WriteLine($"Unknown command: {command}");
    }
}