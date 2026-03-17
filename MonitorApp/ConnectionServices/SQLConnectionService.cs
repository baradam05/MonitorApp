using Microsoft.Data.SqlClient;
using MonitorApp.JsonParsing.DTO.Queries;

namespace MonitorApp.ConnectionServices;

public class SQLConnectionService : IConnectionService
{
    public string connectionString { get; set; }
    public string name { get; set; }
    
    public SQLConnectionService(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public QueryResult ExecuteQuery(QueryDTO query)
    {
        if (query is not SqlQueryDto sqlQuery)
        {
            Console.WriteLine($"Error: SQLConnectionService received a non-SQL query named '{query.name}'.");
            return new QueryResult { HasResults = false };
        }

        var result = new QueryResult();
        var data = new List<Dictionary<string, object>>();

        using (SqlConnection connection = new(connectionString))
        using (SqlCommand command = new SqlCommand(sqlQuery.queryText, connection))
        {
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    result.HasResults = reader.HasRows;
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }
                        data.Add(row);
                    }
                }
                result.Data = data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing query {sqlQuery.name}:\n\n {ex.Message}");
                result.HasResults = false;
            }
        }
        return result;
    }
}