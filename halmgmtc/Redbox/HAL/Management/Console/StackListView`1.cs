using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Management.Console.Properties;

namespace Redbox.HAL.Management.Console
{
    public class StackListView<T> : UserControl
    {
        private readonly DataGridView dataGridView1;
        private IContainer components;
        private ToolStripButton m_slvClearButton;
        private ToolStripComboBox m_slvComboBox;
        private ToolStripLabel m_slvLabel;
        private ToolStripButton m_slvRefeshButton;
        private Panel panel1;
        private ToolStrip toolStrip1;

        public StackListView(string[,] headers, BindingList<T> data)
        {
            InitializeComponent();
            Enabled = false;
            SuspendLayout();
            dataGridView1 = new AutomatedDataGridView<T>(headers, data, false);
            dataGridView1.AutoSize = true;
            panel1.Controls.Add(dataGridView1);
            m_slvComboBox.KeyPress += OnPush;
            ResumeLayout();
            EnvironmentHelper.SelectedJobChanged += (_param1, _param2) => JobHelper.StackList.Clear();
            ProfileManager.Instance.Connected += (_param1, _param2) => Enabled = true;
            ProfileManager.Instance.Disconnected += (_param1, _param2) =>
            {
                Enabled = false;
                JobHelper.StackList.Clear();
            };
        }

        internal void RefreshData()
        {
            JobHelper.RefreshStackList();
        }

        private void OnPush(object sender, KeyPressEventArgs args)
        {
            if (args.KeyChar != '\r')
                return;
            var comboBox = (sender as ToolStripComboBox).ComboBox;
            if (comboBox == null || string.IsNullOrEmpty(comboBox.Text) || !JobHelper.PushStackItem(comboBox.Text))
                return;
            comboBox.Text = string.Empty;
        }

        private void OnClear(object sender, EventArgs args)
        {
            JobHelper.ClearJobStack();
        }

        private void OnRefresh(object sender, EventArgs args)
        {
            JobHelper.StackList.Clear();
            RefreshData();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();
            toolStrip1 = new ToolStrip();
            m_slvRefeshButton = new ToolStripButton();
            m_slvClearButton = new ToolStripButton();
            m_slvLabel = new ToolStripLabel();
            m_slvComboBox = new ToolStripComboBox();
            panel1 = new Panel();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            toolStrip1.Items.AddRange(new ToolStripItem[4]
            {
                m_slvRefeshButton,
                m_slvClearButton,
                m_slvLabel,
                m_slvComboBox
            });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(410, 25);
            toolStrip1.TabIndex = 0;
            m_slvRefeshButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_slvRefeshButton.Image = Resources.refresh;
            m_slvRefeshButton.ImageTransparentColor = Color.Magenta;
            m_slvRefeshButton.Name = "m_slvRefeshButton";
            m_slvRefeshButton.Size = new Size(23, 22);
            m_slvRefeshButton.Text = "Refresh";
            m_slvRefeshButton.Click += OnRefresh;
            m_slvClearButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_slvClearButton.Image = Resources.clear;
            m_slvClearButton.ImageTransparentColor = Color.Magenta;
            m_slvClearButton.Name = "m_slvClearButton";
            m_slvClearButton.Size = new Size(23, 22);
            m_slvClearButton.Text = "Clear Stack";
            m_slvClearButton.Click += OnClear;
            m_slvLabel.Name = "m_slvLabel";
            m_slvLabel.Size = new Size(79, 22);
            m_slvLabel.Text = "Push Values:  ";
            m_slvComboBox.Name = "m_slvComboBox";
            m_slvComboBox.Size = new Size(121, 25);
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Location = new Point(0, 25);
            panel1.Name = "panel1";
            panel1.Size = new Size(409, 382);
            panel1.TabIndex = 1;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(toolStrip1);
            Controls.Add(panel1);
            Name = "SLV";
            Size = new Size(410, 411);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}