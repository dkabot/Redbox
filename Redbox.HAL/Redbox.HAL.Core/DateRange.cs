using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

public class DateRange : IRange<DateTime>
{
    private string m_formatted;

    internal DateRange(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    private DateRange(int start, int end)
    {
        var now = DateTime.Now;
        Start = new DateTime(now.Year, now.Month, now.Day, start, 0, 0);
        End = new DateTime(now.Year, now.Month, now.Day, end, 0, 0);
    }

    public bool Includes(DateTime value)
    {
        return Start <= value && value <= End;
    }

    public bool Includes(IRange<DateTime> range)
    {
        return Start <= range.Start && range.End <= End;
    }

    public DateTime Start { get; private set; }

    public DateTime End { get; private set; }

    public static DateRange FromHourSpan(int start, int end)
    {
        var now = DateTime.Now;
        var end1 = DateTime.Parse(end + ":00");
        var start1 = DateTime.Parse(start + ":00");
        if (now.Hour <= start)
        {
            if (end < start)
                end1 = end1.AddDays(1.0);
            return new DateRange(start1, end1);
        }

        if (end < start)
            end1 = end1.AddDays(1.0);
        if (now >= end1)
        {
            start1 = start1.AddDays(1.0);
            end1 = end1.AddDays(1.0);
        }

        return new DateRange(start1, end1);
    }

    public static bool NowIsBetweenHours(int start, int end)
    {
        return new DateRange(start, end).Includes(DateTime.Now);
    }

    public static DateRange FromDateRange(DateTime start, DateTime end)
    {
        return new DateRange(start, end);
    }

    public override string ToString()
    {
        if (m_formatted == null)
            m_formatted = string.Format("Start {0} {1} -> End {2} {3}", Start.ToShortDateString(),
                Start.ToShortTimeString(), End.ToShortDateString(), End.ToShortTimeString());
        return m_formatted;
    }

    public bool PriorToStart(DateTime time)
    {
        return time <= Start;
    }

    public void ShiftOneDay()
    {
        Start = Start.AddDays(1.0);
        End = End.AddDays(1.0);
        m_formatted = null;
    }
}