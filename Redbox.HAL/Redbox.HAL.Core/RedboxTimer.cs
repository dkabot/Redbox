using System;
using System.Timers;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

public sealed class RedboxTimer
{
    private readonly Timer Timer;

    public RedboxTimer(string name, ElapsedEventHandler handler)
    {
        Name = string.IsNullOrEmpty(name) ? "Not Set" : name;
        Timer = new Timer();
        Timer.Elapsed += handler;
    }

    public bool Started => Timer.Enabled;

    public string Name { get; }

    public DateTime? NextFireTime { get; private set; }

    public void ScheduleAtNext(int hour, int minute)
    {
        ScheduleAtNextInner(new TimeSpan(hour, minute, 0));
    }

    public void ScheduleAtNext(TimeSpan span)
    {
        ScheduleAtNextInner(span);
    }

    public void FireOn(DateTime time)
    {
        var now = DateTime.Now;
        StartTimer((time - now).TotalMilliseconds);
        LogHelper.Instance.Log("Timer should fire on {0} at {1}", time.ToShortDateString(), time.ToShortTimeString());
    }

    public void Disable()
    {
        Timer.Enabled = false;
        NextFireTime = new DateTime?();
    }

    private void ScheduleAtNextInner(TimeSpan span)
    {
        var now = DateTime.Now;
        if (now.TimeOfDay < span)
        {
            var totalMilliseconds = span.Subtract(now.TimeOfDay).TotalMilliseconds;
            NextFireTime = new DateTime(now.Year, now.Month, now.Day, span.Hours, span.Minutes, 0);
            LogHelper.Instance.Log("Timer {0} should fire {1} {2} ( {3}ms from now ).", Name,
                NextFireTime.Value.ToShortDateString(), NextFireTime.Value.ToShortTimeString(), totalMilliseconds);
            StartTimer(totalMilliseconds);
        }
        else
        {
            var dateTime = new DateTime(now.Year, now.Month, now.Day, span.Hours, span.Minutes, 0).AddDays(1.0);
            NextFireTime = dateTime;
            LogHelper.Instance.Log("Timer {0} should fire on {1} {2}", Name, dateTime.ToShortDateString(),
                dateTime.ToShortTimeString());
            StartTimer(dateTime.Subtract(now).TotalMilliseconds);
        }
    }

    private void StartTimer(double delta)
    {
        if (Timer.Enabled)
            return;
        Timer.Interval = delta;
        Timer.Start();
    }
}