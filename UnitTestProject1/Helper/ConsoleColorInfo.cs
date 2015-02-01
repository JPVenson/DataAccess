using System;
using System.Diagnostics;

namespace UnitTestProject1
{
    [DebuggerDisplay("{Text} , {ConsoleForgroundColor}")]
    public class ConsoleColorInfo
    {
        public ConsoleColorInfo(string text, ConsoleColor consoleForgroundColor, ConsoleColor consoleBackgroundColor)
        {
            Text = text;
            ConsoleForgroundColor = consoleForgroundColor;
            ConsoleBackgroundColor = consoleBackgroundColor;
        }

        public string Text { get; set; }
        public ConsoleColor ConsoleForgroundColor { get; set; }
        public ConsoleColor ConsoleBackgroundColor { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}