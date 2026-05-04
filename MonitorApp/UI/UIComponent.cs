namespace MonitorApp.UI
{
    /// <summary>
    /// Represents an item in a user interface menu.
    /// </summary>
    public class UIComponent
    {
        public string Text { get; }
        public Func<string?> Action { get; }

        public UIComponent(string text, Func<string?> action)
        {
            Text = text;
            Action = action;
        }
    }
}
