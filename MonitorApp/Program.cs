using System;
using System.IO;
using System.Text.RegularExpressions;
using MonitorApp.JsonParsing;
using MonitorApp.JsonParsing.Help_classes;

namespace MonitorApp;

class Program
{
    static async Task Main(string[] args)
    {
        string configPath = Path.Combine(AppContext.BaseDirectory, "_Config", "config.json");
        if (!File.Exists(configPath))
        {
            Console.WriteLine($"Configuration file not found at '{configPath}'.");

            Directory.CreateDirectory(configPath);
            string defaultConfig = """
            {
              "Connections": [],
              "Notifications": [],
              "Queries": []
            }
            """;

            File.WriteAllText(configPath, defaultConfig);

            Console.WriteLine($"Default configuration file created at '{configPath}'.");
            Console.WriteLine("Please edit the file with your settings and restart the application.");
            return;
        }
        
        string? command = args.Length != 0 ? args[0] : null;
        App app = new App();
        if (command == null)
        {
            try
            {
                await app.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL: An unhandled error occurred and the application must close.");
                Console.WriteLine(e.Message);
            }
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