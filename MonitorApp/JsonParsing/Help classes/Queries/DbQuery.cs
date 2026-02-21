namespace MonitorApp.JsonParsing.Help_classes;

public class DbQuery : Query
{
    public Connection connection;
    public string queryText = "SELECT * FROM Table";
    public string notificationText = "Notification text";
    public List<Notification> notifications;
}

public class DbQueryDto : Query
{
    public string connection;
    public string queryText = "SELECT * FROM Table";
    public string notificationText = "Notification text";
    public List<string> notifications;
}