using MonitorApp.JsonParsing.Help_classes;

namespace MonitorApp.Queries;

public abstract class Connection
{
    public string Name;
    public abstract bool ExecuteQuery(DbQueryDto query);

}