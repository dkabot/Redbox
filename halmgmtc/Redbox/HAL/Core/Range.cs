using Redbox.HAL.Component.Model;
using System;
using System.Collections.Generic;

namespace Redbox.HAL.Core
{
    public sealed class Range : IRange<int>
    {
        public Range(int start, int end)
        {
            this.End = end;
            this.Start = start;
        }

        public Range(string value)
        {
            string[] strArray = value.Split("..".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strArray.Length < 2)
                throw new ArgumentException("A range must be formatted as x..y");
            this.End = 0;
            this.Start = 0;
            int result1;
            if (int.TryParse(strArray[1], out result1))
                this.End = result1;
            int result2;
            if (!int.TryParse(strArray[0], out result2))
                return;
            this.Start = result2;
        }

        public bool Includes(int value) => value >= this.Start && value <= this.End;

        public bool Includes(IRange<int> range) => range.Start >= this.Start && range.End <= this.End;

        public IEnumerable<int> GetNextInRnage()
        {
            for (int i = this.Start; i <= this.End; ++i)
                yield return i;
        }

        public int Size => this.End - this.Start + 1;

        public int End { get; private set; }

        public int Start { get; private set; }
    }
}