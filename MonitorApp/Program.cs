using System;
using System.Text.RegularExpressions;
using MonitorApp.JsonParsing;
using MonitorApp.JsonParsing.Help_classes;

namespace MonitorApp;

class Program
{
    static void Main(string[] args)
    {
        string? command = args.Length != 0 ? args[0] : null;
        if (command == null)
        {
            //Defualt
            
            App app = new App();
            app.Run();
            return;
        }
        
        Match m = Regex.Match(command,
            @"^/test:(?:""([^""]+)""|(.+))$",
            RegexOptions.IgnoreCase);
        if (m.Success)
        {
            var name = m.Groups[1].Success
                ? m.Groups[1].Value
                : m.Groups[2].Value;

            Console.WriteLine(name);
            return;
        }


        Console.WriteLine($"Unknown command: {command}");
    }
}