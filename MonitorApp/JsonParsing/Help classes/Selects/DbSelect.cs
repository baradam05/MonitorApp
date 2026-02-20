namespace MonitorApp.JsonParsing.Help_classes;

public class DbSelect : Select
{
    public Connection connection;
    public string queryText = "SELECT * FROM Table";
    public List<Notification> notifications;
}

public class DbSelectDTO : Select
{
    public string connection;
    public string queryText = "SELECT * FROM Table";
    public List<string> notifications;
}