namespace MonitorApp.MessageBuilders;

/// <summary>
/// Defines the contract for message renderers.
/// </summary>
public interface IMessageRenderer
{
    /// <summary>
    /// Renders a message into a formatted string.
    /// </summary>
    string Render(Message message);
}
