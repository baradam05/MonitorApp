namespace MonitorApp.JsonParsing.Help_classes;

public class DbQueryDto : QueryDTO
{
    public ConnectionDTO ConnectionDto { get; set; }
    public string queryText { get; set; }
    public string? queryLang { get; set; }
    public string notificationText { get; set; }
    public List<NotificationDto> notifications { get; set; }
}

public class DbQueryStringDto : QueryDTO
{
    public required string connection { get; set; }
    public required string queryText { get; set; }
    public string? queryLang { get; set; }
    public required string notificationText { get; set; }
    public required List<string> notifications { get; set; }
}