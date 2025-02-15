using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Management.Console.Properties;

namespace Redbox.HAL.Management.Console
{
    public class ResultListView<T> : UserControl
    {
        private readonly DataGridView dataGridView1;
        private IContainer components;
        private ToolStripButton m_clearButton;
        private ToolStripButton m_refreshButton;
        private Timer m_timer;
        private Panel panel1;
        private ToolStrip toolStrip1;

        public ResultListView(string[,] headers, BindingList<T> data)
        {
            InitializeComponent();
            Enabled = false;
            SuspendLayout();
            dataGridView1 = new AutomatedDataGridView<T>(headers, data, false);
            dataGridView1.AutoSize = true;
            panel1.Controls.Add(dataGridView1);
            ResumeLayout();
            EnvironmentHelper.SelectedJobChanged += (_param1, _param2) => JobHelper.ResultList.Clear();
            ProfileManager.Instance.Connected += (_param1, _param2) => Enabled = true;
            ProfileManager.Instance.Disconnected += (_param1, _param2) =>
            {
                Enabled = false;
                JobHelper.ResultList.Clear();
            };
        }

        internal void OnRefresh(object sender, EventArgs e)
        {
            OutputWindow.Instance.Append("Refresh RLV " + (Focused || Focused));
            if (!Parent.Focused && !Focused)
                return;
            RefreshData();
        }

        internal void RefreshData()
        {
            JobHelper.RefreshResultList();
        }

        private void OnClear(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
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
            m_refreshButton = new ToolStripButton();
            m_clearButton = new ToolStripButton();
            panel1 = new Panel();
            m_timer = new Timer(components);
            toolStrip1.SuspendLayout();
            SuspendLayout();
            toolStrip1.Items.AddRange(new ToolStripItem[2]
            {
                m_refreshButton,
                m_clearButton
            });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(150, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            m_refreshButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_refreshButton.Image = Resources.refresh;
            m_refreshButton.ImageTransparentColor = Color.Magenta;
            m_refreshButton.Name = "m_refreshButton";
            m_refreshButton.Size = new Size(23, 22);
            m_refreshButton.Text = "Refresh";
            m_clearButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_clearButton.Image = Resources.clear;
            m_clearButton.ImageTransparentColor = Color.Magenta;
            m_clearButton.Name = "m_clearButton";
            m_clearButton.Size = new Size(23, 22);
            m_clearButton.Text = "Clear";
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Location = new Point(0, 26);
            panel1.Margin = new Padding(3, 1, 3, 4);
            panel1.Name = "panel1";
            panel1.Size = new Size(150, 120);
            panel1.TabIndex = 1;
            m_timer.Interval = 1000;
            m_timer.Tick += OnRefresh;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panel1);
            Controls.Add(toolStrip1);
            Name = nameof(ResultListView<T>);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}