using System.Runtime.CompilerServices;
using System.Text.Json;
using MonitorApp.JsonParsing.Help_classes;
using MonitorApp.Queries;

namespace MonitorApp.JsonParsing;

public static class JsonParser
{
    private static string filePath = Path.Combine(AppContext.BaseDirectory, "_Config", "config.json");
    private static Config? config = null;
    
    public static Config? Load()
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found:\n\"{filePath}\"");
            return null;
        }
        if (config != null)
            return config;

        try
        {
            string json = File.ReadAllText(filePath);
            Config? configDTO = JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions
            {
                IncludeFields = true
            });
            
            return DTOToConfig(configDTO);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to load or parse config file: \n" + e.Message);
            return null;
        }
    }

    private static Config? DTOToConfig(Config dto)
    {
        if (dto.Connections == null || dto.Connections.Count == 0)
        {
            Console.WriteLine($"Connections are required in config file.");
            return null;
        }

        if (dto.Notifications == null || dto.Notifications.Count == 0)
        {
            Console.WriteLine($"Notifications are required in config file.");
            return null;
        }

        foreach (NotificationDto notif in dto.Notifications)
        {
            if(notif is EmailNotificationDto email)
            if(int.TryParse(email.smtpPort, out int port) == false)
            {
                Console.WriteLine($"Invalid SMTP port for notification '{email.name}': {email.smtpPort}");
                return null;
            }
        }
        
        Config config = new();
        config.Connections = dto.Connections;
        config.Notifications = dto.Notifications;
        config.QueriesObjects = new();
        
        foreach(QueryDTO selectDTO in dto.Queries)
        {
            if (selectDTO is DbQueryStringDto dbDTO)
            {
                DbQueryDto? query = DTOToSelect(dbDTO, config);
                if (query == null)
                    return null;
                
                config.QueriesObjects.Add(query);
            }
            else if (selectDTO is InFileDTO inFile)
            {
                List <DbQueryStringDto>? selectsFromFile = LoadSelectsFromFile(inFile.path);
                if(selectsFromFile == null)
                    return null;
                foreach (DbQueryStringDto inFileSelect in selectsFromFile)
                {
                    DbQueryDto? query = DTOToSelect(inFileSelect, config);
                    if (query == null)
                        return null;
                    config.QueriesObjects.Add(query);
                }
            }
        }

        if (config.QueriesObjects == null || config.QueriesObjects.Count == 0)
        {
            Console.WriteLine($"Queries are required in config file.");
            return null;
        }
        
        return config;
    }

    private static List<DbQueryStringDto>? LoadSelectsFromFile(string filePath)
    {
        filePath = Path.Combine(AppContext.BaseDirectory, "_Config", filePath);
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found:\n\"{filePath}\"");
            return null;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            List<DbQueryStringDto>? selects = JsonSerializer.Deserialize<List<DbQueryStringDto>>(json, new JsonSerializerOptions
            {
                IncludeFields = true
            });
        
            return selects;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load or parse selects file\n\"{filePath}\": \n\n" + e.Message);
            return null;
        }
        
    }
    
    private static DbQueryDto? DTOToSelect(DbQueryStringDto dbStringDto, Config config)
    {
        ConnectionDTO? c = config.Connections.FirstOrDefault(c => c.name == dbStringDto.connection);
        List<NotificationDto> ns = config.Notifications.Where(n => dbStringDto.notifications.Contains(n.name)).ToList();

        if (c == null)
        {
            Console.WriteLine("Connection not found: " + dbStringDto.connection);
            return null;
        }
        else if (ns.Count == 0)
        {
            Console.WriteLine("Notifications not found: " + dbStringDto.connection);
            return null;
        }
        
        return new DbQueryDto
        {
            name = dbStringDto.name,
            ConnectionDto = c,
            queryText = dbStringDto.queryText,
            queryLang = dbStringDto.queryLang,
            notificationText = dbStringDto.notificationText,
            notifications = ns
        };
    }
}