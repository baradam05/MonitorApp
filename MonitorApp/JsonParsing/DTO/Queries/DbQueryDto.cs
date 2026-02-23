namespace MonitorApp.JsonParsing.Help_classes;

public class DbQueryDto : QueryDTO
{
    public ConnectionDTO ConnectionDto;
    public string queryText = "SELECT * FROM Table";
    public string notificationText = "Notification text";
    public List<NotificationDto> notifications;
}

public class DbQueryStringDto : QueryDTO
{
    public string connection;
    public string queryText = "SELECT * FROM Table";
    public string notificationText = "Notification text";
    public List<string> notifications;
}