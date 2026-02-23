using MonitorApp.JsonParsing.Help_classes;

namespace MonitorApp.Notifications;

public abstract class Notification
{
    public abstract Task Notify(string message);
    public string name;
}