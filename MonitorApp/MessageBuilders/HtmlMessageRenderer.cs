using System.Text;

namespace MonitorApp.MessageBuilders;

/// <summary>
/// Renders a message object into an HTML string.
/// </summary>
public class HtmlMessageRenderer : IMessageRenderer
{
    /// <summary>
    /// Renders the message parts into a single HTML string.
    /// </summary>
    public string Render(Message message)
    {
        StringBuilder sb = new();
        sb.Append("<html><body>");

        if (!string.IsNullOrEmpty(message.Header))
        {
            sb.Append($"<div>{message.Header}</div>");
        }

        if (!string.IsNullOrEmpty(message.Body))
        {
            if (!string.IsNullOrEmpty(message.Header))
            {
                sb.Append("<br/>");
            }
            sb.Append($"<div>{message.Body}</div>");
        }

        if (!string.IsNullOrEmpty(message.Footer))
        {
            if (!string.IsNullOrEmpty(message.Header) || !string.IsNullOrEmpty(message.Body))
            {
                sb.Append("<br/>");
            }
            sb.Append($"<div>{message.Footer}</div>");
        }

        sb.Append("</body></html>");
        return sb.ToString();
    }
}
