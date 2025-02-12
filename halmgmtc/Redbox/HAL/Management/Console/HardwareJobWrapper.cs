using System;
using System.Collections.Generic;
using System.ComponentModel;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Management.Console
{
    public class HardwareJobWrapper : INotifyPropertyChanged, IComparable
    {
        private readonly HardwareJob m_hardwareJob;
        private BindingList<ProgramEvent> m_events;

        public HardwareJobWrapper(HardwareJob job)
        {
            m_hardwareJob = job;
        }

        public string ID => !IsDisplayable() ? Constants.None : m_hardwareJob.ID;

        public string Label => !IsDisplayable() ? string.Empty : m_hardwareJob.Label;

        public HardwareJobConnectionState ConnectionState =>
            !IsDisplayable() ? HardwareJobConnectionState.Disconnected : m_hardwareJob.ConnectionState;

        public string ProgramName => !IsDisplayable() ? string.Empty : m_hardwareJob.ProgramName;

        public HardwareJobStatus Status => !IsDisplayable() ? HardwareJobStatus.Errored : m_hardwareJob.Status;

        public HardwareJobPriority Priority => !IsDisplayable() ? HardwareJobPriority.Lowest : m_hardwareJob.Priority;

        public DateTime? StartTime => !IsDisplayable() ? new DateTime?() : m_hardwareJob.StartTime;

        public TimeSpan? ExecutionTime => !IsDisplayable() ? new TimeSpan?() : m_hardwareJob.ExecutionTime;

        public BindingList<ProgramEvent> Events
        {
            get
            {
                if (m_events == null)
                    m_events = new BindingList<ProgramEvent>();
                return m_events;
            }
            set => m_events = value;
        }

        public int CompareTo(object p_Target)
        {
            var hardwareJobWrapper = p_Target as HardwareJobWrapper;
            if (!IsDisplayable() && !hardwareJobWrapper.IsDisplayable())
                return 0;
            if (!IsDisplayable() && hardwareJobWrapper.IsDisplayable())
                return -1;
            return IsDisplayable() && !hardwareJobWrapper.IsDisplayable() ? 1 : ID.CompareTo(hardwareJobWrapper.ID);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public HardwareCommandResult Connect()
        {
            return m_hardwareJob.Connect();
        }

        public void Disconnect()
        {
            m_hardwareJob.Disconnect();
        }

        public HardwareCommandResult Pend()
        {
            return m_hardwareJob.Pend();
        }

        public HardwareCommandResult SetPriority(HardwareJobPriority priority)
        {
            return m_hardwareJob.SetPriority(priority);
        }

        public HardwareCommandResult SetLabel(string label)
        {
            return m_hardwareJob.SetLabel(label);
        }

        public HardwareCommandResult SetStartTime(DateTime startTime)
        {
            return m_hardwareJob.SetStartTime(startTime);
        }

        public HardwareCommandResult Resume()
        {
            return m_hardwareJob.Resume();
        }

        public HardwareCommandResult Suspend()
        {
            return m_hardwareJob.Suspend();
        }

        public HardwareCommandResult Terminate()
        {
            return m_hardwareJob.Terminate();
        }

        public HardwareCommandResult Trash()
        {
            return m_hardwareJob.Trash();
        }

        public HardwareCommandResult ClearStack()
        {
            return m_hardwareJob.ClearStack();
        }

        public HardwareCommandResult ClearSymbols()
        {
            return m_hardwareJob.ClearSymbols();
        }

        public HardwareCommandResult GetErrors(out ErrorList errors)
        {
            return m_hardwareJob.GetErrors(out errors);
        }

        public HardwareCommandResult GetMessages(out string[] messages)
        {
            return m_hardwareJob.GetMessages(out messages);
        }

        public HardwareCommandResult GetResults(out ProgramResult[] results)
        {
            return m_hardwareJob.GetResults(out results);
        }

        public HardwareCommandResult GetStack(out Stack<string> stack)
        {
            return m_hardwareJob.GetStack(out stack);
        }

        public HardwareCommandResult GetSymbols(out IDictionary<string, string> symbols)
        {
            return m_hardwareJob.GetSymbols(out symbols);
        }

        public HardwareCommandResult Pop<T>(out T value)
        {
            return m_hardwareJob.Pop(out value);
        }

        public HardwareCommandResult Push<T>(params object[] values)
        {
            return m_hardwareJob.Push(values);
        }

        public bool IsDisplayable()
        {
            return m_hardwareJob != null;
        }

        public void Merge(HardwareJob job)
        {
            if (!IsDisplayable())
            {
                LogHelper.Instance.Log("ERROR: Attempting to merge data to a null jobwrapper", LogEntryType.Error);
                var list = new ErrorList();
                list.Add(Error.NewError("BadJobDataMerge", "Attempted to merge job data to null item", string.Empty));
                EnvironmentHelper.DisplayErrors(list, true, false);
            }
            else
            {
                if (!m_hardwareJob.Merge(job))
                    return;
                NotifyPropertyChanged("Label");
                NotifyPropertyChanged("ConnectionState");
                NotifyPropertyChanged("ProgramName");
                NotifyPropertyChanged("Status");
                NotifyPropertyChanged("Priority");
                NotifyPropertyChanged("StartTime");
                NotifyPropertyChanged("ExecutionTime");
            }
        }

        public event HardwareEvent EventRaised
        {
            add => m_hardwareJob.EventRaised += value;
            remove => m_hardwareJob.EventRaised -= value;
        }

        public override string ToString()
        {
            return !IsDisplayable() ? Constants.None : m_hardwareJob.ToString();
        }

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged == null)
                return;
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}