using System;

namespace Redbox.JSONPrettyPrinter
{
    [Serializable]
    internal struct ConsoleBrush
    {
        public ConsoleBrush(ConsoleColor color)
            : this(color, color)
        {
        }

        public ConsoleBrush(ConsoleColor color, ConsoleColor background)
        {
            Foreground = color;
            this.Background = background;
        }

        public ConsoleColor Foreground { get; }

        public ConsoleColor Background { get; }

        public ConsoleBrush ResetForeground(ConsoleColor value)
        {
            return new ConsoleBrush(value, Background);
        }

        public ConsoleBrush ResetBackground(ConsoleColor value)
        {
            return new ConsoleBrush(Foreground, value);
        }

        public void Apply()
        {
            Console.BackgroundColor = Background;
            Console.ForegroundColor = Foreground;
        }

        public static ConsoleBrush Current => new ConsoleBrush(Console.ForegroundColor, Console.BackgroundColor);

        public override string ToString()
        {
            var consoleColor = Foreground;
            var str1 = consoleColor.ToString();
            consoleColor = Background;
            var str2 = consoleColor.ToString();
            return str1 + " on " + str2;
        }
    }
}