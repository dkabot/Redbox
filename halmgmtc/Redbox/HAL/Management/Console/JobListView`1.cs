using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Management.Console.Properties;

namespace Redbox.HAL.Management.Console
{
    public class JobListView<T> : UserControl
    {
        private readonly DataGridView dataGridView1;
        private IContainer components;
        private Timer m_timer;
        private Panel panel1;
        private ToolStrip toolStrip1;

        public JobListView(string[,] headers, BindingList<T> data)
        {
            InitializeComponent();
            Enabled = false;
            SuspendLayout();
            dataGridView1 = new AutomatedDataGridView<T>(headers, data, true);
            dataGridView1.AutoSize = true;
            panel1.Controls.Add(dataGridView1);
            Dock = DockStyle.Fill;
            dataGridView1.SelectionChanged += (toolStrip1 as JobToolStrip).UpdateButtons;
            dataGridView1.DataBindingComplete += (toolStrip1 as JobToolStrip).UpdateButtons;
            CreatePriorityDropDown();
            ResumeLayout();
            var justConnected = false;
            ProfileManager.Instance.Connected += (_param1, _param2) =>
            {
                Enabled = true;
                m_timer.Enabled = true;
                justConnected = true;
            };
            ProfileManager.Instance.Disconnected += (_param1, _param2) =>
            {
                Enabled = false;
                m_timer.Enabled = false;
                JobHelper.JobList.Clear();
            };
            dataGridView1.DataBindingComplete += (_param1, _param2) =>
            {
                if (!justConnected || dataGridView1.Rows.Count <= 1)
                    return;
                dataGridView1.Rows[0].Selected = false;
                justConnected = false;
            };
        }

        public List<HardwareJobWrapper> GetSelected()
        {
            var selected = new List<HardwareJobWrapper>();
            foreach (DataGridViewRow selectedRow in dataGridView1.SelectedRows)
                selected.Add(selectedRow.DataBoundItem as HardwareJobWrapper);
            return selected;
        }

        internal void RefreshData()
        {
            JobHelper.RefreshJobList();
            JobHelper.UpdateButtons(toolStrip1.Items, GetSelected());
        }

        private void OnTimer(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void CreatePriorityDropDown()
        {
            var stripDropDownButton = new ToolStripDropDownButton();
            foreach (var keyValuePair in new Dictionary<HardwareJobPriority, string>
                     {
                         {
                             HardwareJobPriority.Lowest,
                             "Lowest"
                         },
                         {
                             HardwareJobPriority.Low,
                             "Low"
                         },
                         {
                             HardwareJobPriority.BelowNormal,
                             "Below Normal"
                         },
                         {
                             HardwareJobPriority.Normal,
                             "Normal"
                         },
                         {
                             HardwareJobPriority.AboveNormal,
                             "Above Normal"
                         },
                         {
                             HardwareJobPriority.High,
                             "High"
                         },
                         {
                             HardwareJobPriority.Highest,
                             "Highest"
                         }
                     })
            {
                var toolStripButton = new ToolStripButton();
                toolStripButton.Tag = keyValuePair.Key;
                toolStripButton.Text = keyValuePair.Value;
                toolStripButton.Click += OnPriorityChanged;
                stripDropDownButton.DropDownItems.Add(toolStripButton);
            }

            stripDropDownButton.Name = JobItems.Priority.ToString();
            stripDropDownButton.ToolTipText = "Priority";
            stripDropDownButton.Image = Resources.flag;
            stripDropDownButton.Tag = HardwareJobPriority.Normal;
            stripDropDownButton.Text = "Priority";
            stripDropDownButton.DropDown.Width = 150;
            toolStrip1.Items.Add(stripDropDownButton);
        }

        private void OnPriorityChanged(object sender, EventArgs e)
        {
            if (!(sender is ToolStripItem toolStripItem) || toolStripItem.Tag == null)
                return;
            foreach (var job in GetSelected())
                JobHelper.SetPriority(job, (HardwareJobPriority)toolStripItem.Tag);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            toolStrip1 = new JobToolStrip(GetSelected, UpdateSource.JobListView, false);
            m_timer = new Timer();
            panel1 = new Panel();
            SuspendLayout();
            m_timer.Interval = 1000;
            m_timer.Tick += OnTimer;
            m_timer.Enabled = false;
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Location = new Point(0, 25);
            panel1.Name = "panel1";
            panel1.Size = new Size(409, 382);
            panel1.TabIndex = 1;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(toolStrip1);
            Controls.Add(panel1);
            Name = "JLV";
            Size = new Size(410, 411);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}