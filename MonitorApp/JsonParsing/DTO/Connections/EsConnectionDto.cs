namespace MonitorApp.JsonParsing.DTO.Connections;

public class EsConnectionDto : ConnectionDTO
{
    public required string uri { get; set; }
    public string? username { get; set; }
    public string? password { get; set; }
    public required string defaultIndex { get; set; }
}