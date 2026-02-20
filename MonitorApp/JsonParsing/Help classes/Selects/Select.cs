using System.Text.Json.Serialization;

namespace MonitorApp.JsonParsing.Help_classes;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(DbSelectDTO), "select")]
[JsonDerivedType(typeof(InFile), "inFile")]
public abstract class Select
{
    public string name;
}