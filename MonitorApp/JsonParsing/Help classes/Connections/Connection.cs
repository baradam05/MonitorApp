using System.Text.Json.Serialization;

namespace MonitorApp.JsonParsing.Help_classes;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SQLConnection), "sql")]
[JsonDerivedType(typeof(ESConnection), "elastic")]
public abstract class Connection
{
    public string name { get; set; }
}