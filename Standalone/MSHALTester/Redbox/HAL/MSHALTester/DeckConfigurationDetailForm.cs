using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.MSHALTester;

public class DeckConfigurationDetailForm : Form
{
    private IContainer components;
    private DataGridViewTextBoxColumn End;
    private DataGridViewCheckBoxColumn Excluded;
    private Label label9;
    private bool m_allowSlotDataEdit;
    private Label m_approachOffsetLabel;
    private TextBox m_approachOffsetTextBox;
    private Button m_cancelButton;
    private Button m_computeQuadrantsButton;
    private Button m_editSlotDataButton;
    private ErrorProvider m_errorProvider;
    private CheckBox m_hasDumpSlotCheckBox;
    private CheckBox m_isQlmDeckCheckBox;
    private Label m_numberLabel;
    private Label m_numberOfSlotsLabel;
    private TextBox m_numberOfSlotsTextBox;
    private TextBox m_numberTextBox;
    private Button m_okButton;
    private DataGridView m_quadrantDataGridView;
    private Label m_sellThruOffsetLabel;
    private TextBox m_sellThruOffsetTextBox;
    private Label m_sellThruSlotsLabel;
    private TextBox m_sellThruSlotsTextBox;
    private bool m_showDumpSlot;
    private Label m_slotsPerQuadrantLabel;
    private TextBox m_slotsPerQuadrantTextBox;
    private Label m_slotWidthLabel;
    private TextBox m_slotWidthTextBox;
    private Label m_yOffsetLabel;
    private TextBox m_yoffsetTextBox;
    private DataGridViewTextBoxColumn Offset;
    private DataGridViewTextBoxColumn Quadrant;
    private DataGridViewTextBoxColumn Start;

    public DeckConfigurationDetailForm(bool allowExcludeUpdates, DecksConfigurationManager manager)
    {
        InitializeComponent();
        AllowExcludeUpdates = allowExcludeUpdates;
        Manager = manager;
        m_quadrantDataGridView.Columns[DeckConfigurationDetailColumns.Excluded].ReadOnly = !AllowExcludeUpdates;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool AllowSlotDataEdit
    {
        get => m_allowSlotDataEdit;
        set
        {
            m_allowSlotDataEdit = value;
            m_editSlotDataButton.Visible = m_allowSlotDataEdit;
            m_editSlotDataButton.Enabled = m_allowSlotDataEdit;
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int? Number
    {
        get
        {
            int result;
            return int.TryParse(m_numberTextBox.Text, out result) ? result : new int?();
        }
        set
        {
            m_numberTextBox.Clear();
            if (!value.HasValue)
                return;
            m_numberTextBox.Text = value.ToString();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int? YOffset
    {
        get
        {
            int result;
            return int.TryParse(m_yoffsetTextBox.Text, out result) ? result : new int?();
        }
        set
        {
            if (!value.HasValue)
                return;
            m_yoffsetTextBox.Clear();
            m_yoffsetTextBox.Text = value.ToString();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int? NumberOfSlots
    {
        get
        {
            int result;
            return int.TryParse(m_numberOfSlotsTextBox.Text, out result) ? result : new int?();
        }
        set
        {
            m_numberOfSlotsTextBox.Clear();
            if (!value.HasValue)
                return;
            m_numberOfSlotsTextBox.Text = value.ToString();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public decimal? SlotWidth
    {
        get
        {
            decimal result;
            return decimal.TryParse(m_slotWidthTextBox.Text, out result) ? result : new decimal?();
        }
        set
        {
            m_slotWidthTextBox.Clear();
            if (!value.HasValue)
                return;
            m_slotWidthTextBox.Text = value.ToString();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int? SellThruSlots
    {
        get
        {
            int result;
            return int.TryParse(m_sellThruSlotsTextBox.Text, out result) ? result : new int?();
        }
        set
        {
            m_sellThruSlotsTextBox.Clear();
            if (!value.HasValue)
                return;
            m_sellThruSlotsTextBox.Text = value.ToString();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int? SellThruOffset
    {
        get
        {
            int result;
            return int.TryParse(m_sellThruOffsetTextBox.Text, out result) ? result : new int?();
        }
        set
        {
            m_sellThruOffsetTextBox.Clear();
            if (!value.HasValue)
                return;
            m_sellThruOffsetTextBox.Text = value.ToString();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int? ApproachOffset
    {
        get
        {
            int result;
            return int.TryParse(m_approachOffsetTextBox.Text, out result) ? result : new int?();
        }
        set
        {
            m_approachOffsetTextBox.Clear();
            if (!value.HasValue)
                return;
            m_approachOffsetTextBox.Text = value.ToString();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int? SlotsPerQuadrant
    {
        get
        {
            int result;
            return int.TryParse(m_slotsPerQuadrantTextBox.Text, out result) ? result : new int?();
        }
        set
        {
            m_slotsPerQuadrantTextBox.Clear();
            if (!value.HasValue)
                return;
            m_slotsPerQuadrantTextBox.Text = value.ToString();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsQlmDeck
    {
        get => m_isQlmDeckCheckBox.Checked;
        set
        {
            m_isQlmDeckCheckBox.Checked = value;
            m_computeQuadrantsButton.Enabled = !value;
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool HasDumpSlot
    {
        get => m_hasDumpSlotCheckBox.Checked;
        set => m_hasDumpSlotCheckBox.Checked = value;
    }

    public bool ShowDumpSlot
    {
        get => m_showDumpSlot;
        set
        {
            m_showDumpSlot = value;
            m_hasDumpSlotCheckBox.Visible = m_showDumpSlot;
            m_hasDumpSlotCheckBox.Checked = value;
        }
    }

    public HardwareService Service { get; set; }

    public DecksConfigurationManager Manager { get; }

    private bool AllowExcludeUpdates { get; }

    private XmlNode WorkingNode { get; set; }

    public void EnableAddRemove()
    {
        m_isQlmDeckCheckBox.Enabled = false;
    }

    private void OnLoad(object sender, EventArgs e)
    {
        if (!AllowExcludeUpdates)
        {
            m_quadrantDataGridView.Columns[DeckConfigurationDetailColumns.Excluded].DefaultCellStyle.BackColor =
                Color.LightGray;
            m_quadrantDataGridView.Columns[DeckConfigurationDetailColumns.Excluded].DefaultCellStyle.ForeColor =
                Color.Gray;
            ((DataGridViewCheckBoxColumn)m_quadrantDataGridView.Columns[DeckConfigurationDetailColumns.Excluded])
                .FlatStyle = FlatStyle.Flat;
        }

        if (m_isQlmDeckCheckBox.Checked)
        {
            m_quadrantDataGridView.Columns[DeckConfigurationDetailColumns.Excluded].Visible = false;
            m_approachOffsetLabel.Visible = true;
            m_approachOffsetTextBox.Visible = true;
            m_editSlotDataButton.Enabled = false;
        }

        m_hasDumpSlotCheckBox.Enabled = ShowDumpSlot;
        WorkingNode = Manager.FindDeckNode(Number.Value).CloneNode(true);
        RefreshView();
    }

    private void OnOK(object sender, EventArgs e)
    {
        m_errorProvider.SetError(m_numberTextBox, string.Empty);
        m_errorProvider.SetError(m_yoffsetTextBox, string.Empty);
        m_errorProvider.SetError(m_slotWidthTextBox, string.Empty);
        m_errorProvider.SetError(m_numberOfSlotsTextBox, string.Empty);
        if (!YOffset.HasValue)
        {
            m_errorProvider.SetError(m_yoffsetTextBox, "Y Offset is required field.");
        }
        else if (!SlotWidth.HasValue)
        {
            m_errorProvider.SetError(m_slotWidthTextBox, "Slot Width is a required field.");
        }
        else
        {
            var intList = new List<int>();
            var deckNode = Manager.FindDeckNode(Number.Value);
            for (var index = 0; index < WorkingNode.ChildNodes.Count; ++index)
            {
                var childNode1 = deckNode.ChildNodes[index];
                var childNode2 = WorkingNode.ChildNodes[index];
                if (!childNode1.GetAttributeValue("IsExcluded", false) && IsQuadrantExcluded(index))
                    intList.Add(index);
            }

            if (intList.Count == 0)
            {
                UpdateOriginalNode(deckNode);
                Manager.FlushChanges(false);
                DialogResult = DialogResult.OK;
                Close();
            }
            else if (DialogResult.Cancel == MessageBox.Show(this,
                         "This will update the inventory for all slots in excluded quadrants to EMPTY.  You MUST remove ALL discs in the excluded quadrants prior to excluding the quadrant(s)." +
                         Environment.NewLine + "To cancel this operation, click ‘Cancel.’",
                         "You are about to exclude sections of a deck.", MessageBoxButtons.OKCancel))
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            else
            {
                UpdateOriginalNode(deckNode);
                Manager.FlushChanges(false);
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }

    private void OnCancel(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private XmlNode UpdateOriginalNode(XmlNode originalNode)
    {
        originalNode.SetAttributeValue("Number", Number.Value);
        originalNode.SetAttributeValue("Offset", YOffset.Value);
        originalNode.SetAttributeValue("NumberOfSlots", NumberOfSlots.Value);
        originalNode.SetAttributeValue("SlotWidth", SlotWidth.Value);
        originalNode.SetAttributeValue("IsQlm", IsQlmDeck);
        originalNode.SetAttributeValue("SellThruSlots", SellThruSlots);
        originalNode.SetAttributeValue("SellThruOffset", SellThruOffset);
        originalNode.SetAttributeValue("ApproachOffset", ApproachOffset);
        originalNode.SetAttributeValue("SlotsPerQuadrant", SlotsPerQuadrant);
        for (var index = 0; index < WorkingNode.ChildNodes.Count; ++index)
        {
            var childNode1 = originalNode.ChildNodes[index];
            var childNode2 = WorkingNode.ChildNodes[index];
            childNode1.SetAttributeValue("IsExcluded", IsQuadrantExcluded(index));
            childNode1.SetAttributeValue("Offset", m_quadrantDataGridView[1, index].Value);
        }

        return originalNode;
    }

    private void RefreshView()
    {
        m_quadrantDataGridView.SuspendLayout();
        m_quadrantDataGridView.Rows.Clear();
        var num = 1;
        foreach (XmlNode childNode in WorkingNode.ChildNodes)
        {
            var attributeValue1 = childNode.GetAttributeValue<int>("Offset");
            var attributeValue2 = childNode.GetAttributeValue<int?>("StartSlot");
            var attributeValue3 = childNode.GetAttributeValue<int?>("EndSlot");
            var attributeValue4 = childNode.GetAttributeValue("IsExcluded", false);
            m_quadrantDataGridView.Rows.Add(num++, attributeValue1, attributeValue2, attributeValue3, attributeValue4);
        }

        m_quadrantDataGridView.ResumeLayout();
    }

    private void OnComputeQuadrants(object sender, EventArgs e)
    {
        if (SlotWidth.HasValue)
        {
            var slotWidth = SlotWidth;
            var num1 = 0M;
            if (!((slotWidth.GetValueOrDefault() <= num1) & slotWidth.HasValue))
            {
                m_errorProvider.SetError(m_slotWidthTextBox, string.Empty);
                var computeQuadrantsForm1 = new ComputeQuadrantsForm();
                var numberOfSlots1 = NumberOfSlots;
                var num2 = 90;
                computeQuadrantsForm1.NumberOfQuadrants =
                    (numberOfSlots1.GetValueOrDefault() == num2) & numberOfSlots1.HasValue ? 6 : 12;
                var numberOfSlots2 = NumberOfSlots;
                var num3 = 90;
                computeQuadrantsForm1.SlotsPerQuadrant =
                    (numberOfSlots2.GetValueOrDefault() == num3) & numberOfSlots2.HasValue ? 15 : 6;
                var computeQuadrantsForm2 = computeQuadrantsForm1;
                if (DialogResult.Cancel == computeQuadrantsForm2.ShowDialog())
                    return;
                CommonFunctions.ComputeQuadrants(computeQuadrantsForm2.StartOffset,
                    computeQuadrantsForm2.NumberOfQuadrants, SellThruSlots, SellThruOffset,
                    computeQuadrantsForm2.SlotsPerQuadrant, SlotWidth, WorkingNode);
                RefreshView();
                return;
            }
        }

        m_errorProvider.SetError(m_slotWidthTextBox, "Slot Width must be a positive value.");
    }

    private void m_editSlotDataButton_Click(object sender, EventArgs e)
    {
        if (!m_allowSlotDataEdit)
            return;
        var num = (int)new EditSlotData(Service, Number.Value, Manager).ShowDialog();
        WorkingNode = Manager.FindDeckNode(Number.Value).CloneNode(true);
        RefreshView();
    }

    private void m_quadrantDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex != DeckConfigurationDetailColumns.Offset || e.RowIndex == -1)
            return;
        var numberPadForm1 = new NumberPadForm();
        numberPadForm1.Text = "Offset";
        var numberPadForm2 = numberPadForm1;
        if (numberPadForm2.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(numberPadForm2.Number))
            return;
        m_quadrantDataGridView.EditingControl.Text = numberPadForm2.Number;
    }

    private void m_quadrantDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
        if (!AllowExcludeUpdates || e.RowIndex == -1 || e.ColumnIndex != DeckConfigurationDetailColumns.Excluded)
            return;
        if ((bool)m_quadrantDataGridView.CurrentCell.FormattedValue)
            m_quadrantDataGridView.CurrentCell.Value = false;
        else
            m_quadrantDataGridView.CurrentCell.Value = true;
    }

    private void OnKeyPress(object sender, KeyPressEventArgs e)
    {
        if ((e.KeyChar >= '0' && e.KeyChar <= '9') || e.KeyChar == '\b')
            return;
        e.Handled = true;
    }

    private bool IsQuadrantExcluded(int quadrant)
    {
        var cell =
            m_quadrantDataGridView.Rows[quadrant].Cells[DeckConfigurationDetailColumns.Excluded] as
                DataGridViewCheckBoxCell;
        return cell.Value.ToString().Equals((string)cell.TrueValue, StringComparison.CurrentCultureIgnoreCase);
    }

    private void OnEditControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
    {
        m_quadrantDataGridView.EditingControl.KeyPress += OnKeyPress;
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
        var gridViewCellStyle = new DataGridViewCellStyle();
        m_okButton = new Button();
        m_cancelButton = new Button();
        m_errorProvider = new ErrorProvider(components);
        m_numberLabel = new Label();
        m_yOffsetLabel = new Label();
        m_numberOfSlotsLabel = new Label();
        m_slotWidthLabel = new Label();
        m_sellThruSlotsLabel = new Label();
        m_sellThruOffsetLabel = new Label();
        m_approachOffsetLabel = new Label();
        m_slotsPerQuadrantLabel = new Label();
        m_isQlmDeckCheckBox = new CheckBox();
        label9 = new Label();
        m_numberTextBox = new TextBox();
        m_yoffsetTextBox = new TextBox();
        m_numberOfSlotsTextBox = new TextBox();
        m_slotWidthTextBox = new TextBox();
        m_sellThruSlotsTextBox = new TextBox();
        m_sellThruOffsetTextBox = new TextBox();
        m_approachOffsetTextBox = new TextBox();
        m_slotsPerQuadrantTextBox = new TextBox();
        m_computeQuadrantsButton = new Button();
        m_hasDumpSlotCheckBox = new CheckBox();
        m_editSlotDataButton = new Button();
        m_quadrantDataGridView = new DataGridView();
        Quadrant = new DataGridViewTextBoxColumn();
        Offset = new DataGridViewTextBoxColumn();
        Start = new DataGridViewTextBoxColumn();
        End = new DataGridViewTextBoxColumn();
        Excluded = new DataGridViewCheckBoxColumn();
        ((ISupportInitialize)m_errorProvider).BeginInit();
        ((ISupportInitialize)m_quadrantDataGridView).BeginInit();
        SuspendLayout();
        m_okButton.Location = new Point(260, 456);
        m_okButton.Name = "m_okButton";
        m_okButton.Size = new Size(75, 23);
        m_okButton.TabIndex = 19;
        m_okButton.Text = "OK";
        m_okButton.UseVisualStyleBackColor = true;
        m_okButton.Click += OnOK;
        m_cancelButton.DialogResult = DialogResult.Cancel;
        m_cancelButton.Location = new Point(341, 456);
        m_cancelButton.Name = "m_cancelButton";
        m_cancelButton.Size = new Size(75, 23);
        m_cancelButton.TabIndex = 20;
        m_cancelButton.Text = "Cancel";
        m_cancelButton.UseVisualStyleBackColor = true;
        m_cancelButton.Click += OnCancel;
        m_errorProvider.ContainerControl = this;
        m_numberLabel.AutoSize = true;
        m_numberLabel.Location = new Point(12, 12);
        m_numberLabel.Name = "m_numberLabel";
        m_numberLabel.Size = new Size(47, 13);
        m_numberLabel.TabIndex = 0;
        m_numberLabel.Text = "Number:";
        m_yOffsetLabel.AutoSize = true;
        m_yOffsetLabel.Location = new Point(12, 36);
        m_yOffsetLabel.Name = "m_yOffsetLabel";
        m_yOffsetLabel.Size = new Size(48, 13);
        m_yOffsetLabel.TabIndex = 2;
        m_yOffsetLabel.Text = "Y Offset:";
        m_numberOfSlotsLabel.AutoSize = true;
        m_numberOfSlotsLabel.Location = new Point(12, 62);
        m_numberOfSlotsLabel.Name = "m_numberOfSlotsLabel";
        m_numberOfSlotsLabel.Size = new Size(87, 13);
        m_numberOfSlotsLabel.TabIndex = 4;
        m_numberOfSlotsLabel.Text = "Number Of Slots:";
        m_slotWidthLabel.AutoSize = true;
        m_slotWidthLabel.Location = new Point(12, 88);
        m_slotWidthLabel.Name = "m_slotWidthLabel";
        m_slotWidthLabel.Size = new Size(59, 13);
        m_slotWidthLabel.TabIndex = 6;
        m_slotWidthLabel.Text = "Slot Width:";
        m_sellThruSlotsLabel.AutoSize = true;
        m_sellThruSlotsLabel.Location = new Point(12, 114);
        m_sellThruSlotsLabel.Name = "m_sellThruSlotsLabel";
        m_sellThruSlotsLabel.Size = new Size(78, 13);
        m_sellThruSlotsLabel.TabIndex = 8;
        m_sellThruSlotsLabel.Text = "Sell Thru Slots:";
        m_sellThruOffsetLabel.AutoSize = true;
        m_sellThruOffsetLabel.Location = new Point(12, 140);
        m_sellThruOffsetLabel.Name = "m_sellThruOffsetLabel";
        m_sellThruOffsetLabel.Size = new Size(83, 13);
        m_sellThruOffsetLabel.TabIndex = 10;
        m_sellThruOffsetLabel.Text = "Sell Thru Offset:";
        m_approachOffsetLabel.AutoSize = true;
        m_approachOffsetLabel.Location = new Point(12, 188);
        m_approachOffsetLabel.Name = "m_approachOffsetLabel";
        m_approachOffsetLabel.Size = new Size(87, 13);
        m_approachOffsetLabel.TabIndex = 12;
        m_approachOffsetLabel.Text = "Approach Offset:";
        m_approachOffsetLabel.Visible = false;
        m_slotsPerQuadrantLabel.AutoSize = true;
        m_slotsPerQuadrantLabel.Location = new Point(12, 164);
        m_slotsPerQuadrantLabel.Name = "m_slotsPerQuadrantLabel";
        m_slotsPerQuadrantLabel.Size = new Size(99, 13);
        m_slotsPerQuadrantLabel.TabIndex = 14;
        m_slotsPerQuadrantLabel.Text = "Slots Per Quadrant:";
        m_isQlmDeckCheckBox.AutoSize = true;
        m_isQlmDeckCheckBox.Enabled = false;
        m_isQlmDeckCheckBox.Location = new Point(117, 218);
        m_isQlmDeckCheckBox.Name = "m_isQlmDeckCheckBox";
        m_isQlmDeckCheckBox.Size = new Size(95, 17);
        m_isQlmDeckCheckBox.TabIndex = 16;
        m_isQlmDeckCheckBox.Text = "Is QLM Deck?";
        m_isQlmDeckCheckBox.UseVisualStyleBackColor = true;
        label9.AutoSize = true;
        label9.BackColor = Color.Yellow;
        label9.Font = new Font("Microsoft Sans Serif", 9f, FontStyle.Bold, GraphicsUnit.Point, 0);
        label9.Location = new Point(12, 260);
        label9.Name = "label9";
        label9.Size = new Size(265, 15);
        label9.TabIndex = 17;
        label9.Text = "Double Click The Offset to Edit Slot Data";
        m_numberTextBox.Location = new Point(117, 9);
        m_numberTextBox.Name = "m_numberTextBox";
        m_numberTextBox.ReadOnly = true;
        m_numberTextBox.Size = new Size(100, 20);
        m_numberTextBox.TabIndex = 1;
        m_yoffsetTextBox.Location = new Point(117, 36);
        m_yoffsetTextBox.Name = "m_yoffsetTextBox";
        m_yoffsetTextBox.Size = new Size(100, 20);
        m_yoffsetTextBox.TabIndex = 3;
        m_numberOfSlotsTextBox.Location = new Point(117, 62);
        m_numberOfSlotsTextBox.Name = "m_numberOfSlotsTextBox";
        m_numberOfSlotsTextBox.ReadOnly = true;
        m_numberOfSlotsTextBox.Size = new Size(100, 20);
        m_numberOfSlotsTextBox.TabIndex = 5;
        m_slotWidthTextBox.Location = new Point(117, 88);
        m_slotWidthTextBox.Name = "m_slotWidthTextBox";
        m_slotWidthTextBox.Size = new Size(100, 20);
        m_slotWidthTextBox.TabIndex = 7;
        m_sellThruSlotsTextBox.Location = new Point(117, 114);
        m_sellThruSlotsTextBox.Name = "m_sellThruSlotsTextBox";
        m_sellThruSlotsTextBox.ReadOnly = true;
        m_sellThruSlotsTextBox.Size = new Size(100, 20);
        m_sellThruSlotsTextBox.TabIndex = 9;
        m_sellThruOffsetTextBox.Location = new Point(117, 140);
        m_sellThruOffsetTextBox.Name = "m_sellThruOffsetTextBox";
        m_sellThruOffsetTextBox.ReadOnly = true;
        m_sellThruOffsetTextBox.Size = new Size(100, 20);
        m_sellThruOffsetTextBox.TabIndex = 11;
        m_approachOffsetTextBox.Location = new Point(117, 188);
        m_approachOffsetTextBox.Name = "m_approachOffsetTextBox";
        m_approachOffsetTextBox.Size = new Size(100, 20);
        m_approachOffsetTextBox.TabIndex = 13;
        m_approachOffsetTextBox.Visible = false;
        m_slotsPerQuadrantTextBox.Location = new Point(117, 164);
        m_slotsPerQuadrantTextBox.Name = "m_slotsPerQuadrantTextBox";
        m_slotsPerQuadrantTextBox.ReadOnly = true;
        m_slotsPerQuadrantTextBox.Size = new Size(100, 20);
        m_slotsPerQuadrantTextBox.TabIndex = 15;
        m_computeQuadrantsButton.Location = new Point(287, 250);
        m_computeQuadrantsButton.Name = "m_computeQuadrantsButton";
        m_computeQuadrantsButton.Size = new Size(129, 23);
        m_computeQuadrantsButton.TabIndex = 24;
        m_computeQuadrantsButton.Text = "Compute Quadrants...";
        m_computeQuadrantsButton.UseVisualStyleBackColor = true;
        m_computeQuadrantsButton.Click += OnComputeQuadrants;
        m_hasDumpSlotCheckBox.AutoSize = true;
        m_hasDumpSlotCheckBox.Location = new Point(117, 241);
        m_hasDumpSlotCheckBox.Name = "m_hasDumpSlotCheckBox";
        m_hasDumpSlotCheckBox.Size = new Size(103, 17);
        m_hasDumpSlotCheckBox.TabIndex = 25;
        m_hasDumpSlotCheckBox.Text = "Has Dump Slot?";
        m_hasDumpSlotCheckBox.UseVisualStyleBackColor = true;
        m_editSlotDataButton.Enabled = false;
        m_editSlotDataButton.Location = new Point(287, 221);
        m_editSlotDataButton.Name = "m_editSlotDataButton";
        m_editSlotDataButton.Size = new Size(129, 23);
        m_editSlotDataButton.TabIndex = 26;
        m_editSlotDataButton.Text = "Edit Slot Data";
        m_editSlotDataButton.UseVisualStyleBackColor = true;
        m_editSlotDataButton.Visible = false;
        m_editSlotDataButton.Click += m_editSlotDataButton_Click;
        m_quadrantDataGridView.AllowUserToAddRows = false;
        m_quadrantDataGridView.AllowUserToDeleteRows = false;
        m_quadrantDataGridView.AllowUserToResizeRows = false;
        m_quadrantDataGridView.BackgroundColor = Color.White;
        m_quadrantDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        m_quadrantDataGridView.Columns.AddRange(Quadrant, Offset, Start, End, Excluded);
        m_quadrantDataGridView.EditMode = DataGridViewEditMode.EditOnEnter;
        m_quadrantDataGridView.Location = new Point(12, 281);
        m_quadrantDataGridView.Name = "m_quadrantDataGridView";
        m_quadrantDataGridView.RowHeadersVisible = false;
        m_quadrantDataGridView.RowHeadersWidth = 25;
        m_quadrantDataGridView.ScrollBars = ScrollBars.Vertical;
        m_quadrantDataGridView.Size = new Size(404, 169);
        m_quadrantDataGridView.TabIndex = 27;
        m_quadrantDataGridView.CellDoubleClick += m_quadrantDataGridView_CellDoubleClick;
        m_quadrantDataGridView.EditingControlShowing += OnEditControlShowing;
        m_quadrantDataGridView.CellContentClick += m_quadrantDataGridView_CellContentClick;
        Quadrant.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        Quadrant.FillWeight = 75f;
        Quadrant.HeaderText = "Quadrant";
        Quadrant.Name = "Quadrant";
        Quadrant.ReadOnly = true;
        Quadrant.SortMode = DataGridViewColumnSortMode.NotSortable;
        Quadrant.Width = 57;
        Offset.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        Offset.FillWeight = 75f;
        Offset.HeaderText = "Offset";
        Offset.Name = "Offset";
        Offset.SortMode = DataGridViewColumnSortMode.NotSortable;
        Offset.Width = 41;
        Start.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        Start.FillWeight = 75f;
        Start.HeaderText = "Start Slot";
        Start.Name = "Start";
        Start.ReadOnly = true;
        Start.SortMode = DataGridViewColumnSortMode.NotSortable;
        Start.Width = 56;
        End.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        End.FillWeight = 75f;
        End.HeaderText = "End Slot";
        End.Name = "End";
        End.ReadOnly = true;
        End.SortMode = DataGridViewColumnSortMode.NotSortable;
        End.Width = 53;
        Excluded.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        gridViewCellStyle.Alignment = DataGridViewContentAlignment.TopCenter;
        gridViewCellStyle.NullValue = false;
        Excluded.DefaultCellStyle = gridViewCellStyle;
        Excluded.FalseValue = "false";
        Excluded.FillWeight = 60f;
        Excluded.HeaderText = "Excluded?";
        Excluded.IndeterminateValue = "null";
        Excluded.Name = "Excluded";
        Excluded.TrueValue = "true";
        AcceptButton = m_okButton;
        AutoScaleDimensions = new SizeF(6f, 13f);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = m_cancelButton;
        ClientSize = new Size(428, 491);
        ControlBox = false;
        Controls.Add(m_quadrantDataGridView);
        Controls.Add(m_editSlotDataButton);
        Controls.Add(m_hasDumpSlotCheckBox);
        Controls.Add(m_computeQuadrantsButton);
        Controls.Add(m_slotsPerQuadrantTextBox);
        Controls.Add(m_approachOffsetTextBox);
        Controls.Add(m_sellThruOffsetTextBox);
        Controls.Add(m_sellThruSlotsTextBox);
        Controls.Add(m_slotWidthTextBox);
        Controls.Add(m_numberOfSlotsTextBox);
        Controls.Add(m_yoffsetTextBox);
        Controls.Add(m_numberTextBox);
        Controls.Add(label9);
        Controls.Add(m_isQlmDeckCheckBox);
        Controls.Add(m_slotsPerQuadrantLabel);
        Controls.Add(m_approachOffsetLabel);
        Controls.Add(m_sellThruOffsetLabel);
        Controls.Add(m_sellThruSlotsLabel);
        Controls.Add(m_slotWidthLabel);
        Controls.Add(m_numberOfSlotsLabel);
        Controls.Add(m_yOffsetLabel);
        Controls.Add(m_numberLabel);
        Controls.Add(m_cancelButton);
        Controls.Add(m_okButton);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Name = nameof(DeckConfigurationDetailForm);
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Edit Deck Configuration";
        Load += OnLoad;
        ((ISupportInitialize)m_errorProvider).EndInit();
        ((ISupportInitialize)m_quadrantDataGridView).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}