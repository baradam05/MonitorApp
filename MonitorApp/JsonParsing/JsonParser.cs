using System.Runtime.CompilerServices;
using System.Text.Json;
using MonitorApp.JsonParsing.Help_classes;
using MonitorApp.Queries;

namespace MonitorApp.JsonParsing;

public static class JsonParser
{
    private static string filePath = Path.Combine(AppContext.BaseDirectory, "config.json");
    private static Config? config = null;
    
    public static Config? Load()
    {
        if (!File.Exists(filePath))
            return null;
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
            Console.WriteLine(e);
            return null;
        }
    }

    private static Config? DTOToConfig(Config dto)
    {
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

        return config;
    }

    private static List<DbQueryStringDto>? LoadSelectsFromFile(string filePath)
    {
        filePath = Path.Combine(AppContext.BaseDirectory, filePath);
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found: " + filePath);
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
            Console.WriteLine(e);
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
            Console.WriteLine("Connection not found: " + dbStringDto.connection);
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