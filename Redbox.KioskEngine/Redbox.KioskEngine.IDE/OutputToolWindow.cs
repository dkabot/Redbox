using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE
{
    public class OutputToolWindow : ToolWindow
    {
        private IContainer components;
        private ToolStrip m_toolStrip;
        private ToolStripLabel toolStripLabel1;
        private ToolStripComboBox m_outputSourceToolStripComboBox;
        private ToolStripSeparator toolStripSeparator1;
        private RichTextBox m_richTextBox;

        public OutputToolWindow()
        {
            InitializeComponent();
        }

        public void UpdateText(string text)
        {
            if (m_richTextBox == null || m_richTextBox.IsDisposed)
                return;
            if (m_richTextBox.InvokeRequired)
            {
                m_richTextBox.Invoke((Action)(() =>
                {
                    m_richTextBox.AppendText("\n" + DateTime.Now.ToString("yyyy-dd-MM hh:mm:ss,fff : ") + text);
                    m_richTextBox.ScrollToCaret();
                }));
            }
            else
            {
                m_richTextBox.AppendText("\n" + DateTime.Now.ToString("yyyy-dd-MM hh:mm:ss,fff : ") + text);
                m_richTextBox.ScrollToCaret();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            var componentResourceManager = new ComponentResourceManager(typeof(OutputToolWindow));
            m_toolStrip = new ToolStrip();
            toolStripLabel1 = new ToolStripLabel();
            m_outputSourceToolStripComboBox = new ToolStripComboBox();
            toolStripSeparator1 = new ToolStripSeparator();
            m_richTextBox = new RichTextBox();
            m_toolStrip.SuspendLayout();
            SuspendLayout();
            m_toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            m_toolStrip.Items.AddRange(new ToolStripItem[3]
            {
                (ToolStripItem)toolStripLabel1,
                (ToolStripItem)m_outputSourceToolStripComboBox,
                (ToolStripItem)toolStripSeparator1
            });
            m_toolStrip.Location = new Point(0, 0);
            m_toolStrip.Name = "m_toolStrip";
            m_toolStrip.Size = new Size(515, 25);
            m_toolStrip.TabIndex = 0;
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(107, 22);
            toolStripLabel1.Text = "Show output from:";
            m_outputSourceToolStripComboBox.Name = "m_outputSourceToolStripComboBox";
            m_outputSourceToolStripComboBox.Size = new Size(121, 25);
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            m_richTextBox.Dock = DockStyle.Fill;
            m_richTextBox.Location = new Point(0, 25);
            m_richTextBox.Name = "m_richTextBox";
            m_richTextBox.Size = new Size(515, 221);
            m_richTextBox.TabIndex = 1;
            m_richTextBox.Text = "";
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(515, 246);
            Controls.Add((Control)m_richTextBox);
            Controls.Add((Control)m_toolStrip);
            Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            HideOnClose = true;
            Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            Name = nameof(OutputToolWindow);
            ShowHint = DockState.DockBottom;
            Text = "Output";
            m_toolStrip.ResumeLayout(false);
            m_toolStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}