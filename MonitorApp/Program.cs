using System.Text.RegularExpressions;

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
                          "notificationText": "This is a mock notification."
                        },
                        {
                          "type": "teams",
                          "name": "MockTeamsNotification",
                          "webhookUrl": "YOUR_TEAMS_WEBHOOK_URL",
                          "format": "xml",
                          "notificationText": "This is another mock notification."
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
                      "notifications": []
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
        

        string? command = args.Length != 0 ? args[0] : null;
        App app = new App();
        
        //If no command is provided, run the app normally.
        if (command == null)
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
            return;
        }

        //Argument to test certain XXX
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