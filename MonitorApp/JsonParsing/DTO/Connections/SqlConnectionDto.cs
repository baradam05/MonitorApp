namespace MonitorApp.JsonParsing.DTO.Connections;

public class SqlConnectionDto : ConnectionDTO
{
    public required string connectionString { get; set; }
}