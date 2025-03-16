using Redbox.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class TaskManagerForm : Form
    {
        public List<string> ProcessNamesToIgnore;
        private bool m_isRefreshing;
        private IContainer components;
        private ListView m_taskListView;
        private Panel m_buttonPanel;
        private ColumnHeader m_nameColumnHeader;
        private ColumnHeader m_pidColumnHeader;
        private ColumnHeader m_memoryColumnHeader;
        private Button m_killButton;
        private Button m_refreshButton;

        public TaskManagerForm()
        {
            InitializeComponent();
        }

        public void RefreshList()
        {
            if (m_isRefreshing)
                return;
            m_isRefreshing = true;
            var itemIndex = -1;
            if (m_taskListView.SelectedIndices.Count > 0)
                itemIndex = m_taskListView.SelectedIndices[0];
            m_taskListView.SuspendLayout();
            m_taskListView.BeginUpdate();
            m_taskListView.Items.Clear();
            foreach (var process in Process.GetProcesses())
                if (ProcessNamesToIgnore == null ||
                    (ProcessNamesToIgnore != null && !ProcessNamesToIgnore.Contains(process.ProcessName)))
                    m_taskListView.Items.Add(new ListViewItem(process.ProcessName)
                    {
                        SubItems =
                        {
                            process.Id.ToString(),
                            string.Format("{0:#,###}K", (object)(process.WorkingSet64 / 1000L))
                        },
                        Tag = (object)process
                    });
            m_taskListView.EndUpdate();
            m_taskListView.ResumeLayout();
            if (itemIndex != -1)
                m_taskListView.SelectedIndices.Add(itemIndex);
            m_isRefreshing = false;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList<Process> SelectedProcesses
        {
            get
            {
                var selectedProcesses = new List<Process>();
                foreach (ListViewItem selectedItem in m_taskListView.SelectedItems)
                    selectedProcesses.Add((Process)selectedItem.Tag);
                return (IList<Process>)selectedProcesses;
            }
        }

        public event EventHandler Kill;

        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Show();
            RefreshList();
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            Cursor.Hide();
        }

        private void OnSelectedTaskChanged(object sender, EventArgs e)
        {
            m_killButton.Enabled = m_taskListView.SelectedItems.Count > 0;
        }

        private void OnKill(object sender, EventArgs e)
        {
            if (Kill == null)
                return;
            try
            {
                Kill(sender, e);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in TaskManagerForm.OnKill.", ex);
            }
            finally
            {
                RefreshList();
            }
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            RefreshList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_taskListView = new ListView();
            m_nameColumnHeader = new ColumnHeader();
            m_pidColumnHeader = new ColumnHeader();
            m_memoryColumnHeader = new ColumnHeader();
            m_buttonPanel = new Panel();
            m_refreshButton = new Button();
            m_killButton = new Button();
            m_buttonPanel.SuspendLayout();
            SuspendLayout();
            m_taskListView.Columns.AddRange(new ColumnHeader[3]
            {
                m_nameColumnHeader,
                m_pidColumnHeader,
                m_memoryColumnHeader
            });
            m_taskListView.Dock = DockStyle.Fill;
            m_taskListView.FullRowSelect = true;
            m_taskListView.Location = new Point(0, 0);
            m_taskListView.Name = "m_taskListView";
            m_taskListView.Size = new Size(431, 324);
            m_taskListView.TabIndex = 0;
            m_taskListView.UseCompatibleStateImageBehavior = false;
            m_taskListView.View = View.Details;
            m_taskListView.SelectedIndexChanged += new EventHandler(OnSelectedTaskChanged);
            m_nameColumnHeader.Text = "Name";
            m_nameColumnHeader.Width = 200;
            m_pidColumnHeader.Text = "PID";
            m_pidColumnHeader.Width = 75;
            m_memoryColumnHeader.Text = "Memory";
            m_memoryColumnHeader.TextAlign = HorizontalAlignment.Right;
            m_memoryColumnHeader.Width = 95;
            m_buttonPanel.Controls.Add((Control)m_refreshButton);
            m_buttonPanel.Controls.Add((Control)m_killButton);
            m_buttonPanel.Dock = DockStyle.Bottom;
            m_buttonPanel.Location = new Point(0, 324);
            m_buttonPanel.Name = "m_buttonPanel";
            m_buttonPanel.Size = new Size(431, 65);
            m_buttonPanel.TabIndex = 1;
            m_refreshButton.Location = new Point(12, 13);
            m_refreshButton.Name = "m_refreshButton";
            m_refreshButton.Size = new Size(193, 40);
            m_refreshButton.TabIndex = 0;
            m_refreshButton.Text = "Refresh";
            m_refreshButton.UseVisualStyleBackColor = true;
            m_refreshButton.Click += new EventHandler(OnRefresh);
            m_killButton.Enabled = false;
            m_killButton.Location = new Point(226, 13);
            m_killButton.Name = "m_killButton";
            m_killButton.Size = new Size(193, 40);
            m_killButton.TabIndex = 1;
            m_killButton.Text = "Kill";
            m_killButton.UseVisualStyleBackColor = true;
            m_killButton.Click += new EventHandler(OnKill);
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(431, 389);
            Controls.Add((Control)m_taskListView);
            Controls.Add((Control)m_buttonPanel);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = nameof(TaskManagerForm);
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Task Manager";
            TopMost = true;
            Load += new EventHandler(OnLoad);
            FormClosed += new FormClosedEventHandler(OnFormClosed);
            m_buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}