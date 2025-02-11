using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;
using System;

namespace Redbox.UpdateManager.Kernel
{
    internal static class Win32TaskSchedulerFunctions
    {
        [KernelFunction(Name = "kernel.win32removescheduledtask")]
        internal static bool RemoveScheduledTask(string name)
        {
            IWindowsTaskScheduler service = ServiceLocator.Instance.GetService<IWindowsTaskScheduler>();
            return service != null && service.RemoveScheduledTask(name);
        }

        [KernelFunction(Name = "kernel.win32getscheduledtasknames")]
        internal static LuaTable GetScheduledTaskNames()
        {
            LuaTable scheduledTaskNames = new LuaTable(KernelService.Instance.LuaRuntime);
            IWindowsTaskScheduler service = ServiceLocator.Instance.GetService<IWindowsTaskScheduler>();
            if (service == null)
                return scheduledTaskNames;
            int num = 1;
            foreach (string scheduledTask in service.GetScheduledTasks())
                scheduledTaskNames[(object)num++] = (object)scheduledTask;
            return scheduledTaskNames;
        }

        [KernelFunction(Name = "kernel.win32createscheduledtask")]
        internal static void CreateScheduledTask(
          string name,
          string applicationName,
          string arguments,
          string workingDirectory,
          string comment,
          string creatorName,
          string accountName,
          string password)
        {
            ServiceLocator.Instance.GetService<IWindowsTaskScheduler>()?.CreateScheduledTask(name, applicationName, arguments, workingDirectory, comment, creatorName, accountName, password);
        }

        [KernelFunction(Name = "kernel.win32addonidletrigger")]
        internal static void AddOnIdleTrigger(
          string name,
          object beginDate,
          object endDate,
          object idleWaitMinutes)
        {
            IWindowsTaskScheduler service = ServiceLocator.Instance.GetService<IWindowsTaskScheduler>();
            if (service == null)
                return;
            DateTime? nullable = new DateTime?();
            if (beginDate is LuaTable luaTable1)
                nullable = luaTable1.ToDateTime();
            if (!nullable.HasValue)
                return;
            DateTime? endDate1 = new DateTime?();
            if (endDate is LuaTable luaTable2)
                endDate1 = luaTable2.ToDateTime();
            short? idleWaitMinutes1 = new short?();
            if (idleWaitMinutes != null)
                idleWaitMinutes1 = new short?(Convert.ToInt16(idleWaitMinutes));
            service.AddOnIdleTrigger(name, nullable.Value, endDate1, idleWaitMinutes1);
        }

        [KernelFunction(Name = "kernel.win32addondailytrigger")]
        internal static void AddDailyTrigger(
          string name,
          object beginDate,
          object endDate,
          object hours,
          object minutes,
          object daysInterval,
          object minutesInterval)
        {
            IWindowsTaskScheduler service = ServiceLocator.Instance.GetService<IWindowsTaskScheduler>();
            if (service == null)
                return;
            DateTime? nullable = new DateTime?();
            if (beginDate is LuaTable luaTable1)
                nullable = luaTable1.ToDateTime();
            if (!nullable.HasValue)
                return;
            DateTime? endDate1 = new DateTime?();
            if (endDate is LuaTable luaTable2)
                endDate1 = luaTable2.ToDateTime();
            DateTime now = DateTime.Now;
            int hours1 = now.Hour;
            if (hours != null)
                hours1 = (int)Convert.ToInt16(hours);
            now = DateTime.Now;
            int minutes1 = now.Minute;
            if (minutes != null)
                minutes1 = (int)Convert.ToInt16(minutes);
            int num = 1;
            if (daysInterval != null)
                num = (int)Convert.ToInt16(daysInterval);
            int? minutesInterval1 = new int?();
            if (minutesInterval != null)
                minutesInterval1 = new int?(Convert.ToInt32(minutesInterval));
            service.AddDailyTrigger(name, nullable.Value, endDate1, (short)hours1, (short)minutes1, new short?((short)num), minutesInterval1);
        }

        [KernelFunction(Name = "kernel.win32addonsystemstartuptrigger")]
        internal static void AddSystemStartupTrigger(string name, object beginDate, object endDate)
        {
            IWindowsTaskScheduler service = ServiceLocator.Instance.GetService<IWindowsTaskScheduler>();
            if (service == null)
                return;
            DateTime? nullable = new DateTime?();
            if (beginDate is LuaTable luaTable1)
                nullable = luaTable1.ToDateTime();
            if (!nullable.HasValue)
                return;
            DateTime? endDate1 = new DateTime?();
            if (endDate is LuaTable luaTable2)
                endDate1 = luaTable2.ToDateTime();
            service.AddSystemStartupTrigger(name, nullable.Value, endDate1);
        }
    }
}
