using Alsing.SourceCode;
using Alsing.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class DataStorePreferencePage : UserControl, IPreferencePageHost
    {
        private int? m_currentRowIndex;
        private IContainer components;
        private DataGridView m_dataGridView;
        private ToolStrip m_toolStrip;
        private ToolStripButton m_deleteToolStripButton;
        private ToolStripButton m_refreshToolStripButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripLabel toolStripLabel1;
        private ToolStripComboBox m_storeToolStripComboBox;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripLabel toolStripLabel2;
        private ToolStripComboBox m_tableToolStripComboBox;
        private ToolStripButton m_propertiesToolStripButton;
        private SyntaxBoxControl m_syntaxBoxControl;
        private SyntaxDocument m_syntaxDocument;
        private ToolStripButton m_saveToolStripButton;
        private ToolStripButton m_findToolStripButton;
        private ToolStripButton m_addToolStripButton;
        private ToolStripSeparator toolStripSeparator3;
        private Label m_headerLabel;

        public DataStorePreferencePage()
        {
            InitializeComponent();
            m_syntaxDocument.SetSyntaxFromEmbeddedResource(typeof(QueuePreferencePage).Assembly,
                "Redbox.KioskEngine.IDE.Lua.syn");
        }

        public bool SaveValues(IPreferencePage preferencePage)
        {
            return true;
        }

        public void LoadValues(IPreferencePage preferencePage)
        {
            RefreshStores();
        }

        private void OnAdd(object sender, EventArgs e)
        {
            var newKeyValueForm = new NewKeyValueForm();
            if (newKeyValueForm.ShowDialog() != DialogResult.OK)
                return;
            var service = ServiceLocator.Instance.GetService<IDataStoreService>();
            if (service == null)
                return;
            var withoutExtension = Path.GetFileNameWithoutExtension(m_storeToolStripComboBox.SelectedItem.ToString());
            var tableName = m_tableToolStripComboBox.SelectedItem.ToString();
            service.Set(withoutExtension, tableName, newKeyValueForm.KeyName, (object)newKeyValueForm.Value);
            RefreshTable();
        }

        private void OnDelete(object sender, EventArgs e)
        {
            if (m_dataGridView.SelectedRows.Count == 0)
                return;
            var service = ServiceLocator.Instance.GetService<IDataStoreService>();
            if (service == null)
                return;
            var withoutExtension = Path.GetFileNameWithoutExtension(m_storeToolStripComboBox.SelectedItem.ToString());
            var tableName = m_tableToolStripComboBox.SelectedItem.ToString();
            foreach (DataGridViewRow selectedRow in (BaseCollection)m_dataGridView.SelectedRows)
                service.Remove(withoutExtension, tableName, selectedRow.Cells[0].FormattedValue as string);
            RefreshTable();
        }

        private void OnSave(object sender, EventArgs e)
        {
            var service = ServiceLocator.Instance.GetService<IDataStoreService>();
            if (service == null)
                return;
            var withoutExtension = Path.GetFileNameWithoutExtension(m_storeToolStripComboBox.SelectedItem.ToString());
            var tableName = m_tableToolStripComboBox.SelectedItem.ToString();
            try
            {
                Cursor = Cursors.WaitCursor;
                var values = new Dictionary<string, object>();
                foreach (DataGridViewRow row in (IEnumerable)m_dataGridView.Rows)
                    if (!((string)row.HeaderCell.Value != "*"))
                        values[(string)row.Cells[0].FormattedValue] = (object)(row.Cells[1].FormattedValue as string);
                service.BulkSet(withoutExtension, tableName, (IDictionary<string, object>)values);
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            RefreshTable();
        }

        private void OnSelectedStoreChanged(object sender, EventArgs e)
        {
            try
            {
                var service = ServiceLocator.Instance.GetService<IDataStoreService>();
                if (service == null || m_storeToolStripComboBox.SelectedItem == null)
                    return;
                Cursor = Cursors.WaitCursor;
                var tables =
                    service.GetTables(
                        Path.GetFileNameWithoutExtension(m_storeToolStripComboBox.SelectedItem.ToString()));
                m_tableToolStripComboBox.BeginUpdate();
                m_tableToolStripComboBox.Items.Clear();
                foreach (object obj in tables)
                    m_tableToolStripComboBox.Items.Add(obj);
                m_tableToolStripComboBox.EndUpdate();
                if (m_tableToolStripComboBox.Items.Count <= 0)
                    return;
                m_tableToolStripComboBox.SelectedIndex = 0;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void OnSelectedTableChanged(object sender, EventArgs e)
        {
            RefreshTable();
        }

        private void RefreshTable()
        {
            m_currentRowIndex = new int?();
            var service = ServiceLocator.Instance.GetService<IDataStoreService>();
            if (service == null)
                return;
            if (m_tableToolStripComboBox.SelectedItem == null)
                return;
            try
            {
                Cursor = Cursors.WaitCursor;
                m_syntaxDocument.Clear();
                var withoutExtension =
                    Path.GetFileNameWithoutExtension(m_storeToolStripComboBox.SelectedItem.ToString());
                var tableName = m_tableToolStripComboBox.SelectedItem.ToString();
                var dataTable = new DataTable();
                dataTable.Columns.Add("Key", typeof(string));
                dataTable.Columns.Add("Value", typeof(string));
                foreach (var key in service.GetKeys(withoutExtension, tableName))
                {
                    var row = dataTable.NewRow();
                    dataTable.Rows.Add(row);
                    row.BeginEdit();
                    row["Key"] = (object)key;
                    row["Value"] = service.Get(withoutExtension, tableName, key);
                    row.EndEdit();
                    row.AcceptChanges();
                }

                m_dataGridView.DataSource = (object)dataTable;
                m_dataGridView.Focus();
            }
            finally
            {
                Cursor = Cursors.Default;
                if (m_findToolStripButton.Checked)
                    m_findToolStripButton.Checked = false;
            }
        }

        private void RefreshStores()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                m_storeToolStripComboBox.BeginUpdate();
                m_storeToolStripComboBox.Items.Clear();
                ServiceLocator.Instance.GetService<IMacroService>();
                foreach (var file in Directory.GetFiles(
                             ServiceLocator.Instance.GetService<IEngineApplication>().DataPath, "*.data"))
                    m_storeToolStripComboBox.Items.Add((object)Path.GetFileName(file));
                if (m_storeToolStripComboBox.Items.Count > 0)
                    m_storeToolStripComboBox.SelectedIndex = 0;
                m_storeToolStripComboBox.EndUpdate();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            m_currentRowIndex = new int?();
            RefreshStores();
        }

        private void OnRowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                m_currentRowIndex = new int?();
                m_syntaxDocument.Clear();
            }

            var currentRowIndex = m_currentRowIndex;
            var rowIndex1 = e.RowIndex;
            if ((currentRowIndex.GetValueOrDefault() == rowIndex1) & currentRowIndex.HasValue)
                return;
            if (m_currentRowIndex.HasValue)
            {
                currentRowIndex = m_currentRowIndex;
                var rowIndex2 = e.RowIndex;
                if (!((currentRowIndex.GetValueOrDefault() == rowIndex2) & currentRowIndex.HasValue) &&
                    m_dataGridView[1, m_currentRowIndex.Value].Value as string != m_syntaxDocument.Text)
                {
                    m_dataGridView[1, m_currentRowIndex.Value].Value = (object)m_syntaxDocument.Text;
                    m_dataGridView.Rows[m_currentRowIndex.Value].HeaderCell.Value = (object)"*";
                }
            }

            m_currentRowIndex = new int?(e.RowIndex);
            m_syntaxDocument.Text = m_dataGridView[1, e.RowIndex].FormattedValue as string;
        }

        private void OnFind(object sender, EventArgs e)
        {
            if (m_findToolStripButton.Checked)
            {
                m_findToolStripButton.Checked = false;
                RefreshTable();
            }
            else
            {
                var findKeyValueForm = new FindKeyValueForm();
                if (findKeyValueForm.ShowDialog() != DialogResult.OK ||
                    !(m_dataGridView.DataSource is DataTable dataSource))
                    return;
                m_syntaxDocument.Clear();
                var dataRowArray = findKeyValueForm.SearchType == KeyValueSearchType.Key
                    ? dataSource.Select(string.Format("[Key] = '{0}'", (object)findKeyValueForm.SearchFor))
                    : dataSource.Select(string.Format("[Value] LIKE '%{0}%'", (object)findKeyValueForm.SearchFor));
                var dataTable = new DataTable();
                dataTable.Columns.Add("Key", typeof(string));
                dataTable.Columns.Add("Value", typeof(string));
                foreach (var dataRow in dataRowArray)
                    dataTable.Rows.Add(dataRow[0], dataRow[1]);
                m_dataGridView.DataSource = (object)dataTable;
                m_dataGridView.Focus();
                m_findToolStripButton.Checked = true;
            }
        }

        private void OnDocumentModified(object sender, EventArgs e)
        {
            m_saveToolStripButton.Enabled = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = (IContainer)new Container();
            m_dataGridView = new DataGridView();
            m_toolStrip = new ToolStrip();
            m_refreshToolStripButton = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripLabel1 = new ToolStripLabel();
            m_storeToolStripComboBox = new ToolStripComboBox();
            toolStripLabel2 = new ToolStripLabel();
            m_tableToolStripComboBox = new ToolStripComboBox();
            m_propertiesToolStripButton = new ToolStripButton();
            m_findToolStripButton = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            m_addToolStripButton = new ToolStripButton();
            m_deleteToolStripButton = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            m_saveToolStripButton = new ToolStripButton();
            m_syntaxBoxControl = new SyntaxBoxControl();
            m_syntaxDocument = new SyntaxDocument(components);
            m_headerLabel = new Label();
            ((ISupportInitialize)m_dataGridView).BeginInit();
            m_toolStrip.SuspendLayout();
            SuspendLayout();
            m_dataGridView.AllowUserToAddRows = false;
            m_dataGridView.AllowUserToDeleteRows = false;
            m_dataGridView.AllowUserToResizeRows = false;
            m_dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            m_dataGridView.Location = new Point(0, 70);
            m_dataGridView.Name = "m_dataGridView";
            m_dataGridView.ReadOnly = true;
            m_dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            m_dataGridView.ShowCellErrors = false;
            m_dataGridView.ShowCellToolTips = false;
            m_dataGridView.ShowEditingIcon = false;
            m_dataGridView.ShowRowErrors = false;
            m_dataGridView.Size = new Size(542, 254);
            m_dataGridView.TabIndex = 8;
            m_dataGridView.RowEnter += new DataGridViewCellEventHandler(OnRowEnter);
            m_toolStrip.AutoSize = false;
            m_toolStrip.CanOverflow = false;
            m_toolStrip.Dock = DockStyle.None;
            m_toolStrip.Items.AddRange(new ToolStripItem[13]
            {
                (ToolStripItem)m_refreshToolStripButton,
                (ToolStripItem)toolStripSeparator1,
                (ToolStripItem)toolStripLabel1,
                (ToolStripItem)m_storeToolStripComboBox,
                (ToolStripItem)toolStripLabel2,
                (ToolStripItem)m_tableToolStripComboBox,
                (ToolStripItem)m_propertiesToolStripButton,
                (ToolStripItem)m_findToolStripButton,
                (ToolStripItem)toolStripSeparator2,
                (ToolStripItem)m_addToolStripButton,
                (ToolStripItem)m_deleteToolStripButton,
                (ToolStripItem)toolStripSeparator3,
                (ToolStripItem)m_saveToolStripButton
            });
            m_toolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
            m_toolStrip.Location = new Point(0, 42);
            m_toolStrip.Name = "m_toolStrip";
            m_toolStrip.Size = new Size(542, 25);
            m_toolStrip.TabIndex = 10;
            m_toolStrip.Text = "toolStrip1";
            m_refreshToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_refreshToolStripButton.Image = (Image)Resources.Refresh;
            m_refreshToolStripButton.ImageTransparentColor = Color.Magenta;
            m_refreshToolStripButton.Name = "m_refreshToolStripButton";
            m_refreshToolStripButton.Size = new Size(23, 20);
            m_refreshToolStripButton.Text = "Refresh";
            m_refreshToolStripButton.Click += new EventHandler(OnRefresh);
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 23);
            toolStripLabel1.AutoSize = false;
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(63, 20);
            toolStripLabel1.Text = "Data Store:";
            m_storeToolStripComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            m_storeToolStripComboBox.Name = "m_storeToolStripComboBox";
            m_storeToolStripComboBox.Size = new Size(121, 23);
            m_storeToolStripComboBox.SelectedIndexChanged += new EventHandler(OnSelectedStoreChanged);
            toolStripLabel2.AutoSize = false;
            toolStripLabel2.Name = "toolStripLabel2";
            toolStripLabel2.Size = new Size(37, 20);
            toolStripLabel2.Text = "Table:";
            m_tableToolStripComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            m_tableToolStripComboBox.Name = "m_tableToolStripComboBox";
            m_tableToolStripComboBox.Size = new Size(121, 23);
            m_tableToolStripComboBox.SelectedIndexChanged += new EventHandler(OnSelectedTableChanged);
            m_propertiesToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_propertiesToolStripButton.Image = (Image)Resources.Properties;
            m_propertiesToolStripButton.ImageTransparentColor = Color.Magenta;
            m_propertiesToolStripButton.Name = "m_propertiesToolStripButton";
            m_propertiesToolStripButton.Size = new Size(23, 20);
            m_propertiesToolStripButton.Text = "Properties";
            m_findToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_findToolStripButton.Image = (Image)Resources.Filter;
            m_findToolStripButton.ImageTransparentColor = Color.Magenta;
            m_findToolStripButton.Name = "m_findToolStripButton";
            m_findToolStripButton.Size = new Size(23, 20);
            m_findToolStripButton.Text = "Find";
            m_findToolStripButton.Click += new EventHandler(OnFind);
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 23);
            m_addToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_addToolStripButton.Image = (Image)Resources.Plus;
            m_addToolStripButton.ImageTransparentColor = Color.Magenta;
            m_addToolStripButton.Name = "m_addToolStripButton";
            m_addToolStripButton.Size = new Size(23, 20);
            m_addToolStripButton.Text = "Add";
            m_addToolStripButton.Click += new EventHandler(OnAdd);
            m_deleteToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_deleteToolStripButton.Image = (Image)Resources.Cross;
            m_deleteToolStripButton.ImageTransparentColor = Color.Magenta;
            m_deleteToolStripButton.Name = "m_deleteToolStripButton";
            m_deleteToolStripButton.Size = new Size(23, 20);
            m_deleteToolStripButton.Text = "Delete";
            m_deleteToolStripButton.Click += new EventHandler(OnDelete);
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 23);
            m_saveToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_saveToolStripButton.Enabled = false;
            m_saveToolStripButton.Image = (Image)Resources.Save;
            m_saveToolStripButton.ImageTransparentColor = Color.Magenta;
            m_saveToolStripButton.Name = "m_saveToolStripButton";
            m_saveToolStripButton.Size = new Size(23, 20);
            m_saveToolStripButton.Text = "Save";
            m_saveToolStripButton.Click += new EventHandler(OnSave);
            m_syntaxBoxControl.ActiveView = ActiveView.BottomRight;
            m_syntaxBoxControl.AllowBreakPoints = false;
            m_syntaxBoxControl.AutoListPosition = (TextPoint)null;
            m_syntaxBoxControl.AutoListSelectedText = "a123";
            m_syntaxBoxControl.AutoListVisible = false;
            m_syntaxBoxControl.BackColor = Color.White;
            m_syntaxBoxControl.BorderStyle = Alsing.Windows.Forms.BorderStyle.None;
            m_syntaxBoxControl.CopyAsRTF = false;
            m_syntaxBoxControl.Document = m_syntaxDocument;
            m_syntaxBoxControl.FontName = "Courier new";
            m_syntaxBoxControl.HighLightActiveLine = true;
            m_syntaxBoxControl.ImeMode = ImeMode.NoControl;
            m_syntaxBoxControl.InfoTipCount = 1;
            m_syntaxBoxControl.InfoTipPosition = (TextPoint)null;
            m_syntaxBoxControl.InfoTipSelectedIndex = 1;
            m_syntaxBoxControl.InfoTipVisible = false;
            m_syntaxBoxControl.Location = new Point(0, 330);
            m_syntaxBoxControl.LockCursorUpdate = false;
            m_syntaxBoxControl.Name = "m_syntaxBoxControl";
            m_syntaxBoxControl.ShowEOLMarker = true;
            m_syntaxBoxControl.ShowGutterMargin = false;
            m_syntaxBoxControl.ShowScopeIndicator = false;
            m_syntaxBoxControl.ShowTabGuides = true;
            m_syntaxBoxControl.ShowWhitespace = true;
            m_syntaxBoxControl.Size = new Size(542, 176);
            m_syntaxBoxControl.SmoothScroll = false;
            m_syntaxBoxControl.SplitView = false;
            m_syntaxBoxControl.SplitviewH = -4;
            m_syntaxBoxControl.SplitviewV = -4;
            m_syntaxBoxControl.TabGuideColor = Color.FromArgb(222, 219, 214);
            m_syntaxBoxControl.TabIndex = 12;
            m_syntaxBoxControl.TabsToSpaces = true;
            m_syntaxBoxControl.WhitespaceColor = SystemColors.ControlDark;
            m_syntaxDocument.Lines = new string[1] { "" };
            m_syntaxDocument.MaxUndoBufferSize = 1000;
            m_syntaxDocument.Modified = false;
            m_syntaxDocument.UndoStep = 0;
            m_syntaxDocument.ModifiedChanged += new EventHandler(OnDocumentModified);
            m_headerLabel.Anchor = AnchorStyles.Top;
            m_headerLabel.BackColor = Color.MediumBlue;
            m_headerLabel.Font = new Font("Microsoft Sans Serif", 14.25f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            m_headerLabel.ForeColor = Color.White;
            m_headerLabel.Location = new Point(0, 0);
            m_headerLabel.Name = "m_headerLabel";
            m_headerLabel.Size = new Size(542, 38);
            m_headerLabel.TabIndex = 30;
            m_headerLabel.Text = "  Engine Core: Database";
            m_headerLabel.TextAlign = ContentAlignment.MiddleLeft;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add((Control)m_headerLabel);
            Controls.Add((Control)m_syntaxBoxControl);
            Controls.Add((Control)m_dataGridView);
            Controls.Add((Control)m_toolStrip);
            Name = nameof(DataStorePreferencePage);
            Size = new Size(542, 506);
            ((ISupportInitialize)m_dataGridView).EndInit();
            m_toolStrip.ResumeLayout(false);
            m_toolStrip.PerformLayout();
            ResumeLayout(false);
        }
    }
}