namespace MonitorApp.NotificationServices;

/// <summary>
/// Defines the contract for notification services.
/// </summary>
public interface INotificationService
{
    Task Notify(string message);
    string name { get; set; }
}