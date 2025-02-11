using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TaskScheduler;

namespace Redbox.UpdateManager.Remoting
{
    internal class WindowsTaskScheduler : IWindowsTaskScheduler
    {
        public static WindowsTaskScheduler Instance => Singleton<WindowsTaskScheduler>.Instance;

        public void Initialize()
        {
            ServiceLocator.Instance.AddService(typeof(IWindowsTaskScheduler), (object)this);
        }

        public ReadOnlyCollection<string> GetScheduledTasks()
        {
            using (ScheduledTasks scheduledTasks = new ScheduledTasks())
                return new List<string>((IEnumerable<string>)scheduledTasks.GetTaskNames()).AsReadOnly();
        }

        public bool RemoveScheduledTask(string name)
        {
            try
            {
                using (ScheduledTasks scheduledTasks = new ScheduledTasks())
                    return scheduledTasks.DeleteTask(name);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in WindowsTaskScheduler.RemoveScheduledTask.", ex);
                return false;
            }
        }

        public void CreateScheduledTask(
          string name,
          string applicationName,
          string arguments,
          string workingDirectory,
          string comment,
          string creatorName,
          string accountName,
          string password)
        {
            try
            {
                using (ScheduledTasks scheduledTasks = new ScheduledTasks())
                {
                    Task task = (Task)null;
                    try
                    {
                        task = scheduledTasks.OpenTask(name) ?? scheduledTasks.CreateTask(name);
                        task.Flags = TaskFlags.Hidden;
                        if (string.IsNullOrEmpty(password))
                            task.Flags |= TaskFlags.RunOnlyIfLoggedOn;
                        task.Priority = ProcessPriorityClass.Normal;
                        task.ApplicationName = applicationName;
                        if (!string.IsNullOrEmpty(arguments))
                            task.Parameters = arguments;
                        if (!string.IsNullOrEmpty(workingDirectory))
                            task.WorkingDirectory = workingDirectory;
                        if (!string.IsNullOrEmpty(comment))
                            task.Comment = comment;
                        if (!string.IsNullOrEmpty(creatorName))
                            task.Creator = creatorName;
                        if (!string.IsNullOrEmpty(accountName))
                        {
                            if (string.Compare(accountName, "system", true) == 0)
                                task.SetAccountInformation("", (string)null);
                            else
                                task.SetAccountInformation(accountName, password);
                        }
                        task.Save();
                    }
                    finally
                    {
                        task?.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in WindowsTaskScheduler.CreateScheduledTask.", ex);
            }
        }

        public void AddOnIdleTrigger(
          string name,
          DateTime beginDate,
          DateTime? endDate,
          short? idleWaitMinutes)
        {
            try
            {
                using (ScheduledTasks scheduledTasks = new ScheduledTasks())
                {
                    Task task = null;
                    try
                    {
                        task = scheduledTasks.OpenTask(name);
                        if (task == null)
                            return;
                        task.IdleWaitMinutes = (short)(idleWaitMinutes ?? 1);
                        OnIdleTrigger onIdleTrigger1 = new OnIdleTrigger();
                        onIdleTrigger1.BeginDate = beginDate;
                        OnIdleTrigger onIdleTrigger2 = onIdleTrigger1;
                        if (endDate.HasValue)
                            onIdleTrigger2.EndDate = endDate.Value;
                        task.Triggers.Add(onIdleTrigger2);
                        task.Save();
                    }
                    finally
                    {
                        task?.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in WindowsTaskScheduler.AddOnIdleTrigger.", ex);
            }
        }

        public void AddDailyTrigger(
          string name,
          DateTime beginDate,
          DateTime? endDate,
          short hours,
          short minutes,
          short? daysInterval,
          int? minutesInterval)
        {
            try
            {
                using (ScheduledTasks scheduledTasks = new ScheduledTasks())
                {
                    Task task = null;
                    try
                    {
                        task = scheduledTasks.OpenTask(name);
                        if (task == null)
                            return;
                        DailyTrigger dailyTrigger1 = new DailyTrigger(hours, minutes);
                        dailyTrigger1.BeginDate = beginDate;
                        DailyTrigger dailyTrigger2 = dailyTrigger1;
                        if (daysInterval.HasValue)
                            dailyTrigger2.DaysInterval = daysInterval.Value;
                        if (endDate.HasValue)
                            dailyTrigger2.EndDate = endDate.Value;
                        if (minutesInterval.HasValue)
                        {
                            dailyTrigger2.DurationMinutes = 1440;
                            dailyTrigger2.IntervalMinutes = minutesInterval.Value;
                        }
                        task.Triggers.Add(dailyTrigger2);
                        task.Save();
                    }
                    finally
                    {
                        task?.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in WindowsTaskScheduler.AddDailyTrigger.", ex);
            }
        }

        public void AddSystemStartupTrigger(string name, DateTime beginDate, DateTime? endDate)
        {
            try
            {
                using (ScheduledTasks scheduledTasks = new ScheduledTasks())
                {
                    Task task = null;
                    try
                    {
                        task = scheduledTasks.OpenTask(name);
                        if (task == null)
                            return;
                        OnSystemStartTrigger systemStartTrigger1 = new OnSystemStartTrigger();
                        systemStartTrigger1.BeginDate = beginDate;
                        OnSystemStartTrigger systemStartTrigger2 = systemStartTrigger1;
                        if (endDate.HasValue)
                            systemStartTrigger2.EndDate = endDate.Value;
                        task.Triggers.Add(systemStartTrigger2);
                        task.Save();
                    }
                    finally
                    {
                        task?.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in WindowsTaskScheduler.AddSystemStartupTrigger.", ex);
            }
        }

        private WindowsTaskScheduler()
        {
        }
    }
}
