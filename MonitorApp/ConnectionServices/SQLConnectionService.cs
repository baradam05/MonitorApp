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

    public bool ExecuteQuery(QueryDTO query)
    {
        if (query is not SqlQueryDto sqlQuery)
        {
            Console.WriteLine($"Error: SQLConnectionService received a non-SQL query named '{query.name}'.");
            return false;
        }

        using (SqlConnection connection = new(connectionString))
        using (SqlCommand command = new SqlCommand(sqlQuery.queryText, connection))
        {
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                    return reader.HasRows;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing query {sqlQuery.name}:\n\n {ex.Message}");
                return false;
            }
        }
    }
}