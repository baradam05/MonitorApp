using Microsoft.Data.SqlClient;
using MonitorApp.JsonParsing.DTO.Queries;

namespace MonitorApp.ConnectionServices;

/// <summary>
/// Connection service for SQL databases.
/// </summary>
public class SQLConnectionService : IConnectionService
{
    public string connectionString { get; set; }
    public string name { get; set; }

    public SQLConnectionService(string connectionString)
    {
        this.connectionString = connectionString;
    }

    /// <summary>
    /// Executes a SQL query and returns the results.
    /// </summary>
    public async Task<QueryResult> ExecuteQuery(QueryDTO query)
    {
        if (query is not SqlQueryDto sqlQuery)
        {
            Console.WriteLine($"Error: SQLConnectionService received a non-SQL query named '{query.name}'.");
            return new QueryResult { HasResults = false };
        }

        QueryResult result = new();
        List<Dictionary<string, object>> data = new();

        using (SqlConnection connection = new(connectionString))
        using (SqlCommand command = new SqlCommand(sqlQuery.queryText, connection))
        {
            try
            {
                await connection.OpenAsync();
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    result.HasResults = reader.HasRows;
                    while (await reader.ReadAsync())
                    {
                        Dictionary<string, object> row = new(StringComparer.OrdinalIgnoreCase);
                        for (int i = 0; i < reader.FieldCount; i++)
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