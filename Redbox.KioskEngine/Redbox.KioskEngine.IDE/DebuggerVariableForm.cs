using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class DebuggerVariableForm : Form
    {
        private IContainer components;
        private Label label1;
        private TextBox m_nameTextBox;
        private Label label2;
        private TextBox m_valueTextBox;
        private Panel m_topPanel;
        private Panel m_bottomPanel;
        private Panel m_middlePanel;

        public DebuggerVariableForm()
        {
            InitializeComponent();
        }

        public string VariableName
        {
            get => m_nameTextBox.Text;
            set => m_nameTextBox.Text = value;
        }

        public string VariableValue
        {
            get => m_valueTextBox.Text;
            set => m_valueTextBox.Text = value;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            label1 = new Label();
            m_nameTextBox = new TextBox();
            label2 = new Label();
            m_valueTextBox = new TextBox();
            m_topPanel = new Panel();
            m_bottomPanel = new Panel();
            m_middlePanel = new Panel();
            m_topPanel.SuspendLayout();
            m_bottomPanel.SuspendLayout();
            m_middlePanel.SuspendLayout();
            SuspendLayout();
            label1.AutoSize = true;
            label1.Location = new Point(2, 5);
            label1.Name = "label1";
            label1.Size = new Size(38, 13);
            label1.TabIndex = 0;
            label1.Text = "Name:";
            m_nameTextBox.Dock = DockStyle.Bottom;
            m_nameTextBox.Location = new Point(5, 20);
            m_nameTextBox.Name = "m_nameTextBox";
            m_nameTextBox.ReadOnly = true;
            m_nameTextBox.Size = new Size(382, 20);
            m_nameTextBox.TabIndex = 1;
            label2.AutoSize = true;
            label2.Location = new Point(3, 3);
            label2.Name = "label2";
            label2.Size = new Size(37, 13);
            label2.TabIndex = 2;
            label2.Text = "Value:";
            m_valueTextBox.Dock = DockStyle.Fill;
            m_valueTextBox.Location = new Point(5, 5);
            m_valueTextBox.Multiline = true;
            m_valueTextBox.Name = "m_valueTextBox";
            m_valueTextBox.ReadOnly = true;
            m_valueTextBox.ScrollBars = ScrollBars.Both;
            m_valueTextBox.Size = new Size(382, 200);
            m_valueTextBox.TabIndex = 3;
            m_topPanel.Controls.Add((Control)label1);
            m_topPanel.Controls.Add((Control)m_nameTextBox);
            m_topPanel.Dock = DockStyle.Top;
            m_topPanel.Location = new Point(0, 0);
            m_topPanel.Name = "m_topPanel";
            m_topPanel.Padding = new Padding(5);
            m_topPanel.Size = new Size(392, 45);
            m_topPanel.TabIndex = 4;
            m_bottomPanel.Controls.Add((Control)m_valueTextBox);
            m_bottomPanel.Dock = DockStyle.Fill;
            m_bottomPanel.Location = new Point(0, 66);
            m_bottomPanel.Name = "m_bottomPanel";
            m_bottomPanel.Padding = new Padding(5);
            m_bottomPanel.Size = new Size(392, 210);
            m_bottomPanel.TabIndex = 5;
            m_middlePanel.Controls.Add((Control)label2);
            m_middlePanel.Dock = DockStyle.Top;
            m_middlePanel.Location = new Point(0, 45);
            m_middlePanel.Name = "m_middlePanel";
            m_middlePanel.Size = new Size(392, 21);
            m_middlePanel.TabIndex = 6;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(392, 276);
            Controls.Add((Control)m_bottomPanel);
            Controls.Add((Control)m_middlePanel);
            Controls.Add((Control)m_topPanel);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MinimumSize = new Size(400, 300);
            Name = nameof(DebuggerVariableForm);
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Variable Detail";
            m_topPanel.ResumeLayout(false);
            m_topPanel.PerformLayout();
            m_bottomPanel.ResumeLayout(false);
            m_bottomPanel.PerformLayout();
            m_middlePanel.ResumeLayout(false);
            m_middlePanel.PerformLayout();
            ResumeLayout(false);
        }
    }
}