using MonitorApp.JsonParsing.DTO.Connections;

namespace MonitorApp.JsonParsing.DTO.Queries;

public class EsQueryDto : QueryDTO
{
    public ConnectionDTO ConnectionDto { get; set; }
    public string queryText { get; set; }
    public string? index { get; set; }
    public string? queryLang { get; set; }
}

public class EsQueryStringDto : QueryDTO
{
    public required string connection { get; set; }
    public required string queryText { get; set; }
    public string? index { get; set; }
    public string? queryLang { get; set; }
}
