using System.Threading;

namespace Redbox.KioskEngine.ComponentModel
{
  public interface ITimer
  {
    void Stop();

    void Start();

    void ClearFire();

    void RaiseFire();

    object Tag { get; set; }

    int? Period { get; set; }

    string Name { get; set; }

    int? DueTime { get; set; }

    bool Enabled { get; set; }

    event TimerCallback Fire;
  }
}
