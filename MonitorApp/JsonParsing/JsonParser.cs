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
        
        foreach(QueryDTO selectDTO in dto.Queries)
        {
            if (selectDTO is DbQueryStringDto dbDTO)
            {
                QueryDTO s = DTOToSelect(dbDTO, config);
                config.Queries.Add(s);
            }
            else if (selectDTO is InFileDTO inFile)
            {
                List <DbQueryStringDto> selectsFromFile = LoadSelectsFromFile(inFile.path);
                foreach (DbQueryStringDto inFileSelect in selectsFromFile)
                {
                    config.Queries.Add(DTOToSelect(inFileSelect, config));
                }
            }
        }

        return config;
    }

    private static List<DbQueryStringDto> LoadSelectsFromFile(string filePath)
    {
        filePath = Path.Combine(AppContext.BaseDirectory, filePath);
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found: " + filePath);
            return new();
        }
        
        string json = File.ReadAllText(filePath);
        List<DbQueryStringDto> selects = JsonSerializer.Deserialize<List<DbQueryStringDto>>(json, new JsonSerializerOptions
        {
            IncludeFields = true
        });
        
        return selects;
    }
    
    private static QueryDTO DTOToSelect(DbQueryStringDto dbStringDto, Config config)
    {
        return new DbQueryDto
        {
            name = dbStringDto.name,
            ConnectionDto = config.Connections.FirstOrDefault(c => c.name == dbStringDto.connection),
            queryText = dbStringDto.queryText,
            notifications = config.Notifications
                .Where(n => dbStringDto.notifications.Contains(n.name))
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
                if (s is DbQueryDto dbSelect)
                {
                    return new DbQueryStringDto
                    {
                        name = dbSelect.name,
                        connection = dbSelect.ConnectionDto?.name,
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