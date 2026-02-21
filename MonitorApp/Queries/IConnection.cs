namespace MonitorApp.Queries;

public interface IConnection
{
    public bool ExecuteQuery(string queryText);

}