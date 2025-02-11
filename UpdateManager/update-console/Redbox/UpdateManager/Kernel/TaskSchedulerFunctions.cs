using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Redbox.UpdateManager.Kernel
{
    internal static class TaskSchedulerFunctions
    {
        [KernelFunction(Name = "kernel.getrandomnumber")]
        internal static string GetRandomNumber(int min, int max)
        {
            RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider();
            byte[] numArray = new byte[4];
            byte[] data = numArray;
            cryptoServiceProvider.GetBytes(data);
            int int32 = BitConverter.ToInt32(numArray, 0);
            return (min + Math.Abs(int32 % (max - min))).ToString();
        }

        [KernelFunction(Name = "kernel.schedulecronjob")]
        internal static LuaTable ScheduleCronJob(
          string name,
          string payload,
          string payloadType,
          int startOffset,
          string cronExpression)
        {
            ErrorList errors = ServiceLocator.Instance.GetService<ITaskScheduler>().ScheduleCronJob(name, payload, payloadType, startOffset > 0 ? new TimeSpan?(new TimeSpan(0, 0, startOffset)) : new TimeSpan?(), new DateTime?(), new DateTime?(), cronExpression);
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return TaskSchedulerFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.schedulecronscript")]
        internal static LuaTable ScheduleCronScript(
          string name,
          string script,
          int startOffset,
          string cronExpression)
        {
            return TaskSchedulerFunctions.ScheduleCronJob(name, script, nameof(script), startOffset, cronExpression);
        }

        [KernelFunction(Name = "kernel.schedulecronwork")]
        internal static LuaTable ScheduleCronWork(int startOffset, string cronExpression)
        {
            return new LuaTable(KernelService.Instance.LuaRuntime);
        }

        [KernelFunction(Name = "kernel.schedulesimplejob")]
        internal static LuaTable ScheduleSimpleJob(
          string name,
          string payload,
          string payloadTypeString,
          int startOffset,
          string repeatIntervalString)
        {
            ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
            TimeSpan result;
            if (!TimeSpan.TryParse(repeatIntervalString, out result))
            {
                ErrorList errors = new ErrorList();
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("I001", "Could not parse time interval", "Please use a parsable time interval"));
                return TaskSchedulerFunctions.CreateResultTable(errors);
            }
            ErrorList errors1 = service.ScheduleSimpleJob(name, payload, payloadTypeString, startOffset > 0 ? new TimeSpan?(new TimeSpan(0, 0, startOffset)) : new TimeSpan?(), new DateTime?(), new DateTime?(), result);
            if (errors1.ContainsError())
                errors1.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return TaskSchedulerFunctions.CreateResultTable(errors1);
        }

        [KernelFunction(Name = "kernel.scheduleonetimescript")]
        internal static LuaTable ScheduleSimpleJob(
          string name,
          string script,
          int startOffset,
          string timeString)
        {
            ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
            TimeSpan result;
            if (!TimeSpan.TryParse(timeString, out result))
            {
                ErrorList errors = new ErrorList();
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("I001", "Could not parse time interval", "Please use a parsable time interval"));
                return TaskSchedulerFunctions.CreateResultTable(errors);
            }
            ErrorList errors1 = service.ScheduleSimpleJob(name, script, nameof(script), startOffset > 0 ? new TimeSpan?(new TimeSpan(0, 0, startOffset)) : new TimeSpan?(), new DateTime?(DateTime.MinValue.Add(result)), new DateTime?(), TimeSpan.Zero);
            if (errors1.ContainsError())
                errors1.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return TaskSchedulerFunctions.CreateResultTable(errors1);
        }

        [KernelFunction(Name = "kernel.schedulesimplescript")]
        internal static LuaTable ScheduleSimpleScript(
          string name,
          string payload,
          int startOffset,
          string repeatIntervalString)
        {
            return TaskSchedulerFunctions.ScheduleSimpleJob(name, payload, "script", startOffset, repeatIntervalString);
        }

        [KernelFunction(Name = "kernel.schedulesimplerandomwork")]
        internal static LuaTable ScheduleSimpleRandomWork(int startOffset, string repeatIntervalString)
        {
            return new LuaTable(KernelService.Instance.LuaRuntime);
        }

        [KernelFunction(Name = "kernel.scheduleeventlist")]
        internal static LuaTable List()
        {
            System.Collections.Generic.List<ITask> tasks;
            ErrorList errors = ServiceLocator.Instance.GetService<ITaskScheduler>().List(out tasks);
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
            int key = 1;
            foreach (ITask task in tasks)
            {
                luaTable[(object)key] = (object)string.Format("{0} {1}", (object)task.Name, (object)task.Payload);
                ++key;
            }
            LuaTable resultTable = TaskSchedulerFunctions.CreateResultTable(errors);
            resultTable[(object)"events"] = (object)luaTable;
            return resultTable;
        }

        [KernelFunction(Name = "kernel.scheduleeventdelete")]
        internal static LuaTable Delete(string name)
        {
            ErrorList errors = ServiceLocator.Instance.GetService<ITaskScheduler>().Delete(name);
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return TaskSchedulerFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.eraseschedule")]
        internal static void Erase()
        {
            ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
            if (service == null)
                return;
            ErrorList errorList = service.Clear();
            if (!errorList.ContainsError())
                return;
            errorList.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
        }

        [KernelFunction(Name = "kernel.eraseschedulenodefault")]
        internal static LuaTable EraseNoDefault()
        {
            ErrorList errors = new ErrorList();
            ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
            System.Collections.Generic.List<ITask> tasks;
            errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)service.List(out tasks));
            foreach (ITask task in tasks)
                errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)service.Delete(task.Name));
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return TaskSchedulerFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.schedulerstop")]
        internal static LuaTable Stop()
        {
            ErrorList errors = ServiceLocator.Instance.GetService<ITaskScheduler>().Stop();
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return TaskSchedulerFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.schedulerstart")]
        internal static LuaTable Start()
        {
            ErrorList errors = ServiceLocator.Instance.GetService<ITaskScheduler>().Start();
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return TaskSchedulerFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.schedulerrestart")]
        internal static LuaTable Restart()
        {
            ErrorList errors = ServiceLocator.Instance.GetService<ITaskScheduler>().Restart();
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return TaskSchedulerFunctions.CreateResultTable(errors);
        }

        private static LuaTable CreateResultTable(ErrorList errors)
        {
            LuaTable resultTable = new LuaTable(KernelService.Instance.LuaRuntime);
            resultTable[(object)"success"] = (object)!errors.ContainsError();
            LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
            foreach (Redbox.UpdateManager.ComponentModel.Error error in (System.Collections.Generic.List<Redbox.UpdateManager.ComponentModel.Error>)errors)
                luaTable[(object)error.Code] = (object)error.Description;
            resultTable[(object)nameof(errors)] = (object)luaTable;
            return resultTable;
        }
    }
}
