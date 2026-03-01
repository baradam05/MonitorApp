namespace MonitorApp.JsonParsing.Help_classes;

public class Config
{
    public List<ConnectionDTO> Connections { get; set; }
    public List<NotificationDto> Notifications { get; set; }
    public List<DbQueryDto> QueriesObjects { get; set; }
    public List<QueryDTO> Queries { get; set; }
}