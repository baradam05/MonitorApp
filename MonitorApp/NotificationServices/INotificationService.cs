namespace MonitorApp.NotificationServices;

public interface INotificationService
{
    Task Notify(string message);
    string Name { get; set; }
}