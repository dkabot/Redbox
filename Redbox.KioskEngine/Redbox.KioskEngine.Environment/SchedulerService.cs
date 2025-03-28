using Quartz;
using Quartz.Impl;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Redbox.KioskEngine.Environment
{
  public class SchedulerService : ISchedulerService
  {
    private readonly ISchedulerFactory m_schedulerFactory;
    private List<SchedulerService.SchedulerEntry> m_entries;

    public static SchedulerService Instance => Singleton<SchedulerService>.Instance;

    public void Clear()
    {
      this.Entries = new List<SchedulerService.SchedulerEntry>();
      this.SaveEntries();
    }

    public void Reset()
    {
    }

    public void Shutdown() => this.m_schedulerFactory.GetScheduler().Shutdown();

    public ErrorList Initialize()
    {
      ErrorList errorList = new ErrorList();
      ServiceLocator.Instance.AddService(typeof (ISchedulerService), (object) this);
      List<SchedulerService.SchedulerEntry> schedulerEntryList = new List<SchedulerService.SchedulerEntry>();
      foreach (SchedulerService.SchedulerEntry entry in this.Entries)
      {
        if (!this.ScheduleEntry((ISchedulerEntry) entry))
          schedulerEntryList.Add(entry);
      }
      schedulerEntryList.ForEach((Action<SchedulerService.SchedulerEntry>) (s =>
      {
        this.m_entries.Remove(s);
        LogHelper.Instance.Log("ScheduleEntry failed, Deleting: jobname: {0} label: {1} ", (object) s.JobName, (object) s.Label);
      }));
      this.SaveEntries();
      return errorList;
    }

    public void Start() => this.m_schedulerFactory.GetScheduler().Start();

    public void AddScheduleEntry(
      string jobname,
      string label,
      DateTime startTime,
      DateTime? endTime,
      string cronExpression,
      string functionName,
      string programName,
      string misfireInstruction,
      object[] parms)
    {
      List<SchedulerService.SchedulerEntry> entries = this.Entries;
      if (entries.FindIndex((Predicate<SchedulerService.SchedulerEntry>) (each => string.Compare(each.JobName, jobname, true) == 0)) != -1)
        return;
      SchedulerService.SchedulerEntry entry = new SchedulerService.SchedulerEntry()
      {
        JobName = jobname,
        Label = label,
        StartTime = startTime,
        EndTime = endTime,
        CronExpression = cronExpression,
        FunctionName = functionName,
        Program = programName,
        MisfireInstruction = misfireInstruction
      };
      entry.Parameters.AddRange((IEnumerable<object>) parms);
      entries.Add(entry);
      if (this.ScheduleEntry((ISchedulerEntry) entry))
      {
        this.Entries = entries;
        this.SaveEntries();
      }
      else
        LogHelper.Instance.Log("ScheduleEntry failed, jobname: {0} label: {1} ", (object) jobname, (object) label);
    }

    public bool RemoveScheduleEntry(string jobname)
    {
      SchedulerService.SchedulerEntry schedulerEntry = this.Entries.Find((Predicate<SchedulerService.SchedulerEntry>) (each => string.Compare(each.JobName, jobname, true) == 0));
      if (schedulerEntry == null)
        return false;
      this.Entries.Remove(schedulerEntry);
      this.m_schedulerFactory.GetScheduler().UnscheduleJob(string.Format("{0}-trigger", (object) jobname), "cron");
      this.SaveEntries();
      return true;
    }

    public ISchedulerEntry GetScheduleEntry(string jobname)
    {
      return (ISchedulerEntry) this.Entries.Find((Predicate<SchedulerService.SchedulerEntry>) (each => string.Compare(each.JobName, jobname, true) == 0));
    }

    public List<ISchedulerEntry> GetScheduleEntries()
    {
      List<ISchedulerEntry> scheduleEntries = new List<ISchedulerEntry>();
      foreach (SchedulerService.SchedulerEntry entry in this.Entries)
        scheduleEntries.Add((ISchedulerEntry) entry);
      return scheduleEntries;
    }

    internal List<SchedulerService.SchedulerEntry> Entries
    {
      get
      {
        if (this.m_entries != null)
          return this.m_entries;
        IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
        this.m_entries = service == null ? new List<SchedulerService.SchedulerEntry>() : service.GetValue<string>("Core", "SchedulerEntries", "[]").ToObject<List<SchedulerService.SchedulerEntry>>();
        return this.m_entries ?? new List<SchedulerService.SchedulerEntry>();
      }
      set => this.m_entries = value;
    }

    internal bool SaveEntries()
    {
      IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
      if (service == null)
        return false;
      service.SetValue<string>("Core", "SchedulerEntries", this.m_entries.ToJson());
      return true;
    }

    private SchedulerService()
    {
      this.m_schedulerFactory = (ISchedulerFactory) new StdSchedulerFactory();
    }

    private bool ScheduleEntry(ISchedulerEntry entry)
    {
      try
      {
        JobDetail jobDetail = new JobDetail(entry.JobName, "kiosk-engine", typeof (SchedulerService.SchedulerJob));
        jobDetail.JobDataMap[(object) "function_name"] = (object) entry.FunctionName;
        jobDetail.JobDataMap[(object) "parameters"] = (object) entry.Parameters;
        CronTrigger cronTrigger = new CronTrigger(string.Format("{0}-trigger", (object) entry.JobName), "cron", entry.JobName, "kiosk-engine", entry.StartTime, entry.EndTime, entry.CronExpression);
        if (entry.MisfireInstruction == "DoNothing")
          cronTrigger.MisfireInstruction = 2;
        else
          cronTrigger.MisfireInstruction = 1;
        this.m_schedulerFactory.GetScheduler().ScheduleJob(jobDetail, (Trigger) cronTrigger);
      }
      catch (SchedulerException ex)
      {
        LogHelper.Instance.Log("Quartz exception, ScheduleEntry failed.", (Exception) ex);
        return false;
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("Unhandled exception, ScheduleEntry failed.", ex);
        return false;
      }
      return true;
    }

    internal sealed class SchedulerEntry : ISchedulerEntry
    {
      public SchedulerEntry() => this.Parameters = new List<object>();

      public string JobName { get; set; }

      public string Label { get; set; }

      public DateTime StartTime { get; set; }

      public DateTime? EndTime { get; set; }

      public string CronExpression { get; set; }

      public string FunctionName { get; set; }

      public string Program { get; set; }

      public string MisfireInstruction { get; set; }

      public List<object> Parameters { get; set; }

      public override bool Equals(object obj)
      {
        return obj != null && obj is SchedulerService.SchedulerEntry schedulerEntry && this.JobName == schedulerEntry.JobName && this.CronExpression == schedulerEntry.CronExpression;
      }

      public override int GetHashCode()
      {
        return this.JobName.GetHashCode() ^ this.Program.GetHashCode() ^ this.CronExpression.GetHashCode();
      }
    }

    internal sealed class SchedulerJob : IJob
    {
      public void Execute(JobExecutionContext context)
      {
        try
        {
          string functionName = context.JobDetail.JobDataMap[(object) "function_name"] as string;
          if (string.IsNullOrEmpty(functionName) || !(context.JobDetail.JobDataMap[(object) "parameters"] is List<object> jobData))
            return;
          object[] parameters = new object[jobData.Count];
          int index = 0;
          foreach (object obj in jobData)
          {
            parameters[index] = (object) obj.ToString();
            ++index;
          }
          IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
          if (kernelService == null)
            return;
          LogHelper.Instance.Log("Scheduler Service: Execute job '{0}', trigger '{1}', execute function: {2}", (object) context.JobDetail.Name, (object) context.Trigger.Name, (object) functionName);
          ServiceLocator.Instance.GetService<IEngineApplication>()?.ThreadSafeHostUpdate((MethodInvoker) (() => kernelService.ExecuteFunction(functionName, parameters)));
        }
        catch (Exception ex)
        {
          LogHelper.Instance.Log("An unhandled exception was raised from SchedulerService+SchedulerJob.Execute.", ex);
        }
      }
    }
  }
}
