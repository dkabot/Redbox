using System;

namespace Redbox.KioskEngine.ComponentModel
{
  public interface IIdleTimer
  {
    void Initialize(int timeout, Action action);

    void Start();

    void Stop();

    void Reset();

    void Suspend();

    void Resume();

    int? Timeout { get; set; }
  }
}
