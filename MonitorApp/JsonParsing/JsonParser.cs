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
        config.Selects = new();
        
        foreach(Select selectDTO in dto.Selects)
        {
            if (selectDTO is DbSelectDTO dbDTO)
            {
                Select s = DTOToSelect(dbDTO, config);
                config.Selects.Add(s);
            }
            else if (selectDTO is InFile inFile)
            {
                List <DbSelectDTO> selectsFromFile = LoadSelectsFromFile(inFile.path);
                foreach (DbSelectDTO inFileSelect in selectsFromFile)
                {
                    config.Selects.Add(DTOToSelect(inFileSelect, config));
                }
            }
        }

        return config;
    }

    private static List<DbSelectDTO> LoadSelectsFromFile(string filePath)
    {
        filePath = Path.Combine(AppContext.BaseDirectory, filePath);
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found: " + filePath);
            return new();
        }
        
        string json = File.ReadAllText(filePath);
        List<DbSelectDTO> selects = JsonSerializer.Deserialize<List<DbSelectDTO>>(json, new JsonSerializerOptions
        {
            IncludeFields = true
        });
        
        return selects;
    }
    
    private static Select DTOToSelect(DbSelectDTO dbDTO, Config config)
    {
        return new DbSelect
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
            Selects = config.Selects.Select(s =>
            {
                if (s is DbSelect dbSelect)
                {
                    return new DbSelectDTO
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