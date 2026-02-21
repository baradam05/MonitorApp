namespace MonitorApp.JsonParsing.Help_classes;

public class Config
{
    public List<Connection> Connections { get; set; }
    public List<Notification> Notifications { get; set; }
    public List<Query> Queries { get; set; }
}