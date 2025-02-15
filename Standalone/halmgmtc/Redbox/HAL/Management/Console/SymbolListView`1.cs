using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Management.Console.Properties;

namespace Redbox.HAL.Management.Console
{
    public class SymbolListView<T> : UserControl
    {
        private readonly DataGridView dataGridView1;
        private IContainer components;
        private Panel panel1;
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;

        public SymbolListView(string[,] headers, BindingList<T> data)
        {
            InitializeComponent();
            Enabled = false;
            SuspendLayout();
            dataGridView1 = new AutomatedDataGridView<T>(headers, data, false);
            dataGridView1.AutoSize = true;
            panel1.Controls.Add(dataGridView1);
            ResumeLayout();
            EnvironmentHelper.SelectedJobChanged += (_param1, _param2) => JobHelper.SymbolList.Clear();
            ProfileManager.Instance.Connected += (_param1, _param2) => Enabled = true;
            ProfileManager.Instance.Disconnected += (_param1, _param2) =>
            {
                Enabled = false;
                JobHelper.SymbolList.Clear();
            };
        }

        internal void RefreshData()
        {
            JobHelper.RefreshSymbolList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            toolStrip1 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            toolStripButton2 = new ToolStripButton();
            panel1 = new Panel();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            toolStrip1.Items.AddRange(new ToolStripItem[2]
            {
                toolStripButton1,
                toolStripButton2
            });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(410, 25);
            toolStrip1.TabIndex = 0;
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton1.Image = Resources.refresh;
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(23, 22);
            toolStripButton1.Text = "Refresh";
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton2.Image = Resources.clear;
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(23, 22);
            toolStripButton2.Text = "Clear";
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.BackColor = SystemColors.ActiveCaption;
            panel1.Location = new Point(0, 25);
            panel1.Name = "panel1";
            panel1.Size = new Size(409, 382);
            panel1.TabIndex = 1;
            Controls.Add(toolStrip1);
            Controls.Add(panel1);
            Name = nameof(SymbolListView<T>);
            Size = new Size(410, 411);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}