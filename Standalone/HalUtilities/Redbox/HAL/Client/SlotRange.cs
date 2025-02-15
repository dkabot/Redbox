using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client
{
    public sealed class SlotRange : IRange<int>
    {
        public SlotRange(int start, int end)
        {
            End = end;
            Start = start;
        }

        public SlotRange(string value)
        {
            var strArray = value.Split("..".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strArray.Length < 2)
                throw new ArgumentException("A range must be formatted as x..y");
            End = 0;
            Start = 0;
            int result1;
            if (int.TryParse(strArray[1], out result1))
                End = result1;
            int result2;
            if (!int.TryParse(strArray[0], out result2))
                return;
            Start = result2;
        }

        public bool Includes(int value)
        {
            return value >= Start && value <= End;
        }

        public bool Includes(IRange<int> range)
        {
            return range.Start >= Start && range.End <= End;
        }

        public int End { get; }

        public int Start { get; }
    }
}