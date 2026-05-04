using MonitorApp.JsonParsing;
using System.Diagnostics;

namespace MonitorApp.UI
{
    /// <summary>
    /// Represents an interactive console menu.
    /// </summary>
    public class UIMenu
    {
        private readonly List<UIComponent> items = new List<UIComponent>();
        private int selectedIndex = 0;

        public void AddItem(UIComponent item)
        {
            items.Add(item);
        }

        /// <summary>
        /// Runs the interactive menu loop.
        /// </summary>
        public string? Run()
        {
            Console.CursorVisible = false;
            while (true)
            {
                Render();
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : items.Count - 1;
                        break;
                    
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex < items.Count - 1) ? selectedIndex + 1 : 0;
                        break;
                    
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.Enter:
                        string? result = items[selectedIndex].Action();
                        if (result != "CONTINUE_MENU")
                        {
                            Console.Clear();
                            return result;
                        }
                        break;
                    
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.Escape:
                         Console.CursorVisible = true;
                         Console.Clear();
                         return null; 
                }
            }
        }

        private void Render()
        {
            Console.Clear();
            Console.WriteLine("MONITOR APP");
            Console.WriteLine("-----------------");
            for (int i = 0; i < items.Count; i++)
            {
                if (i == selectedIndex)
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                }

                Console.WriteLine(items[i].Text);
                Console.ResetColor();
            }
            Console.WriteLine("-----------------");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Use ↑ and ↓ to navigate. Press Enter to select or Esc to leave.");
            Console.ResetColor();
        }
    }
}
