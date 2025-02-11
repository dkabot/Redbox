using System;
using System.Collections.Generic;

namespace Redbox.Core
{
    public class Range
    {
        public Range(int start, int end)
        {
            End = end;
            Start = start;
        }

        public Range(string value)
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

        public int Size => End - Start + 1;

        public int End { get; }

        public int Start { get; }

        public bool InRange(int value)
        {
            return value >= Start && value <= End;
        }

        public IEnumerable<int> GetNextInRnage()
        {
            for (var i = Start; i <= End; ++i)
                yield return i;
        }
    }
}