namespace MonitorApp.JsonParsing.Help_classes;

public class Config
{
    public List<ConnectionDTO> Connections { get; set; }
    public List<NotificationDto> Notifications { get; set; }
    public List<DbQueryDto> Queries { get; set; }
}