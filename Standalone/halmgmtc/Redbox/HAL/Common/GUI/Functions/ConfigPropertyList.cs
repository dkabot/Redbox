using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions.Properties;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Common.GUI.Functions
{
    public class ConfigPropertyList : UserControl, IDisposable
    {
        public delegate void ConfigItemChanged();

        public delegate void SaveCompleted();

        private readonly XmlDocument m_camera;
        private readonly List<ConfigItem> m_cameraConfig = new List<ConfigItem>();
        private readonly XmlDocument m_controller;
        private readonly List<ConfigItem> m_controllerConfig = new List<ConfigItem>();
        private readonly DataGridViewCellStyle m_defaultStyle;
        private readonly DataGridViewCellStyle m_valueChangedStyle;
        private readonly HardwareService Service;
        private IContainer components;
        private bool IsDirty;
        private Button m_alphDescSortButton;
        private Button m_alphSortButton;
        private ListBox m_configItemListBox;
        private Label m_descriptionLabel;
        private Label m_filterLabel;
        private TextBox m_filterTextBox;
        private DataGridView m_propertyDataGridView;
        private Button m_resetChangesButton;
        private Panel panel1;
        private DataGridViewTextBoxColumn Property;
        private DataGridViewTextBoxColumn Value;

        public ConfigPropertyList(HardwareService service)
        {
            InitializeComponent();
            Service = service;
            m_defaultStyle = m_propertyDataGridView.DefaultCellStyle;
            m_valueChangedStyle = new DataGridViewCellStyle
            {
                Font = new Font(Font.Name, Font.Size, FontStyle.Bold)
            };
            SetToolTips();
            m_configItemListBox.SelectedIndexChanged += OnListBoxSelectionChanged;
            m_propertyDataGridView.CellEndEdit += OnDataChanged;
            m_propertyDataGridView.SelectionChanged += OnConfigPropertiesItemChanged;
            m_propertyDataGridView.CellClick += OnButtonClick;
            m_configItemListBox.Items.Clear();
            m_propertyDataGridView.Rows.Clear();
            m_descriptionLabel.Text = string.Empty;
            m_camera = LoadConfigurationInner(Configuration.Camera, m_cameraConfig);
            if (m_camera != null)
                m_configItemListBox.Items.Add(Configuration.Camera);
            m_controller = LoadConfigurationInner(Configuration.Controller, m_controllerConfig);
            if (m_controller != null)
                m_configItemListBox.Items.Add(Configuration.Controller);
            IsDirty = false;
            Dock = DockStyle.Fill;
            Disposed += ConfigPropertyList_Disposed;
        }

        public event ConfigItemChanged OnConfigItemChange;

        public event SaveCompleted OnSave;

        private void ConfigPropertyList_Disposed(object sender, EventArgs e)
        {
            m_descriptionLabel.Text = string.Empty;
            m_configItemListBox.Items.Clear();
            m_propertyDataGridView.Rows.Clear();
        }

        public void Save()
        {
            if (IsDirty)
            {
                if (m_cameraConfig != null)
                {
                    if (!Service.SetConfiguration("camera", m_camera.InnerXml).Success)
                    {
                        SaveFailed();
                        return;
                    }

                    foreach (var configItem in m_cameraConfig)
                        configItem.DefaultValue = configItem.Value;
                }

                if (m_controllerConfig != null)
                {
                    if (!Service.SetConfiguration("controller", m_controller.InnerXml).Success)
                    {
                        SaveFailed();
                        return;
                    }

                    foreach (var configItem in m_controllerConfig)
                        configItem.DefaultValue = configItem.Value;
                }
            }

            m_propertyDataGridView.SuspendLayout();
            ListBoxSelectionChangedInner();
            m_propertyDataGridView.ResumeLayout();
            if (OnSave != null)
                OnSave();
            IsDirty = false;
        }

        private void SetToolTips()
        {
            var toolTip1 = new ToolTip();
            var toolTip2 = new ToolTip();
            var toolTip3 = new ToolTip();
            toolTip1.AutoPopDelay = toolTip2.AutoPopDelay = toolTip3.AutoPopDelay = 5000;
            toolTip1.InitialDelay = toolTip2.InitialDelay = toolTip3.InitialDelay = 1000;
            toolTip1.ReshowDelay = toolTip2.ReshowDelay = toolTip3.ReshowDelay = 500;
            toolTip1.ShowAlways = toolTip2.ShowAlways = toolTip3.ShowAlways = true;
            toolTip1.SetToolTip(m_alphSortButton, "Sort Ascending");
            toolTip2.SetToolTip(m_alphDescSortButton, "Sort Descending");
            toolTip3.SetToolTip(m_resetChangesButton, "Reset All Configuration to Defaults");
        }

        private void SaveFailed()
        {
            var num = (int)MessageBox.Show("Save Failed!");
        }

        private XmlDocument LoadConfigurationInner(Configuration conf, List<ConfigItem> list)
        {
            var configuration = Service.GetConfiguration(conf.ToString());
            if (!configuration.Success)
                return null;
            var xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.LoadXml(configuration.CommandMessages[0]);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An exception was thrown when trying to load configuration from Hal", ex,
                    LogEntryType.Error);
                return null;
            }

            foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
                if (childNode.ChildNodes.Count > 0 && childNode.ChildNodes[0].Name == "property")
                {
                    CreateSubNodeConfigItems(childNode, list);
                }
                else
                {
                    var configItem = new ConfigItem(childNode);
                    if (configItem != null)
                        list.Add(configItem);
                }

            return xmlDocument;
        }

        private void CreateSubNodeConfigItems(XmlNode node, List<ConfigItem> list)
        {
            var attributeValue = node.GetAttributeValue<string>("display-name");
            foreach (XmlNode childNode in node.ChildNodes)
            {
                var configItem = new ConfigItem(childNode);
                configItem.DisplayName = attributeValue + " - " + configItem.DisplayName;
                list.Add(configItem);
            }
        }

        private void PopulateDataGrid(List<ConfigItem> list)
        {
            if (list.Count <= 0)
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

        private void OnListBoxSelectionChanged(object sender, EventArgs args)
        {
            ListBoxSelectionChangedInner();
        }

        private void ListBoxSelectionChangedInner()
        {
            PopulateDataGrid(m_configItemListBox.SelectedIndex == 0 ? m_cameraConfig : m_controllerConfig);
            m_filterTextBox.Text = string.Empty;
            ConfigPropertiesItemChangedInner();
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
            if (tag.ItemType == typeof(int) && !int.TryParse(obj.ToString(), out _))
            {
                var num = (int)MessageBox.Show("Data entered is not an integer");
                cell.Value = tag.Value;
            }
            else
            {
                tag.Value = obj;
                SetStyle(cell);
                if (OnConfigItemChange != null)
                    OnConfigItemChange();
                IsDirty = true;
            }
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

        private void OnConfigPropertiesItemChanged(object sender, EventArgs e)
        {
            ConfigPropertiesItemChangedInner();
        }

        private void ConfigPropertiesItemChangedInner()
        {
            var selectedRows = m_propertyDataGridView.SelectedRows;
            if (selectedRows == null || selectedRows.Count <= 0 || selectedRows[0].Cells[1].Tag == null)
                m_descriptionLabel.Text = string.Empty;
            else if (!(selectedRows[0].Cells[1].Tag is ConfigItem tag))
                m_descriptionLabel.Text = string.Empty;
            else
                m_descriptionLabel.Text = tag.Description;
        }

        private void OnReset(object sender, EventArgs e)
        {
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
            var num = (int)MessageBox.Show("Successfully Reset Properties to Previously Loaded Values.", string.Empty,
                MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void OnButtonClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.RowIndex > m_propertyDataGridView.Rows.Count - 1 ||
                e.ColumnIndex > 1)
                return;
            var cell = m_propertyDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewButtonCell;
        }

        private void m_alphSortButton_Click(object sender, EventArgs e)
        {
            m_propertyDataGridView.Sort(new RowComparerAscending());
        }

        private void m_alphSortDescButton_Click(object sender, EventArgs e)
        {
            m_propertyDataGridView.Sort(new RowComparerDescending());
        }

        private void m_filterTextBox_TextChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in m_propertyDataGridView.Rows)
                if (string.IsNullOrEmpty(m_filterTextBox.Text))
                {
                    row.Visible = true;
                }
                else
                {
                    var dataGridViewRow = row;
                    dataGridViewRow.Visible = dataGridViewRow.Cells[0].Value.ToString().ToLower()
                        .Contains(m_filterTextBox.Text.ToLower());
                }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_descriptionLabel = new Label();
            m_configItemListBox = new ListBox();
            m_propertyDataGridView = new DataGridView();
            Property = new DataGridViewTextBoxColumn();
            Value = new DataGridViewTextBoxColumn();
            panel1 = new Panel();
            m_resetChangesButton = new Button();
            m_alphDescSortButton = new Button();
            m_filterLabel = new Label();
            m_filterTextBox = new TextBox();
            m_alphSortButton = new Button();
            ((ISupportInitialize)m_propertyDataGridView).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            m_descriptionLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            m_descriptionLabel.BorderStyle = BorderStyle.FixedSingle;
            m_descriptionLabel.Location = new Point(6, 431);
            m_descriptionLabel.Name = "m_descriptionLabel";
            m_descriptionLabel.Padding = new Padding(10);
            m_descriptionLabel.Size = new Size(342, 97);
            m_descriptionLabel.TabIndex = 0;
            m_descriptionLabel.TextAlign = ContentAlignment.MiddleLeft;
            m_configItemListBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            m_configItemListBox.FormattingEnabled = true;
            m_configItemListBox.Location = new Point(6, 3);
            m_configItemListBox.Name = "m_configItemListBox";
            m_configItemListBox.Size = new Size(342, 56);
            m_configItemListBox.TabIndex = 1;
            m_propertyDataGridView.AllowUserToAddRows = false;
            m_propertyDataGridView.AllowUserToDeleteRows = false;
            m_propertyDataGridView.AllowUserToResizeRows = false;
            m_propertyDataGridView.Anchor =
                AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            m_propertyDataGridView.BackgroundColor = SystemColors.Window;
            m_propertyDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            m_propertyDataGridView.Columns.AddRange(Property, Value);
            m_propertyDataGridView.Location = new Point(6, 101);
            m_propertyDataGridView.MultiSelect = false;
            m_propertyDataGridView.Name = "m_propertyDataGridView";
            m_propertyDataGridView.RowHeadersVisible = false;
            m_propertyDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            m_propertyDataGridView.Size = new Size(342, 327);
            m_propertyDataGridView.TabIndex = 2;
            Property.HeaderText = "Property";
            Property.Name = "Property";
            Property.Width = 125;
            Value.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Value.HeaderText = "Value";
            Value.Name = "Value";
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Controls.Add(m_resetChangesButton);
            panel1.Controls.Add(m_alphDescSortButton);
            panel1.Controls.Add(m_filterLabel);
            panel1.Controls.Add(m_filterTextBox);
            panel1.Controls.Add(m_alphSortButton);
            panel1.Location = new Point(6, 65);
            panel1.Name = "panel1";
            panel1.Size = new Size(342, 30);
            panel1.TabIndex = 3;
            m_resetChangesButton.BackColor = Color.Transparent;
            m_resetChangesButton.BackgroundImageLayout = ImageLayout.None;
            m_resetChangesButton.Enabled = false;
            m_resetChangesButton.Image = Resources.undo;
            m_resetChangesButton.Location = new Point(72, 3);
            m_resetChangesButton.Name = "m_resetChangesButton";
            m_resetChangesButton.Size = new Size(29, 27);
            m_resetChangesButton.TabIndex = 4;
            m_resetChangesButton.UseVisualStyleBackColor = false;
            m_resetChangesButton.Visible = false;
            m_resetChangesButton.Click += OnReset;
            m_alphDescSortButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            m_alphDescSortButton.BackColor = Color.Transparent;
            m_alphDescSortButton.BackgroundImageLayout = ImageLayout.None;
            m_alphDescSortButton.Image = Resources.sortDescending2;
            m_alphDescSortButton.Location = new Point(37, 3);
            m_alphDescSortButton.Name = "m_alphDescSortButton";
            m_alphDescSortButton.Size = new Size(29, 27);
            m_alphDescSortButton.TabIndex = 5;
            m_alphDescSortButton.Tag = "Sort Descending";
            m_alphDescSortButton.UseVisualStyleBackColor = false;
            m_alphDescSortButton.Click += m_alphSortDescButton_Click;
            m_filterLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            m_filterLabel.Location = new Point(107, 6);
            m_filterLabel.Name = "m_filterLabel";
            m_filterLabel.Size = new Size(50, 20);
            m_filterLabel.TabIndex = 4;
            m_filterLabel.Text = "Filter: ";
            m_filterLabel.TextAlign = ContentAlignment.MiddleRight;
            m_filterTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            m_filterTextBox.Location = new Point(163, 7);
            m_filterTextBox.Name = "m_filterTextBox";
            m_filterTextBox.Size = new Size(176, 20);
            m_filterTextBox.TabIndex = 1;
            m_filterTextBox.TextChanged += m_filterTextBox_TextChanged;
            m_alphSortButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            m_alphSortButton.BackColor = Color.Transparent;
            m_alphSortButton.BackgroundImageLayout = ImageLayout.None;
            m_alphSortButton.Image = Resources.sortAscending2;
            m_alphSortButton.Location = new Point(3, 3);
            m_alphSortButton.Name = "m_alphSortButton";
            m_alphSortButton.Size = new Size(28, 27);
            m_alphSortButton.TabIndex = 0;
            m_alphSortButton.Tag = "Sort Ascending";
            m_alphSortButton.UseVisualStyleBackColor = false;
            m_alphSortButton.Click += m_alphSortButton_Click;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.WhiteSmoke;
            Controls.Add(panel1);
            Controls.Add(m_propertyDataGridView);
            Controls.Add(m_configItemListBox);
            Controls.Add(m_descriptionLabel);
            Name = nameof(ConfigPropertyList);
            Size = new Size(351, 538);
            ((ISupportInitialize)m_propertyDataGridView).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        private class RowComparerAscending : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return (x as DataGridViewRow).Cells[0].Value.ToString()
                    .CompareTo((y as DataGridViewRow).Cells[0].Value.ToString());
            }
        }

        private class RowComparerDescending : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return (y as DataGridViewRow).Cells[0].Value.ToString()
                    .CompareTo((x as DataGridViewRow).Cells[0].Value.ToString());
            }
        }
    }
}