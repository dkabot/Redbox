using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE
{
    public class ErrorListToolWindow : ToolWindow
    {
        private IContainer components;
        private ToolStrip m_toolStrip;
        private ToolStripButton m_errorsToolStripButton;
        private ToolStripButton m_warningsToolStripButton;
        private ToolStripButton m_messagesToolStripButton;
        private ListView m_listView;
        private ColumnHeader m_codeColumnHeader;
        private ColumnHeader m_descriptionColumnHeader;

        public ErrorListToolWindow()
        {
            InitializeComponent();
        }

        private void OnClickErrors(object sender, EventArgs e)
        {
            m_errorsToolStripButton.Checked = !m_errorsToolStripButton.Checked;
        }

        private void OnClickWarnings(object sender, EventArgs e)
        {
            m_warningsToolStripButton.Checked = !m_warningsToolStripButton.Checked;
        }

        private void OnClickMessages(object sender, EventArgs e)
        {
            m_messagesToolStripButton.Checked = !m_messagesToolStripButton.Checked;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            var componentResourceManager = new ComponentResourceManager(typeof(ErrorListToolWindow));
            m_toolStrip = new ToolStrip();
            m_errorsToolStripButton = new ToolStripButton();
            m_warningsToolStripButton = new ToolStripButton();
            m_messagesToolStripButton = new ToolStripButton();
            m_listView = new ListView();
            m_codeColumnHeader = new ColumnHeader();
            m_descriptionColumnHeader = new ColumnHeader();
            m_toolStrip.SuspendLayout();
            SuspendLayout();
            m_toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            m_toolStrip.Items.AddRange(new ToolStripItem[3]
            {
                (ToolStripItem)m_errorsToolStripButton,
                (ToolStripItem)m_warningsToolStripButton,
                (ToolStripItem)m_messagesToolStripButton
            });
            m_toolStrip.Location = new Point(0, 0);
            m_toolStrip.Name = "m_toolStrip";
            m_toolStrip.Size = new Size(589, 25);
            m_toolStrip.TabIndex = 0;
            m_toolStrip.Text = "toolStrip1";
            m_errorsToolStripButton.Image = (Image)Properties.Resources.Critical;
            m_errorsToolStripButton.ImageTransparentColor = Color.Magenta;
            m_errorsToolStripButton.Name = "m_errorsToolStripButton";
            m_errorsToolStripButton.Size = new Size(56, 22);
            m_errorsToolStripButton.Text = "Errors";
            m_errorsToolStripButton.Click += new EventHandler(OnClickErrors);
            m_warningsToolStripButton.Image = (Image)Properties.Resources.Warning;
            m_warningsToolStripButton.ImageTransparentColor = Color.Magenta;
            m_warningsToolStripButton.Name = "m_warningsToolStripButton";
            m_warningsToolStripButton.Size = new Size(72, 22);
            m_warningsToolStripButton.Text = "Warnings";
            m_warningsToolStripButton.Click += new EventHandler(OnClickWarnings);
            m_messagesToolStripButton.Image = (Image)Properties.Resources.Information;
            m_messagesToolStripButton.ImageTransparentColor = Color.Magenta;
            m_messagesToolStripButton.Name = "m_messagesToolStripButton";
            m_messagesToolStripButton.Size = new Size(74, 22);
            m_messagesToolStripButton.Text = "Messages";
            m_messagesToolStripButton.Click += new EventHandler(OnClickMessages);
            m_listView.Columns.AddRange(new ColumnHeader[2]
            {
                m_codeColumnHeader,
                m_descriptionColumnHeader
            });
            m_listView.Dock = DockStyle.Fill;
            m_listView.FullRowSelect = true;
            m_listView.GridLines = true;
            m_listView.Location = new Point(0, 25);
            m_listView.Name = "m_listView";
            m_listView.Size = new Size(589, 259);
            m_listView.TabIndex = 1;
            m_listView.UseCompatibleStateImageBehavior = false;
            m_listView.View = View.Details;
            m_codeColumnHeader.Text = "Code";
            m_codeColumnHeader.Width = 85;
            m_descriptionColumnHeader.Text = "Description";
            m_descriptionColumnHeader.Width = 600;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(589, 284);
            Controls.Add((Control)m_listView);
            Controls.Add((Control)m_toolStrip);
            Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            HideOnClose = true;
            Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            Name = nameof(ErrorListToolWindow);
            ShowHint = DockState.DockBottom;
            Text = "Error List";
            m_toolStrip.ResumeLayout(false);
            m_toolStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}