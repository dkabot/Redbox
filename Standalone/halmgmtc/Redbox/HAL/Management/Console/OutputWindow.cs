using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console
{
    public class OutputWindow : UserControl
    {
        private static OutputWindow m_instance;
        private IContainer components;
        private ListBox m_outputListBox;

        private OutputWindow()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            AutoScroll = true;
        }

        public static OutputWindow Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new OutputWindow();
                return m_instance;
            }
        }

        public void Clear()
        {
            m_outputListBox.Items.Clear();
        }

        public void Append(string s)
        {
            if (string.IsNullOrEmpty(s))
                return;
            m_outputListBox.Items.Add(s);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_outputListBox = new ListBox();
            SuspendLayout();
            m_outputListBox.BorderStyle = BorderStyle.FixedSingle;
            m_outputListBox.Dock = DockStyle.Fill;
            m_outputListBox.FormattingEnabled = true;
            m_outputListBox.Location = new Point(0, 0);
            m_outputListBox.Name = "m_outputListBox";
            m_outputListBox.Size = new Size(217, 145);
            m_outputListBox.TabIndex = 0;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            BorderStyle = BorderStyle.Fixed3D;
            Controls.Add(m_outputListBox);
            Name = nameof(OutputWindow);
            Size = new Size(217, 156);
            ResumeLayout(false);
        }
    }
}