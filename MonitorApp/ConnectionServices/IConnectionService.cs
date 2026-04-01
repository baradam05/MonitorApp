using MonitorApp.JsonParsing.DTO.Queries;

namespace MonitorApp.ConnectionServices;

/// <summary>
/// Defines the contract for connection services.
/// </summary>
public interface IConnectionService
{
    string name { get; set; }
    /// <summary>
    /// Executes a query and returns the results.
    /// </summary>
    Task<QueryResult> ExecuteQuery(QueryDTO query);
}