namespace MonitorApp.JsonParsing.DTO.Notifications;

public class NotificationBodyDto
{
    public string? head { get; set; }
    public string? groupBy { get; set; }
    public required string body { get; set; }
    public string? groupHead { get; set; }
    public string? groupFoot { get; set; }
    public string? foot { get; set; }
}