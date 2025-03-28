using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Windows.Forms;

namespace Redbox.KioskEngine.Environment
{
  public class CallbackService : ICallbackService
  {
    private bool m_isRunning;
    private readonly object m_syncObject = new object();
    private readonly Queue<CallbackService.CallbackEntryWrapper> m_entries = new Queue<CallbackService.CallbackEntryWrapper>();
    private System.Timers.Timer _waitTimer;
    private Queue<CallbackService.CallbackEntryWrapper> _callbackTracking = new Queue<CallbackService.CallbackEntryWrapper>();
    private int _numberOfCallbacks;
    private TimeSpan _maxCallbackDuration;
    private TimeSpan _totalCallbackDuration;
    private string _maxCallbackDurationMessage;
    private DateTime _lastLoggedDateTime = DateTime.Now;
    private TimeSpan _loggingTargetInterval = TimeSpan.FromSeconds(20.0);

    public static CallbackService Instance => Singleton<CallbackService>.Instance;

    public int QueueCount
    {
      get
      {
        lock (this.m_syncObject)
          return this.m_entries.Count;
      }
    }

    public bool UseTimerToInvokeCallbacks
    {
      get
      {
        IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
        return service == null || service.UseTimerToInvokeCallbackEntries;
      }
    }

    public void Reset()
    {
      lock (this.m_syncObject)
      {
        LogHelper.Instance.Log(string.Format("Clear callback service queue.  Current queue count: {0}", (object) this.m_entries.Count));
        this.m_entries.Clear();
        this.ResetStatistics();
      }
    }

    public void Flush()
    {
      lock (this.m_syncObject)
      {
        LogHelper.Instance.Log(string.Format("Flush callback service queue. Current queue count: {0}", (object) this.m_entries.Count));
        do
          ;
        while (this.InvokeNextCallback());
      }
    }

    public void Resume()
    {
      lock (this.m_syncObject)
      {
        LogHelper.Instance.Log(string.Format("Resume callback service queue.  Current queue count: {0}", (object) this.m_entries.Count));
        this.m_isRunning = true;
        this.StartTimer();
      }
    }

    public void Suspend()
    {
      lock (this.m_syncObject)
      {
        LogHelper.Instance.Log(string.Format("Suspend callback service queue.  Current queue count: {0}", (object) this.m_entries.Count));
        this.m_isRunning = false;
      }
    }

    public DateTime? GetEnqueuedDateTimeFromNextCallbackEntry()
    {
      DateTime? nextCallbackEntry = new DateTime?();
      lock (this.m_syncObject)
      {
        if (this.m_entries.Count > 0)
        {
          CallbackService.CallbackEntryWrapper callbackEntryWrapper = this.m_entries.Peek();
          if (callbackEntryWrapper != null)
            nextCallbackEntry = callbackEntryWrapper.EnqueueDateTime;
        }
      }
      return nextCallbackEntry;
    }

    public bool InvokeNextCallback()
    {
      try
      {
        lock (this.m_syncObject)
        {
          if (!this.m_isRunning || this.m_entries.Count == 0)
            return false;
          CallbackService.CallbackEntryWrapper callbackEntryWrapper = this.m_entries.Dequeue();
          callbackEntryWrapper.DequeueDateTime = new DateTime?(DateTime.Now);
          Stopwatch stopwatch = Stopwatch.StartNew();
          callbackEntryWrapper?.CallbackEntry.Invoke();
          stopwatch.Stop();
          callbackEntryWrapper.ExecutionTime = stopwatch.ElapsedMilliseconds;
          callbackEntryWrapper.Completed = true;
          if (callbackEntryWrapper.ExecutionTime < 2000L)
            callbackEntryWrapper.Stack = string.Empty;
          this.UpdateStatistics(stopwatch.Elapsed, callbackEntryWrapper?.CallbackEntry?.Name);
          if (callbackEntryWrapper?.CallbackEntry is IDisposable callbackEntry)
            callbackEntry.Dispose();
          callbackEntryWrapper.CallbackEntry = (ICallbackEntry) null;
          this.TrimCallback();
        }
        if (this.GetTimeSinceLastLogged() >= this.LoggingTargetInterval)
        {
          this.LogStatistics();
          this.ResetStatistics();
        }
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was rasied by CallbackService.InvokeNextCallback.", ex);
      }
      return true;
    }

    private void TrimCallback()
    {
      while (this._callbackTracking.Count > 300)
        this._callbackTracking.Dequeue();
    }

    private void UpdateStatistics(TimeSpan callbackDuration, string callbackMessage)
    {
      ++this._numberOfCallbacks;
      this._totalCallbackDuration = this._totalCallbackDuration.Add(callbackDuration);
      if (!(callbackDuration > this._maxCallbackDuration))
        return;
      this._maxCallbackDuration = callbackDuration;
      this._maxCallbackDurationMessage = callbackMessage;
    }

    public ICallbackServiceStatistics GetStatics()
    {
      return (ICallbackServiceStatistics) new CallbackService.CallbackServiceStatistics()
      {
        NumberOfCallbacksExecuted = this._numberOfCallbacks,
        TotalCallbackDuration = this._totalCallbackDuration,
        MaxCallbackDuration = this._maxCallbackDuration,
        MaxCallbackDurationMessage = this._maxCallbackDurationMessage,
        AverageCallbackDuration = this.GetAverageCallbackDuration(),
        TimeSinceLastLogged = this.GetTimeSinceLastLogged()
      };
    }

    public TimeSpan LoggingTargetInterval => this._loggingTargetInterval;

    public void EnqueueCallback(ICallbackEntry callbackEntry)
    {
      lock (this.m_syncObject)
      {
        if (!this.m_isRunning)
        {
          LogHelper.Instance.Log("Enqueing of callback entry rejected because the Callback queue is suspended.");
        }
        else
        {
          CallbackService.CallbackEntryWrapper callbackEntryWrapper = new CallbackService.CallbackEntryWrapper()
          {
            CallbackEntry = callbackEntry,
            EnqueueDateTime = new DateTime?(DateTime.Now),
            CallBackName = callbackEntry.Name,
            Stack = System.Environment.StackTrace
          };
          this.m_entries.Enqueue(callbackEntryWrapper);
          this._callbackTracking.Enqueue(callbackEntryWrapper);
          this.StartTimer();
        }
      }
    }

    private CallbackService()
    {
    }

    public void LogCallbackTracking()
    {
      this._callbackTracking.ForEach<CallbackService.CallbackEntryWrapper>((Action<CallbackService.CallbackEntryWrapper>) (x => LogHelper.Instance.Log(x.ToString())));
    }

    public void ResetStatistics()
    {
      this._numberOfCallbacks = 0;
      this._maxCallbackDuration = TimeSpan.FromMilliseconds(0.0);
      this._totalCallbackDuration = TimeSpan.FromMilliseconds(0.0);
      this._lastLoggedDateTime = DateTime.Now;
      this._maxCallbackDurationMessage = (string) null;
    }

    public void LogStatistics()
    {
      int queueCount = this.QueueCount;
      ICallbackServiceStatistics statics = this.GetStatics();
      LogHelper.Instance.Log(string.Format("CallbackService statistics: current queue count: {0}; callbacks invoked since last check: {1}; Time since Last callback statistics logged: {2}", (object) queueCount, (object) statics.NumberOfCallbacksExecuted, (object) statics.TimeSinceLastLogged));
      LogHelper.Instance.Log(string.Format("CallbackService statistics: average execution time: {0}; total execution time: {1}; max execution time: {2}; max execution time callback name: {3} ", (object) statics.AverageCallbackDuration, (object) statics.TotalCallbackDuration, (object) statics.MaxCallbackDuration, (object) statics.MaxCallbackDurationMessage));
    }

    private TimeSpan GetAverageCallbackDuration()
    {
      TimeSpan callbackDuration = TimeSpan.FromMilliseconds(0.0);
      if (this._numberOfCallbacks > 0)
        callbackDuration = TimeSpan.FromMilliseconds(this._totalCallbackDuration.TotalMilliseconds / (double) this._numberOfCallbacks);
      return callbackDuration;
    }

    private TimeSpan GetTimeSinceLastLogged() => DateTime.Now - this._lastLoggedDateTime;

    private void StartTimer()
    {
      if (!this.UseTimerToInvokeCallbacks)
        return;
      if (this._waitTimer == null)
      {
        this._waitTimer = new System.Timers.Timer(5.0);
        this._waitTimer.Elapsed += new ElapsedEventHandler(this._waitTimer_Elapsed);
        this._waitTimer.AutoReset = false;
      }
      if (this._waitTimer.Enabled)
        return;
      this._waitTimer.Start();
    }

    private void _waitTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      ServiceLocator.Instance.GetService<IEngineApplication>()?.ThreadSafeHostUpdate((MethodInvoker) (() => this.InvokeNextCallback()));
      lock (this.m_syncObject)
      {
        if (!this.m_isRunning || this.m_entries.Count <= 0)
          return;
        this.StartTimer();
      }
    }

    public class CallbackServiceStatistics : ICallbackServiceStatistics
    {
      public int NumberOfCallbacksExecuted { get; set; }

      public TimeSpan TotalCallbackDuration { get; set; }

      public TimeSpan MaxCallbackDuration { get; set; }

      public TimeSpan AverageCallbackDuration { get; set; }

      public TimeSpan TimeSinceLastLogged { get; set; }

      public string MaxCallbackDurationMessage { get; set; }
    }

    private class CallbackEntryWrapper
    {
      public ICallbackEntry CallbackEntry { get; set; }

      public DateTime? EnqueueDateTime { get; set; }

      public string CallBackName { get; set; }

      public string AddedFromStack { get; set; }

      public DateTime? DequeueDateTime { get; set; }

      public long ExecutionTime { get; set; }

      public bool Completed { get; set; }

      public string Stack { get; set; }

      public override string ToString()
      {
        string callBackName = this.CallBackName;
        DateTime? nullable = this.EnqueueDateTime;
        ref DateTime? local1 = ref nullable;
        string str1 = local1.HasValue ? local1.GetValueOrDefault().ToString("o") : (string) null;
        nullable = this.DequeueDateTime;
        ref DateTime? local2 = ref nullable;
        string str2 = local2.HasValue ? local2.GetValueOrDefault().ToString("o") : (string) null;
        long executionTime = this.ExecutionTime;
        int num = this.Completed ? 1 : 0;
        string stack = this.Stack;
        return new
        {
          Name = callBackName,
          EnqueueDT = str1,
          DequeueDT = str2,
          ExecutionTime = executionTime,
          Completed = (num != 0),
          Stack = stack
        }.ToJson();
      }
    }
  }
}
