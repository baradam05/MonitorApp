using System.Text.Json.Serialization;
using MonitorApp.JsonParsing.DTO.Notifications;

namespace MonitorApp.JsonParsing.DTO.Queries;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SqlQueryStringDto), "sql")]
[JsonDerivedType(typeof(EsQueryStringDto), "elastic")]
[JsonDerivedType(typeof(InFileDTO), "inFile")]

public abstract class QueryDTO
{
    public required string name { get; set; }
    public string notificationText { get; set; }
    public List<NotificationDto> notifications { get; set; }
}