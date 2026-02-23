namespace MonitorApp.Queries;

public abstract class Connection
{
    public string Name;
    public abstract bool ExecuteQuery(string queryText);

}