using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Redbox.UpdateManager.StoreInstallerFrontEnd
{
    public class Host : Form
    {
        private readonly Dictionary<string, EventPanel> m_eventDict = new Dictionary<string, EventPanel>();
        private IContainer components;
        private FlowLayoutPanel m_events;
        private OpenFileDialog m_fileDialog;

        public Host() => this.InitializeComponent();

        public FileInfo ShowFileDialog()
        {
            return this.m_fileDialog.ShowDialog() != DialogResult.OK ? (FileInfo)null : new FileInfo(this.m_fileDialog.FileName);
        }

        public void AddEvent(string name, string description)
        {
            EventPanel eventPanel = new EventPanel(name, description);
            this.m_events.Controls.Add((Control)eventPanel);
            this.m_eventDict.Add(name, eventPanel);
        }

        public void EventStart(string name)
        {
            this.m_eventDict[name].Start();
            this.m_eventDict[name].Focus();
        }

        public void EventComplete(string name)
        {
            this.m_eventDict[name].Complete();
            this.m_eventDict[name].Focus();
        }

        public void EventError(string name, Error error)
        {
            this.m_eventDict[name].Error(error);
            this.m_eventDict[name].Focus();
        }

        public void Pending(string name)
        {
            this.m_eventDict[name].Pending();
            this.m_eventDict[name].Focus();
        }

        public void ErrorAll()
        {
            foreach (EventPanel eventPanel in this.m_eventDict.Values)
                eventPanel.Error(Error.NewError("E999", "A unrecoverable error occurred.", "Please contact support staff."));
        }

        public void ShowFinish() => Application.Exit();

        private void m_exit_Click(object sender, EventArgs e) => Application.Exit();

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(Host));
            this.m_events = new FlowLayoutPanel();
            this.m_fileDialog = new OpenFileDialog();
            this.SuspendLayout();
            this.m_events.AutoScroll = true;
            this.m_events.Dock = DockStyle.Fill;
            this.m_events.Location = new Point(0, 0);
            this.m_events.Name = "m_events";
            this.m_events.Size = new Size(538, 577);
            this.m_events.TabIndex = 0;
            this.m_fileDialog.AddExtension = false;
            this.m_fileDialog.FileName = "HAL.xml";
            this.m_fileDialog.InitialDirectory = "C:\\";
            this.m_fileDialog.SupportMultiDottedExtensions = true;
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(538, 577);
            this.Controls.Add((Control)this.m_events);
            this.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            this.MaximizeBox = false;
            this.Name = nameof(Host);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Store Installer";
            this.ResumeLayout(false);
        }
    }
}
