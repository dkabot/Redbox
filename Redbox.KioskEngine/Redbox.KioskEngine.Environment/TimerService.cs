using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Redbox.KioskEngine.Environment
{
  public class TimerService : ITimerService
  {
    private IDictionary<string, SoftTimer> m_innerTimers;
    private readonly object m_syncLock = new object();

    public static TimerService Instance => Singleton<TimerService>.Instance;

    public void Reset()
    {
      LogHelper.Instance.Log("Reset timer service.");
      lock (this.m_syncLock)
        this.InnerTimers.Clear();
    }

    public void StopAll()
    {
      lock (this.m_syncLock)
      {
        foreach (SoftTimer softTimer in (IEnumerable<SoftTimer>) this.InnerTimers.Values)
          softTimer.Stop();
      }
    }

    public void StartAll()
    {
      lock (this.m_syncLock)
      {
        foreach (SoftTimer softTimer in (IEnumerable<SoftTimer>) this.InnerTimers.Values)
          softTimer.Start();
      }
    }

    public ITimer CreateTimer(string name, int? dueTime, int? period, TimerCallback callback)
    {
      lock (this.m_syncLock)
      {
        SoftTimer timer;
        if (this.InnerTimers.ContainsKey(name))
        {
          timer = this.InnerTimers[name];
        }
        else
        {
          timer = new SoftTimer()
          {
            DueTime = dueTime,
            Period = period,
            Name = name
          };
          this.InnerTimers[name] = timer;
        }
        timer.ClearFire();
        timer.Fire += callback;
        return (ITimer) timer;
      }
    }

    public void RemoveTimer(string name)
    {
      lock (this.m_syncLock)
        this.InnerTimers.Remove(name);
    }

    public ITimer GetTimer(string name)
    {
      lock (this.m_syncLock)
        return this.InnerTimers.ContainsKey(name) ? (ITimer) this.InnerTimers[name] : (ITimer) null;
    }

    public void UpdateTimers()
    {
      lock (this.m_syncLock)
      {
        List<ITimer> timerList = new List<ITimer>();
        foreach (SoftTimer softTimer in (IEnumerable<SoftTimer>) this.InnerTimers.Values)
          timerList.Add((ITimer) softTimer);
        for (int index = 0; index < timerList.Count && index < timerList.Count; ++index)
          ((SoftTimer) timerList[index]).UpdateTimer();
      }
    }

    public void Wait(int milliseconds)
    {
      ManualResetEvent manualResetEvent = new ManualResetEvent(false);
      manualResetEvent.WaitOne(milliseconds);
      manualResetEvent.Close();
    }

    public ReadOnlyCollection<ITimer> Timers
    {
      get
      {
        List<ITimer> timerList = new List<ITimer>();
        foreach (KeyValuePair<string, SoftTimer> innerTimer in (IEnumerable<KeyValuePair<string, SoftTimer>>) this.InnerTimers)
          timerList.Add((ITimer) innerTimer.Value);
        return timerList.AsReadOnly();
      }
    }

    internal IDictionary<string, SoftTimer> InnerTimers
    {
      get
      {
        if (this.m_innerTimers == null)
          this.m_innerTimers = (IDictionary<string, SoftTimer>) new Dictionary<string, SoftTimer>();
        return this.m_innerTimers;
      }
    }

    private TimerService()
    {
    }
  }
}
