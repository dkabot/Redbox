using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Text;

namespace Redbox.UpdateManager.ServiceProxies
{
    internal class SchedulerProxy : ITaskScheduler
    {
        private string m_url;

        public static SchedulerProxy Instance => Singleton<SchedulerProxy>.Instance;

        public void Initialize(string url)
        {
            this.m_url = url;
            ServiceLocator.Instance.AddService(typeof(ITaskScheduler), (object)this);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList Stop()
        {
            return this.ExecuteCommandString("schedule-event stop");
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList Start()
        {
            return this.ExecuteCommandString("schedule-event start");
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList Restart()
        {
            return this.ExecuteCommandString("schedule-event restart");
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList Delete(string name)
        {
            return this.ExecuteCommandString(string.Format("SCHEDULE-EVENT delete name:'{0}'", (object)name));
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList Clear()
        {
            return this.ExecuteCommandString("SCHEDULE-EVENT clear");
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList List(out System.Collections.Generic.List<ITask> tasks)
        {
            tasks = new System.Collections.Generic.List<ITask>();
            ClientCommandResult clientCommandResult = this.ExecuteCommandStringWithReturnValue("schedule-event list");
            if (clientCommandResult.Success)
            {
                foreach (Task task in clientCommandResult.CommandMessages[0].ToObject<System.Collections.Generic.List<Task>>())
                    tasks.Add((ITask)task);
            }
            return SchedulerProxy.ConvertToComponentErrors((System.Collections.Generic.List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ForceTask(string name, out bool success)
        {
            success = false;
            ClientCommandResult clientCommandResult = this.ExecuteCommandStringWithReturnValue(string.Format("SCHEDULE-EVENT force name:'{0}'", (object)name));
            if (clientCommandResult.Success)
                success = clientCommandResult.CommandMessages[0].ToObject<bool>();
            return SchedulerProxy.ConvertToComponentErrors((System.Collections.Generic.List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ScheduleCronJob(
          string name,
          string payload,
          string payloadTypeString,
          TimeSpan? startOffset,
          DateTime? startTime,
          DateTime? endTime,
          string cronExpression)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("SCHEDULE-EVENT create name:'{0}' payload:'{1}' payloadType:'{2}'  cronExpression:'{3}'", (object)name, (object)payload, (object)payloadTypeString, (object)cronExpression);
            if (startOffset.HasValue)
                stringBuilder.AppendFormat(" startOffset:'{0}'", (object)startOffset.Value.TotalSeconds);
            if (startTime.HasValue)
                stringBuilder.AppendFormat(" startTime:'{0}'", (object)startTime.Value);
            if (endTime.HasValue)
                stringBuilder.AppendFormat(" endTime:'{0}'", (object)endTime.Value);
            return SchedulerProxy.ConvertToComponentErrors((System.Collections.Generic.List<Redbox.IPC.Framework.Error>)this.ExecuteCommandStringWithReturnValue(stringBuilder.ToString()).Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ScheduleCronJob(
          string name,
          string payload,
          PayloadType payloadType,
          TimeSpan? startOffset,
          DateTime? startTime,
          DateTime? endTime,
          string cronExpression)
        {
            return this.ScheduleCronJob(name, payload, Enum.GetName(typeof(PayloadType), (object)payloadType), startOffset, startTime, endTime, cronExpression);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ScheduleSimpleJob(
          string name,
          string payload,
          string payloadTypeString,
          TimeSpan? startOffset,
          DateTime? startTime,
          DateTime? endTime,
          TimeSpan repeatInterval)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("SCHEDULE-EVENT create name:'{0}' payload:'{1}' payloadType:'{2}' repeatInterval:'{3}'", (object)name, (object)payload, (object)payloadTypeString, (object)repeatInterval);
            if (startOffset.HasValue)
                stringBuilder.AppendFormat(" startOffset:'{0}'", (object)startOffset.Value.TotalSeconds);
            if (startTime.HasValue)
                stringBuilder.AppendFormat(" startTime:'{0}'", (object)startTime.Value);
            if (endTime.HasValue)
                stringBuilder.AppendFormat(" endTime:'{0}'", (object)endTime.Value);
            return SchedulerProxy.ConvertToComponentErrors((System.Collections.Generic.List<Redbox.IPC.Framework.Error>)this.ExecuteCommandStringWithReturnValue(stringBuilder.ToString()).Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ScheduleSimpleJob(
          string name,
          string payload,
          PayloadType payloadType,
          TimeSpan? startOffset,
          DateTime? startTime,
          DateTime? endTime,
          TimeSpan repeatInterval)
        {
            return this.ScheduleSimpleJob(name, payload, Enum.GetName(typeof(PayloadType), (object)payloadType), startOffset, startTime, endTime, repeatInterval);
        }

        public bool IsRunning
        {
            get
            {
                bool isRunning = false;
                ClientCommandResult clientCommandResult = this.ExecuteCommandStringWithReturnValue("schedule-event isRunning");
                if (!clientCommandResult.Errors.ContainsError())
                    isRunning = clientCommandResult.CommandMessages[0].ToObject<bool>();
                return isRunning;
            }
        }

        private SchedulerProxy()
        {
        }

        private Redbox.UpdateManager.ComponentModel.ErrorList ExecuteCommandString(string command)
        {
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
                return SchedulerProxy.ConvertToComponentErrors((System.Collections.Generic.List<Redbox.IPC.Framework.Error>)service.ExecuteCommandString(command).Errors);
        }

        private ClientCommandResult ExecuteCommandStringWithReturnValue(string command)
        {
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
                return service.ExecuteCommandString(command);
        }

        private static Redbox.UpdateManager.ComponentModel.ErrorList ConvertToComponentErrors(
          System.Collections.Generic.List<Redbox.IPC.Framework.Error> ipcErrors)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errors = new Redbox.UpdateManager.ComponentModel.ErrorList();
            ipcErrors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errors.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details))));
            return errors;
        }
    }
}
