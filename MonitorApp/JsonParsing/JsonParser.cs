using System.Text.Json;
using MonitorApp.JsonParsing.DTO;
using MonitorApp.JsonParsing.DTO.Connections;
using MonitorApp.JsonParsing.DTO.Queries;

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

        if (dto.Queries == null || dto.Queries.Count == 0)
        {
            Console.WriteLine($"Queries are required in config file.");
            return null;
        }
        
        Config resolvedConfig = new();
        resolvedConfig.Connections = dto.Connections;
        resolvedConfig.QueriesObjects = new();
        
        foreach(QueryDTO queryDto in dto.Queries)
        {
            if (queryDto is InFileDTO inFile)
            {
                List <QueryDTO>? queriesFromFile = LoadQueriesFromFile(inFile.path);
                if(queriesFromFile == null)
                    return null;
                foreach (QueryDTO queryFromFile in queriesFromFile)
                {
                    QueryDTO? resolvedQuery = ResolveQuery(queryFromFile, resolvedConfig.Connections);
                    if (resolvedQuery == null)
                        return null; 
                    resolvedConfig.QueriesObjects.Add(resolvedQuery);
                }
            }
            else
            {
                QueryDTO? resolvedQuery = ResolveQuery(queryDto, resolvedConfig.Connections);
                if (resolvedQuery == null)
                    return null;
                resolvedConfig.QueriesObjects.Add(resolvedQuery);
            }
        }

        if (resolvedConfig.QueriesObjects.Count == 0)
        {
            Console.WriteLine($"No valid queries were loaded.");
            return null;
        }
        
        return resolvedConfig;
    }

    private static List<QueryDTO>? LoadQueriesFromFile(string file)
    {
        string fullPath = Path.Combine(AppContext.BaseDirectory, "_Config", file);
        if (!File.Exists(fullPath))
        {
            Console.WriteLine($"File not found:\n\"{fullPath}\"");
            return null;
        }

        try
        {
            string json = File.ReadAllText(fullPath);
            return JsonSerializer.Deserialize<List<QueryDTO>>(json, new JsonSerializerOptions
            {
                IncludeFields = true
            });
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load or parse queries file\n\"{fullPath}\": \n\n" + e.Message);
            return null;
        }
    }
    
    private static QueryDTO? ResolveQuery(QueryDTO query, List<ConnectionDTO> connections)
    {
        if (query is SqlQueryStringDto sqlStringDto)
        {
            ConnectionDTO? c = connections.FirstOrDefault(c => c.name == sqlStringDto.connection);
            if (c == null)
            {
                Console.WriteLine($"Connection '{sqlStringDto.connection}' not found for query '{sqlStringDto.name}'.");
                return null;
            }
            
            return new SqlQueryDto
            {
                name = sqlStringDto.name,
                ConnectionDto = c,
                queryText = sqlStringDto.queryText,
                notificationText = sqlStringDto.notificationText,
                notifications = sqlStringDto.notifications
            };
        }
        
        if (query is EsQueryStringDto esStringDto)
        {
            ConnectionDTO? c = connections.FirstOrDefault(c => c.name == esStringDto.connection);
            if (c == null)
            {
                Console.WriteLine($"Connection '{esStringDto.connection}' not found for query '{esStringDto.name}'.");
                return null;
            }

            if (esStringDto.queryLang == null && esStringDto.index == null)
            {
                Console.WriteLine($"Query '{esStringDto.name}' must have default index if not using SQL format");
                return null;
            }
            
            return new EsQueryDto
            {
                name = esStringDto.name,
                ConnectionDto = c,
                queryText = esStringDto.queryText,
                index = esStringDto.index,
                notificationText = esStringDto.notificationText,
                notifications = esStringDto.notifications
            };
        }

        Console.WriteLine($"Unknown query type for query named '{query.name}'.");
        return null;
    }
}