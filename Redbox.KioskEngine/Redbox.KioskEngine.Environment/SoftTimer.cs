using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Threading;

namespace Redbox.KioskEngine.Environment
{
  internal class SoftTimer : ITimer
  {
    public void Stop()
    {
      this.Enabled = false;
      this.LastTime = new DateTime?();
    }

    public void Start()
    {
      this.LastTime = new DateTime?();
      this.Enabled = true;
    }

    public void ClearFire() => this.Fire = (TimerCallback) null;

    public void RaiseFire()
    {
      if (this.Fire == null)
        return;
      try
      {
        this.Fire(this.Tag);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log(string.Format("An unhandled exception was raised in SoftTimer {0}", (object) this.Name), ex);
      }
    }

    public object Tag { get; set; }

    public int? Period { get; set; }

    public string Name { get; set; }

    public int? DueTime { get; set; }

    public bool Enabled { get; set; }

    public event TimerCallback Fire;

    internal bool UpdateTimer()
    {
      if (!this.Enabled)
        return false;
      if (!this.LastTime.HasValue)
      {
        this.LastTime = new DateTime?(DateTime.UtcNow);
        return false;
      }
      DateTime utcNow = DateTime.UtcNow;
      TimeSpan timeSpan = utcNow - this.LastTime.Value;
      if (this.DueTime.HasValue)
      {
        double totalMilliseconds = timeSpan.TotalMilliseconds;
        int? dueTime = this.DueTime;
        double? nullable = dueTime.HasValue ? new double?((double) dueTime.GetValueOrDefault()) : new double?();
        double valueOrDefault = nullable.GetValueOrDefault();
        if (!(totalMilliseconds >= valueOrDefault & nullable.HasValue))
          return false;
        this.DueTime = new int?();
        this.LastTime = new DateTime?(utcNow);
        this.RaiseFire();
        return true;
      }
      if (!this.Period.HasValue)
        return false;
      double totalMilliseconds1 = timeSpan.TotalMilliseconds;
      int? period = this.Period;
      double? nullable1 = period.HasValue ? new double?((double) period.GetValueOrDefault()) : new double?();
      double valueOrDefault1 = nullable1.GetValueOrDefault();
      if (!(totalMilliseconds1 >= valueOrDefault1 & nullable1.HasValue))
        return false;
      this.LastTime = new DateTime?(utcNow);
      this.RaiseFire();
      return true;
    }

    internal DateTime? LastTime { get; set; }
  }
}
