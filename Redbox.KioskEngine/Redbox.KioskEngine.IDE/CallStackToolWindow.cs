using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE
{
    public class CallStackToolWindow : ToolWindow
    {
        private IContainer components;
        private ToolStrip m_toolStrip;
        private ToolStripButton m_refreshToolStripButton;
        private ListView m_listView;
        private ColumnHeader m_functionNameColumnHeader;
        private ColumnHeader m_scriptNameColumnHeader;
        private ColumnHeader m_lineNumberColumnHeader;

        public CallStackToolWindow()
        {
            InitializeComponent();
        }

        public void RefreshCallStack()
        {
            InnerRefreshCallStack();
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            InnerRefreshCallStack();
        }

        private void InnerRefreshCallStack()
        {
            var callStack = ((LuaDebugger)ServiceLocator.Instance.GetService<IDebugService>().DebuggerInstance)
                .GetCallStack();
            m_listView.BeginUpdate();
            m_listView.Items.Clear();
            foreach (var callStackEntry in callStack)
                m_listView.Items.Add(new ListViewItem(callStackEntry.FunctionName)
                {
                    SubItems =
                    {
                        callStackEntry.FileName,
                        callStackEntry.Line.ToString()
                    }
                });
            m_listView.EndUpdate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            var componentResourceManager = new ComponentResourceManager(typeof(CallStackToolWindow));
            m_toolStrip = new ToolStrip();
            m_refreshToolStripButton = new ToolStripButton();
            m_listView = new ListView();
            m_functionNameColumnHeader = new ColumnHeader();
            m_scriptNameColumnHeader = new ColumnHeader();
            m_lineNumberColumnHeader = new ColumnHeader();
            m_toolStrip.SuspendLayout();
            SuspendLayout();
            m_toolStrip.Items.AddRange(new ToolStripItem[1]
            {
                (ToolStripItem)m_refreshToolStripButton
            });
            m_toolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
            m_toolStrip.Location = new Point(0, 0);
            m_toolStrip.Name = "m_toolStrip";
            m_toolStrip.Size = new Size(610, 23);
            m_toolStrip.TabIndex = 0;
            m_toolStrip.Text = "toolStrip1";
            m_refreshToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_refreshToolStripButton.Image = (Image)Properties.Resources.Refresh;
            m_refreshToolStripButton.ImageTransparentColor = Color.Magenta;
            m_refreshToolStripButton.Name = "m_refreshToolStripButton";
            m_refreshToolStripButton.Size = new Size(23, 20);
            m_refreshToolStripButton.Click += new EventHandler(OnRefresh);
            m_listView.Columns.AddRange(new ColumnHeader[3]
            {
                m_functionNameColumnHeader,
                m_scriptNameColumnHeader,
                m_lineNumberColumnHeader
            });
            m_listView.Dock = DockStyle.Fill;
            m_listView.Location = new Point(0, 23);
            m_listView.Name = "m_listView";
            m_listView.Size = new Size(610, 252);
            m_listView.TabIndex = 1;
            m_listView.UseCompatibleStateImageBehavior = false;
            m_listView.View = View.Details;
            m_functionNameColumnHeader.Text = "Function Name";
            m_functionNameColumnHeader.Width = 200;
            m_scriptNameColumnHeader.Text = "Script";
            m_scriptNameColumnHeader.Width = 200;
            m_lineNumberColumnHeader.Text = "Line #";
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(610, 275);
            Controls.Add((Control)m_listView);
            Controls.Add((Control)m_toolStrip);
            Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            HideOnClose = true;
            Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            Name = nameof(CallStackToolWindow);
            ShowHint = DockState.DockBottom;
            Text = "Call Stack";
            m_toolStrip.ResumeLayout(false);
            m_toolStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}