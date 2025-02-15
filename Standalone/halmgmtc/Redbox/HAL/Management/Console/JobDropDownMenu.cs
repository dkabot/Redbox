using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console
{
    public class JobDropDownMenu : ToolStripDropDownMenu, IJobStrip
    {
        private readonly Func<List<HardwareJobWrapper>> UpdateJobSource;
        private IContainer components;

        public JobDropDownMenu(
            Func<List<HardwareJobWrapper>> UpdateJobSource,
            UpdateSource source,
            bool trashLocked)
        {
            this.UpdateJobSource = UpdateJobSource;
            Name = "m_jobMenuItem";
            Text = "&Job";
            Tag = source;
            InitializeComponent();
            Items.AddRange(new ToolStripItem[12]
            {
                JobItem.Instance.MakeJobMenuItem(JobItems.Garbage, false),
                JobItem.Instance.MakeJobMenuItem(JobItems.ViewErrors, false),
                new ToolStripSeparator(),
                JobItem.Instance.MakeJobMenuItem(JobItems.Connect, false),
                JobItem.Instance.MakeJobMenuItem(JobItems.Disconnect, false),
                new ToolStripSeparator(),
                JobItem.Instance.MakeJobMenuItem(JobItems.SetLabel, false),
                JobItem.Instance.MakeJobMenuItem(JobItems.SetStartTime, false),
                new ToolStripSeparator(),
                JobItem.Instance.MakeJobMenuItem(JobItems.Resume, false),
                JobItem.Instance.MakeJobMenuItem(JobItems.Suspend, false),
                JobItem.Instance.MakeJobMenuItem(JobItems.Stop, false)
            });
            if (trashLocked)
            {
                Items.Add(JobItem.Instance.MakeJobMenuItem(JobItems.TrashLocked, true));
                ProfileManager.Instance.Connected +=
                    (_param1, _param2) => JobHelper.UpdateButtons(Items, UpdateSource());
                ProfileManager.Instance.Disconnected +=
                    (_param1, _param2) => JobHelper.UpdateButtons(Items, UpdateSource());
                EnvironmentHelper.LockStatusChanged +=
                    (_param1, _param2) => JobHelper.UpdateButtons(Items, UpdateSource());
            }
            else
            {
                Items.Add(JobItem.Instance.MakeJobMenuItem(JobItems.Trash, false));
            }
        }

        public List<HardwareJobWrapper> UpdateSource()
        {
            return UpdateJobSource();
        }

        public void UpdateButtons(object sender, EventArgs e)
        {
            JobHelper.UpdateButtons(Items, UpdateJobSource());
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