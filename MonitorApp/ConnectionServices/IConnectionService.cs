using MonitorApp.JsonParsing.DTO.Queries;

namespace MonitorApp.ConnectionServices;

public interface IConnectionService
{
    string Name { get; set; }
    bool ExecuteQuery(DbQueryDto query);
}