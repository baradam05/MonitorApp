using System.Text.Json.Serialization;

namespace MonitorApp.JsonParsing.DTO.Queries;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(DbQueryStringDto), "query")]
[JsonDerivedType(typeof(InFileDTO), "inFile")]
public abstract class QueryDTO
{
    public required string name;
}