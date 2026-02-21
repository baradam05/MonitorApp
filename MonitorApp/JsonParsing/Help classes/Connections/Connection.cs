using System.Text.Json.Serialization;

namespace MonitorApp.JsonParsing.Help_classes;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SQLConnectionDTO), "sql")]
[JsonDerivedType(typeof(ESConnectionDTO), "elastic")]
public abstract class Connection
{
    public string name { get; set; }
}