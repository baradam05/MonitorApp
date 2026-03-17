using System.Text.Json.Serialization;

namespace MonitorApp.JsonParsing.DTO.Connections;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SqlConnectionDto), "sql")]
[JsonDerivedType(typeof(EsConnectionDto), "elastic")]
public abstract class ConnectionDTO
{
    public string name { get; set; }    
}