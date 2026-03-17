namespace MonitorApp.NotificationServices;

public interface INotificationService
{
    Task Notify(string message);
    string name { get; set; }
}