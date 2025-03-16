using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE
{
    public class TimersToolWindow : ToolWindow
    {
        private IContainer components;
        private ListView m_listView;
        private ColumnHeader m_nameColumnHeader;
        private ColumnHeader m_dueTimeColumnHeader;
        private ColumnHeader m_periodTimeColumnHeader;
        private ColumnHeader m_statusColumnHeader;
        private ToolStrip m_toolStrip;
        private ToolStripButton m_refreshToolStripButton;
        private ToolStripButton m_clearToolStripButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton m_startToolStripButton;
        private ToolStripButton m_stopToolStripButton;
        private ToolStripButton m_executeToolStripButton;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton m_removeToolStripButton;

        public TimersToolWindow()
        {
            InitializeComponent();
        }

        private void OnStart(object sender, EventArgs e)
        {
            if (m_listView.SelectedItems.Count == 0)
                return;
            foreach (ListViewItem selectedItem in m_listView.SelectedItems)
                if (selectedItem.Tag is ITimer tag)
                    tag.Enabled = true;
            RefreshList();
        }

        private void OnStop(object sender, EventArgs e)
        {
            if (m_listView.SelectedItems.Count == 0)
                return;
            foreach (ListViewItem selectedItem in m_listView.SelectedItems)
                if (selectedItem.Tag is ITimer tag)
                    tag.Enabled = false;
            RefreshList();
        }

        private void OnRemove(object sender, EventArgs e)
        {
            if (m_listView.SelectedItems.Count == 0)
                return;
            foreach (ListViewItem selectedItem in m_listView.SelectedItems)
                if (selectedItem.Tag is ITimer tag)
                    ServiceLocator.Instance.GetService<ITimerService>().RemoveTimer(tag.Name);
            RefreshList();
        }

        private void OnClear(object sender, EventArgs e)
        {
            ServiceLocator.Instance.GetService<ITimerService>().Reset();
        }

        private void OnSelectedTimerChanged(object sender, EventArgs e)
        {
            RefreshButtonState();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Show();
            RefreshList();
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void OnExecute(object sender, EventArgs e)
        {
            if (m_listView.SelectedItems.Count == 0)
                return;
            foreach (ListViewItem selectedItem in m_listView.SelectedItems)
                if (selectedItem.Tag is ITimer tag)
                    tag.RaiseFire();
        }

        private void RefreshList()
        {
            m_listView.BeginUpdate();
            m_listView.Items.Clear();
            foreach (var timer in ServiceLocator.Instance.GetService<ITimerService>().Timers)
            {
                var listViewItem = new ListViewItem(timer.Name)
                {
                    Tag = (object)timer
                };
                var subItems1 = listViewItem.SubItems;
                var nullable = timer.DueTime;
                var text1 = nullable.ToString();
                subItems1.Add(text1);
                var subItems2 = listViewItem.SubItems;
                nullable = timer.Period;
                var text2 = nullable.ToString();
                subItems2.Add(text2);
                listViewItem.SubItems.Add(timer.Enabled ? "Enabled" : "Disabled");
                m_listView.Items.Add(listViewItem);
            }

            m_listView.EndUpdate();
            RefreshButtonState();
        }

        private void RefreshButtonState()
        {
            m_startToolStripButton.Enabled = m_listView.SelectedItems.Count > 0;
            m_stopToolStripButton.Enabled = m_listView.SelectedItems.Count > 0;
            m_executeToolStripButton.Enabled = m_listView.SelectedItems.Count > 0;
            m_removeToolStripButton.Enabled = m_listView.SelectedItems.Count > 0;
            m_clearToolStripButton.Enabled = m_listView.Items.Count > 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            var componentResourceManager = new ComponentResourceManager(typeof(TimersToolWindow));
            m_listView = new ListView();
            m_nameColumnHeader = new ColumnHeader();
            m_dueTimeColumnHeader = new ColumnHeader();
            m_periodTimeColumnHeader = new ColumnHeader();
            m_statusColumnHeader = new ColumnHeader();
            m_toolStrip = new ToolStrip();
            m_refreshToolStripButton = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            m_startToolStripButton = new ToolStripButton();
            m_stopToolStripButton = new ToolStripButton();
            m_executeToolStripButton = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            m_clearToolStripButton = new ToolStripButton();
            m_removeToolStripButton = new ToolStripButton();
            m_toolStrip.SuspendLayout();
            SuspendLayout();
            m_listView.Columns.AddRange(new ColumnHeader[4]
            {
                m_nameColumnHeader,
                m_dueTimeColumnHeader,
                m_periodTimeColumnHeader,
                m_statusColumnHeader
            });
            m_listView.Dock = DockStyle.Fill;
            m_listView.FullRowSelect = true;
            m_listView.HideSelection = false;
            m_listView.Location = new Point(0, 23);
            m_listView.Name = "m_listView";
            m_listView.Size = new Size(630, 263);
            m_listView.TabIndex = 0;
            m_listView.UseCompatibleStateImageBehavior = false;
            m_listView.View = View.Details;
            m_listView.SelectedIndexChanged += new EventHandler(OnSelectedTimerChanged);
            m_nameColumnHeader.Text = "Name";
            m_nameColumnHeader.Width = 250;
            m_dueTimeColumnHeader.Text = "Due (ms)";
            m_dueTimeColumnHeader.Width = 105;
            m_periodTimeColumnHeader.Text = "Period (ms)";
            m_periodTimeColumnHeader.Width = 105;
            m_statusColumnHeader.Text = "Status";
            m_statusColumnHeader.Width = 95;
            m_toolStrip.CanOverflow = false;
            m_toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            m_toolStrip.Items.AddRange(new ToolStripItem[8]
            {
                (ToolStripItem)m_refreshToolStripButton,
                (ToolStripItem)toolStripSeparator1,
                (ToolStripItem)m_startToolStripButton,
                (ToolStripItem)m_stopToolStripButton,
                (ToolStripItem)m_executeToolStripButton,
                (ToolStripItem)toolStripSeparator2,
                (ToolStripItem)m_clearToolStripButton,
                (ToolStripItem)m_removeToolStripButton
            });
            m_toolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
            m_toolStrip.Location = new Point(0, 0);
            m_toolStrip.Name = "m_toolStrip";
            m_toolStrip.Size = new Size(630, 23);
            m_toolStrip.TabIndex = 8;
            m_refreshToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_refreshToolStripButton.Image = (Image)Properties.Resources.Refresh;
            m_refreshToolStripButton.ImageTransparentColor = Color.Magenta;
            m_refreshToolStripButton.Name = "m_refreshToolStripButton";
            m_refreshToolStripButton.Size = new Size(23, 20);
            m_refreshToolStripButton.ToolTipText = "Refresh";
            m_refreshToolStripButton.Click += new EventHandler(OnRefresh);
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 23);
            m_startToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_startToolStripButton.Enabled = false;
            m_startToolStripButton.Image = (Image)Properties.Resources.DebugPlay;
            m_startToolStripButton.ImageTransparentColor = Color.Magenta;
            m_startToolStripButton.Name = "m_startToolStripButton";
            m_startToolStripButton.Size = new Size(23, 20);
            m_startToolStripButton.ToolTipText = "Start";
            m_startToolStripButton.Click += new EventHandler(OnStart);
            m_stopToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_stopToolStripButton.Enabled = false;
            m_stopToolStripButton.Image = (Image)Properties.Resources.DebugStop;
            m_stopToolStripButton.ImageTransparentColor = Color.Magenta;
            m_stopToolStripButton.Name = "m_stopToolStripButton";
            m_stopToolStripButton.Size = new Size(23, 20);
            m_stopToolStripButton.ToolTipText = "Stop";
            m_stopToolStripButton.Click += new EventHandler(OnStop);
            m_executeToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_executeToolStripButton.Enabled = false;
            m_executeToolStripButton.Image = (Image)Properties.Resources.Execute;
            m_executeToolStripButton.ImageTransparentColor = Color.Magenta;
            m_executeToolStripButton.Name = "m_executeToolStripButton";
            m_executeToolStripButton.Size = new Size(23, 20);
            m_executeToolStripButton.ToolTipText = "Execute";
            m_executeToolStripButton.Click += new EventHandler(OnExecute);
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 23);
            m_clearToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_clearToolStripButton.Enabled = false;
            m_clearToolStripButton.Image = (Image)Properties.Resources.Clean;
            m_clearToolStripButton.ImageTransparentColor = Color.Magenta;
            m_clearToolStripButton.Name = "m_clearToolStripButton";
            m_clearToolStripButton.Size = new Size(23, 20);
            m_clearToolStripButton.ToolTipText = "Clear";
            m_clearToolStripButton.Click += new EventHandler(OnClear);
            m_removeToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_removeToolStripButton.Enabled = false;
            m_removeToolStripButton.Image = (Image)Properties.Resources.Delete;
            m_removeToolStripButton.ImageTransparentColor = Color.Magenta;
            m_removeToolStripButton.Name = "m_removeToolStripButton";
            m_removeToolStripButton.Size = new Size(23, 20);
            m_removeToolStripButton.ToolTipText = "Remove";
            m_removeToolStripButton.Click += new EventHandler(OnRemove);
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(630, 286);
            Controls.Add((Control)m_listView);
            Controls.Add((Control)m_toolStrip);
            DockAreas = DockAreas.Float | DockAreas.DockBottom;
            Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            HideOnClose = true;
            Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = nameof(TimersToolWindow);
            ShowHint = DockState.DockBottom;
            ShowIcon = false;
            ShowInTaskbar = false;
            Text = "Timers";
            Load += new EventHandler(OnLoad);
            m_toolStrip.ResumeLayout(false);
            m_toolStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}