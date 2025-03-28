using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System.Collections.Generic;

namespace Redbox.KioskEngine.Environment
{
  public class IdleTimerService : IIdleTimerService
  {
    private Dictionary<string, IIdleTimer> _innerIdleTimers = new Dictionary<string, IIdleTimer>();
    private readonly object _syncLock = new object();

    public static IdleTimerService Instance => Singleton<IdleTimerService>.Instance;

    public void Reset()
    {
      LogHelper.Instance.Log("Reset idletimer service.");
      lock (this._syncLock)
        this._innerIdleTimers.Clear();
    }

    public IIdleTimer CreateIdleTimer(string name)
    {
      lock (this._syncLock)
      {
        IIdleTimer idleTimer = (IIdleTimer) null;
        this._innerIdleTimers.TryGetValue(name, out idleTimer);
        if (idleTimer == null)
        {
          idleTimer = (IIdleTimer) new IdleTimer(name);
          this._innerIdleTimers.Add(name, idleTimer);
        }
        return idleTimer;
      }
    }

    public void RemoveIdleTimer(string name)
    {
      lock (this._syncLock)
      {
        if (!this._innerIdleTimers.ContainsKey(name))
          return;
        this._innerIdleTimers[name]?.Stop();
        ServiceLocator.Instance.GetService<ITimerService>()?.RemoveTimer(name);
        this._innerIdleTimers.Remove(name);
      }
    }

    public IIdleTimer GetIdleTimer(string name)
    {
      lock (this._syncLock)
        return this._innerIdleTimers.ContainsKey(name) ? this._innerIdleTimers[name] : (IIdleTimer) null;
    }

    private IdleTimerService()
    {
    }
  }
}
