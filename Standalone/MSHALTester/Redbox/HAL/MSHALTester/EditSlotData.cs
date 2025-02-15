using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Services;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.MSHALTester;

public class EditSlotData : Form
{
    public enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }

    private readonly XmlNode DeckNode;
    private readonly int? m_preferredDeck;
    private readonly int? m_qlmDeckNumber;
    private readonly DecksConfigurationManager Manager;
    private readonly OutputBox OutputBox;
    private readonly HardwareService Service;
    private Button button1;
    private Button button2;
    private IContainer components;
    private int CurrentEX;
    private int CurrentEY;
    private GroupBox groupBox1;
    private Label label1;
    private Label label2;
    private Label label3;
    private Label lbl_IncMoves;
    private CheckBox m_applyToAllDecksCheckBox;
    private Button m_cancelButton;
    private int m_changes;
    private TextBox m_deckTextBox;
    private TextBox m_encoderUnitsTextBox;
    private ErrorProvider m_errorProvider;
    private Button m_gripperExtendButton;
    private Button m_gripperFingerClose;
    private Button m_gripperFingerOpen;
    private Button m_gripperRetractButton;
    private Button m_moveButton;
    private Button m_moveLeftButton;
    private Button m_moveRightButton;
    private TextBox m_moveStatusTB;
    private Button m_okButton;
    private ListBox m_OutputListBox;
    private Button m_ringlightOffButton;
    private Button m_ringlightOnButton;
    private Button m_saveChangesButton;
    private Button m_sensorBarOffButton;
    private Button m_sensorBarOnButton;
    private bool MoveFailed;
    private int OriginalEX;
    private int OriginalEY;
    private Panel panel1;
    private bool PositionsRecorded;
    private bool RinglightActive;
    private bool SensorbarActive;
    private int StartXPosition;
    private TextBox textBox1;

    public EditSlotData(HardwareService service, DecksConfigurationManager mgr)
    {
        InitializeComponent();
        OutputBox = new OutputBox(m_OutputListBox);
        Service = service;
        Manager = mgr;
        var deckNode = Manager.FindDeckNode(8);
        if (deckNode == null || !deckNode.GetAttributeValue<bool>("IsQlm"))
            return;
        m_qlmDeckNumber = 8;
    }

    public EditSlotData(
        HardwareService service,
        DecksConfigurationManager mgr,
        int deck,
        int measureSlot)
        : this(service, mgr)
    {
        DeckNode = mgr.FindDeckNode(deck);
        m_preferredDeck = deck;
        m_deckTextBox.Text = deck.ToString();
        m_deckTextBox.ReadOnly = true;
    }

    public EditSlotData(HardwareService service, int deck, DecksConfigurationManager mgr)
        : this(service, mgr)
    {
        ToggleApplyCheckBox(deck == 1);
        m_deckTextBox.Text = deck.ToString();
    }

    internal int Deck
    {
        get
        {
            if (m_preferredDeck.HasValue)
                return m_preferredDeck.Value;
            var text = m_deckTextBox.Text;
            int result;
            if (string.IsNullOrEmpty(text) || !int.TryParse(text, out result))
                return -1;
            m_applyToAllDecksCheckBox.Enabled = result == 1;
            m_applyToAllDecksCheckBox.Visible = result == 1;
            return result;
        }
    }

    internal bool ApplyToAllDecks => !m_preferredDeck.HasValue && m_applyToAllDecksCheckBox.Checked;

    private void moveRightButton_Click(object sender, EventArgs e)
    {
        moveEncoderUnits(Direction.Right);
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        CleanupHardware();
        Close();
    }

    private bool moveEncoderUnits(Direction direction)
    {
        if (-1 == Deck)
        {
            m_errorProvider.SetError(m_deckTextBox, "Please select a deck.");
            return false;
        }

        if (!PositionsRecorded || MoveFailed)
            return false;
        var text = m_encoderUnitsTextBox.Text;
        var result = 0;
        if (int.TryParse(text, out result))
        {
            if ((direction == Direction.Up || direction == Direction.Down) && Math.Abs(result) > 1000)
            {
                m_errorProvider.SetError(m_encoderUnitsTextBox, "Units exceeds threshold for axis.");
                return false;
            }

            if ((direction == Direction.Left || direction == Direction.Right) && Math.Abs(result) > 100)
            {
                m_errorProvider.SetError(m_encoderUnitsTextBox, "Units exceeds threshold for axis.");
                return false;
            }

            if (direction == Direction.Left || direction == Direction.Down)
                result = -result;
            var flag = direction == Direction.Up || direction == Direction.Down;
            var units = flag ? CurrentEY + result : CurrentEX + result;
            var errorCodes = new MoveHelper(Service).MoveAbs(flag ? Axis.Y : Axis.X, units);
            OutputBox.Write("MOVE {0} {1} UNITS - New Location {2} ...", direction.ToString().ToUpper(), text, units);
            OutputBox.Write(errorCodes.ToString());
            if (errorCodes != ErrorCodes.Success)
                return false;
            if (flag)
                CurrentEY = units;
            else
                CurrentEX = units;
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(RinglightActive ? "RINGLIGHT ON" : "RINGLIGHT OFF" + Environment.NewLine);
            stringBuilder.Append(SensorbarActive ? "SENSOR PICKER-ON" : "SENSOR PICKER-OFF" + Environment.NewLine);
            Service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
            IncrementChange();
            return true;
        }

        m_errorProvider.SetError(m_encoderUnitsTextBox, "Must specify an integer!");
        return false;
    }

    private void m_moveLeftButton_Click(object sender, EventArgs e)
    {
        moveEncoderUnits(Direction.Left);
    }

    private void button1_Click(object sender, EventArgs e)
    {
        moveEncoderUnits(Direction.Up);
    }

    private void button2_Click(object sender, EventArgs e)
    {
        moveEncoderUnits(Direction.Down);
    }

    private void m_moveButton_Click(object sender, EventArgs e)
    {
        m_errorProvider.Clear();
        if (m_preferredDeck.HasValue)
        {
            MoveToStart(m_preferredDeck.Value, BaseSlot(m_preferredDeck.Value));
            m_moveButton.Enabled = false;
        }
        else
        {
            var deck = Deck;
            if (deck == -1)
                m_errorProvider.SetError(m_deckTextBox, "The deck value must be valid and not excluded.");
            else if (m_qlmDeckNumber.HasValue && m_qlmDeckNumber.Value == Deck)
                m_errorProvider.SetError(m_deckTextBox, "The deck value must be valid and not excluded.");
            else
                MoveToStart(Deck, BaseSlot(deck));
        }
    }

    private void MoveToStart(int deck, int slot)
    {
        var moveHelper = new MoveHelper(Service);
        var errorCodes = moveHelper.MoveTo(deck, slot);
        if (errorCodes == ErrorCodes.Success)
        {
            m_moveStatusTB.Text = errorCodes.ToString().ToUpper();
            var position = moveHelper.GetPosition();
            PositionsRecorded = false;
            if (position.ReadOk)
            {
                OriginalEY = CurrentEY = position.YCoordinate.Value;
                OriginalEX = CurrentEX = position.XCoordinate.Value;
                PositionsRecorded = true;
            }

            StartXPosition = CurrentEX;
            OutputBox.Write("Start positions: X = {0} Y = {1}", CurrentEX, CurrentEY);
            ClearChanges();
        }
        else
        {
            var controlLimitResponse = moveHelper.ReadLimits();
            var stringBuilder = new StringBuilder();
            if (controlLimitResponse.ReadOk)
            {
                stringBuilder.Append(errorCodes.ToString().ToUpper());
                foreach (var limit in controlLimitResponse.Limits)
                    if (limit.Blocked)
                    {
                        stringBuilder.Append(" ");
                        stringBuilder.Append(limit);
                    }
            }

            m_moveStatusTB.Text = stringBuilder.ToString();
            var num = (int)MessageBox.Show("Move to position failed!");
            MoveFailed = true;
        }
    }

    private void RunSimpleCommand(object sender, EventArgs e)
    {
        if (Deck == -1)
        {
            m_errorProvider.SetError(m_deckTextBox, "Please select a deck.");
        }
        else if (Service == null)
        {
            NotifyCommunicationProblem();
        }
        else
        {
            if (!(sender is Button button))
                return;
            var service = ServiceLocator.Instance.GetService<IControlSystem>();
            var controlResponse = (IControlResponse)null;
            if (button == m_ringlightOnButton)
            {
                controlResponse = service.ToggleRingLight(true, new int?());
                if (controlResponse.Success)
                    RinglightActive = true;
            }
            else if (button == m_ringlightOffButton)
            {
                controlResponse = service.ToggleRingLight(false, new int?());
                if (controlResponse.Success)
                    RinglightActive = false;
            }
            else if (button == m_sensorBarOnButton)
            {
                controlResponse = service.SetSensors(true);
                if (controlResponse.Success)
                    SensorbarActive = true;
            }
            else if (button == m_sensorBarOffButton)
            {
                controlResponse = service.SetSensors(false);
                if (controlResponse.Success)
                    SensorbarActive = false;
            }
            else if (button == m_gripperExtendButton)
            {
                controlResponse = service.ExtendArm();
            }
            else if (button == m_gripperFingerOpen)
            {
                controlResponse = service.SetFinger(GripperFingerState.Rent);
            }
            else if (button == m_gripperRetractButton)
            {
                controlResponse = service.RetractArm();
            }
            else if (button == m_gripperFingerClose)
            {
                controlResponse = service.SetFinger(GripperFingerState.Closed);
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(button.Tag + " ... ");
            if (controlResponse.CommError)
                stringBuilder.Append(controlResponse.Diagnostic);
            else if (controlResponse.TimedOut)
                stringBuilder.Append(ErrorCodes.Timeout.ToString().ToUpper());
            else
                stringBuilder.Append(ErrorCodes.Success.ToString().ToUpper());
            OutputBox.Write(stringBuilder.ToString());
        }
    }

    private void NotifyCommunicationProblem()
    {
        OutputBox.Write("Unable to communicate with HAL. Might be on a lunch break.");
        m_errorProvider.SetError(m_deckTextBox, "Unable to communicate with HAL.");
    }

    private void ToggleApplyCheckBox(bool enabled)
    {
        m_applyToAllDecksCheckBox.Visible = enabled;
        m_applyToAllDecksCheckBox.Enabled = enabled;
    }

    private void saveChangesButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        if (m_changes > 0 && ApplyChanges())
            Manager.FlushChanges(false);
        CleanupHardware();
        Close();
    }

    private void button3_Click(object sender, EventArgs e)
    {
        if (ApplyChanges())
            Manager.FlushChanges(false);
        ClearChanges();
    }

    private bool ApplyChanges()
    {
        var deltaY = CurrentEY - OriginalEY;
        var deltaX = CurrentEX - OriginalEX;
        OriginalEX = CurrentEX;
        OriginalEY = CurrentEY;
        if (m_preferredDeck.HasValue)
            return UpdateDeckNode(DeckNode, deltaX, deltaY, false);
        if (Deck != 1 || !m_applyToAllDecksCheckBox.Checked)
            return UpdateDeckNode(Manager.FindDeckNode(Deck), deltaX, deltaY, false);
        var num = 0;
        foreach (XmlNode allDeckNode in Manager.FindAllDeckNodes())
            if (UpdateDeckNode(allDeckNode, deltaX, deltaY, true))
                ++num;
        return num > 0;
    }

    private bool UpdateDeckNode(XmlNode node, int deltaX, int deltaY, bool applyBlind)
    {
        if ((deltaY == 0 && deltaX == 0) || node.GetAttributeValue<bool>("IsQlm"))
            return false;
        var attributeValue1 = node.GetAttributeValue<int>("Number");
        if (deltaY != 0)
        {
            var num = node.GetAttributeValue<int>("Offset") + deltaY;
            node.SetAttributeValue("Offset", num);
        }

        if (deltaX != 0)
        {
            var childNode = node.ChildNodes[0];
            var attributeValue2 = node.GetAttributeValue<decimal?>("SlotWidth");
            var num = attributeValue1 != 8 || applyBlind
                ? childNode.GetAttributeValue<int>("Offset") + (decimal)deltaX
                : StartXPosition + (decimal)deltaX - attributeValue2.Value;
            var attributeValue3 = node.GetAttributeValue<int>("Offset");
            var attributeValue4 = node.GetAttributeValue<int?>("NumberOfSlots");
            var attributeValue5 = node.GetAttributeValue<int?>("SellThruSlots");
            var attributeValue6 = node.GetAttributeValue<int?>("SellThruOffset");
            var attributeValue7 = node.GetAttributeValue<int?>("ApproachOffset");
            var attributeValue8 = node.GetAttributeValue<bool>("IsQlm");
            int? slotsPerQuadrant = attributeValue5.HasValue ? 6 : 15;
            var count = node.ChildNodes.Count;
            node.Attributes.RemoveAll();
            CommonFunctions.ComputeQuadrants(num, count, attributeValue5, attributeValue6, slotsPerQuadrant,
                attributeValue2, node);
            node.SetAttributeValue("Number", attributeValue1);
            node.SetAttributeValue("Offset", attributeValue3);
            node.SetAttributeValue("NumberOfSlots", attributeValue4);
            node.SetAttributeValue("SlotWidth", attributeValue2);
            node.SetAttributeValue("IsQlm", attributeValue8);
            node.SetAttributeValue("SellThruSlots", attributeValue5);
            node.SetAttributeValue("SellThruOffset", attributeValue6);
            node.SetAttributeValue("ApproachOffset", attributeValue7);
            node.SetAttributeValue("SlotsPerQuadrant", slotsPerQuadrant);
        }

        return true;
    }

    private void ClearChanges()
    {
        m_changes = 0;
        m_saveChangesButton.Enabled = false;
    }

    private void IncrementChange()
    {
        ++m_changes;
        m_saveChangesButton.Enabled = m_changes > 0;
    }

    private void CleanupHardware()
    {
        if (Service == null)
            return;
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("RINGLIGHT OFF" + Environment.NewLine);
        stringBuilder.Append("SENSOR PICKER-OFF" + Environment.NewLine);
        Service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
    }

    private int BaseSlot(int targetDeck)
    {
        return targetDeck != 8 ? 1 : 2;
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
        m_moveLeftButton = new Button();
        m_encoderUnitsTextBox = new TextBox();
        m_moveRightButton = new Button();
        m_okButton = new Button();
        m_cancelButton = new Button();
        button1 = new Button();
        button2 = new Button();
        m_errorProvider = new ErrorProvider(components);
        label1 = new Label();
        m_deckTextBox = new TextBox();
        m_moveButton = new Button();
        panel1 = new Panel();
        m_gripperFingerClose = new Button();
        m_gripperFingerOpen = new Button();
        m_gripperRetractButton = new Button();
        m_gripperExtendButton = new Button();
        label2 = new Label();
        textBox1 = new TextBox();
        m_OutputListBox = new ListBox();
        lbl_IncMoves = new Label();
        m_saveChangesButton = new Button();
        groupBox1 = new GroupBox();
        m_sensorBarOffButton = new Button();
        m_sensorBarOnButton = new Button();
        m_ringlightOffButton = new Button();
        m_ringlightOnButton = new Button();
        m_applyToAllDecksCheckBox = new CheckBox();
        label3 = new Label();
        m_moveStatusTB = new TextBox();
        ((ISupportInitialize)m_errorProvider).BeginInit();
        panel1.SuspendLayout();
        groupBox1.SuspendLayout();
        SuspendLayout();
        m_moveLeftButton.BackColor = Color.LightGray;
        m_moveLeftButton.Location = new Point(529, 174);
        m_moveLeftButton.Name = "m_moveLeftButton";
        m_moveLeftButton.Size = new Size(92, 40);
        m_moveLeftButton.TabIndex = 0;
        m_moveLeftButton.Text = "< Move &Left";
        m_moveLeftButton.UseVisualStyleBackColor = false;
        m_moveLeftButton.Click += m_moveLeftButton_Click;
        m_encoderUnitsTextBox.Location = new Point(627, 185);
        m_encoderUnitsTextBox.Name = "m_encoderUnitsTextBox";
        m_encoderUnitsTextBox.Size = new Size(74, 20);
        m_encoderUnitsTextBox.TabIndex = 1;
        m_encoderUnitsTextBox.Text = "10";
        m_moveRightButton.BackColor = Color.LightGray;
        m_moveRightButton.Location = new Point(707, 167);
        m_moveRightButton.Name = "m_moveRightButton";
        m_moveRightButton.Size = new Size(92, 40);
        m_moveRightButton.TabIndex = 2;
        m_moveRightButton.Text = "Move &Right >";
        m_moveRightButton.UseVisualStyleBackColor = false;
        m_moveRightButton.Click += moveRightButton_Click;
        m_okButton.BackColor = Color.LightGray;
        m_okButton.Location = new Point(327, 425);
        m_okButton.Name = "m_okButton";
        m_okButton.Size = new Size(90, 40);
        m_okButton.TabIndex = 3;
        m_okButton.Text = "&Ok";
        m_okButton.UseVisualStyleBackColor = false;
        m_okButton.Click += saveChangesButton_Click;
        m_cancelButton.BackColor = Color.LightGray;
        m_cancelButton.Location = new Point(423, 425);
        m_cancelButton.Name = "m_cancelButton";
        m_cancelButton.Size = new Size(90, 40);
        m_cancelButton.TabIndex = 4;
        m_cancelButton.Text = "&Cancel";
        m_cancelButton.UseVisualStyleBackColor = false;
        m_cancelButton.Click += cancelButton_Click;
        button1.BackColor = Color.LightGray;
        button1.Location = new Point(626, 124);
        button1.Name = "button1";
        button1.Size = new Size(75, 55);
        button1.TabIndex = 5;
        button1.Text = "&Up";
        button1.UseVisualStyleBackColor = false;
        button1.Click += button1_Click;
        button2.BackColor = Color.LightGray;
        button2.Location = new Point(627, 211);
        button2.Name = "button2";
        button2.Size = new Size(75, 53);
        button2.TabIndex = 6;
        button2.Text = "&Down";
        button2.UseVisualStyleBackColor = false;
        button2.Click += button2_Click;
        m_errorProvider.ContainerControl = this;
        label1.AutoSize = true;
        label1.Location = new Point(14, 55);
        label1.Name = "label1";
        label1.Size = new Size(73, 13);
        label1.TabIndex = 7;
        label1.Text = "Current Deck:";
        m_deckTextBox.Location = new Point(23, 71);
        m_deckTextBox.Name = "m_deckTextBox";
        m_deckTextBox.Size = new Size(64, 20);
        m_deckTextBox.TabIndex = 8;
        m_moveButton.BackColor = Color.LightGray;
        m_moveButton.Location = new Point(108, 50);
        m_moveButton.Name = "m_moveButton";
        m_moveButton.Size = new Size(95, 47);
        m_moveButton.TabIndex = 13;
        m_moveButton.Text = "Move";
        m_moveButton.UseVisualStyleBackColor = false;
        m_moveButton.Click += m_moveButton_Click;
        panel1.Controls.Add(m_gripperFingerClose);
        panel1.Controls.Add(m_gripperFingerOpen);
        panel1.Controls.Add(m_gripperRetractButton);
        panel1.Controls.Add(m_gripperExtendButton);
        panel1.Location = new Point(15, 164);
        panel1.Name = "panel1";
        panel1.Size = new Size(213, 115);
        panel1.TabIndex = 14;
        m_gripperFingerClose.BackColor = Color.LightGray;
        m_gripperFingerClose.Location = new Point(117, 72);
        m_gripperFingerClose.Name = "m_gripperFingerClose";
        m_gripperFingerClose.Size = new Size(92, 40);
        m_gripperFingerClose.TabIndex = 3;
        m_gripperFingerClose.Tag = "GRIPPER CLOSE";
        m_gripperFingerClose.Text = "Close";
        m_gripperFingerClose.UseVisualStyleBackColor = false;
        m_gripperFingerClose.Click += RunSimpleCommand;
        m_gripperFingerOpen.BackColor = Color.LightGray;
        m_gripperFingerOpen.Location = new Point(117, 3);
        m_gripperFingerOpen.Name = "m_gripperFingerOpen";
        m_gripperFingerOpen.Size = new Size(92, 40);
        m_gripperFingerOpen.TabIndex = 2;
        m_gripperFingerOpen.Tag = "GRIPPER RENT";
        m_gripperFingerOpen.Text = "Rent";
        m_gripperFingerOpen.UseVisualStyleBackColor = false;
        m_gripperFingerOpen.Click += RunSimpleCommand;
        m_gripperRetractButton.BackColor = Color.LightGray;
        m_gripperRetractButton.Location = new Point(3, 72);
        m_gripperRetractButton.Name = "m_gripperRetractButton";
        m_gripperRetractButton.Size = new Size(92, 40);
        m_gripperRetractButton.TabIndex = 1;
        m_gripperRetractButton.Tag = "GRIPPER RETRACT";
        m_gripperRetractButton.Text = "Retract";
        m_gripperRetractButton.UseVisualStyleBackColor = false;
        m_gripperRetractButton.Click += RunSimpleCommand;
        m_gripperExtendButton.BackColor = Color.LightGray;
        m_gripperExtendButton.Location = new Point(3, 3);
        m_gripperExtendButton.Name = "m_gripperExtendButton";
        m_gripperExtendButton.Size = new Size(92, 40);
        m_gripperExtendButton.TabIndex = 0;
        m_gripperExtendButton.Tag = "GRIPPER EXTEND";
        m_gripperExtendButton.Text = "Extend";
        m_gripperExtendButton.UseVisualStyleBackColor = false;
        m_gripperExtendButton.Click += RunSimpleCommand;
        label2.AutoSize = true;
        label2.Location = new Point(15, 148);
        label2.Name = "label2";
        label2.Size = new Size(114, 13);
        label2.TabIndex = 15;
        label2.Text = "Gripper Test Functions";
        textBox1.BackColor = Color.Yellow;
        textBox1.Location = new Point(12, 12);
        textBox1.Name = "textBox1";
        textBox1.ReadOnly = true;
        textBox1.Size = new Size(230, 20);
        textBox1.TabIndex = 16;
        textBox1.Text = "** This tool does not work on the QLM deck. **";
        m_OutputListBox.FormattingEnabled = true;
        m_OutputListBox.Location = new Point(15, 285);
        m_OutputListBox.Name = "m_OutputListBox";
        m_OutputListBox.SelectionMode = SelectionMode.MultiExtended;
        m_OutputListBox.Size = new Size(570, 134);
        m_OutputListBox.TabIndex = 17;
        lbl_IncMoves.AutoSize = true;
        lbl_IncMoves.Location = new Point(510, 124);
        lbl_IncMoves.Name = "lbl_IncMoves";
        lbl_IncMoves.Size = new Size(97, 13);
        lbl_IncMoves.TabIndex = 18;
        lbl_IncMoves.Text = "Incremental Moves";
        m_saveChangesButton.BackColor = Color.LightGray;
        m_saveChangesButton.Enabled = false;
        m_saveChangesButton.Location = new Point(231, 425);
        m_saveChangesButton.Name = "m_saveChangesButton";
        m_saveChangesButton.Size = new Size(90, 40);
        m_saveChangesButton.TabIndex = 19;
        m_saveChangesButton.Text = "Save Current Changes";
        m_saveChangesButton.UseVisualStyleBackColor = false;
        m_saveChangesButton.Click += button3_Click;
        groupBox1.Controls.Add(m_sensorBarOffButton);
        groupBox1.Controls.Add(m_sensorBarOnButton);
        groupBox1.Controls.Add(m_ringlightOffButton);
        groupBox1.Controls.Add(m_ringlightOnButton);
        groupBox1.Location = new Point(243, 148);
        groupBox1.Name = "groupBox1";
        groupBox1.Size = new Size(216, 131);
        groupBox1.TabIndex = 20;
        groupBox1.TabStop = false;
        groupBox1.Text = "Lighting";
        m_sensorBarOffButton.BackColor = Color.LightGray;
        m_sensorBarOffButton.Location = new Point(118, 85);
        m_sensorBarOffButton.Name = "m_sensorBarOffButton";
        m_sensorBarOffButton.Size = new Size(92, 40);
        m_sensorBarOffButton.TabIndex = 3;
        m_sensorBarOffButton.Tag = "SENSOR PICKER-OFF";
        m_sensorBarOffButton.Text = "Sensor Bar Off";
        m_sensorBarOffButton.UseVisualStyleBackColor = false;
        m_sensorBarOffButton.Click += RunSimpleCommand;
        m_sensorBarOnButton.BackColor = Color.LightGray;
        m_sensorBarOnButton.Location = new Point(118, 17);
        m_sensorBarOnButton.Name = "m_sensorBarOnButton";
        m_sensorBarOnButton.Size = new Size(92, 40);
        m_sensorBarOnButton.TabIndex = 2;
        m_sensorBarOnButton.Tag = "SENSOR PICKER-ON";
        m_sensorBarOnButton.Text = "Sensor Bar On";
        m_sensorBarOnButton.UseVisualStyleBackColor = false;
        m_sensorBarOnButton.Click += RunSimpleCommand;
        m_ringlightOffButton.BackColor = Color.LightGray;
        m_ringlightOffButton.Location = new Point(6, 85);
        m_ringlightOffButton.Name = "m_ringlightOffButton";
        m_ringlightOffButton.Size = new Size(92, 40);
        m_ringlightOffButton.TabIndex = 1;
        m_ringlightOffButton.Tag = "RINGLIGHT OFF";
        m_ringlightOffButton.Text = "Ringlight Off";
        m_ringlightOffButton.UseVisualStyleBackColor = false;
        m_ringlightOffButton.Click += RunSimpleCommand;
        m_ringlightOnButton.BackColor = Color.LightGray;
        m_ringlightOnButton.Location = new Point(6, 17);
        m_ringlightOnButton.Name = "m_ringlightOnButton";
        m_ringlightOnButton.Size = new Size(92, 40);
        m_ringlightOnButton.TabIndex = 0;
        m_ringlightOnButton.Tag = "RINGLIGHT ON";
        m_ringlightOnButton.Text = "Ringlight On";
        m_ringlightOnButton.UseVisualStyleBackColor = false;
        m_ringlightOnButton.Click += RunSimpleCommand;
        m_applyToAllDecksCheckBox.AutoSize = true;
        m_applyToAllDecksCheckBox.Enabled = false;
        m_applyToAllDecksCheckBox.Location = new Point(23, 107);
        m_applyToAllDecksCheckBox.Name = "m_applyToAllDecksCheckBox";
        m_applyToAllDecksCheckBox.Size = new Size(115, 17);
        m_applyToAllDecksCheckBox.TabIndex = 21;
        m_applyToAllDecksCheckBox.Text = "Apply to all decks?";
        m_applyToAllDecksCheckBox.UseVisualStyleBackColor = true;
        label3.AutoSize = true;
        label3.Location = new Point(228, 50);
        label3.Name = "label3";
        label3.Size = new Size(67, 13);
        label3.TabIndex = 22;
        label3.Text = "Move Status";
        m_moveStatusTB.Location = new Point(231, 71);
        m_moveStatusTB.Name = "m_moveStatusTB";
        m_moveStatusTB.Size = new Size(186, 20);
        m_moveStatusTB.TabIndex = 23;
        AutoScaleDimensions = new SizeF(6f, 13f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(832, 488);
        ControlBox = false;
        Controls.Add(m_moveStatusTB);
        Controls.Add(label3);
        Controls.Add(m_applyToAllDecksCheckBox);
        Controls.Add(groupBox1);
        Controls.Add(m_saveChangesButton);
        Controls.Add(lbl_IncMoves);
        Controls.Add(m_OutputListBox);
        Controls.Add(textBox1);
        Controls.Add(label2);
        Controls.Add(panel1);
        Controls.Add(m_moveButton);
        Controls.Add(m_deckTextBox);
        Controls.Add(label1);
        Controls.Add(button2);
        Controls.Add(button1);
        Controls.Add(m_cancelButton);
        Controls.Add(m_okButton);
        Controls.Add(m_moveRightButton);
        Controls.Add(m_encoderUnitsTextBox);
        Controls.Add(m_moveLeftButton);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Name = nameof(EditSlotData);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Edit Slot Data";
        ((ISupportInitialize)m_errorProvider).EndInit();
        panel1.ResumeLayout(false);
        groupBox1.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
}