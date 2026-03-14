namespace MonitorApp.JsonParsing.DTO.Notifications;

public class TeamsNotificationsDto : NotificationDto
{
    public required string webhookUrl { get; set; }
}
