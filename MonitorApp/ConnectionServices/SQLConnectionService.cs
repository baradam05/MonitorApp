using Microsoft.Data.SqlClient;
using MonitorApp.JsonParsing.DTO.Queries;

namespace MonitorApp.ConnectionServices;

public class SQLConnectionService : IConnectionService
{
    public string connectionString { get; set; }
    public string Name { get; set; }
    
    public SQLConnectionService(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public bool ExecuteQuery(DbQueryDto query)
    {
        using (SqlConnection connection = new(connectionString))
        using (SqlCommand command = new SqlCommand(query.queryText, connection))
        {
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                    return reader.HasRows;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing query {query.name}:\n\n {ex.Message}");
                return false;
            }
        }
    }
}