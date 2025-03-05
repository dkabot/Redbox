using System;

namespace Redbox.KioskEngine.ComponentModel
{
  public interface ICallbackService
  {
    void Reset();

    void Flush();

    void Resume();

    void Suspend();

    bool InvokeNextCallback();

    void EnqueueCallback(ICallbackEntry callbackEntry);

    int QueueCount { get; }

    DateTime? GetEnqueuedDateTimeFromNextCallbackEntry();

    ICallbackServiceStatistics GetStatics();

    void ResetStatistics();

    void LogStatistics();

    void LogCallbackTracking();
  }
}
