using MonitorApp.JsonParsing.DTO.Connections;
using MonitorApp.JsonParsing.DTO.Notifications;
using MonitorApp.JsonParsing.DTO.Queries;

namespace MonitorApp.JsonParsing.DTO;

/// <summary>
/// Represents the root configuration structure.
/// This DTO is used for deserializing the main configuration JSON.
/// </summary>
public class Config
{
    public List<ConnectionDTO> Connections { get; set; } = new();
    /// <summary>
    /// Resolved query objects, ready for execution.
    /// </summary>
    public List<QueryDTO> QueriesObjects { get; set; } = new();
    /// <summary>
    /// Raw query configurations as defined in the JSON. Later resolved into <see cref="QueriesObjects"/>.
    /// </summary>
    public List<QueryDTO> Queries { get; set; } = new();
}