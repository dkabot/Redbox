using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.Command
{
    [Redbox.IPC.Framework.Command("schedule-event")]
    internal class ScheduleEventCommand
    {
        [CommandForm(Name = "list")]
        [Usage("SCHEDULE-EVENT list")]
        public void List(CommandContext context)
        {
            ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
            System.Collections.Generic.List<ITask> tasks;
            context.Errors.AddRange(ScheduleEventCommand.ConvertToIPCErrors((System.Collections.Generic.List<Redbox.UpdateManager.ComponentModel.Error>)service.List(out tasks)));
            if (context.Errors.ContainsError())
                return;
            context.Messages.Add(tasks.ToJson());
        }

        [CommandForm(Name = "isRunning")]
        [Usage("SCHEDULE-EVENT isRunning")]
        public void IsRunning(CommandContext context)
        {
            ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
            context.Messages.Add(service.IsRunning.ToJson());
        }

        [CommandForm(Name = "create")]
        [Usage("SCHEDULE-EVENT create name: payload: payloadType:{script, shell, work} [startOffset:(sec)] [startTime:] [endTime:] [[repeatInterval:] [cronExpression:]]")]
        public void Create(
          CommandContext context,
          [CommandKeyValue(IsRequired = true)] string name,
          [CommandKeyValue(IsRequired = true)] string payload,
          [CommandKeyValue(IsRequired = true)] string payloadType,
          [CommandKeyValue(IsRequired = false)] int startOffset,
          [CommandKeyValue(IsRequired = false)] string startTime,
          [CommandKeyValue(IsRequired = false)] string endTime,
          [CommandKeyValue(IsRequired = false)] string cronExpression,
          [CommandKeyValue(IsRequired = false)] string repeatInterval)
        {
            ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
            TimeSpan? startOffset1 = new TimeSpan?();
            if (startOffset > 0)
                startOffset1 = new TimeSpan?(new TimeSpan(0, 0, startOffset));
            DateTime? startTime1 = new DateTime?();
            if (!string.IsNullOrEmpty(startTime))
            {
                DateTime result;
                if (DateTime.TryParse(startTime, out result))
                {
                    startTime1 = new DateTime?(result);
                }
                else
                {
                    context.Errors.Add(Redbox.IPC.Framework.Error.NewError("C434", "Could not parse startTime.", "Please enter a time that can be parsed."));
                    return;
                }
            }
            DateTime? endTime1 = new DateTime?();
            if (!string.IsNullOrEmpty(endTime))
            {
                DateTime result;
                if (DateTime.TryParse(endTime, out result))
                {
                    endTime1 = new DateTime?(result);
                }
                else
                {
                    context.Errors.Add(Redbox.IPC.Framework.Error.NewError("C433", "Could not parse endTime.", "Please enter a time that can be parsed."));
                    return;
                }
            }
            if (!string.IsNullOrEmpty(repeatInterval) && !string.IsNullOrEmpty(cronExpression) || string.IsNullOrEmpty(repeatInterval) && string.IsNullOrEmpty(cronExpression))
            {
                context.Errors.Add(Redbox.IPC.Framework.Error.NewError("C435", "Must specify either repeatInterval or cronExpression.", "Must specify exactly one of repeatInterval or cronExpression, not both."));
            }
            else
            {
                if (!string.IsNullOrEmpty(repeatInterval))
                {
                    TimeSpan result;
                    if (!TimeSpan.TryParse(repeatInterval, out result))
                    {
                        context.Errors.Add(Redbox.IPC.Framework.Error.NewError("C437", "Could not parse repeatInterval.", "Please enter a timespan that can be parsed."));
                        return;
                    }
                    context.Errors.AddRange(ScheduleEventCommand.ConvertToIPCErrors((System.Collections.Generic.List<Redbox.UpdateManager.ComponentModel.Error>)service.ScheduleSimpleJob(name, payload, payloadType, startOffset1, startTime1, endTime1, result)));
                }
                else
                    context.Errors.AddRange(ScheduleEventCommand.ConvertToIPCErrors((System.Collections.Generic.List<Redbox.UpdateManager.ComponentModel.Error>)service.ScheduleCronJob(name, payload, payloadType, startOffset1, startTime1, endTime1, cronExpression)));
                if (context.Errors.ContainsError())
                    return;
                service.Restart();
                context.Messages.Add("Success");
            }
        }

        [CommandForm(Name = "delete")]
        [Usage("SCHEDULE-EVENT delete name:")]
        public void Delete(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
        {
            ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
            context.Errors.AddRange(ScheduleEventCommand.ConvertToIPCErrors((System.Collections.Generic.List<Redbox.UpdateManager.ComponentModel.Error>)service.Delete(name)));
        }

        [CommandForm(Name = "clear")]
        [Usage("SCHEDULE-EVENT clear")]
        public void Delete(CommandContext context)
        {
            ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
            context.Errors.AddRange(ScheduleEventCommand.ConvertToIPCErrors((System.Collections.Generic.List<Redbox.UpdateManager.ComponentModel.Error>)service.Clear()));
        }

        [CommandForm(Name = "stop")]
        [Usage("SCHEDULE-EVENT stop")]
        public void Stop(CommandContext context)
        {
            ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
            context.Errors.AddRange(ScheduleEventCommand.ConvertToIPCErrors((System.Collections.Generic.List<Redbox.UpdateManager.ComponentModel.Error>)service.Stop()));
        }

        [CommandForm(Name = "start")]
        [Usage("SCHEDULE-EVENT start")]
        public void Start(CommandContext context)
        {
            ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
            context.Errors.AddRange(ScheduleEventCommand.ConvertToIPCErrors((System.Collections.Generic.List<Redbox.UpdateManager.ComponentModel.Error>)service.Start()));
        }

        [CommandForm(Name = "restart")]
        [Usage("SCHEDULE-EVENT restart")]
        public void Restart(CommandContext context)
        {
            ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
            context.Errors.AddRange(ScheduleEventCommand.ConvertToIPCErrors((System.Collections.Generic.List<Redbox.UpdateManager.ComponentModel.Error>)service.Restart()));
        }

        [CommandForm(Name = "force")]
        [Usage("SCHEDULE-EVENT force name:")]
        public void Force(CommandContext context, string name)
        {
            ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
            bool success;
            context.Errors.AddRange(ScheduleEventCommand.ConvertToIPCErrors((System.Collections.Generic.List<Redbox.UpdateManager.ComponentModel.Error>)service.ForceTask(name, out success)));
            if (context.Errors.ContainsError())
                return;
            context.Messages.Add(success.ToJson());
        }

        private static IEnumerable<Redbox.IPC.Framework.Error> ConvertToIPCErrors(System.Collections.Generic.List<Redbox.UpdateManager.ComponentModel.Error> errors)
        {
            Redbox.IPC.Framework.ErrorList ipcErrors = new Redbox.IPC.Framework.ErrorList();
            errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => ipcErrors.Add(e.IsWarning ? Redbox.IPC.Framework.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.IPC.Framework.Error.NewError(e.Code, e.Description, e.Details))));
            return (IEnumerable<Redbox.IPC.Framework.Error>)ipcErrors;
        }
    }
}
