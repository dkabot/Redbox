using Quartz;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Redbox.UpdateManager.TaskScheduler
{
    internal class Task : ITask, IJob
    {
        internal int InExecution;
        private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private static int _lockTimeout = 3000;

        public Task()
        {
        }

        public Task(
          string name,
          string payload,
          PayloadType payloadType,
          TimeSpan? startOffset,
          DateTime? startTime,
          DateTime? endTime,
          string cronExpression)
        {
            this.Name = name;
            this.Payload = payload;
            this.PayloadType = payloadType;
            this.StartOffset = startOffset;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.ScheduleType = ScheduleType.Cron;
            this.CronExpression = cronExpression;
        }

        public Task(
          string name,
          string payload,
          PayloadType payloadType,
          TimeSpan? startOffset,
          DateTime? startTime,
          DateTime? endTime,
          TimeSpan repeatInterval)
        {
            this.Name = name;
            this.Payload = payload;
            this.PayloadType = payloadType;
            this.StartOffset = startOffset;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.ScheduleType = ScheduleType.Simple;
            this.RepeatInterval = new TimeSpan?(repeatInterval);
        }

        public ScheduleType ScheduleType { get; set; }

        public string CronExpression { get; set; }

        public string Name { get; set; }

        public TimeSpan? StartOffset { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public TimeSpan? RepeatInterval { get; set; }

        public string Payload { get; set; }

        public PayloadType PayloadType { get; set; }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                Task task;
                if (context == null)
                {
                    task = this;
                }
                else
                {
                    task = context.JobDetail.JobDataMap["Task"] as Task;
                    if (task == null)
                    {
                        LogHelper.Instance.Log("WARNING: JobExecutionContext did not have a valid Task in the JobDataMap.", Array.Empty<object>());
                        return;
                    }
                }
                if (Interlocked.CompareExchange(ref task.InExecution, 1, 0) == 1)
                {
                    LogHelper.Instance.Log(string.Format("Task: {0} already executing.", task.Payload), Array.Empty<object>());
                }
                else
                {
                    try
                    {
                        Task.ExecuteEvent(task);
                    }
                    finally
                    {
                        task.InExecution = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was thrown from Task.Execute.", ex);
            }
        }

        public new string ToString()
        {
            return string.Format("Name:{0} Type:{1} PayloadType:{2} @ {3}", (object)this.Name, (object)this.ScheduleType, (object)this.PayloadType, this.ScheduleType == ScheduleType.Cron ? (object)this.CronExpression : (object)this.RepeatInterval.ToString());
        }

        private static void ExecuteEvent(ITask task)
        {
            if (!Task._lock.TryEnterWriteLock(Task._lockTimeout))
            {
                new ErrorList().Add(Redbox.UpdateManager.ComponentModel.Error.NewError("Task.ExecuteEvent", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
            }
            else
            {
                try
                {
                    IMacroService service1 = ServiceLocator.Instance.GetService<IMacroService>();
                    using (ExecutionTimer executionTimer = new ExecutionTimer())
                    {
                        LogHelper.Instance.Log(string.Format("SCHEDULER: *** Begin Schedule Event: {0}", (object)task.Payload), LogEntryType.Info);
                        try
                        {
                            switch (task.PayloadType)
                            {
                                case PayloadType.Script:
                                    string path = task.Payload;
                                    if (!Path.IsPathRooted(path))
                                        path = Path.Combine(service1.ExpandProperties("${Scripts}"), task.Payload);
                                    if (!File.Exists(path))
                                    {
                                        LogHelper.Instance.Log("{0} not found.", (object)path);
                                        return;
                                    }
                                    try
                                    {
                                        if (string.Compare(task.Payload, "watchdog.lua", StringComparison.CurrentCultureIgnoreCase) == 0)
                                            ServiceLocator.Instance.GetService<IHealthService>().Update("UPDATE_SERVICE_WATCHDOG");
                                    }
                                    catch (Exception ex)
                                    {
                                        LogHelper.Instance.Log("(TASK0099) - An unhandled exception occurred updating health for watchdog.", ex);
                                    }
                                    IKernelService service2 = ServiceLocator.Instance.GetService<IKernelService>();
                                    string chunk = File.ReadAllText(path);
                                    LogHelper.Instance.Log("Executing lua script at {0}", (object)path);
                                    service2.ExecuteChunk(chunk);
                                    break;
                                case PayloadType.Shell:
                                    string str1;
                                    string str2;
                                    if (task.Payload.Contains(" "))
                                    {
                                        str1 = task.Payload.Substring(0, task.Payload.IndexOf(" "));
                                        str2 = task.Payload.Substring(task.Payload.IndexOf(" ") + 1);
                                    }
                                    else
                                    {
                                        str1 = task.Payload;
                                        str2 = string.Empty;
                                    }
                                    LogHelper.Instance.Log("Executing shell command process: {0} arguments: {1}", (object)str1, (object)str2);
                                    using (Process process = Process.Start(new ProcessStartInfo()
                                    {
                                        WorkingDirectory = service1.ExpandProperties("${RunningPath}"),
                                        ErrorDialog = false,
                                        UseShellExecute = false,
                                        Arguments = str2,
                                        CreateNoWindow = true,
                                        WindowStyle = ProcessWindowStyle.Hidden,
                                        FileName = str1
                                    }))
                                    {
                                        LogHelper.Instance.Log("Process '{0}' started with PID '{1}'.", (object)process.ProcessName, (object)process.Id);
                                        break;
                                    }
                                case PayloadType.ServerPoll:
                                    LogHelper.Instance.Log("Running scheduled ServerPoll");
                                    using (List<Redbox.UpdateManager.ComponentModel.Error>.Enumerator enumerator = ServiceLocator.Instance.GetService<IUpdateService>().ServerPoll().GetEnumerator())
                                    {
                                        while (enumerator.MoveNext())
                                            LogHelper.Instance.Log(enumerator.Current.ToString(), LogEntryType.Error);
                                        break;
                                    }
                            }
                            ServiceLocator.Instance.GetService<IKernelService>().PerformShutdown();
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Instance.Log("An unhandled exception was raised in Task ExecuteEvent.", ex);
                        }
                        finally
                        {
                            LogHelper.Instance.Log(string.Format("SCHEDULER: *** End Schedule Event: {0}, Execution Time = {1}", (object)task.Payload, (object)executionTimer.Elapsed), LogEntryType.Info);
                        }
                    }
                }
                finally
                {
                    Task._lock.ExitWriteLock();
                }
            }
        }
    }
}
