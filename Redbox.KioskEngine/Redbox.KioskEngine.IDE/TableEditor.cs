using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class TableEditor : UserControl
    {
        private IContainer components;
        private DataGridView m_dataGridView;

        public TableEditor()
        {
            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_dataGridView = new DataGridView();
            ((ISupportInitialize)m_dataGridView).BeginInit();
            SuspendLayout();
            m_dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            m_dataGridView.Dock = DockStyle.Fill;
            m_dataGridView.Location = new Point(0, 0);
            m_dataGridView.Name = "m_dataGridView";
            m_dataGridView.Size = new Size(725, 453);
            m_dataGridView.TabIndex = 0;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add((Control)m_dataGridView);
            Name = nameof(TableEditor);
            Size = new Size(725, 453);
            ((ISupportInitialize)m_dataGridView).EndInit();
            ResumeLayout(false);
        }
    }
}