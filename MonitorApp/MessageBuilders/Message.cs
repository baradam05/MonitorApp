namespace MonitorApp.MessageBuilders;

/// <summary>
/// Represents a structured message with header, body, and footer.
/// </summary>
public class Message
{
    public string Header { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Footer { get; set; } = string.Empty;
}
