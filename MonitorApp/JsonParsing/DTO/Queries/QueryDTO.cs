using System.Text.Json.Serialization;

namespace MonitorApp.JsonParsing.Help_classes;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(DbQueryStringDto), "query")]
[JsonDerivedType(typeof(InFileDTO), "inFile")]
public abstract class QueryDTO
{
    public required string name;
}