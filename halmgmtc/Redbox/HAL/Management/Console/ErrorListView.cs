using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Management.Console.Properties;

namespace Redbox.HAL.Management.Console
{
    public class ErrorListView : UserControl
    {
        private static ErrorList m_list;
        private static ErrorListView m_instance;
        private DataGridViewTextBoxColumn Code;
        private IContainer components;
        private DataGridView dataGridView1;
        private DataGridViewTextBoxColumn Description;
        private ToolStripButton m_errorButton;
        private ToolStripButton m_warningButton;
        private Panel panel1;
        private ToolStrip toolStrip1;
        private DataGridViewTextBoxColumn Type;

        private ErrorListView()
        {
            InitializeComponent();
            Enabled = true;
            m_list = new ErrorList();
        }

        public static ErrorListView Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new ErrorListView();
                return m_instance;
            }
        }

        public bool KeepOpenOnSuccessfulInstruction { get; set; }

        internal void Add(ErrorList list, bool clearPrevious)
        {
            if (clearPrevious)
                Clear();
            m_list.AddRange(list);
            RefreshData();
        }

        internal void AddTop(ErrorList list, bool clearPrevious)
        {
            if (clearPrevious)
                Clear();
            foreach (var error in list)
                m_list.Insert(0, error);
            RefreshData();
        }

        internal void RefreshData()
        {
            dataGridView1.SuspendLayout();
            dataGridView1.Rows.Clear();
            foreach (var error in m_list)
                if ((m_warningButton.Checked || !error.IsWarning) && (m_errorButton.Checked || error.IsWarning))
                {
                    var dataGridViewRow = new DataGridViewRow();
                    dataGridViewRow.Cells.AddRange(new DataGridViewImageCell(), new DataGridViewTextBoxCell(),
                        new DataGridViewTextBoxCell());
                    dataGridViewRow.Cells[0].Value = !error.IsWarning ? Resources.cross : (object)Resources.warning;
                    dataGridViewRow.Cells[1].Value = error.Code;
                    dataGridViewRow.Cells[2].Value = error.Description + Environment.NewLine + error.Details;
                    dataGridView1.Rows.Add(dataGridViewRow);
                }

            dataGridView1.ResumeLayout();
        }

        internal void Clear()
        {
            if (m_list.Count <= 0)
                return;
            m_list.Clear();
            RefreshData();
        }

        private void OnWarning_Click(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void OnError_Click(object sender, EventArgs e)
        {
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
            var gridViewCellStyle = new DataGridViewCellStyle();
            toolStrip1 = new ToolStrip();
            m_warningButton = new ToolStripButton();
            m_errorButton = new ToolStripButton();
            panel1 = new Panel();
            dataGridView1 = new DataGridView();
            Type = new DataGridViewTextBoxColumn();
            Code = new DataGridViewTextBoxColumn();
            Description = new DataGridViewTextBoxColumn();
            toolStrip1.SuspendLayout();
            panel1.SuspendLayout();
            ((ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            toolStrip1.Items.AddRange(new ToolStripItem[2]
            {
                m_warningButton,
                m_errorButton
            });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(368, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            m_warningButton.Checked = true;
            m_warningButton.CheckOnClick = true;
            m_warningButton.CheckState = CheckState.Checked;
            m_warningButton.Image = Resources.warning;
            m_warningButton.ImageTransparentColor = Color.Magenta;
            m_warningButton.Name = "m_warningButton";
            m_warningButton.Size = new Size(72, 22);
            m_warningButton.Text = "Warning";
            m_warningButton.Click += OnWarning_Click;
            m_errorButton.Checked = true;
            m_errorButton.CheckOnClick = true;
            m_errorButton.CheckState = CheckState.Checked;
            m_errorButton.Image = Resources.cross;
            m_errorButton.ImageTransparentColor = Color.Magenta;
            m_errorButton.Name = "m_errorButton";
            m_errorButton.Size = new Size(52, 22);
            m_errorButton.Text = "Error";
            m_errorButton.Click += OnError_Click;
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Controls.Add(dataGridView1);
            panel1.Location = new Point(3, 30);
            panel1.Name = "panel1";
            panel1.Size = new Size(365, 117);
            panel1.TabIndex = 1;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(Type, Code, Description);
            gridViewCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            gridViewCellStyle.BackColor = SystemColors.Window;
            gridViewCellStyle.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
            gridViewCellStyle.ForeColor = SystemColors.ControlText;
            gridViewCellStyle.SelectionBackColor = SystemColors.Highlight;
            gridViewCellStyle.SelectionForeColor = SystemColors.HighlightText;
            gridViewCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.DefaultCellStyle = gridViewCellStyle;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(0, 0);
            dataGridView1.Margin = new Padding(3, 1, 3, 4);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridView1.Size = new Size(365, 117);
            dataGridView1.TabIndex = 0;
            Type.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            Type.HeaderText = "";
            Type.MinimumWidth = 50;
            Type.Name = "Type";
            Type.ReadOnly = true;
            Type.Resizable = DataGridViewTriState.False;
            Type.Width = 50;
            Code.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Code.HeaderText = "Code";
            Code.Name = "Code";
            Code.ReadOnly = true;
            Code.Width = 57;
            Description.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Description.HeaderText = "Description";
            Description.Name = "Description";
            Description.ReadOnly = true;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panel1);
            Controls.Add(toolStrip1);
            Name = nameof(ErrorListView);
            Size = new Size(368, 150);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            panel1.ResumeLayout(false);
            ((ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}