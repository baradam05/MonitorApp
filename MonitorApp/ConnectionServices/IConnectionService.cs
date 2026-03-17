using MonitorApp.JsonParsing.DTO.Queries;

namespace MonitorApp.ConnectionServices;

public interface IConnectionService
{
    string name { get; set; }
    QueryResult ExecuteQuery(QueryDTO query);
}