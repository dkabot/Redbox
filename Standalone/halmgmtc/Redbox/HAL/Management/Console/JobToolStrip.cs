using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console
{
    public class JobToolStrip : ToolStrip, IJobStrip
    {
        private readonly Func<List<HardwareJobWrapper>> m_updateJobSource;
        private IContainer components;

        public JobToolStrip(
            Func<List<HardwareJobWrapper>> UpdateJobSource,
            UpdateSource source,
            bool trashLocked)
        {
            m_updateJobSource = UpdateJobSource;
            InitializeComponent();
            var toolStripLabel = new ToolStripLabel();
            Tag = source;
            Items.AddRange(new ToolStripItem[12]
            {
                JobItem.Instance.MakeJobToolStripButton(JobItems.Garbage, false),
                JobItem.Instance.MakeJobToolStripButton(JobItems.ViewErrors, false),
                new ToolStripSeparator(),
                JobItem.Instance.MakeJobToolStripButton(JobItems.Connect, false),
                JobItem.Instance.MakeJobToolStripButton(JobItems.Disconnect, false),
                new ToolStripSeparator(),
                JobItem.Instance.MakeJobToolStripButton(JobItems.SetLabel, false),
                JobItem.Instance.MakeJobToolStripButton(JobItems.SetStartTime, false),
                new ToolStripSeparator(),
                JobItem.Instance.MakeJobToolStripButton(JobItems.Resume, false),
                JobItem.Instance.MakeJobToolStripButton(JobItems.Suspend, false),
                JobItem.Instance.MakeJobToolStripButton(JobItems.Stop, false)
            });
            if (trashLocked)
            {
                Items.Add(JobItem.Instance.MakeJobToolStripButton(JobItems.TrashLocked, trashLocked));
                ProfileManager.Instance.Connected +=
                    (_param1, _param2) => JobHelper.UpdateButtons(Items, UpdateSource());
                ProfileManager.Instance.Disconnected +=
                    (_param1, _param2) => JobHelper.UpdateButtons(Items, UpdateSource());
                EnvironmentHelper.LockStatusChanged +=
                    (_param1, _param2) => JobHelper.UpdateButtons(Items, UpdateSource());
            }
            else
            {
                Items.Add(JobItem.Instance.MakeJobToolStripButton(JobItems.Trash, trashLocked));
            }
        }

        public List<HardwareJobWrapper> UpdateSource()
        {
            return m_updateJobSource();
        }

        public void UpdateButtons(object sender, EventArgs e)
        {
            JobHelper.UpdateButtons(Items, m_updateJobSource());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();
        }
    }
}