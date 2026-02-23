using System;
using Microsoft.Data.SqlClient;
using MonitorApp.Queries;

public class SQLConnection : Connection
{
    public string connectionString { get; set; }
    
    public SQLConnection(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public override bool ExecuteQuery(string queryText)
    {
        using (SqlConnection connection = new(connectionString))
        using (SqlCommand command = new SqlCommand(queryText, connection))
        {
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                    return reader.HasRows;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing query: {ex.Message}");
                return false;
            }
        }
    }
}