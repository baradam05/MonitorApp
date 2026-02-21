using System.Runtime.CompilerServices;
using System.Text.Json;
using MonitorApp.JsonParsing.Help_classes;

namespace MonitorApp.JsonParsing;

public static class JsonParser
{
    private static string filePath = Path.Combine(AppContext.BaseDirectory, "config.json");
    private static Config config = null;
    
    public static Config? Load()
    {
        if (!File.Exists(filePath))
            return null;
        if (config != null)
            return config;
        
        string json = File.ReadAllText(filePath);
        Config? configDTO = JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions
        {
            IncludeFields = true
        });

        return DTOToConfig(configDTO);
    }

    public static void Save(Config config)
    {
        throw new Exception("\n\nNot tested yet, use with caution\n\n");
        Config configDTO = ConfigToDTO(config);

        string json = JsonSerializer.Serialize(configDTO, new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true
        });

        if (!File.Exists(filePath))
            File.Create(filePath).Close();

        using (StreamWriter sw = new StreamWriter(filePath))
            sw.Write(json);
    }

    private static Config DTOToConfig(Config dto)
    {
        Config config = new();
        config.Connections = dto.Connections;
        config.Notifications = dto.Notifications;
        config.Queries = new();
        
        foreach(Query selectDTO in dto.Queries)
        {
            if (selectDTO is DbQueryDto dbDTO)
            {
                Query s = DTOToSelect(dbDTO, config);
                config.Queries.Add(s);
            }
            else if (selectDTO is InFile inFile)
            {
                List <DbQueryDto> selectsFromFile = LoadSelectsFromFile(inFile.path);
                foreach (DbQueryDto inFileSelect in selectsFromFile)
                {
                    config.Queries.Add(DTOToSelect(inFileSelect, config));
                }
            }
        }

        return config;
    }

    private static List<DbQueryDto> LoadSelectsFromFile(string filePath)
    {
        filePath = Path.Combine(AppContext.BaseDirectory, filePath);
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found: " + filePath);
            return new();
        }
        
        string json = File.ReadAllText(filePath);
        List<DbQueryDto> selects = JsonSerializer.Deserialize<List<DbQueryDto>>(json, new JsonSerializerOptions
        {
            IncludeFields = true
        });
        
        return selects;
    }
    
    private static Query DTOToSelect(DbQueryDto dbDTO, Config config)
    {
        return new DbQuery
        {
            name = dbDTO.name,
            connection = config.Connections.FirstOrDefault(c => c.name == dbDTO.connection),
            queryText = dbDTO.queryText,
            notifications = config.Notifications
                .Where(n => dbDTO.notifications.Contains(n.name))
                .ToList()
        };
    }
    
    private static Config ConfigToDTO(Config config)
    {
        var dto = new Config
        {
            Connections = config.Connections,
            Notifications = config.Notifications,
            Queries = config.Queries.Select(s =>
            {
                if (s is DbQuery dbSelect)
                {
                    return new DbQueryDto
                    {
                        name = dbSelect.name,
                        connection = dbSelect.connection?.name,
                        queryText = dbSelect.queryText,
                        notifications = dbSelect.notifications.Select(n => n.name).ToList()
                    };
                }

                return s;
            }).ToList()
        };

        return dto;
    }
}