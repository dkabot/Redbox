using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console
{
    public class AutomatedDataGridView<T> : DataGridView
    {
        public AutomatedDataGridView(string[,] headers, BindingList<T> data, bool emptyEndColumn)
        {
            InitializeComponent();
            BuildGrid(headers, emptyEndColumn);
            DataSource = data;
        }

        public AutomatedDataGridView(string[,] headers, BindingSource data, bool emptyEndColumn)
        {
            InitializeComponent();
            BuildGrid(headers, emptyEndColumn);
            DataSource = data;
        }

        private void BuildGrid(string[,] headers, bool emptyEndColumn)
        {
            for (var index = 0; index < headers.Length / 2; ++index)
            {
                var dataGridViewColumn = (DataGridViewColumn)new DataGridViewTextBoxColumn();
                dataGridViewColumn.HeaderText = headers[index, 0];
                dataGridViewColumn.DataPropertyName = headers[index, 1];
                dataGridViewColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridViewColumn.ReadOnly = true;
                dataGridViewColumn.SortMode = DataGridViewColumnSortMode.Automatic;
                Columns.Add(dataGridViewColumn);
            }

            if (emptyEndColumn)
            {
                var dataGridViewColumn = (DataGridViewColumn)new DataGridViewTextBoxColumn();
                dataGridViewColumn.HeaderText = string.Empty;
                dataGridViewColumn.ReadOnly = true;
                Columns.Add(dataGridViewColumn);
            }

            Columns[Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void InitializeComponent()
        {
            ((ISupportInitialize)this).BeginInit();
            SuspendLayout();
            AutoGenerateColumns = false;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToResizeColumns = true;
            AllowUserToResizeRows = false;
            BackgroundColor = Color.White;
            ScrollBars = ScrollBars.Both;
            ColumnHeadersHeight = 25;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DefaultCellStyle.BackColor = Color.White;
            Dock = DockStyle.Fill;
            RowHeadersVisible = true;
            RowHeadersWidth = 25;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            TabIndex = 1;
            ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            ((ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }
    }
}