namespace MonitorApp.JsonParsing.Help_classes;

public class ESConnectionDTO : Connection
{
    public string uri { get; set; }
    public string username { get; set; }
    public string password { get; set; }
    public string deafultIndex { get; set; }
}