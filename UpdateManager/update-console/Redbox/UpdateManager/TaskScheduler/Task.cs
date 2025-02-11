using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Quartz;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.TaskScheduler
{
    class Task : ITask, IJob
    {
        public Task()
        {
        }

        public Task(string name, string payload, PayloadType payloadType, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, string cronExpression)
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

        public Task(string name, string payload, PayloadType payloadType, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, TimeSpan repeatInterval)
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
            return string.Format("Name:{0} Type:{1} PayloadType:{2} @ {3}", new object[]
            {
                this.Name,
                this.ScheduleType,
                this.PayloadType,
                (this.ScheduleType == ScheduleType.Cron) ? this.CronExpression : this.RepeatInterval.ToString()
            });
        }

        static void ExecuteEvent(ITask task)
        {
            if (!Task._lock.TryEnterWriteLock(Task._lockTimeout))
            {
                new ErrorList().Add(Redbox.UpdateManager.ComponentModel.Error.NewError("Task.ExecuteEvent", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
                return;
            }
            try
            {
                IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
                using (ExecutionTimer executionTimer = new ExecutionTimer())
                {
                    LogHelper.Instance.Log(string.Format("SCHEDULER: *** Begin Schedule Event: {0}", task.Payload), LogEntryType.Info);
                    try
                    {
                        switch (task.PayloadType)
                        {
                            case PayloadType.Script:
                                {
                                    string text = task.Payload;
                                    if (!Path.IsPathRooted(text))
                                    {
                                        text = Path.Combine(service.ExpandProperties("${Scripts}"), task.Payload);
                                    }
                                    if (!File.Exists(text))
                                    {
                                        LogHelper.Instance.Log("{0} not found.", new object[] { text });
                                        return;
                                    }
                                    try
                                    {
                                        if (string.Compare(task.Payload, "watchdog.lua", StringComparison.CurrentCultureIgnoreCase) == 0)
                                        {
                                            ServiceLocator.Instance.GetService<IHealthService>().Update("UPDATE_SERVICE_WATCHDOG");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogHelper.Instance.Log("(TASK0099) - An unhandled exception occurred updating health for watchdog.", ex);
                                    }
                                    IKernelService service2 = ServiceLocator.Instance.GetService<IKernelService>();
                                    string text2 = File.ReadAllText(text);
                                    LogHelper.Instance.Log("Executing lua script at {0}", new object[] { text });
                                    service2.ExecuteChunk(text2);
                                    goto IL_02C8;
                                }
                            case PayloadType.Shell:
                                {
                                    string text3;
                                    string text4;
                                    if (task.Payload.Contains(" "))
                                    {
                                        text3 = task.Payload.Substring(0, task.Payload.IndexOf(" "));
                                        text4 = task.Payload.Substring(task.Payload.IndexOf(" ") + 1);
                                    }
                                    else
                                    {
                                        text3 = task.Payload;
                                        text4 = string.Empty;
                                    }
                                    LogHelper.Instance.Log("Executing shell command process: {0} arguments: {1}", new object[] { text3, text4 });
                                    using (Process process = Process.Start(new ProcessStartInfo
                                    {
                                        WorkingDirectory = service.ExpandProperties("${RunningPath}"),
                                        ErrorDialog = false,
                                        UseShellExecute = false,
                                        Arguments = text4,
                                        CreateNoWindow = true,
                                        WindowStyle = ProcessWindowStyle.Hidden,
                                        FileName = text3
                                    }))
                                    {
                                        LogHelper.Instance.Log("Process '{0}' started with PID '{1}'.", new object[] { process.ProcessName, process.Id });
                                        goto IL_02C8;
                                    }
                                    break;
                                }
                            case PayloadType.ServerPoll:
                                break;
                            default:
                                goto IL_02C8;
                        }
                        LogHelper.Instance.Log("Running scheduled ServerPoll", Array.Empty<object>());
                        foreach (Redbox.UpdateManager.ComponentModel.Error error in ServiceLocator.Instance.GetService<IUpdateService>().ServerPoll())
                        {
                            LogHelper.Instance.Log(error.ToString(), LogEntryType.Error);
                        }
                    IL_02C8:
                        ServiceLocator.Instance.GetService<IKernelService>().PerformShutdown();
                    }
                    catch (Exception ex2)
                    {
                        LogHelper.Instance.Log("An unhandled exception was raised in Task ExecuteEvent.", ex2);
                    }
                    finally
                    {
                        LogHelper.Instance.Log(string.Format("SCHEDULER: *** End Schedule Event: {0}, Execution Time = {1}", task.Payload, executionTimer.Elapsed), LogEntryType.Info);
                    }
                }
            }
            finally
            {
                Task._lock.ExitWriteLock();
            }
        }

        internal int InExecution;

        static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        static int _lockTimeout = 3000;
    }
}
