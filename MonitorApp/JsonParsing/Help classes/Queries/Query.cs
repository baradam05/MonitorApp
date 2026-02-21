using System.Text.Json.Serialization;

namespace MonitorApp.JsonParsing.Help_classes;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(DbQueryDto), "query")]
[JsonDerivedType(typeof(InFile), "inFile")]
public abstract class Query
{
    public string name;
}