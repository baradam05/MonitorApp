using MonitorApp.JsonParsing.DTO.Connections;
using MonitorApp.JsonParsing.DTO.Notifications;
using MonitorApp.JsonParsing.DTO.Queries;

namespace MonitorApp.JsonParsing.DTO;

public class Config
{
    public List<ConnectionDTO> Connections { get; set; }
    public List<NotificationDto> Notifications { get; set; }
    public List<DbQueryDto> QueriesObjects { get; set; }
    public List<QueryDTO> Queries { get; set; }
}