using MonitorApp.JsonParsing.DTO.Connections;
using MonitorApp.JsonParsing.DTO.Notifications;

namespace MonitorApp.JsonParsing.DTO.Queries;

public class SqlQueryDto : QueryDTO
{
    public ConnectionDTO ConnectionDto { get; set; }
    public string queryText { get; set; }
}

public class SqlQueryStringDto : QueryDTO
{
    public required string connection { get; set; }
    public required string queryText { get; set; }
}