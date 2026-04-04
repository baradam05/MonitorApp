using System.Text;

namespace MonitorApp.MessageBuilders;

/// <summary>
/// Renders a message object into a Markdown string.
/// </summary>
public class MarkdownMessageRenderer : IMessageRenderer
{
    /// <summary>
    /// Renders the message parts into a single Markdown string.
    /// </summary>
    public string Render(Message message)
    {
        StringBuilder sb = new();
        if (!string.IsNullOrEmpty(message.Header))
        {
            sb.Append(message.Header);
        }

        if (!string.IsNullOrEmpty(message.Body))
        {
            if (sb.Length > 0)
            {
                sb.Append("\n\n");
            }
            sb.Append(message.Body);
        }

        if (!string.IsNullOrEmpty(message.Footer))
        {
            if (sb.Length > 0)
            {
                sb.Append("\n\n");
            }
            sb.Append(message.Footer);
        }

        return sb.ToString();
    }
}
