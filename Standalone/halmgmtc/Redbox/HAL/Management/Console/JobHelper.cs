using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;
using Redbox.HAL.Management.Console.Properties;

namespace Redbox.HAL.Management.Console
{
    public static class JobHelper
    {
        private static readonly AtomicFlag m_jobListLock = new AtomicFlag();
        private static BindingList<ProgramEvent> m_programEventsList;
        private static BindingList<StringWrapper> m_resultList;
        private static SortableBindingList<HardwareJobWrapper> m_jobList;
        private static BindingList<HardwareJobWrapper> m_jobComboList;
        private static BindingList<StringWrapper> m_stackList;
        private static SortableBindingList<Symbol> m_symbolList;
        private static List<string> m_secureJobs;

        private static readonly Dictionary<string, List<ProgramEvent>> m_newEvents =
            new Dictionary<string, List<ProgramEvent>>();

        public static SortableBindingList<HardwareJobWrapper> JobList
        {
            get
            {
                if (m_jobList == null)
                    m_jobList = new SortableBindingList<HardwareJobWrapper>();
                return m_jobList;
            }
        }

        public static BindingList<HardwareJobWrapper> JobComboList
        {
            get
            {
                if (m_jobComboList == null)
                {
                    m_jobComboList = new BindingList<HardwareJobWrapper>();
                    m_jobComboList.Add(Constants.NullJob);
                }

                return m_jobComboList;
            }
        }

        public static BindingList<StringWrapper> StackList
        {
            get
            {
                if (m_stackList == null)
                    m_stackList = new BindingList<StringWrapper>();
                return m_stackList;
            }
        }

        public static SortableBindingList<Symbol> SymbolList
        {
            get
            {
                if (m_symbolList == null)
                    m_symbolList = new SortableBindingList<Symbol>();
                return m_symbolList;
            }
        }

        public static BindingList<StringWrapper> ResultList
        {
            get
            {
                if (m_resultList == null)
                    m_resultList = new BindingList<StringWrapper>();
                return m_resultList;
            }
        }

        public static BindingList<ProgramEvent> ProgramEventList
        {
            get
            {
                if (m_programEventsList == null)
                    m_programEventsList = new BindingList<ProgramEvent>();
                return m_programEventsList;
            }
            set => m_programEventsList = value;
        }

        public static void DisconnectJobs()
        {
            foreach (var job in JobList)
                if (job.ConnectionState == HardwareJobConnectionState.Connected)
                    job.Disconnect();
        }

        public static bool PushStackItem(string s)
        {
            if (!ProfileManager.Instance.IsConnected ||
                !(JobComboBox.Instance.SelectedItem is HardwareJobWrapper selectedItem))
                return false;
            int result1;
            if (int.TryParse(s, out result1))
            {
                var result2 = selectedItem.Push<int>(result1);
                CommonFunctions.ProcessCommandResult(result2);
                return result2.Success;
            }

            var str = s.Replace("\"", string.Empty).Replace("'", "\\'");
            var result3 = selectedItem.Push<string>(str);
            CommonFunctions.ProcessCommandResult(result3);
            return result3.Success;
        }

        public static bool ClearJobStack()
        {
            if (!ProfileManager.Instance.IsConnected ||
                !(JobComboBox.Instance.SelectedItem is HardwareJobWrapper selectedItem) ||
                !selectedItem.IsDisplayable())
                return false;
            var result = selectedItem.ClearStack();
            if (result.Success)
                return true;
            CommonFunctions.ProcessCommandResultError(result);
            return false;
        }

        public static void ClearJobComboList()
        {
            var hardwareJobWrapperList = new List<HardwareJobWrapper>();
            foreach (var jobCombo in JobComboList)
                if (jobCombo.IsDisplayable())
                    hardwareJobWrapperList.Add(jobCombo);
            foreach (var hardwareJobWrapper in hardwareJobWrapperList)
                JobComboList.Remove(hardwareJobWrapper);
        }

        public static void ProcessJobEvent(HardwareJob job, DateTime eventTime, string message)
        {
            if (message != null && message.StartsWith(">STATUS CHANGE<"))
                return;
            var programEvent = new ProgramEvent(eventTime, message);
            if (m_newEvents.ContainsKey(job.ID))
                m_newEvents[job.ID].Add(programEvent);
            else
                m_newEvents.Add(job.ID, new List<ProgramEvent>
                {
                    programEvent
                });
        }

        public static bool RefreshJobList()
        {
            if (!ProfileManager.Instance.IsConnected)
                return false;
            if (!m_jobListLock.Set())
                LogHelper.Instance.Log("Refresh Job List Denied - Locked", LogEntryType.Error);
            try
            {
                HardwareJob[] jobs1;
                var jobs2 = ProfileManager.Instance.Service.GetJobs(out jobs1);
                if (!jobs2.Success)
                {
                    CommonFunctions.ProcessCommandResultError(jobs2);
                    return false;
                }

                for (var index = 0; index < JobList.Count; ++index)
                {
                    var flag = true;
                    foreach (var job in jobs1)
                        if (job.ID == JobList[index].ID)
                        {
                            flag = false;
                            JobList[index].Merge(job);
                            break;
                        }

                    if (flag)
                    {
                        if (JobList[index].ConnectionState == HardwareJobConnectionState.Connected)
                            JobList[index].Disconnect();
                        JobComboList.Remove(JobList[index]);
                        JobList.RemoveAt(index);
                    }
                }

                foreach (var job in jobs1)
                {
                    var flag = true;
                    for (var index = 0; index < JobList.Count; ++index)
                        if (job.ID == JobList[index].ID)
                        {
                            flag = false;
                            break;
                        }

                    if (flag)
                    {
                        var hardwareJobWrapper = new HardwareJobWrapper(job);
                        JobList.Add(hardwareJobWrapper);
                        JobComboList.Add(hardwareJobWrapper);
                    }
                }
            }
            finally
            {
                m_jobListLock.Clear();
            }

            return true;
        }

        public static bool RefreshProgramEventList()
        {
            foreach (var newEvent in m_newEvents)
                if (newEvent.Value != null && newEvent.Value.Count > 0)
                {
                    var hardwareJobWrapper = (HardwareJobWrapper)null;
                    foreach (var job in JobList)
                        if (newEvent.Key == job.ID)
                        {
                            hardwareJobWrapper = job;
                            break;
                        }

                    if (hardwareJobWrapper != null)
                    {
                        var programEventArray = new ProgramEvent[newEvent.Value.Count];
                        newEvent.Value.CopyTo(programEventArray);
                        foreach (var programEvent in programEventArray.OrderBy(x => x.EventTime))
                        {
                            hardwareJobWrapper.Events.Add(programEvent);
                            newEvent.Value.Remove(programEvent);
                        }
                    }
                }

            return true;
        }

        public static bool RefreshResultList()
        {
            if (!(JobComboBox.Instance.SelectedItem is HardwareJobWrapper selectedItem) ||
                !selectedItem.IsDisplayable())
                return false;
            ProgramResult[] results1;
            var results2 = selectedItem.GetResults(out results1);
            if (!results2.Success)
            {
                CommonFunctions.ProcessCommandResultError(results2);
                return false;
            }

            for (var index = 0; index < ResultList.Count; ++index)
            {
                var flag = true;
                foreach (object obj in results1)
                    if (obj.ToString().Equals(m_resultList[index].Value))
                    {
                        flag = false;
                        break;
                    }

                if (flag)
                    ResultList.RemoveAt(index);
            }

            foreach (var programResult in results1)
            {
                var flag = true;
                for (var index = 0; index < ResultList.Count; ++index)
                    if (programResult.ToString().Equals(ResultList[index].Value))
                    {
                        flag = false;
                        break;
                    }

                if (flag)
                    ResultList.Add(new StringWrapper(programResult.ToString()));
            }

            return true;
        }

        public static void RefreshSymbolList()
        {
            var jobId = JobComboBox.Instance.GetJobID();
            if (string.IsNullOrEmpty(jobId) ||
                !(JobComboBox.Instance.SelectedItem as HardwareJobWrapper).IsDisplayable())
                return;
            var job = (HardwareJob)null;
            ProfileManager.Instance.Service.GetJob(jobId, out job);
            IDictionary<string, string> symbols1;
            var symbols2 = job.GetSymbols(out symbols1);
            if (!symbols2.Success)
            {
                CommonFunctions.ProcessCommandResultError(symbols2);
            }
            else
            {
                for (var index = 0; index < SymbolList.Count; ++index)
                {
                    var flag = true;
                    foreach (var keyValuePair in symbols1)
                        if (keyValuePair.Key.Equals(SymbolList[index].Name))
                        {
                            if (!keyValuePair.Value.Equals(SymbolList[index].Value))
                                SymbolList[index].Value = keyValuePair.Value;
                            flag = false;
                            break;
                        }

                    if (flag)
                        m_symbolList.RemoveAt(index);
                }

                foreach (var keyValuePair in symbols1)
                {
                    var flag = true;
                    for (var index = 0; index < SymbolList.Count; ++index)
                        if (keyValuePair.Key.Equals(SymbolList[index].Name))
                        {
                            flag = false;
                            break;
                        }

                    if (flag)
                        SymbolList.Add(new Symbol(keyValuePair.Key, keyValuePair.Value));
                }
            }
        }

        public static bool RefreshStackList()
        {
            if (!(JobComboBox.Instance.SelectedItem is HardwareJobWrapper selectedItem) ||
                !selectedItem.IsDisplayable())
                return false;
            var stack1 = new Stack<string>();
            var stack2 = selectedItem.GetStack(out stack1);
            if (!stack2.Success)
            {
                CommonFunctions.ProcessCommandResultError(stack2);
                OutputWindow.Instance.Append("ERROR: Unable to get stack");
                return false;
            }

            while (StackList.Count > stack1.Count)
                StackList.RemoveAt(stack1.Count);
            for (var index = 0; index < StackList.Count; ++index)
            {
                var str = stack1.Pop();
                if (StackList[index].Value != str)
                    StackList[index].Value = str;
            }

            while (stack1.Count > 0)
                StackList.Add(new StringWrapper(stack1.Pop()));
            return true;
        }

        public static void ViewErrors(List<HardwareJobWrapper> jobs)
        {
            if (jobs == null)
                return;
            ErrorListView.Instance.Clear();
            var num1 = 0;
            foreach (var job in jobs)
                if (job != null && job.IsDisplayable())
                {
                    ErrorList errors1;
                    var errors2 = job.GetErrors(out errors1);
                    if (!errors2.Success)
                    {
                        CommonFunctions.ProcessCommandResultError(errors2);
                    }
                    else if (errors1 != null && errors1.Count > 0)
                    {
                        EnvironmentHelper.DisplayErrors(errors1, false, false);
                        num1 += errors1.Count;
                    }
                }

            if (num1 != 0)
                return;
            var num2 = (int)MessageBox.Show("No Errors To Display", "Hal Management Console", MessageBoxButtons.OK,
                MessageBoxIcon.Asterisk);
        }

        public static void Connect(HardwareJobWrapper job)
        {
            if (job == null || !job.IsDisplayable())
                return;
            var result = job.Connect();
            CommonFunctions.ProcessCommandResult(result);
            if (!result.Success)
                return;
            job.EventRaised += ProcessJobEvent;
        }

        public static void ConnectJobs(List<HardwareJobWrapper> jobs)
        {
            foreach (var job in jobs)
                Connect(job);
        }

        public static void Disconnect(HardwareJobWrapper job)
        {
            if (job == null || !job.IsDisplayable())
                return;
            job.EventRaised -= ProcessJobEvent;
            job.Disconnect();
        }

        public static void DisconnectJobs(List<HardwareJobWrapper> jobs)
        {
            foreach (var job in jobs)
                Disconnect(job);
        }

        public static void SetLabel(HardwareJobWrapper job, string value)
        {
            if (job == null || !job.IsDisplayable() || IsCompleted(job))
                return;
            CommonFunctions.ProcessCommandResult(job.SetLabel(value));
        }

        public static void SetLabels(List<HardwareJobWrapper> jobs)
        {
            var inputBoxForm1 = new InputBoxForm();
            inputBoxForm1.Text = "Set Job Label";
            inputBoxForm1.Label = "Enter the label for this job:";
            var inputBoxForm2 = inputBoxForm1;
            if (inputBoxForm2.ShowDialog() == DialogResult.Cancel)
                return;
            foreach (var job in jobs)
                SetLabel(job, inputBoxForm2.Value);
        }

        public static void SetPriority(HardwareJobWrapper job, HardwareJobPriority priority)
        {
            if (job == null || IsCompleted(job))
                return;
            CommonFunctions.ProcessCommandResult(job.SetPriority(priority));
        }

        public static void SetPriorities(List<HardwareJobWrapper> jobs, HardwareJobPriority priority)
        {
            foreach (var job in jobs)
                SetPriority(job, priority);
        }

        public static void SetStartTime(HardwareJobWrapper job, DateTime value)
        {
            if (job == null && !job.IsDisplayable() && IsCompleted(job))
                return;
            CommonFunctions.ProcessCommandResult(job.SetStartTime(value));
        }

        public static void SetStartTimes(List<HardwareJobWrapper> jobs)
        {
            var startTimeForm = new StartTimeForm();
            if (startTimeForm.ShowDialog() == DialogResult.Cancel || jobs == null)
                return;
            foreach (var job in jobs)
                SetStartTime(job, startTimeForm.StartTime.Value);
        }

        public static void ResumeJob(HardwareJobWrapper job)
        {
            if (job == null || !job.IsDisplayable() || IsCompleted(job))
                return;
            CommonFunctions.ProcessCommandResult(job.Resume());
        }

        public static void ResumeJobs(List<HardwareJobWrapper> jobs)
        {
            foreach (var job in jobs)
                ResumeJob(job);
        }

        public static void SuspendJob(HardwareJobWrapper job)
        {
            if (job == null || !job.IsDisplayable())
                return;
            CommonFunctions.ProcessCommandResult(job.Suspend());
        }

        public static void SuspendJobs(List<HardwareJobWrapper> jobs)
        {
            foreach (var job in jobs)
                SuspendJob(job);
        }

        public static void TerminateJob(HardwareJobWrapper job)
        {
            if (job == null || !job.IsDisplayable())
                return;
            CommonFunctions.ProcessCommandResult(job.Terminate());
        }

        public static void TerminateJobs(List<HardwareJobWrapper> jobs)
        {
            foreach (var job in jobs)
                TerminateJob(job);
        }

        public static void TrashJob(HardwareJobWrapper job)
        {
            if (job == null || !job.IsDisplayable() || job.ID == "IMMEDIATE" ||
                (EnvironmentHelper.IsLocked && IsSecureJob(job)))
                return;
            CommonFunctions.ProcessCommandResult(job.Trash());
        }

        public static void TrashJobs(List<HardwareJobWrapper> jobs)
        {
            foreach (var job in jobs)
                TrashJob(job);
        }

        public static void CollectGarbage(bool force)
        {
            CommonFunctions.ProcessCommandResult(ProfileManager.Instance.Service.CollectGarbage(force));
        }

        public static void UpdateButtons(
            ToolStripItemCollection collection,
            List<HardwareJobWrapper> list)
        {
            var toolStripItem1 = collection[JobItems.Garbage.ToString()];
            var toolStripItem2 = collection[JobItems.ViewErrors.ToString()];
            var toolStripItem3 = collection[JobItems.Connect.ToString()];
            var toolStripItem4 = collection[JobItems.Disconnect.ToString()];
            var toolStripItem5 = collection[JobItems.SetLabel.ToString()];
            var toolStripItem6 = collection[JobItems.SetStartTime.ToString()];
            var toolStripItem7 = collection[JobItems.Resume.ToString()];
            var toolStripItem8 = collection[JobItems.Suspend.ToString()];
            var toolStripItem9 = collection[JobItems.Stop.ToString()];
            var toolStripItem10 = collection[JobItems.Trash.ToString()];
            var toolStripItem11 = collection[JobItems.TrashLocked.ToString()];
            var toolStripItem12 = collection[JobItems.Priority.ToString()];
            toolStripItem1.Enabled = false;
            toolStripItem2.Enabled = false;
            toolStripItem3.Enabled = false;
            toolStripItem4.Enabled = false;
            toolStripItem5.Enabled = false;
            toolStripItem6.Enabled = false;
            toolStripItem7.Enabled = false;
            toolStripItem8.Enabled = false;
            toolStripItem9.Enabled = false;
            if (toolStripItem12 != null)
                toolStripItem12.Enabled = false;
            if (toolStripItem10 != null)
                toolStripItem10.Enabled = false;
            if (toolStripItem11 != null)
                toolStripItem11.Enabled = false;
            if (!ProfileManager.Instance.IsConnected)
                return;
            toolStripItem1.Enabled = true;
            if (list == null || list.Count < 1)
                return;
            foreach (var job in list)
                if (job != null && job.IsDisplayable())
                {
                    if (toolStripItem12 != null)
                        toolStripItem12.Enabled = true;
                    toolStripItem2.Enabled = true;
                    if (job.ConnectionState == HardwareJobConnectionState.Disconnected)
                        toolStripItem3.Enabled = true;
                    if (job.ConnectionState == HardwareJobConnectionState.Connected)
                        toolStripItem4.Enabled = true;
                    if (job.Status == HardwareJobStatus.Pending || job.Status == HardwareJobStatus.Running ||
                        job.Status == HardwareJobStatus.Suspended)
                    {
                        toolStripItem5.Enabled = true;
                        toolStripItem9.Enabled = true;
                        if (job.Status != HardwareJobStatus.Running)
                            toolStripItem6.Enabled = true;
                    }

                    if (job.Status != HardwareJobStatus.Garbage && job.ID != "IMMEDIATE" && !IsSecureJob(job))
                    {
                        if (toolStripItem10 != null)
                            toolStripItem10.Enabled = true;
                        if (toolStripItem11 != null && !EnvironmentHelper.IsLocked)
                            toolStripItem11.Enabled = true;
                    }

                    if (job.Status == HardwareJobStatus.Suspended)
                        toolStripItem7.Enabled = true;
                    if (job.Status == HardwareJobStatus.Running || job.Status == HardwareJobStatus.Pending)
                        toolStripItem8.Enabled = true;
                }
        }

        public static void OnConnect(object sender, EventArgs e)
        {
            var jobs = ((sender as ToolStripItem).Owner as IJobStrip).UpdateSource();
            if (jobs == null)
                return;
            ConnectJobs(jobs);
        }

        public static void OnDisconnect(object sender, EventArgs e)
        {
            var jobs = ((sender as ToolStripItem).Owner as IJobStrip).UpdateSource();
            if (jobs == null)
                return;
            DisconnectJobs(jobs);
        }

        public static void OnGarbage(object sender, EventArgs e)
        {
            CollectGarbage(false);
        }

        public static void OnResume(object sender, EventArgs e)
        {
            var jobs = ((sender as ToolStripItem).Owner as IJobStrip).UpdateSource();
            if (jobs == null)
                return;
            ResumeJobs(jobs);
        }

        public static void OnSuspend(object sender, EventArgs e)
        {
            var jobs = ((sender as ToolStripItem).Owner as IJobStrip).UpdateSource();
            if (jobs == null)
                return;
            SuspendJobs(jobs);
        }

        public static void OnTerminate(object sender, EventArgs e)
        {
            var jobs = ((sender as ToolStripItem).Owner as IJobStrip).UpdateSource();
            if (jobs == null)
                return;
            TerminateJobs(jobs);
        }

        public static void OnTrash(object sender, EventArgs e)
        {
            var jobs = ((sender as ToolStripItem).Owner as IJobStrip).UpdateSource();
            if (jobs == null)
                return;
            TrashJobs(jobs);
        }

        public static void OnSetLabel(object sender, EventArgs e)
        {
            var jobs = ((sender as ToolStripItem).Owner as IJobStrip).UpdateSource();
            if (jobs == null)
                return;
            SetLabels(jobs);
        }

        public static void OnSetStartTimes(object sender, EventArgs e)
        {
            var jobs = ((sender as ToolStripItem).Owner as IJobStrip).UpdateSource();
            if (jobs == null)
                return;
            SetStartTimes(jobs);
        }

        public static void OnViewErrors(object sender, EventArgs e)
        {
            var jobs = ((sender as ToolStripItem).Owner as IJobStrip).UpdateSource();
            if (jobs == null)
                return;
            ViewErrors(jobs);
        }

        private static void ExecuteCommand()
        {
        }

        private static bool IsSecureJob(HardwareJobWrapper job)
        {
            if (!EnvironmentHelper.IsLocked)
                return false;
            if (m_secureJobs == null)
            {
                m_secureJobs = new List<string>();
                var secureJob = Settings.Default.SecureJob;
                m_secureJobs.AddRange(secureJob.Split(','));
            }

            return m_secureJobs.Find(x => x == job.ProgramName) != null;
        }

        private static bool IsCompleted(HardwareJobWrapper job)
        {
            return job.Status == HardwareJobStatus.Completed || job.Status == HardwareJobStatus.Errored ||
                   job.Status == HardwareJobStatus.Garbage || job.Status == HardwareJobStatus.Stopped;
        }
    }
}