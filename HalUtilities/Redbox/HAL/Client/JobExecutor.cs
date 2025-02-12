using Redbox.HAL.Component.Model;
using System;
using System.Collections.Generic;

namespace Redbox.HAL.Client
{
  public abstract class JobExecutor : IDisposable
  {
    protected readonly ClientHelper Helper;
    protected readonly HardwareService Service;
    protected internal readonly HardwareCommandResult ScheduleResult;
    private readonly HardwareJob m_job;
    private HardwareEvent UserEvent;
    private HardwareJobStatus m_endStatus = HardwareJobStatus.Errored;
    private bool m_disposed;

    public void Dispose()
    {
      if (this.m_disposed)
        return;
      this.m_disposed = true;
      this.DisposeInner();
      this.Helper.Dispose();
      this.Results.Clear();
      this.Results = (List<ProgramResult>) null;
      this.Errors.Clear();
      this.Errors = (ErrorList) null;
    }

    public void AddSink(HardwareEvent callback)
    {
      if (callback == null)
        throw new ArgumentNullException(nameof (callback));
      this.m_job.EventRaised += new HardwareEvent(this.OnHardwareEvent);
      this.UserEvent = callback;
    }

    public void Run()
    {
      if (this.m_job == null)
      {
        this.Errors.Add(Error.NewError("E785", "Job schedule failure.", "Failed to schedule job."));
      }
      else
      {
        this.ID = this.m_job.ID;
        this.SetupJob();
        if (!this.Helper.WaitForJob(this.m_job, out this.m_endStatus))
        {
          this.Errors.Add(Error.NewError("E786", "Job wait failure.", "Failed to wait for job."));
        }
        else
        {
          ProgramResult[] results;
          if (!this.m_job.GetResults(out results).Success)
          {
            this.Errors.Add(Error.NewError("E787", "GetResults failure.", "Failed to get program results. I'm sorry!"));
          }
          else
          {
            this.Results.AddRange((IEnumerable<ProgramResult>) results);
            results = (ProgramResult[]) null;
          }
          ErrorList errors;
          if (this.m_job.GetErrors(out errors).Success)
          {
            this.Errors.AddRange((IEnumerable<Error>) errors);
            errors.Clear();
          }
          IDictionary<string, string> symbols;
          if (this.m_job.GetSymbols(out symbols).Success)
          {
            foreach (string key in (IEnumerable<string>) symbols.Keys)
              this.Symbols[key] = symbols[key];
          }
          this.OnJobCompleted();
          this.m_job.Trash();
        }
      }
    }

    public IDictionary<string, string> Symbols { get; private set; }

    public List<ProgramResult> Results { get; private set; }

    public ErrorList Errors { get; private set; }

    public HardwareJobStatus EndStatus => this.m_endStatus;

    public string ID { get; private set; }

    public HardwareJob Job => this.m_job;

    protected virtual void SetupJob()
    {
    }

    protected virtual void DisposeInner()
    {
    }

    protected virtual void OnJobCompleted()
    {
    }

    protected JobExecutor(HardwareService service)
      : this(service, HardwareJobPriority.Highest)
    {
    }

    protected JobExecutor(HardwareService service, HardwareJobPriority priority)
    {
      this.Service = service;
      this.Helper = new ClientHelper(service);
      this.Results = new List<ProgramResult>();
      this.Errors = new ErrorList();
      this.Symbols = (IDictionary<string, string>) new Dictionary<string, string>();
      this.ScheduleResult = this.Service.ScheduleJob(this.JobName, this.Label, false, new HardwareJobSchedule()
      {
        Priority = priority
      }, out this.m_job);
      if (this.ScheduleResult.Success)
        return;
      this.m_job = (HardwareJob) null;
    }

    protected abstract string JobName { get; }

    protected virtual string Label => string.Empty;

    private void OnHardwareEvent(HardwareJob job, DateTime eventTime, string eventMessage)
    {
      if (eventMessage.StartsWith(">STATUS") || eventMessage.StartsWith(">IPC"))
        return;
      this.UserEvent(job, eventTime, eventMessage);
    }
  }
}
