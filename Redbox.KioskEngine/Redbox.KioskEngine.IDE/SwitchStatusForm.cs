using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class SwitchStatusForm : Form
    {
        private IContainer components;
        private Label m_statusLabel;
        private Panel panel1;

        public SwitchStatusForm()
        {
            InitializeComponent();
        }

        public string StatusLabel
        {
            get => m_statusLabel.Text;
            set => m_statusLabel.Text = value;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_statusLabel = new Label();
            panel1 = new Panel();
            panel1.SuspendLayout();
            SuspendLayout();
            m_statusLabel.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            m_statusLabel.Location = new Point(11, 8);
            m_statusLabel.Name = "m_statusLabel";
            m_statusLabel.Size = new Size(355, 55);
            m_statusLabel.TabIndex = 1;
            m_statusLabel.Text = "Please Wait...";
            m_statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add((Control)m_statusLabel);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(379, 73);
            panel1.TabIndex = 2;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(379, 73);
            ControlBox = false;
            Controls.Add((Control)panel1);
            FormBorderStyle = FormBorderStyle.None;
            Name = nameof(SwitchStatusForm);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = nameof(SwitchStatusForm);
            TopMost = true;
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}