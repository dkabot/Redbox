using System;

namespace Redbox.HAL.Component.Model;

public sealed class Range : IRange<int>
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