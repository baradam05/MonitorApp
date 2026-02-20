namespace MonitorApp.JsonParsing.Help_classes;

public class ESConnection : Connection
{
    public string url { get; set; }
    public string username { get; set; }
    public string password { get; set; }
    public string deafultIndex { get; set; }
}