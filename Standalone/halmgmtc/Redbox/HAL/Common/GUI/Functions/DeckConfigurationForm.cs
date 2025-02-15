using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Redbox.HAL.Client;

namespace Redbox.HAL.Common.GUI.Functions
{
    public class DeckConfigurationForm : Form
    {
        private readonly bool m_allowInventoryUpdate;
        private readonly HardwareService m_hardwareService;
        private IContainer components;
        private DataGridViewTextBoxColumn m_deckTextColumn;
        private Button m_exitButton;
        private DataGridViewTextBoxColumn m_fillerColumn;
        private Button m_fixSlotAlignment;
        private DataGridViewTextBoxColumn m_isQlmTextColumn;
        private DataGridViewTextBoxColumn m_numSlotsTextColumn;
        private DataGridViewTextBoxColumn m_offsetTextColumn;
        private Button m_okButton;
        private Button m_propertiesButton;
        private DataGridView m_sandGrid;
        private DataGridViewTextBoxColumn m_slotWidthTextColumn;

        public DeckConfigurationForm(HardwareService service, bool allowInventoryUpdate)
        {
            Manager = new DecksConfigurationManager(service);
            m_hardwareService = service;
            InitializeComponent();
            m_allowInventoryUpdate = allowInventoryUpdate;
            RefreshList();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DecksConfigurationManager Manager { get; }

        private void OnOK(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void OnCancel(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OnProperties(object sender, EventArgs e)
        {
            var selectedRow = m_sandGrid.SelectedRows[0];
            if (selectedRow == null || !(selectedRow.Tag is XmlNode tag))
                return;
            var attributeValue = tag.GetAttributeValue<bool>("IsQlm");
            if (new DeckConfigurationDetailForm(m_allowInventoryUpdate, Manager)
                {
                    Number = tag.GetAttributeValue<int?>("Number"),
                    YOffset = tag.GetAttributeValue<int?>("Offset"),
                    NumberOfSlots = tag.GetAttributeValue<int?>("NumberOfSlots"),
                    SlotWidth = tag.GetAttributeValue<decimal?>("SlotWidth"),
                    SellThruSlots = tag.GetAttributeValue<int?>("SellThruSlots"),
                    SellThruOffset = tag.GetAttributeValue<int?>("SellThruOffset"),
                    ApproachOffset = tag.GetAttributeValue<int?>("ApproachOffset"),
                    SlotsPerQuadrant = tag.GetAttributeValue<int?>("SlotsPerQuadrant"),
                    IsQlmDeck = attributeValue,
                    Service = m_hardwareService,
                    ShowDumpSlot = false
                }.ShowDialog() != DialogResult.OK)
                return;
            RefreshList();
            m_okButton.Enabled = true;
        }

        private void OnSelectedRowChanged(object sender, EventArgs e)
        {
            m_propertiesButton.Enabled = m_sandGrid.SelectedRows.Count > 0;
        }

        private void RefreshList()
        {
            m_sandGrid.SuspendLayout();
            m_sandGrid.Rows.Clear();
            foreach (XmlNode allDeckNode in Manager.FindAllDeckNodes())
            {
                var attributeValue1 = allDeckNode.GetAttributeValue<int>("Offset");
                var attributeValue2 = allDeckNode.GetAttributeValue<int>("Number");
                var attributeValue3 = allDeckNode.GetAttributeValue<bool>("IsQlm");
                var attributeValue4 = allDeckNode.GetAttributeValue<int>("NumberOfSlots");
                var attributeValue5 = allDeckNode.GetAttributeValue<decimal>("SlotWidth");
                var dataGridViewRow1 = new DataGridViewRow();
                dataGridViewRow1.Tag = allDeckNode;
                var dataGridViewRow2 = dataGridViewRow1;
                dataGridViewRow2.Cells.AddRange(new DataGridViewTextBoxCell(), new DataGridViewTextBoxCell(),
                    new DataGridViewTextBoxCell(), new DataGridViewTextBoxCell(), new DataGridViewTextBoxCell());
                var index = m_sandGrid.Rows.Add(dataGridViewRow2);
                m_sandGrid.Rows[index].Cells[0].Value = attributeValue2;
                m_sandGrid.Rows[index].Cells[1].Value = attributeValue1;
                m_sandGrid.Rows[index].Cells[2].Value = attributeValue5;
                m_sandGrid.Rows[index].Cells[3].Value = attributeValue4;
                m_sandGrid.Rows[index].Cells[4].Value = attributeValue3;
            }

            m_sandGrid.ResumeLayout();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (new EditSlotData(m_hardwareService, Manager).ShowDialog() != DialogResult.OK)
                return;
            RefreshList();
        }

        private void m_exitButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_okButton = new Button();
            m_sandGrid = new DataGridView();
            m_deckTextColumn = new DataGridViewTextBoxColumn();
            m_offsetTextColumn = new DataGridViewTextBoxColumn();
            m_slotWidthTextColumn = new DataGridViewTextBoxColumn();
            m_numSlotsTextColumn = new DataGridViewTextBoxColumn();
            m_isQlmTextColumn = new DataGridViewTextBoxColumn();
            m_fillerColumn = new DataGridViewTextBoxColumn();
            m_propertiesButton = new Button();
            m_fixSlotAlignment = new Button();
            m_exitButton = new Button();
            ((ISupportInitialize)m_sandGrid).BeginInit();
            SuspendLayout();
            m_okButton.BackColor = Color.LightGray;
            m_okButton.Enabled = false;
            m_okButton.Location = new Point(434, 12);
            m_okButton.Name = "m_okButton";
            m_okButton.Size = new Size(75, 23);
            m_okButton.TabIndex = 5;
            m_okButton.Text = "Save";
            m_okButton.UseVisualStyleBackColor = false;
            m_okButton.Visible = false;
            m_okButton.Click += OnOK;
            m_sandGrid.AllowUserToAddRows = false;
            m_sandGrid.AllowUserToDeleteRows = false;
            m_sandGrid.BackgroundColor = SystemColors.Window;
            m_sandGrid.Columns.AddRange(m_deckTextColumn, m_offsetTextColumn, m_slotWidthTextColumn,
                m_numSlotsTextColumn, m_isQlmTextColumn, m_fillerColumn);
            m_sandGrid.Location = new Point(12, 12);
            m_sandGrid.Name = "m_sandGrid";
            m_sandGrid.ReadOnly = true;
            m_sandGrid.RowHeadersWidth = 24;
            m_sandGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            m_sandGrid.Size = new Size(416, 289);
            m_sandGrid.TabIndex = 0;
            m_sandGrid.SelectionChanged += OnSelectedRowChanged;
            m_deckTextColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            m_deckTextColumn.Frozen = true;
            m_deckTextColumn.HeaderText = "Deck #";
            m_deckTextColumn.Name = "m_deckTextColumn";
            m_deckTextColumn.ReadOnly = true;
            m_deckTextColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            m_deckTextColumn.Width = 49;
            m_offsetTextColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            m_offsetTextColumn.Frozen = true;
            m_offsetTextColumn.HeaderText = "Y Axis Offset";
            m_offsetTextColumn.Name = "m_offsetTextColumn";
            m_offsetTextColumn.ReadOnly = true;
            m_offsetTextColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            m_offsetTextColumn.Width = 73;
            m_slotWidthTextColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            m_slotWidthTextColumn.Frozen = true;
            m_slotWidthTextColumn.HeaderText = "Slot Width";
            m_slotWidthTextColumn.Name = "m_slotWidthTextColumn";
            m_slotWidthTextColumn.ReadOnly = true;
            m_slotWidthTextColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            m_slotWidthTextColumn.Width = 62;
            m_numSlotsTextColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            m_numSlotsTextColumn.Frozen = true;
            m_numSlotsTextColumn.HeaderText = "# Slots";
            m_numSlotsTextColumn.Name = "m_numSlotsTextColumn";
            m_numSlotsTextColumn.ReadOnly = true;
            m_numSlotsTextColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            m_numSlotsTextColumn.Width = 46;
            m_isQlmTextColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            m_isQlmTextColumn.Frozen = true;
            m_isQlmTextColumn.HeaderText = "QLM Deck?";
            m_isQlmTextColumn.Name = "m_isQlmTextColumn";
            m_isQlmTextColumn.ReadOnly = true;
            m_isQlmTextColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            m_isQlmTextColumn.Width = 71;
            m_fillerColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            m_fillerColumn.HeaderText = "";
            m_fillerColumn.Name = "m_fillerColumn";
            m_fillerColumn.ReadOnly = true;
            m_propertiesButton.BackColor = Color.LightGray;
            m_propertiesButton.Enabled = false;
            m_propertiesButton.Location = new Point(179, 307);
            m_propertiesButton.Name = "m_propertiesButton";
            m_propertiesButton.Size = new Size(75, 23);
            m_propertiesButton.TabIndex = 3;
            m_propertiesButton.Text = "Properties...";
            m_propertiesButton.UseVisualStyleBackColor = false;
            m_propertiesButton.Click += OnProperties;
            m_fixSlotAlignment.BackColor = Color.LightGray;
            m_fixSlotAlignment.Location = new Point(274, 307);
            m_fixSlotAlignment.Name = "m_fixSlotAlignment";
            m_fixSlotAlignment.Size = new Size(100, 23);
            m_fixSlotAlignment.TabIndex = 7;
            m_fixSlotAlignment.Text = "Fix Slot Alignment";
            m_fixSlotAlignment.UseVisualStyleBackColor = false;
            m_fixSlotAlignment.Click += button1_Click;
            m_exitButton.BackColor = Color.LightGray;
            m_exitButton.Location = new Point(434, 302);
            m_exitButton.Name = "m_exitButton";
            m_exitButton.Size = new Size(75, 32);
            m_exitButton.TabIndex = 8;
            m_exitButton.Text = "Close";
            m_exitButton.UseVisualStyleBackColor = false;
            m_exitButton.Click += m_exitButton_Click;
            AcceptButton = m_okButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(521, 342);
            ControlBox = false;
            Controls.Add(m_exitButton);
            Controls.Add(m_fixSlotAlignment);
            Controls.Add(m_propertiesButton);
            Controls.Add(m_sandGrid);
            Controls.Add(m_okButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = nameof(DeckConfigurationForm);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Deck Configuration Editor";
            ((ISupportInitialize)m_sandGrid).EndInit();
            ResumeLayout(false);
        }
    }
}