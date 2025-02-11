using System;

namespace Redbox.JSONPrettyPrinter
{
    [Serializable]
    internal struct ConsoleBrush
    {
        private readonly ConsoleColor foreground;
        private readonly ConsoleColor background;

        public ConsoleBrush(ConsoleColor color)
        {
            int num = (int)color;
            this = new ConsoleBrush((ConsoleColor)num, (ConsoleColor)num);
        }

        public ConsoleBrush(ConsoleColor foreground, ConsoleColor background)
        {
            this.foreground = foreground;
            this.background = background;
        }

        public ConsoleColor Foreground => this.foreground;

        public ConsoleColor Background => this.background;

        public ConsoleBrush ResetForeground(ConsoleColor value)
        {
            return new ConsoleBrush(value, this.Background);
        }

        public ConsoleBrush ResetBackground(ConsoleColor value)
        {
            return new ConsoleBrush(this.Foreground, value);
        }

        public void Apply()
        {
            Console.BackgroundColor = this.Background;
            Console.ForegroundColor = this.Foreground;
        }

        public static ConsoleBrush Current
        {
            get => new ConsoleBrush(Console.ForegroundColor, Console.BackgroundColor);
        }

        public override string ToString()
        {
            return this.Foreground.ToString() + " on " + (object)this.Background;
        }
    }
}
