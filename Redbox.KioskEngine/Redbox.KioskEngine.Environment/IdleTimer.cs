using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Threading;

namespace Redbox.KioskEngine.Environment
{
  public class IdleTimer : IIdleTimer
  {
    private string _name;
    private bool _isEnabled;
    private bool _isSuspended;
    private bool _isRegistered;
    private bool _isTimerStarted;

    public IdleTimer(string name) => this._name = name;

    public string Name => this._name;

    public void Initialize(int timeout, Action action)
    {
      ServiceLocator.Instance.GetService<ITimerService>()?.CreateTimer(this.Name, new int?(timeout), new int?(timeout), (TimerCallback) (o => action()));
    }

    public int? Timeout
    {
      get => ServiceLocator.Instance.GetService<ITimerService>()?.GetTimer(this.Name)?.Period;
      set
      {
        ITimer timer = ServiceLocator.Instance.GetService<ITimerService>().GetTimer(this.Name);
        if (timer == null)
          return;
        timer.Period = value;
        timer.DueTime = value;
      }
    }

    public void Start()
    {
      if (this._isEnabled)
        return;
      LogHelper.Instance.Log("idle timer {0} started.", (object) this.Name);
      this._isEnabled = true;
      IInputService service = ServiceLocator.Instance.GetService<IInputService>();
      service?.RegisterMouseClickHandler(this.Name, (MouseClickHandler) ((button, x, y) => this.Reset()));
      service?.RegisterKeyPressHandler(this.Name, (KeyPressHandler) ((key, keyCode, modifier) => this.Reset()));
      this.Reset();
    }

    public void Stop()
    {
      if (!this._isEnabled)
        return;
      LogHelper.Instance.Log("idle timer {0} stopped.", (object) this.Name);
      this.StopIdleTimer();
      IInputService service = ServiceLocator.Instance.GetService<IInputService>();
      service?.RemoveKeyPressHandler(this.Name);
      service?.RemoveMouseClickHandler(this.Name);
      service?.RemoveIdleHandler(this.Name);
      this._isRegistered = false;
      this._isEnabled = false;
    }

    public void Suspend()
    {
      if (!this._isEnabled)
        return;
      this._isSuspended = true;
      this.Stop();
    }

    public void Resume()
    {
      if (!this._isSuspended)
        return;
      this._isSuspended = false;
      this.Start();
    }

    public void Reset()
    {
      this.StopIdleTimer();
      if (this._isRegistered)
        return;
      this._isRegistered = true;
      IInputService inputService = ServiceLocator.Instance.GetService<IInputService>();
      inputService.RegisterIdleHandler(this.Name, (IdleEventHandler) (() =>
      {
        this._isRegistered = false;
        inputService.RemoveIdleHandler(this.Name);
        this.StartIdleTimer();
        LogHelper.Instance.Log("system is idle, timer: {0}", (object) this.Name);
      }));
    }

    private void StartIdleTimer()
    {
      if (this._isTimerStarted)
        return;
      this._isTimerStarted = true;
      ServiceLocator.Instance.GetService<ITimerService>()?.GetTimer(this.Name)?.Start();
    }

    private void StopIdleTimer()
    {
      if (!this._isTimerStarted)
        return;
      this._isTimerStarted = false;
      ServiceLocator.Instance.GetService<ITimerService>()?.GetTimer(this.Name)?.Stop();
    }
  }
}
