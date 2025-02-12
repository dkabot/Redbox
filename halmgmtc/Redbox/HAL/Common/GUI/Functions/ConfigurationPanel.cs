using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions
{
    public class ConfigurationPanel : UserControl
    {
        private readonly List<ConfigItem> m_barcodeConfig = new List<ConfigItem>();
        private readonly List<ConfigItem> m_cameraConfig = new List<ConfigItem>();
        private readonly List<ConfigItem> m_controllerConfig = new List<ConfigItem>();
        private readonly DataGridViewCellStyle m_defaultStyle;
        private readonly DataGridViewCellStyle m_valueChangedStyle;
        private IContainer components;
        private ListBox m_configItemListBox;
        private TextBox m_descriptionTB;
        private DataGridView m_propertyDataGridView;
        private DataGridViewTextBoxColumn Property;
        private DataGridViewTextBoxColumn Value;

        public ConfigurationPanel()
        {
            InitializeComponent();
            m_defaultStyle = m_propertyDataGridView.DefaultCellStyle;
            m_valueChangedStyle = new DataGridViewCellStyle
            {
                Font = new Font(Font.Name, Font.Size, FontStyle.Bold)
            };
            m_configItemListBox.SelectedIndexChanged += OnListBoxSelectionChanged;
            m_propertyDataGridView.CellEndEdit += OnDataChanged;
            m_propertyDataGridView.SelectionChanged += OnConfigPropertiesItemChanged;
            m_propertyDataGridView.CellClick += OnButtonClick;
            LoadConfiguration();
            Dock = DockStyle.Fill;
        }

        private bool IsDirty { get; set; }

        private void LoadConfiguration()
        {
        }

        private void OnConfigPropertiesItemChanged(object sender, EventArgs e)
        {
            ConfigPropertiesItemChangedInner();
        }

        private void SetStyle(DataGridViewCell cell)
        {
            if (cell == null || !(cell.Tag is ConfigItem tag) || tag.Value == null || tag.DefaultValue == null)
                return;
            if (tag.DefaultValue.Equals(tag.Value))
                cell.Style = m_defaultStyle;
            else
                cell.Style = m_valueChangedStyle;
        }

        private void OnDataChanged(object sender, DataGridViewCellEventArgs e)
        {
            var cell = m_propertyDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
            var tag = (ConfigItem)cell.Tag;
            if (tag == null)
                return;
            var obj = cell.Value;
            if (tag.ItemType == typeof(bool))
                obj = cell.Value.ToString() == "Yes" ? "True" : (object)"False";
            if (tag.ItemType == typeof(int) && !int.TryParse(obj.ToString(), out var _))
            {
                var num = (int)MessageBox.Show("Data entered is not an integer");
                cell.Value = tag.Value;
            }
            else
            {
                tag.Value = obj;
                SetStyle(cell);
                IsDirty = true;
            }
        }

        private void ConfigPropertiesItemChangedInner()
        {
            var selectedRows = m_propertyDataGridView.SelectedRows;
            if (selectedRows == null || selectedRows.Count <= 0 || selectedRows[0].Cells[1].Tag == null)
                m_descriptionTB.Text = string.Empty;
            else if (!(selectedRows[0].Cells[1].Tag is ConfigItem tag))
                m_descriptionTB.Text = string.Empty;
            else
                m_descriptionTB.Text = tag.Description;
        }

        private void OnReset(object sender, EventArgs e)
        {
            m_barcodeConfig.FindAll(x => !x.ReadOnly && x.CustomEditor == null).ForEach(x =>
            {
                var configItem = x;
                configItem.Value = configItem.DefaultValue;
            });
            m_cameraConfig.FindAll(x => !x.ReadOnly && x.CustomEditor == null).ForEach(x =>
            {
                var configItem = x;
                configItem.Value = configItem.DefaultValue;
            });
            m_controllerConfig.FindAll(x => !x.ReadOnly && x.CustomEditor == null).ForEach(x =>
            {
                var configItem = x;
                configItem.Value = configItem.DefaultValue;
            });
            ListBoxSelectionChangedInner();
            IsDirty = false;
            var num = (int)MessageBox.Show("Successfully Reset Properties to Previously Loaded Values.",
                "Configuration Panel", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void OnButtonClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.RowIndex > m_propertyDataGridView.Rows.Count - 1 ||
                e.ColumnIndex > 1)
                return;
            var cell = m_propertyDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewButtonCell;
        }

        private void OnListBoxSelectionChanged(object sender, EventArgs args)
        {
            ListBoxSelectionChangedInner();
        }

        private void PopulateDataGrid(List<ConfigItem> list)
        {
            if (list == null)
                return;
            m_propertyDataGridView.Rows.Clear();
            foreach (var configItem in list)
                if (configItem.CustomEditor == null)
                {
                    var row = m_propertyDataGridView.Rows[m_propertyDataGridView.Rows.Add()];
                    row.Cells[0].Value = configItem.DisplayName;
                    row.Cells[0].ReadOnly = true;
                    var dataGridViewCell = (DataGridViewCell)null;
                    if (configItem.ValidValues != null)
                    {
                        dataGridViewCell = new DataGridViewComboBoxCell();
                        (dataGridViewCell as DataGridViewComboBoxCell).FlatStyle = FlatStyle.Flat;
                        foreach (var validValue in configItem.ValidValues)
                            (dataGridViewCell as DataGridViewComboBoxCell).Items.Add(validValue);
                        if (!(dataGridViewCell as DataGridViewComboBoxCell).Items.Contains(configItem.Value))
                            (dataGridViewCell as DataGridViewComboBoxCell).Items.Add(configItem.Value);
                    }
                    else if (configItem.ItemType == typeof(bool))
                    {
                        dataGridViewCell = new DataGridViewComboBoxCell();
                        (dataGridViewCell as DataGridViewComboBoxCell).FlatStyle = FlatStyle.Flat;
                        (dataGridViewCell as DataGridViewComboBoxCell).Items.Add("Yes");
                        (dataGridViewCell as DataGridViewComboBoxCell).Items.Add("No");
                        (dataGridViewCell as DataGridViewComboBoxCell).Value =
                            configItem.Value.ToString().ToLower() == "true" ? "Yes" : (object)"No";
                    }

                    if (dataGridViewCell != null)
                        row.Cells[1] = dataGridViewCell;
                    if (configItem.ItemType != typeof(bool))
                        row.Cells[1].Value = configItem.Value;
                    row.Cells[1].ReadOnly = configItem.ReadOnly;
                    if (configItem.ReadOnly)
                        row.DefaultCellStyle.ForeColor = Color.Gray;
                    row.Cells[1].Tag = configItem;
                    SetStyle(row.Cells[1]);
                }
        }

        private void ListBoxSelectionChangedInner()
        {
            ConfigPropertiesItemChangedInner();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_configItemListBox = new ListBox();
            m_propertyDataGridView = new DataGridView();
            Property = new DataGridViewTextBoxColumn();
            Value = new DataGridViewTextBoxColumn();
            m_descriptionTB = new TextBox();
            ((ISupportInitialize)m_propertyDataGridView).BeginInit();
            SuspendLayout();
            m_configItemListBox.FormattingEnabled = true;
            m_configItemListBox.Location = new Point(3, 3);
            m_configItemListBox.Name = "m_configItemListBox";
            m_configItemListBox.Size = new Size(344, 95);
            m_configItemListBox.TabIndex = 0;
            m_propertyDataGridView.AllowUserToAddRows = false;
            m_propertyDataGridView.AllowUserToDeleteRows = false;
            m_propertyDataGridView.AllowUserToResizeRows = false;
            m_propertyDataGridView.Anchor =
                AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            m_propertyDataGridView.BackgroundColor = SystemColors.Window;
            m_propertyDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            m_propertyDataGridView.Columns.AddRange(Property, Value);
            m_propertyDataGridView.Location = new Point(3, 135);
            m_propertyDataGridView.MultiSelect = false;
            m_propertyDataGridView.Name = "m_propertyDataGridView";
            m_propertyDataGridView.RowHeadersVisible = false;
            m_propertyDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            m_propertyDataGridView.Size = new Size(340, 330);
            m_propertyDataGridView.TabIndex = 1;
            Property.HeaderText = "Property";
            Property.Name = "Property";
            Property.ReadOnly = true;
            Value.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Value.HeaderText = "Value";
            Value.Name = "Value";
            m_descriptionTB.Location = new Point(3, 471);
            m_descriptionTB.Multiline = true;
            m_descriptionTB.Name = "m_descriptionTB";
            m_descriptionTB.ReadOnly = true;
            m_descriptionTB.Size = new Size(340, 66);
            m_descriptionTB.TabIndex = 2;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(m_descriptionTB);
            Controls.Add(m_propertyDataGridView);
            Controls.Add(m_configItemListBox);
            Name = nameof(ConfigurationPanel);
            Size = new Size(350, 540);
            ((ISupportInitialize)m_propertyDataGridView).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}