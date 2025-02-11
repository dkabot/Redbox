using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Redbox.UpdateManager.TaskScheduler
{
    internal class Scheduler : ITaskScheduler
    {
        private const string ScheduleLabel = "config-schedule";
        private const string JobGroupname = "updatemanager";
        private const string DefaultScheduleFileName = "default-schedule.lua";
        private static readonly object FileLock = new object();
        private readonly ISchedulerFactory m_schedulerFactory = (ISchedulerFactory)new StdSchedulerFactory();
        private System.Collections.Generic.List<Task> m_tasks = new System.Collections.Generic.List<Task>();

        public static Scheduler Instance => Singleton<Scheduler>.Instance;

        public ErrorList ForceTask(string name, out bool success)
        {
            ErrorList errorList = new ErrorList();
            success = false;
            try
            {
                Task task = this.m_tasks.Find((Predicate<Task>)(t => t.Name == name));
                if (task != null)
                {
                    task.Execute((IJobExecutionContext)null);
                    success = true;
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "Unhandled exception while forcing task: " + name, ex));
            }
            return errorList;
        }

        public ErrorList ScheduleCronJob(
          string name,
          string payload,
          string payloadTypeString,
          TimeSpan? startOffset,
          DateTime? startTime,
          DateTime? endTime,
          string cronExpression)
        {
            ErrorList errorList = new ErrorList();
            PayloadType parsed;
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)Scheduler.ParsePayload(payloadTypeString, out parsed));
            if (!errorList.ContainsError())
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.ScheduleCronJob(name, payload, parsed, startOffset, startTime, endTime, cronExpression));
            return errorList;
        }

        public ErrorList ScheduleCronJob(
          string name,
          string payload,
          PayloadType payloadType,
          TimeSpan? startOffset,
          DateTime? startTime,
          DateTime? endTime,
          string cronExpression)
        {
            ErrorList errors = new ErrorList();
            if (!CronExpression.IsValidExpression(cronExpression))
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E428", "The cron expression is not valid.", string.Format("The expression '{0}' is not valid, please specify a valid cron expression.", (object)cronExpression)));
            else
                errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)Scheduler.ReadWriteFile((Func<System.Collections.Generic.List<Task>, System.Collections.Generic.List<Task>>)(tasks =>
                {
                    Task task1 = new Task(name, payload, payloadType, startOffset, startTime, endTime, cronExpression);
                    if (!tasks.Any<Task>((Func<Task, bool>)(task => task.Name == name)))
                        tasks.Add(task1);
                    else
                        errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("S489", "Task with same name already scheduled", "Please use a unique name."));
                    return tasks;
                })));
            return errors;
        }

        public ErrorList ScheduleSimpleJob(
          string name,
          string payload,
          string payloadTypeString,
          TimeSpan? startOffset,
          DateTime? startTime,
          DateTime? endTime,
          TimeSpan repeatInterval)
        {
            ErrorList errorList = new ErrorList();
            PayloadType parsed;
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)Scheduler.ParsePayload(payloadTypeString, out parsed));
            if (!errorList.ContainsError())
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.ScheduleSimpleJob(name, payload, parsed, startOffset, startTime, endTime, repeatInterval));
            return errorList;
        }

        public ErrorList ScheduleSimpleJob(
          string name,
          string payload,
          PayloadType payloadType,
          TimeSpan? startOffset,
          DateTime? startTime,
          DateTime? endTime,
          TimeSpan repeatInterval)
        {
            ErrorList errors = new ErrorList();
            errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)Scheduler.ReadWriteFile((Func<System.Collections.Generic.List<Task>, System.Collections.Generic.List<Task>>)(tasks =>
            {
                Task task1 = new Task(name, payload, payloadType, startOffset, startTime, endTime, repeatInterval);
                if (!tasks.Any<Task>((Func<Task, bool>)(task => task.Name == name)))
                    tasks.Add(task1);
                else
                    errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("S489", "Task with same name already scheduled", "Please use a unique name."));
                return tasks;
            })));
            return errors;
        }

        public ErrorList Stop()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                LogHelper.Instance.Log("Stopping scheduled events.", LogEntryType.Info);
                IScheduler scheduler = this.m_schedulerFactory.GetScheduler();
                if (scheduler.IsStarted)
                {
                    Quartz.Collection.ISet<JobKey> jobKeys = scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals("updatemanager"));
                    scheduler.DeleteJobs((IList<JobKey>)jobKeys.ToList<JobKey>());
                    Quartz.Collection.ISet<TriggerKey> triggerKeys = scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals("updatemanager"));
                    scheduler.UnscheduleJobs((IList<TriggerKey>)triggerKeys.ToList<TriggerKey>());
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "Unhandled exception throw in stop.", ex));
            }
            finally
            {
                this.IsRunning = false;
            }
            return errorList;
        }

        public ErrorList Start()
        {
            ErrorList errorList1 = new ErrorList();
            try
            {
                if (this.IsRunning)
                {
                    ErrorList errorList2 = new ErrorList();
                    errorList2.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E987", "Scheduler is already running.", ""));
                    return errorList2;
                }
                this.IsRunning = true;
                IScheduler scheduler = this.m_schedulerFactory.GetScheduler();
                if (!scheduler.IsStarted)
                    scheduler.Start();
                LogHelper.Instance.Log("Starting scheduled events.", LogEntryType.Info);
                errorList1.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)Scheduler.ReadFile(out this.m_tasks));
                foreach (Task task1 in this.m_tasks)
                {
                    Task task = task1;
                    try
                    {
                        LogHelper.Instance.Log("Scheduling task: " + task.ToString());
                        IJobDetail jobDetail = JobBuilder.Create<Task>().WithIdentity(task.Name, "updatemanager").Build();
                        jobDetail.JobDataMap["Task"] = (object)task;
                        DateTime? startTime = task.StartTime;
                        DateTime dateTime;
                        if (!startTime.HasValue)
                        {
                            dateTime = DateTime.UtcNow;
                        }
                        else
                        {
                            startTime = task.StartTime;
                            dateTime = startTime.Value.ToUniversalTime();
                        }
                        DateTime startTimeUtc = dateTime;
                        TimeSpan? nullable = task.StartOffset;
                        if (nullable.HasValue)
                        {
                            ref DateTime local = ref startTimeUtc;
                            nullable = task.StartOffset;
                            TimeSpan timeSpan = nullable.Value;
                            startTimeUtc = local.Add(timeSpan);
                        }
                        if (task.ScheduleType == ScheduleType.Cron)
                        {
                            ITrigger trigger = TriggerBuilder.Create().WithIdentity(task.Name, "updatemanager").ForJob(jobDetail).WithCronSchedule(task.CronExpression, (Action<CronScheduleBuilder>)(x => x.WithMisfireHandlingInstructionDoNothing())).StartAt((DateTimeOffset)startTimeUtc).Build();
                            scheduler.ScheduleJob(jobDetail, trigger);
                        }
                        if (task.ScheduleType == ScheduleType.Simple)
                        {
                            nullable = task.RepeatInterval;
                            if (nullable.HasValue)
                            {
                                ITrigger trigger = TriggerBuilder.Create().WithIdentity(task.Name, "updatemanager").ForJob(jobDetail).WithSimpleSchedule((Action<SimpleScheduleBuilder>)(x => x.WithIntervalInSeconds(Convert.ToInt32(task.RepeatInterval.Value.TotalSeconds)).RepeatForever())).StartAt((DateTimeOffset)startTimeUtc).Build();
                                scheduler.ScheduleJob(jobDetail, trigger);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errorList1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SCH899", "Unhandled exception throw when scheduling a task in scheduler.start.", ex));
                    }
                }
            }
            catch (Exception ex)
            {
                errorList1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E929", "Unhandled exception throw in start.", ex));
            }
            return errorList1;
        }

        public ErrorList Restart()
        {
            ErrorList errorList = new ErrorList();
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.Stop());
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.Start());
            return errorList;
        }

        public ErrorList Delete(string name)
        {
            return Scheduler.ReadWriteFile((Func<System.Collections.Generic.List<Task>, System.Collections.Generic.List<Task>>)(tasks =>
            {
                tasks.RemoveAll((Predicate<Task>)(task => task.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)));
                return tasks;
            }));
        }

        public ErrorList List(out System.Collections.Generic.List<ITask> outTasks)
        {
            System.Collections.Generic.List<Task> tasks;
            ErrorList errorList = Scheduler.ReadFile(out tasks);
            outTasks = tasks.Cast<ITask>().ToList<ITask>();
            return errorList;
        }

        public bool IsRunning { get; private set; }

        public void Initialize()
        {
            ServiceLocator.Instance.AddService(typeof(ITaskScheduler), (object)this);
            lock (Scheduler.FileLock)
                Scheduler.GetRaw();
        }

        ErrorList ITaskScheduler.Clear() => Scheduler.Clear();

        public static ErrorList Clear()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
                lock (Scheduler.FileLock)
                {
                    service.Delete("config-schedule");
                    Scheduler.RunDefaultSchedule();
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E949", "Unhandled exception when deleting the schedule file.", ex));
            }
            return errorList;
        }

        private Scheduler()
        {
        }

        private static string GetRaw()
        {
            IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
            string raw = service.GetRaw("config-schedule");
            if (string.IsNullOrEmpty(raw))
            {
                Scheduler.RunDefaultSchedule();
                raw = service.GetRaw("config-schedule");
            }
            else
            {
                try
                {
                    raw.ToObject<System.Collections.Generic.List<Scheduler.PersistTask>>();
                }
                catch (Exception ex)
                {
                    service.Delete("config-schedule");
                    Scheduler.RunDefaultSchedule();
                    raw = service.GetRaw("config-schedule");
                }
            }
            return raw;
        }

        private static ErrorList ReadFile(out System.Collections.Generic.List<Task> tasks)
        {
            ErrorList errorList = new ErrorList();
            tasks = new System.Collections.Generic.List<Task>();
            try
            {
                string raw;
                lock (Scheduler.FileLock)
                    raw = Scheduler.GetRaw();
                if (!string.IsNullOrEmpty(raw))
                    tasks = Scheduler.PersistTasksToTasks((IEnumerable<Scheduler.PersistTask>)raw.ToObject<System.Collections.Generic.List<Scheduler.PersistTask>>());
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E990", "Unhandled exception when reading the schedule file.", ex));
            }
            return errorList;
        }

        private static ErrorList ReadWriteFile(Func<System.Collections.Generic.List<Task>, System.Collections.Generic.List<Task>> operation)
        {
            try
            {
                IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
                lock (Scheduler.FileLock)
                {
                    string raw = Scheduler.GetRaw();
                    System.Collections.Generic.List<Scheduler.PersistTask> persistTaskList = new System.Collections.Generic.List<Scheduler.PersistTask>();
                    if (!string.IsNullOrEmpty(raw))
                        persistTaskList = raw.ToObject<System.Collections.Generic.List<Scheduler.PersistTask>>();
                    System.Collections.Generic.List<Task> tasks = Scheduler.PersistTasksToTasks((IEnumerable<Scheduler.PersistTask>)persistTaskList);
                    System.Collections.Generic.List<Scheduler.PersistTask> persistTasks = Scheduler.TasksToPersistTasks((IEnumerable<Task>)operation(tasks));
                    service.Set("config-schedule", (object)persistTasks);
                }
            }
            catch (Exception ex)
            {
                ErrorList errorList = new ErrorList();
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E909", "Unhandled exception when accessing the schedule file.", ex));
                return errorList;
            }
            return new ErrorList();
        }

        private static void RunDefaultSchedule()
        {
            try
            {
                string path = Path.Combine(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties("${Scripts}"), "default-schedule.lua");
                IDataStoreService service1 = ServiceLocator.Instance.GetService<IDataStoreService>();
                if (!File.Exists(path))
                {
                    LogHelper.Instance.Log("RunDefaultSchedule: Script: {0} not found.", (object)path);
                }
                else
                {
                    service1.Set("config-schedule", new object());
                    IKernelService service2 = ServiceLocator.Instance.GetService<IKernelService>();
                    string chunk = File.ReadAllText(path);
                    LogHelper.Instance.Log("RunDefaultSchedule: Executing lua script at {0}", (object)path);
                    service2.ExecuteChunk(chunk);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unknown Error in RunDefaultSchedule", ex);
            }
        }

        private static System.Collections.Generic.List<Task> PersistTasksToTasks(
          IEnumerable<Scheduler.PersistTask> persistTasks)
        {
            return persistTasks.Select<Scheduler.PersistTask, Task>((Func<Scheduler.PersistTask, Task>)(persistTask => persistTask.Task())).ToList<Task>();
        }

        private static System.Collections.Generic.List<Scheduler.PersistTask> TasksToPersistTasks(
          IEnumerable<Task> tasks)
        {
            return tasks.Select<Task, Scheduler.PersistTask>((Func<Task, Scheduler.PersistTask>)(task => new Scheduler.PersistTask((ITask)task))).ToList<Scheduler.PersistTask>();
        }

        private static ErrorList ParsePayload(string text, out PayloadType parsed)
        {
            ErrorList payload = new ErrorList();
            parsed = Enum<PayloadType>.ParseIgnoringCase(text, PayloadType.Unknown);
            if (parsed == PayloadType.Unknown)
                payload.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("P001", "Cannot Parse PayloadType", "Cannot parse PayloadType " + text));
            return payload;
        }

        private class PersistTask
        {
            public PersistTask()
            {
            }

            public PersistTask(ITask task)
            {
                this.Name = task.Name;
                this.ScheduleType = Enum.GetName(typeof(ScheduleType), (object)task.ScheduleType);
                this.Payload = task.Payload;
                this.PayloadType = Enum.GetName(typeof(PayloadType), (object)task.PayloadType);
                this.StartOffset = task.StartOffset.HasValue ? task.StartOffset.ToString() : (string)null;
                DateTime? nullable = task.StartTime;
                DateTime dateTime;
                string str1;
                if (!nullable.HasValue)
                {
                    str1 = (string)null;
                }
                else
                {
                    nullable = task.StartTime;
                    dateTime = nullable.Value;
                    str1 = dateTime.ToLongTimeString();
                }
                this.StartTime = str1;
                nullable = task.StartTime;
                string str2;
                if (!nullable.HasValue)
                {
                    str2 = (string)null;
                }
                else
                {
                    nullable = task.StartTime;
                    dateTime = nullable.Value;
                    str2 = dateTime.ToLongDateString();
                }
                this.StartDate = str2;
                nullable = task.EndTime;
                string str3;
                if (!nullable.HasValue)
                {
                    str3 = (string)null;
                }
                else
                {
                    nullable = task.EndTime;
                    dateTime = nullable.Value;
                    str3 = dateTime.ToLongTimeString();
                }
                this.EndTime = str3;
                nullable = task.EndTime;
                string str4;
                if (!nullable.HasValue)
                {
                    str4 = (string)null;
                }
                else
                {
                    nullable = task.EndTime;
                    dateTime = nullable.Value;
                    str4 = dateTime.ToLongDateString();
                }
                this.EndDate = str4;
                this.CronExpression = task.CronExpression;
                TimeSpan? repeatInterval = task.RepeatInterval;
                string str5;
                if (!repeatInterval.HasValue)
                {
                    str5 = (string)null;
                }
                else
                {
                    repeatInterval = task.RepeatInterval;
                    str5 = repeatInterval.Value.ToString();
                }
                this.RepeatInterval = str5;
            }

            public Task Task()
            {
                Task task = new Task();
                PayloadType parsed;
                Scheduler.ParsePayload(this.PayloadType, out parsed);
                TimeSpan? startOffset = new TimeSpan?();
                TimeSpan result1;
                if (TimeSpan.TryParse(this.StartOffset, out result1))
                    startOffset = new TimeSpan?(result1);
                DateTime? startTime;
                if (!string.IsNullOrEmpty(this.StartDate))
                {
                    DateTime result2;
                    if (!DateTime.TryParse(this.StartDate, out result2))
                    {
                        LogHelper.Instance.Log("Could not parse startDate, skipping task " + this.Name, LogEntryType.Error);
                        return task;
                    }
                    DateTime result3;
                    if (!DateTime.TryParse(this.StartTime, out result3))
                    {
                        LogHelper.Instance.Log("Could not parse startDate, skipping task " + this.Name, LogEntryType.Error);
                        return task;
                    }
                    startTime = new DateTime?(new DateTime(result2.Year, result2.Month, result2.Day, result3.Hour, result3.Minute, result3.Second));
                }
                else
                    startTime = new DateTime?();
                DateTime? endTime;
                if (!string.IsNullOrEmpty(this.EndDate))
                {
                    DateTime result4;
                    if (!DateTime.TryParse(this.EndDate, out result4))
                    {
                        LogHelper.Instance.Log("Could not parse endDate, skipping task " + this.Name, LogEntryType.Error);
                        return task;
                    }
                    DateTime result5;
                    if (!DateTime.TryParse(this.EndTime, out result5))
                    {
                        LogHelper.Instance.Log("Could not parse endTime, skipping task " + this.Name, LogEntryType.Error);
                        return task;
                    }
                    endTime = new DateTime?(new DateTime(result4.Year, result4.Month, result4.Day, result5.Hour, result5.Minute, result5.Second));
                }
                else
                    endTime = new DateTime?();
                switch (Enum<ScheduleType>.ParseIgnoringCase(this.ScheduleType, ComponentModel.ScheduleType.Unknown))
                {
                    case ComponentModel.ScheduleType.Simple:
                        TimeSpan result6;
                        if (!TimeSpan.TryParse(this.RepeatInterval, out result6))
                        {
                            LogHelper.Instance.Log("Could not parse RepeatInterval, skipping task " + this.Name, LogEntryType.Error);
                            break;
                        }
                        task = new Task(this.Name, this.Payload, parsed, startOffset, startTime, endTime, result6);
                        break;
                    case ComponentModel.ScheduleType.Cron:
                        task = new Task(this.Name, this.Payload, parsed, startOffset, startTime, endTime, this.CronExpression);
                        break;
                }
                return task;
            }

            public string Name { get; set; }

            public string ScheduleType { get; set; }

            public string Payload { get; set; }

            public string PayloadType { get; set; }

            public string StartOffset { get; set; }

            public string StartDate { get; set; }

            public string StartTime { get; set; }

            public string EndTime { get; set; }

            public string EndDate { get; set; }

            public string CronExpression { get; set; }

            public string RepeatInterval { get; set; }
        }
    }
}
