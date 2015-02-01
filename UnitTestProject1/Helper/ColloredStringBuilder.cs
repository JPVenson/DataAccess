using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTestProject1
{
    public class ColloredStringBuilder
    {
        public ColloredStringBuilder()
        {
            TextChuncks = new Queue<ConsoleColorInfo>();
            DefauldForground = Console.ForegroundColor;
            DefauldBackground = Console.BackgroundColor;
        }

        public ConsoleColor DefauldForground { get; set; }
        public ConsoleColor DefauldBackground { get; set; }
        public ConsoleColor LastColor { get; set; }

        public Queue<ConsoleColorInfo> TextChuncks { get; private set; }

        public void AppendLine()
        {
            Append(Environment.NewLine);
        }

        public void AppendLine(string value)
        {
            AppendLine(value, DefauldForground);
        }

        public void AppendLine(char value)
        {
            AppendLine(value.ToString(), DefauldForground);
        }

        public void Append(char value)
        {
            Append(value, DefauldForground);
        }

        public void Append(string value)
        {
            Append(value, DefauldForground);
        }

        public void AppendLine(string value, ConsoleColor forground)
        {
            Append(value, forground);
            AppendLine();
        }

        public void AppendLine(char value, ConsoleColor forground)
        {
            Append(value.ToString(), forground);
            AppendLine();
        }

        public void Append(char value, ConsoleColor forground)
        {
            Append(value.ToString(), forground);
        }

        public void Append(string value, ConsoleColor forground)
        {
            Append(value, forground, DefauldBackground);
        }

        public void AppendLine(string value, ConsoleColor forground, ConsoleColor background)
        {
            Append(value, forground, background);
            AppendLine();
        }

        public void AppendLine(char value, ConsoleColor forground, ConsoleColor background)
        {
            Append(value.ToString(), forground, background);
            AppendLine();
        }

        public void Append(char value, ConsoleColor forground, ConsoleColor background)
        {
            Append(value.ToString(), forground, background);
        }

        public void Append(string value, ConsoleColor forground, ConsoleColor background)
        {
            TextChuncks.Enqueue(new ConsoleColorInfo(value, forground, background));
        }

        public long Length
        {
            get
            {
                if (!TextChuncks.Any())
                    return 0;

                return TextChuncks.Select(s => s.Text.Length).Aggregate((e, f) => e + f);
            }
        }

        public void Clear()
        {
            TextChuncks.Clear();
        }

        public void Render()
        {
            Console.SetCursorPosition(0, 0);
            Console.InputEncoding = UnicodeEncoding.Unicode;
            var consoleForgroundColor = Console.ForegroundColor;
            var consolebackgroundColor = Console.BackgroundColor;

            var sb = new StringBuilder();

            foreach (var textChunck in TextChuncks)
            {
                if (textChunck.ConsoleForgroundColor != consoleForgroundColor || textChunck.ConsoleBackgroundColor != consolebackgroundColor)
                {
                    Console.ForegroundColor = consoleForgroundColor;
                    Console.BackgroundColor = consolebackgroundColor;
                    Console.Write(sb.ToString());
                    Console.ForegroundColor = DefauldForground;
                    Console.BackgroundColor = DefauldBackground;
                    sb.Clear();
                    consoleForgroundColor = textChunck.ConsoleForgroundColor;
                    consolebackgroundColor = textChunck.ConsoleBackgroundColor;
                }

                sb.Append(textChunck.Text);
            }

            Console.Write(sb.ToString());
        }
    }
}