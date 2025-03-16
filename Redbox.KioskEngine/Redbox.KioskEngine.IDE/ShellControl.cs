using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class ShellControl : UserControl
    {
        private IContainer components;
        private ShellTextBox m_shellTextBox;

        public ShellControl()
        {
            InitializeComponent();
        }

        public Color ShellTextForeColor
        {
            get => m_shellTextBox == null ? Color.Green : m_shellTextBox.ForeColor;
            set
            {
                if (m_shellTextBox == null)
                    return;
                m_shellTextBox.ForeColor = value;
            }
        }

        public Color ShellTextBackColor
        {
            get => m_shellTextBox == null ? Color.Black : m_shellTextBox.BackColor;
            set
            {
                if (m_shellTextBox == null)
                    return;
                m_shellTextBox.BackColor = value;
            }
        }

        public Font ShellTextFont
        {
            get => m_shellTextBox == null ? new Font("Tahoma", 8f) : m_shellTextBox.Font;
            set
            {
                if (m_shellTextBox == null)
                    return;
                m_shellTextBox.Font = value;
            }
        }

        public void Clear()
        {
            m_shellTextBox.Reset();
        }

        public void WriteText(string text)
        {
            m_shellTextBox.WriteText(text);
        }

        public string[] GetCommandHistory()
        {
            return m_shellTextBox.GetCommandHistory();
        }

        public string Prompt
        {
            get => m_shellTextBox.Prompt;
            set => m_shellTextBox.Prompt = value;
        }

        public event EventCommandEntered CommandEntered;

        protected virtual void OnCommandEntered(string command)
        {
            if (CommandEntered == null)
                return;
            CommandEntered((object)command, new CommandEnteredEventArgs(command));
        }

        internal void FireCommandEntered(string command)
        {
            OnCommandEntered(command);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_shellTextBox = new ShellTextBox();
            SuspendLayout();
            m_shellTextBox.AcceptsReturn = true;
            m_shellTextBox.AcceptsTab = true;
            m_shellTextBox.Dock = DockStyle.Fill;
            m_shellTextBox.Location = new Point(0, 0);
            m_shellTextBox.MaxLength = 0;
            m_shellTextBox.Multiline = true;
            m_shellTextBox.Name = "m_shellTextBox";
            m_shellTextBox.Prompt = "> ";
            m_shellTextBox.ScrollBars = ScrollBars.Both;
            m_shellTextBox.Size = new Size(337, 327);
            m_shellTextBox.TabIndex = 0;
            m_shellTextBox.Text = "> ";
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add((Control)m_shellTextBox);
            Name = nameof(ShellControl);
            Size = new Size(337, 327);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}