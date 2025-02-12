using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using Redbox.HAL.Management.Console.Properties;

namespace Redbox.HAL.Management.Console
{
    public class TouchScreenAccess : UserControl
    {
        private Button button1;
        private IContainer components;
        private TabPage m_audioTabPage;
        private FlowLayoutPanel m_cameraFlowLayoutPanel;
        private Button m_cameraRingOffButton;
        private Button m_cameraRingOnButton;
        private Button m_cameraSnap;
        private Button m_cameraStartButton;
        private Button m_cameraStatusButton;
        private Button m_cameraStopButton;
        private TabPage m_cameraTabPage;
        private Button m_gripperClearButton;
        private Button m_gripperCloseButton;
        private Button m_gripperExtendButton;
        private FlowLayoutPanel m_gripperFlowLayoutPanel;
        private Button m_gripperOpenButton;
        private Button m_gripperPeekButton;
        private Button m_gripperPushButton;
        private Button m_gripperPushDvdInButton;
        private Button m_gripperPutInEmptyButton;
        private Button m_gripperRentButton;
        private Button m_gripperRetractButton;
        private Button m_gripperStatusButton;
        private TabPage m_gripperTabPage;
        private FlowLayoutPanel m_inventoryFlowLayoutPanel;
        private TabPage m_inventoryTabPage;
        private Button m_invFindEmptyButton;
        private Button m_invGetButton;
        private Button m_invGetCenterButton;
        private Button m_invGetVendButton;
        private Button m_invInspectButton;
        private Button m_invPutButton;
        private Button m_invPutinSlotButton;
        private Button m_invReadButton;
        private Button m_invSyncSlotButton;
        private Button m_movementCurrentLocationButton;
        private FlowLayoutPanel m_movementFlowLayoutPanel;
        private Button m_movementHomeXButton;
        private Button m_movementHomeYButton;
        private Button m_movementMoveButton;
        private Button m_movementMoveNoCheckButton;
        private Button m_movementMoveVendButton;
        private TabPage m_movementTabPage;
        private Button m_movementTransferButton;
        private Button m_qlmDisengageButton;
        private Button m_qlmDropButton;
        private Button m_qlmEngageButton;
        private FlowLayoutPanel m_qlmFlowLayoutPanel;
        private Button m_qlmHaltButton;
        private Button m_qlmLiftButton;
        private Button m_qlmStatusButton;
        private TabPage m_qlmTabPage;
        private FlowLayoutPanel m_rollerFlowLayoutPanel;
        private Button m_rollerRollerInButton;
        private Button m_rollerRollerOut;
        private Button m_rollerStopButton;
        private TabPage m_rollerTabPage;
        private Button m_rollerToPositionButton;
        private FlowLayoutPanel m_sensorFlowLayoutPanel;
        private Button m_sensorPickerOffButton;
        private Button m_sensorPickerOnButton;
        private Button m_sensorPickerReadButton;
        private Button m_sensorPickerWaitBlockButton;
        private Button m_sensorPickerWaitClearButton;
        private Button m_sensorReadAuxInputsButton;
        private Button m_sensorReadPickerInputsButton;
        private TabPage m_sensorTabPage;
        private FlowLayoutPanel m_systemFlowLayoutPanel;
        private Button m_systemResetButton;
        private TabPage m_systemTabPage;
        private Button m_systemVersionButton;
        private TabControl m_touchScreenAccessTabControl;
        private Button m_trackCloseButton;
        private FlowLayoutPanel m_trackFlowLayoutPanel;
        private Button m_trackOpenButton;
        private Button m_trackStatusButton;
        private TabPage m_trackTabPage;
        private Button m_vendDoorCloseButton;
        private FlowLayoutPanel m_vendDoorFlowLayoutPanel;
        private Button m_vendDoorRentButton;
        private Button m_vendDoorStatusButton;
        private TabPage m_vendDoorTabPage;

        private TouchScreenAccess()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            var running = Color.Red;
            if (!string.IsNullOrEmpty(Settings.Default.ButtonRunningColor))
                running = Color.FromName(Settings.Default.ButtonRunningColor);
            Manager = new ButtonAspectsManager(running);
            ToggleStyle();
            m_touchScreenAccessTabControl.Enabled = false;
            BackColor = Color.White;
            Enabled = false;
            EnabledChanged += OnEnabledChanged;
            EnvironmentHelper.ExecutingImmediateStausChanged += OnImmediateExecutingStatusChanged;
        }

        public static TouchScreenAccess Instance => Singleton<TouchScreenAccess>.Instance;

        public ButtonAspectsManager Manager { get; }

        private void TouchAccessTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            var graphics = e.Graphics;
            var tabPage = m_touchScreenAccessTabControl.TabPages[e.Index];
            var layoutRectangle = e.Bounds;
            layoutRectangle = new Rectangle(layoutRectangle.X, layoutRectangle.Y + 3, layoutRectangle.Width,
                layoutRectangle.Height - 3);
            Brush brush;
            Font font;
            if (e.State == DrawItemState.Selected)
            {
                brush = new SolidBrush(Color.Black);
                font = new Font(e.Font, FontStyle.Bold);
                if (ProfileManager.Instance.IsConnected)
                    graphics.FillRectangle(Brushes.SkyBlue, e.Bounds);
                else
                    graphics.FillRectangle(Brushes.LightGray, e.Bounds);
            }
            else
            {
                brush = new SolidBrush(e.ForeColor);
                graphics.FillRectangle(Brushes.WhiteSmoke, e.Bounds);
                e.DrawBackground();
                font = e.Font;
            }

            graphics.DrawString(tabPage.Text, font, brush, layoutRectangle, new StringFormat(new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            }));
        }

        private void OnEnabledChanged(object sender, EventArgs e)
        {
            ToggleStyle();
            m_touchScreenAccessTabControl.Enabled = Enabled;
        }

        private void OnImmediateExecutingStatusChanged(object sender, BoolEventArgs e)
        {
            Enabled = !e.State;
        }

        private void OnSimpleCommandButton(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                var tag = (sender as Button).Tag as string;
                if (string.IsNullOrEmpty(tag))
                    return;
                ExecuteInstruction(tag);
            }
        }

        internal static void WriteInstructionToOutput(string instruction)
        {
            OutputWindow.Instance.Append(instruction);
        }

        private void ToggleStyle()
        {
            var enabled = Enabled;
            foreach (Control tabPage in m_touchScreenAccessTabControl.TabPages)
            foreach (Button control in (ArrangedElementCollection)tabPage.Controls[0].Controls)
                if (control != null)
                {
                    if (enabled)
                        control.FlatStyle = FlatStyle.Standard;
                    else
                        control.FlatStyle = FlatStyle.Flat;
                }
        }

        private void OnMove(object sender, EventArgs args)
        {
            using (Manager.MakeAspect(sender))
            {
                var location = CommonFunctions.CurrentLocation();
                if (location == null)
                    location = new Location { Deck = 1, Slot = 1 };
                var moveToForm = new MoveToForm
                {
                    Deck = location.Deck,
                    Slot = location.Slot
                };
                if (moveToForm.ShowDialog() != DialogResult.OK || !moveToForm.Deck.HasValue ||
                    !moveToForm.Slot.HasValue)
                    return;
                var instruction = string.Format("MOVE DECK={0} SLOT={1}", moveToForm.Deck, moveToForm.Slot);
                if (!string.IsNullOrEmpty(moveToForm.Mode))
                    instruction = instruction + " MODE=" + moveToForm.Mode;
                ExecuteInstruction(instruction);
            }
        }

        private void OnTransfer(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                var str1 = "XFER";
                var moveToForm = new MoveToForm();
                moveToForm.DisableModes();
                moveToForm.Text = "Enter Source Deck/Slot";
                if (moveToForm.ShowDialog() != DialogResult.OK || !moveToForm.Deck.HasValue ||
                    !moveToForm.Slot.HasValue)
                    return;
                var str2 = str1 + string.Format(" SRC-DECK={0} SRC-SLOT={1}", moveToForm.Deck, moveToForm.Slot);
                moveToForm.ClearFields();
                moveToForm.Text = "Enter Destination Deck/Slot";
                if (moveToForm.ShowDialog() != DialogResult.OK || !moveToForm.Deck.HasValue ||
                    !moveToForm.Slot.HasValue)
                    return;
                ExecuteInstruction(str2 + string.Format(" DEST-DECK={0} DEST-SLOT={1}", moveToForm.Deck,
                    moveToForm.Slot));
            }
        }

        private void OnInspect(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                var moveToForm1 = new MoveToForm();
                moveToForm1.Text = "Location to Inspect";
                var moveToForm2 = moveToForm1;
                moveToForm2.DisableModes();
                if (moveToForm2.ShowDialog() != DialogResult.OK || !moveToForm2.Deck.HasValue ||
                    !moveToForm2.Slot.HasValue)
                    return;
                ExecuteInstruction(string.Format("INSPECT DECK={0} SLOT={1}", moveToForm2.Deck, moveToForm2.Slot));
            }
        }

        private void OnReadPickerSensors(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                var num1 = 1;
                var num2 = 6;
                var hardwareJob = ExecuteInstruction(string.Format("SENSOR READ PICKER-SENSOR={0}..{1}", num1, num2));
                Stack<string> stack;
                if (hardwareJob == null || !hardwareJob.GetStack(out stack).Success)
                    return;
                SensorView.Instance.ResetSensors();
                if (stack.Count == 0)
                    return;
                stack.Pop();
                for (var index = num2; index >= num1; --index)
                {
                    var str = stack.Pop();
                    SensorView.Instance.Sensors[index - 1] = str.Contains("BLOCKED");
                }

                ExecuteInstruction("SENSOR PICKER-OFF");
            }
        }

        private void OnMoveNoCheck(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                var num = (int)new IncrementalMoveForm(ProfileManager.Instance.Service).ShowDialog();
            }
        }

        private void OnWaitPickerSensorsClear(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                var sensorRangeForm = new SensorRangeForm
                {
                    MinimumValue = 1,
                    MaximumValue = 6
                };
                if (sensorRangeForm.ShowDialog() != DialogResult.OK || !sensorRangeForm.Start.HasValue)
                    return;
                ExecuteInstruction(string.Format("SENSOR WAIT-CLEAR PICKER-SENSOR={0}{1} TIMEOUT=30000",
                    sensorRangeForm.Start,
                    sensorRangeForm.End.HasValue ? ".." + sensorRangeForm.End : (object)string.Empty));
            }
        }

        private void OnWaitPickerSensorsBlock(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                var sensorRangeForm = new SensorRangeForm
                {
                    MinimumValue = 1,
                    MaximumValue = 6
                };
                if (sensorRangeForm.ShowDialog() != DialogResult.OK || !sensorRangeForm.Start.HasValue)
                    return;
                ExecuteInstruction(string.Format("SENSOR WAIT-BLOCK PICKER-SENSOR={0}{1} TIMEOUT=30000",
                    sensorRangeForm.Start,
                    sensorRangeForm.End.HasValue ? ".." + sensorRangeForm.End : (object)string.Empty));
            }
        }

        private void OnTurnOffSensorsButton(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                OnSimpleCommandButton(sender, e);
                SensorView.Instance.ResetSensors();
            }
        }

        private void OnRollerToPosition(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                var sensorRangeForm = new SensorRangeForm
                {
                    MinimumValue = 1,
                    MaximumValue = 6
                };
                sensorRangeForm.DisableEnd();
                if (sensorRangeForm.ShowDialog() != DialogResult.OK || !sensorRangeForm.Start.HasValue)
                    return;
                ExecuteInstruction(string.Format("ROLLER POS={0} TIMEOUT=3000", sensorRangeForm.Start));
            }
        }

        private void OnSyncSlot(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                ExecuteInstruction(string.Format("PUSH \"{0}\"",
                    CommonFunctions.SyncSlot() ? "SYNC OK" : (object)"SYNC ERRORED"));
            }
        }

        private void OnPushDVDIn(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                PushDVDInPickerIntoSlotViaJob();
            }
        }

        private void PushDVDInPickerIntoSlotViaJob()
        {
            try
            {
                Application.DoEvents();
                var schedule = new HardwareJobSchedule
                {
                    Priority = HardwareJobPriority.Highest
                };
                HardwareJob job;
                if (!ProfileManager.Instance.Service.ScheduleJob("push-in-dvd", string.Empty, false, schedule, out job)
                        .Success)
                {
                    LogHelper.Instance.Log("Unable to schedule job", LogEntryType.Error);
                    ExecuteInstruction("PUSH \"JOBERROR\"");
                }
                else
                {
                    job.Resume();
                    job.WaitForCompletion();
                    if (!ProfileManager.Instance.Service.GetJob(job.ID, out job).Success)
                    {
                        LogHelper.Instance.Log("Unable to get job status", LogEntryType.Error);
                        ExecuteInstruction("PUSH \"JOBERROR\"");
                    }
                    else
                    {
                        var status = (int)job.Status;
                        Stack<string> stack;
                        if (job.GetStack(out stack).Errors.Count > 0)
                        {
                            LogHelper.Instance.Log(string.Format("Error getting stack for push-dvd job {0}", job.ID),
                                LogEntryType.Error);
                            ExecuteInstruction("PUSH \"JOBERROR\"");
                        }
                        else
                        {
                            for (var index = 0; index < stack.Count - 1; ++index)
                                stack.Pop();
                            var str = stack.Pop();
                            ProfileManager.Instance.Service.TrashJob(job.ID);
                            ExecuteInstruction(string.Format("PUSH \"{0}\"", str));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("ERROR: push-dvd-in failed.", ex);
            }
        }

        private void OnPutInEmptySlot(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                new PutInEmptySlotResult(ProfileManager.Instance.Service).Run();
            }
        }

        private void OnPutDvdInPickerIntoSlot(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                var location = CommonFunctions.CurrentLocation();
                if (location == null)
                    return;
                using (var inLocationResult =
                       new PutInLocationResult(ProfileManager.Instance.Service, location.Deck, location.Slot))
                {
                    inLocationResult.Run();
                }
            }
        }

        private void OnGetAndCenter(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                var moveToForm = new MoveToForm();
                moveToForm.DisableModes();
                moveToForm.Text = "Get From Location";
                if (moveToForm.ShowDialog() != DialogResult.OK || !moveToForm.Deck.HasValue ||
                    !moveToForm.Slot.HasValue)
                    return;
                using (var getAndCenterResult = new GetAndCenterResult(ProfileManager.Instance.Service,
                           moveToForm.Deck.Value, moveToForm.Slot.Value, true))
                {
                    getAndCenterResult.Run();
                }
            }
        }

        private void OnGetAndVend(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                var moveToForm = new MoveToForm();
                moveToForm.DisableModes();
                moveToForm.Text = "Vend from Location";
                if (moveToForm.ShowDialog() != DialogResult.OK || !moveToForm.Deck.HasValue ||
                    !moveToForm.Slot.HasValue)
                    return;
                var locationList1 = new List<Client.Location>();
                var locationList2 = locationList1;
                var location = new Client.Location();
                var nullable = moveToForm.Deck;
                location.Deck = nullable.Value;
                nullable = moveToForm.Slot;
                location.Slot = nullable.Value;
                locationList2.Add(location);
                var service = ProfileManager.Instance.Service;
                var locations = locationList1;
                var schedule = new HardwareJobSchedule();
                schedule.Priority = HardwareJobPriority.Normal;
                HardwareJob hardwareJob;
                if (!service.Vend(locations, schedule, out hardwareJob).Success)
                    return;
                hardwareJob.Resume();
                Application.DoEvents();
                var result = hardwareJob.WaitForCompletion();
                if (!result.Success)
                    return;
                CommonFunctions.ProcessCommandResult(result);
                ProfileManager.Instance.Service.TrashJob(hardwareJob.ID);
            }
        }

        private HardwareJob ExecuteInstruction(string instruction)
        {
            Application.DoEvents();
            WriteInstructionToOutput(instruction);
            var hardwareJob = CommonFunctions.ExecuteInstruction(instruction);
            Application.DoEvents();
            return hardwareJob;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_touchScreenAccessTabControl = new TabControl();
            m_movementTabPage = new TabPage();
            m_movementFlowLayoutPanel = new FlowLayoutPanel();
            m_movementMoveButton = new Button();
            m_movementMoveVendButton = new Button();
            m_movementHomeXButton = new Button();
            m_movementHomeYButton = new Button();
            m_movementTransferButton = new Button();
            m_movementCurrentLocationButton = new Button();
            m_movementMoveNoCheckButton = new Button();
            m_inventoryTabPage = new TabPage();
            m_inventoryFlowLayoutPanel = new FlowLayoutPanel();
            m_invGetButton = new Button();
            m_invPutButton = new Button();
            m_invReadButton = new Button();
            m_invInspectButton = new Button();
            m_invFindEmptyButton = new Button();
            m_invSyncSlotButton = new Button();
            m_invPutinSlotButton = new Button();
            m_invGetVendButton = new Button();
            m_invGetCenterButton = new Button();
            m_gripperPutInEmptyButton = new Button();
            m_rollerTabPage = new TabPage();
            m_rollerFlowLayoutPanel = new FlowLayoutPanel();
            m_rollerRollerInButton = new Button();
            m_rollerRollerOut = new Button();
            m_rollerToPositionButton = new Button();
            m_rollerStopButton = new Button();
            m_gripperTabPage = new TabPage();
            m_gripperFlowLayoutPanel = new FlowLayoutPanel();
            m_gripperExtendButton = new Button();
            m_gripperRetractButton = new Button();
            m_gripperOpenButton = new Button();
            m_gripperRentButton = new Button();
            m_gripperCloseButton = new Button();
            m_gripperPeekButton = new Button();
            m_gripperPushButton = new Button();
            m_gripperStatusButton = new Button();
            m_gripperClearButton = new Button();
            m_gripperPushDvdInButton = new Button();
            m_cameraTabPage = new TabPage();
            m_cameraFlowLayoutPanel = new FlowLayoutPanel();
            m_cameraRingOnButton = new Button();
            m_cameraRingOffButton = new Button();
            m_cameraStartButton = new Button();
            m_cameraSnap = new Button();
            m_cameraStopButton = new Button();
            m_cameraStatusButton = new Button();
            m_vendDoorTabPage = new TabPage();
            m_vendDoorFlowLayoutPanel = new FlowLayoutPanel();
            m_vendDoorRentButton = new Button();
            m_vendDoorStatusButton = new Button();
            m_vendDoorCloseButton = new Button();
            m_trackTabPage = new TabPage();
            m_trackFlowLayoutPanel = new FlowLayoutPanel();
            m_trackOpenButton = new Button();
            m_trackCloseButton = new Button();
            m_trackStatusButton = new Button();
            m_sensorTabPage = new TabPage();
            m_sensorFlowLayoutPanel = new FlowLayoutPanel();
            m_sensorPickerOnButton = new Button();
            m_sensorPickerOffButton = new Button();
            m_sensorPickerReadButton = new Button();
            m_sensorPickerWaitClearButton = new Button();
            m_sensorPickerWaitBlockButton = new Button();
            m_sensorReadPickerInputsButton = new Button();
            m_sensorReadAuxInputsButton = new Button();
            m_qlmTabPage = new TabPage();
            m_qlmFlowLayoutPanel = new FlowLayoutPanel();
            m_qlmEngageButton = new Button();
            m_qlmDisengageButton = new Button();
            m_qlmStatusButton = new Button();
            m_qlmLiftButton = new Button();
            m_qlmDropButton = new Button();
            m_qlmHaltButton = new Button();
            m_systemTabPage = new TabPage();
            m_systemFlowLayoutPanel = new FlowLayoutPanel();
            m_systemResetButton = new Button();
            m_systemVersionButton = new Button();
            button1 = new Button();
            m_audioTabPage = new TabPage();
            m_touchScreenAccessTabControl.SuspendLayout();
            m_movementTabPage.SuspendLayout();
            m_movementFlowLayoutPanel.SuspendLayout();
            m_inventoryTabPage.SuspendLayout();
            m_inventoryFlowLayoutPanel.SuspendLayout();
            m_rollerTabPage.SuspendLayout();
            m_rollerFlowLayoutPanel.SuspendLayout();
            m_gripperTabPage.SuspendLayout();
            m_gripperFlowLayoutPanel.SuspendLayout();
            m_cameraTabPage.SuspendLayout();
            m_cameraFlowLayoutPanel.SuspendLayout();
            m_vendDoorTabPage.SuspendLayout();
            m_vendDoorFlowLayoutPanel.SuspendLayout();
            m_trackTabPage.SuspendLayout();
            m_trackFlowLayoutPanel.SuspendLayout();
            m_sensorTabPage.SuspendLayout();
            m_sensorFlowLayoutPanel.SuspendLayout();
            m_qlmTabPage.SuspendLayout();
            m_qlmFlowLayoutPanel.SuspendLayout();
            m_systemTabPage.SuspendLayout();
            m_systemFlowLayoutPanel.SuspendLayout();
            SuspendLayout();
            m_touchScreenAccessTabControl.Controls.Add(m_movementTabPage);
            m_touchScreenAccessTabControl.Controls.Add(m_inventoryTabPage);
            m_touchScreenAccessTabControl.Controls.Add(m_rollerTabPage);
            m_touchScreenAccessTabControl.Controls.Add(m_gripperTabPage);
            m_touchScreenAccessTabControl.Controls.Add(m_cameraTabPage);
            m_touchScreenAccessTabControl.Controls.Add(m_vendDoorTabPage);
            m_touchScreenAccessTabControl.Controls.Add(m_trackTabPage);
            m_touchScreenAccessTabControl.Controls.Add(m_sensorTabPage);
            m_touchScreenAccessTabControl.Controls.Add(m_qlmTabPage);
            m_touchScreenAccessTabControl.Controls.Add(m_systemTabPage);
            m_touchScreenAccessTabControl.Dock = DockStyle.Fill;
            m_touchScreenAccessTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            m_touchScreenAccessTabControl.ItemSize = new Size(75, 75);
            m_touchScreenAccessTabControl.Location = new Point(0, 0);
            m_touchScreenAccessTabControl.Name = "m_touchScreenAccessTabControl";
            m_touchScreenAccessTabControl.Padding = new Point(3, 0);
            m_touchScreenAccessTabControl.RightToLeftLayout = true;
            m_touchScreenAccessTabControl.SelectedIndex = 0;
            m_touchScreenAccessTabControl.Size = new Size(620, 620);
            m_touchScreenAccessTabControl.SizeMode = TabSizeMode.Fixed;
            m_touchScreenAccessTabControl.TabIndex = 0;
            m_touchScreenAccessTabControl.DrawItem += TouchAccessTabControl_DrawItem;
            m_movementTabPage.BackColor = SystemColors.AppWorkspace;
            m_movementTabPage.BorderStyle = BorderStyle.FixedSingle;
            m_movementTabPage.Controls.Add(m_movementFlowLayoutPanel);
            m_movementTabPage.Location = new Point(4, 79);
            m_movementTabPage.Name = "m_movementTabPage";
            m_movementTabPage.Size = new Size(612, 537);
            m_movementTabPage.TabIndex = 0;
            m_movementTabPage.Tag = "Movement";
            m_movementTabPage.Text = "Movement";
            m_movementFlowLayoutPanel.BackColor = Color.WhiteSmoke;
            m_movementFlowLayoutPanel.Controls.Add(m_movementMoveButton);
            m_movementFlowLayoutPanel.Controls.Add(m_movementMoveVendButton);
            m_movementFlowLayoutPanel.Controls.Add(m_movementHomeXButton);
            m_movementFlowLayoutPanel.Controls.Add(m_movementHomeYButton);
            m_movementFlowLayoutPanel.Controls.Add(m_movementTransferButton);
            m_movementFlowLayoutPanel.Controls.Add(m_movementCurrentLocationButton);
            m_movementFlowLayoutPanel.Controls.Add(m_movementMoveNoCheckButton);
            m_movementFlowLayoutPanel.Dock = DockStyle.Fill;
            m_movementFlowLayoutPanel.Location = new Point(0, 0);
            m_movementFlowLayoutPanel.Name = "m_movementFlowLayoutPanel";
            m_movementFlowLayoutPanel.Size = new Size(610, 535);
            m_movementFlowLayoutPanel.TabIndex = 7;
            m_movementMoveButton.BackColor = Color.LightGray;
            m_movementMoveButton.BackgroundImageLayout = ImageLayout.Center;
            m_movementMoveButton.Location = new Point(3, 3);
            m_movementMoveButton.Name = "m_movementMoveButton";
            m_movementMoveButton.Padding = new Padding(10);
            m_movementMoveButton.Size = new Size(150, 75);
            m_movementMoveButton.TabIndex = 0;
            m_movementMoveButton.Tag = "Move";
            m_movementMoveButton.Text = "Move";
            m_movementMoveButton.UseVisualStyleBackColor = false;
            m_movementMoveButton.Click += OnMove;
            m_movementMoveVendButton.BackColor = Color.LightGray;
            m_movementMoveVendButton.Location = new Point(159, 3);
            m_movementMoveVendButton.Name = "m_movementMoveVendButton";
            m_movementMoveVendButton.Padding = new Padding(10);
            m_movementMoveVendButton.Size = new Size(150, 75);
            m_movementMoveVendButton.TabIndex = 1;
            m_movementMoveVendButton.Tag = "MOVEVEND";
            m_movementMoveVendButton.Text = "Move Vend";
            m_movementMoveVendButton.UseVisualStyleBackColor = false;
            m_movementMoveVendButton.Click += OnSimpleCommandButton;
            m_movementHomeXButton.BackColor = Color.LightGray;
            m_movementHomeXButton.Location = new Point(315, 3);
            m_movementHomeXButton.Name = "m_movementHomeXButton";
            m_movementHomeXButton.Padding = new Padding(10);
            m_movementHomeXButton.Size = new Size(150, 75);
            m_movementHomeXButton.TabIndex = 2;
            m_movementHomeXButton.Tag = "HOMEX";
            m_movementHomeXButton.Text = "Home X";
            m_movementHomeXButton.UseVisualStyleBackColor = false;
            m_movementHomeXButton.Click += OnSimpleCommandButton;
            m_movementHomeYButton.BackColor = Color.LightGray;
            m_movementHomeYButton.Location = new Point(3, 84);
            m_movementHomeYButton.Name = "m_movementHomeYButton";
            m_movementHomeYButton.Padding = new Padding(10);
            m_movementHomeYButton.Size = new Size(150, 75);
            m_movementHomeYButton.TabIndex = 3;
            m_movementHomeYButton.Tag = "HOMEY";
            m_movementHomeYButton.Text = "Home Y";
            m_movementHomeYButton.UseVisualStyleBackColor = false;
            m_movementHomeYButton.Click += OnSimpleCommandButton;
            m_movementTransferButton.BackColor = Color.LightGray;
            m_movementTransferButton.Location = new Point(159, 84);
            m_movementTransferButton.Name = "m_movementTransferButton";
            m_movementTransferButton.Padding = new Padding(10);
            m_movementTransferButton.Size = new Size(150, 75);
            m_movementTransferButton.TabIndex = 4;
            m_movementTransferButton.Tag = "Transfer";
            m_movementTransferButton.Text = "Transfer";
            m_movementTransferButton.UseVisualStyleBackColor = false;
            m_movementTransferButton.Click += OnTransfer;
            m_movementCurrentLocationButton.BackColor = Color.LightGray;
            m_movementCurrentLocationButton.Location = new Point(315, 84);
            m_movementCurrentLocationButton.Name = "m_movementCurrentLocationButton";
            m_movementCurrentLocationButton.Padding = new Padding(10);
            m_movementCurrentLocationButton.Size = new Size(150, 75);
            m_movementCurrentLocationButton.TabIndex = 5;
            m_movementCurrentLocationButton.Tag = "LOC";
            m_movementCurrentLocationButton.Text = "Current Location";
            m_movementCurrentLocationButton.UseVisualStyleBackColor = false;
            m_movementCurrentLocationButton.Click += OnSimpleCommandButton;
            m_movementMoveNoCheckButton.BackColor = Color.LightGray;
            m_movementMoveNoCheckButton.Location = new Point(3, 165);
            m_movementMoveNoCheckButton.Name = "m_movementMoveNoCheckButton";
            m_movementMoveNoCheckButton.Padding = new Padding(10);
            m_movementMoveNoCheckButton.Size = new Size(150, 75);
            m_movementMoveNoCheckButton.TabIndex = 6;
            m_movementMoveNoCheckButton.Tag = "Move Without Sensor Check";
            m_movementMoveNoCheckButton.Text = "Move Without Sensor Check";
            m_movementMoveNoCheckButton.UseVisualStyleBackColor = false;
            m_movementMoveNoCheckButton.Click += OnMoveNoCheck;
            m_inventoryTabPage.BackColor = SystemColors.AppWorkspace;
            m_inventoryTabPage.Controls.Add(m_inventoryFlowLayoutPanel);
            m_inventoryTabPage.Location = new Point(4, 79);
            m_inventoryTabPage.Name = "m_inventoryTabPage";
            m_inventoryTabPage.Size = new Size(612, 537);
            m_inventoryTabPage.TabIndex = 1;
            m_inventoryTabPage.Tag = "Inventory";
            m_inventoryTabPage.Text = "Inventory";
            m_inventoryFlowLayoutPanel.BackColor = Color.WhiteSmoke;
            m_inventoryFlowLayoutPanel.Controls.Add(m_invGetButton);
            m_inventoryFlowLayoutPanel.Controls.Add(m_invPutButton);
            m_inventoryFlowLayoutPanel.Controls.Add(m_invReadButton);
            m_inventoryFlowLayoutPanel.Controls.Add(m_invInspectButton);
            m_inventoryFlowLayoutPanel.Controls.Add(m_invFindEmptyButton);
            m_inventoryFlowLayoutPanel.Controls.Add(m_invSyncSlotButton);
            m_inventoryFlowLayoutPanel.Controls.Add(m_invPutinSlotButton);
            m_inventoryFlowLayoutPanel.Controls.Add(m_invGetVendButton);
            m_inventoryFlowLayoutPanel.Controls.Add(m_invGetCenterButton);
            m_inventoryFlowLayoutPanel.Controls.Add(m_gripperPutInEmptyButton);
            m_inventoryFlowLayoutPanel.Dock = DockStyle.Fill;
            m_inventoryFlowLayoutPanel.Location = new Point(0, 0);
            m_inventoryFlowLayoutPanel.Name = "m_inventoryFlowLayoutPanel";
            m_inventoryFlowLayoutPanel.Size = new Size(612, 537);
            m_inventoryFlowLayoutPanel.TabIndex = 0;
            m_invGetButton.BackColor = Color.LightGray;
            m_invGetButton.Location = new Point(3, 3);
            m_invGetButton.Name = "m_invGetButton";
            m_invGetButton.Size = new Size(150, 75);
            m_invGetButton.TabIndex = 1;
            m_invGetButton.Tag = "GET";
            m_invGetButton.Text = "Get";
            m_invGetButton.UseVisualStyleBackColor = false;
            m_invGetButton.Click += OnSimpleCommandButton;
            m_invPutButton.BackColor = Color.LightGray;
            m_invPutButton.Location = new Point(159, 3);
            m_invPutButton.Name = "m_invPutButton";
            m_invPutButton.Size = new Size(150, 75);
            m_invPutButton.TabIndex = 2;
            m_invPutButton.Tag = "PUT";
            m_invPutButton.Text = "Put";
            m_invPutButton.UseVisualStyleBackColor = false;
            m_invPutButton.Click += OnSimpleCommandButton;
            m_invReadButton.BackColor = Color.LightGray;
            m_invReadButton.Location = new Point(315, 3);
            m_invReadButton.Name = "m_invReadButton";
            m_invReadButton.Size = new Size(150, 75);
            m_invReadButton.TabIndex = 3;
            m_invReadButton.Tag = "READ";
            m_invReadButton.Text = "Read";
            m_invReadButton.UseVisualStyleBackColor = false;
            m_invReadButton.Click += OnSimpleCommandButton;
            m_invInspectButton.BackColor = Color.LightGray;
            m_invInspectButton.Location = new Point(3, 84);
            m_invInspectButton.Name = "m_invInspectButton";
            m_invInspectButton.Size = new Size(150, 75);
            m_invInspectButton.TabIndex = 4;
            m_invInspectButton.Tag = "Inspect";
            m_invInspectButton.Text = "Inspect";
            m_invInspectButton.UseVisualStyleBackColor = false;
            m_invInspectButton.Click += OnInspect;
            m_invFindEmptyButton.BackColor = Color.LightGray;
            m_invFindEmptyButton.Location = new Point(159, 84);
            m_invFindEmptyButton.Name = "m_invFindEmptyButton";
            m_invFindEmptyButton.Size = new Size(150, 75);
            m_invFindEmptyButton.TabIndex = 5;
            m_invFindEmptyButton.Tag = "FINDEMPTY";
            m_invFindEmptyButton.Text = "Find Empty Location";
            m_invFindEmptyButton.UseVisualStyleBackColor = false;
            m_invFindEmptyButton.Click += OnSimpleCommandButton;
            m_invSyncSlotButton.BackColor = Color.LightGray;
            m_invSyncSlotButton.Location = new Point(315, 84);
            m_invSyncSlotButton.Name = "m_invSyncSlotButton";
            m_invSyncSlotButton.Size = new Size(150, 75);
            m_invSyncSlotButton.TabIndex = 7;
            m_invSyncSlotButton.Tag = "Sync Slot";
            m_invSyncSlotButton.Text = "Sync Slot";
            m_invSyncSlotButton.UseVisualStyleBackColor = false;
            m_invSyncSlotButton.Click += OnSyncSlot;
            m_invPutinSlotButton.BackColor = Color.LightGray;
            m_invPutinSlotButton.Location = new Point(3, 165);
            m_invPutinSlotButton.Name = "m_invPutinSlotButton";
            m_invPutinSlotButton.Size = new Size(150, 75);
            m_invPutinSlotButton.TabIndex = 8;
            m_invPutinSlotButton.Tag = "Put DVD In Picker Into Slot";
            m_invPutinSlotButton.Text = "Put DVD In Picker Into Slot";
            m_invPutinSlotButton.UseVisualStyleBackColor = false;
            m_invPutinSlotButton.Click += OnPutDvdInPickerIntoSlot;
            m_invGetVendButton.BackColor = Color.LightGray;
            m_invGetVendButton.Location = new Point(159, 165);
            m_invGetVendButton.Name = "m_invGetVendButton";
            m_invGetVendButton.Size = new Size(150, 75);
            m_invGetVendButton.TabIndex = 9;
            m_invGetVendButton.Tag = "Get and Vend DVD";
            m_invGetVendButton.Text = "Get and Vend DVD";
            m_invGetVendButton.UseVisualStyleBackColor = false;
            m_invGetVendButton.Click += OnGetAndVend;
            m_invGetCenterButton.BackColor = Color.LightGray;
            m_invGetCenterButton.Location = new Point(315, 165);
            m_invGetCenterButton.Name = "m_invGetCenterButton";
            m_invGetCenterButton.Size = new Size(150, 75);
            m_invGetCenterButton.TabIndex = 10;
            m_invGetCenterButton.Tag = "Get And Center DVD";
            m_invGetCenterButton.Text = "Get And Center DVD";
            m_invGetCenterButton.UseVisualStyleBackColor = false;
            m_invGetCenterButton.Click += OnGetAndCenter;
            m_gripperPutInEmptyButton.BackColor = Color.LightGray;
            m_gripperPutInEmptyButton.Location = new Point(3, 246);
            m_gripperPutInEmptyButton.Name = "m_gripperPutInEmptyButton";
            m_gripperPutInEmptyButton.Size = new Size(150, 75);
            m_gripperPutInEmptyButton.TabIndex = 11;
            m_gripperPutInEmptyButton.Tag = "GRIPPER PUT-DISC-AWAY";
            m_gripperPutInEmptyButton.Text = "Put In Empty Slot";
            m_gripperPutInEmptyButton.UseVisualStyleBackColor = false;
            m_gripperPutInEmptyButton.Click += OnPutInEmptySlot;
            m_rollerTabPage.BackColor = SystemColors.AppWorkspace;
            m_rollerTabPage.Controls.Add(m_rollerFlowLayoutPanel);
            m_rollerTabPage.Location = new Point(4, 79);
            m_rollerTabPage.Name = "m_rollerTabPage";
            m_rollerTabPage.Size = new Size(612, 537);
            m_rollerTabPage.TabIndex = 1;
            m_rollerTabPage.Tag = "Roller";
            m_rollerTabPage.Text = "Roller";
            m_rollerFlowLayoutPanel.BackColor = Color.WhiteSmoke;
            m_rollerFlowLayoutPanel.Controls.Add(m_rollerRollerInButton);
            m_rollerFlowLayoutPanel.Controls.Add(m_rollerRollerOut);
            m_rollerFlowLayoutPanel.Controls.Add(m_rollerToPositionButton);
            m_rollerFlowLayoutPanel.Controls.Add(m_rollerStopButton);
            m_rollerFlowLayoutPanel.Dock = DockStyle.Fill;
            m_rollerFlowLayoutPanel.Location = new Point(0, 0);
            m_rollerFlowLayoutPanel.Name = "m_rollerFlowLayoutPanel";
            m_rollerFlowLayoutPanel.Size = new Size(612, 537);
            m_rollerFlowLayoutPanel.TabIndex = 0;
            m_rollerRollerInButton.BackColor = Color.LightGray;
            m_rollerRollerInButton.Location = new Point(3, 3);
            m_rollerRollerInButton.Name = "m_rollerRollerInButton";
            m_rollerRollerInButton.Size = new Size(150, 75);
            m_rollerRollerInButton.TabIndex = 0;
            m_rollerRollerInButton.Tag = "ROLLER IN";
            m_rollerRollerInButton.Text = "Roller In";
            m_rollerRollerInButton.UseVisualStyleBackColor = false;
            m_rollerRollerInButton.Click += OnSimpleCommandButton;
            m_rollerRollerOut.BackColor = Color.LightGray;
            m_rollerRollerOut.Location = new Point(159, 3);
            m_rollerRollerOut.Name = "m_rollerRollerOut";
            m_rollerRollerOut.Size = new Size(150, 75);
            m_rollerRollerOut.TabIndex = 1;
            m_rollerRollerOut.Tag = "ROLLER OUT";
            m_rollerRollerOut.Text = "Roller Out";
            m_rollerRollerOut.UseVisualStyleBackColor = false;
            m_rollerRollerOut.Click += OnSimpleCommandButton;
            m_rollerToPositionButton.BackColor = Color.LightGray;
            m_rollerToPositionButton.Location = new Point(315, 3);
            m_rollerToPositionButton.Name = "m_rollerToPositionButton";
            m_rollerToPositionButton.Size = new Size(150, 75);
            m_rollerToPositionButton.TabIndex = 2;
            m_rollerToPositionButton.Tag = "Roller To Position";
            m_rollerToPositionButton.Text = "Roller To Position";
            m_rollerToPositionButton.UseVisualStyleBackColor = false;
            m_rollerToPositionButton.Click += OnRollerToPosition;
            m_rollerStopButton.BackColor = Color.LightGray;
            m_rollerStopButton.Location = new Point(3, 84);
            m_rollerStopButton.Name = "m_rollerStopButton";
            m_rollerStopButton.Size = new Size(150, 75);
            m_rollerStopButton.TabIndex = 3;
            m_rollerStopButton.Tag = "ROLLER STOP";
            m_rollerStopButton.Text = "Roller Stop";
            m_rollerStopButton.UseVisualStyleBackColor = false;
            m_rollerStopButton.Click += OnSimpleCommandButton;
            m_gripperTabPage.BackColor = SystemColors.AppWorkspace;
            m_gripperTabPage.Controls.Add(m_gripperFlowLayoutPanel);
            m_gripperTabPage.Location = new Point(4, 79);
            m_gripperTabPage.Name = "m_gripperTabPage";
            m_gripperTabPage.Size = new Size(612, 537);
            m_gripperTabPage.TabIndex = 1;
            m_gripperTabPage.Tag = "Gripper";
            m_gripperTabPage.Text = "Gripper";
            m_gripperFlowLayoutPanel.BackColor = Color.WhiteSmoke;
            m_gripperFlowLayoutPanel.Controls.Add(m_gripperExtendButton);
            m_gripperFlowLayoutPanel.Controls.Add(m_gripperRetractButton);
            m_gripperFlowLayoutPanel.Controls.Add(m_gripperOpenButton);
            m_gripperFlowLayoutPanel.Controls.Add(m_gripperRentButton);
            m_gripperFlowLayoutPanel.Controls.Add(m_gripperCloseButton);
            m_gripperFlowLayoutPanel.Controls.Add(m_gripperPeekButton);
            m_gripperFlowLayoutPanel.Controls.Add(m_gripperPushButton);
            m_gripperFlowLayoutPanel.Controls.Add(m_gripperStatusButton);
            m_gripperFlowLayoutPanel.Controls.Add(m_gripperClearButton);
            m_gripperFlowLayoutPanel.Controls.Add(m_gripperPushDvdInButton);
            m_gripperFlowLayoutPanel.Dock = DockStyle.Fill;
            m_gripperFlowLayoutPanel.Location = new Point(0, 0);
            m_gripperFlowLayoutPanel.Name = "m_gripperFlowLayoutPanel";
            m_gripperFlowLayoutPanel.Size = new Size(612, 537);
            m_gripperFlowLayoutPanel.TabIndex = 0;
            m_gripperExtendButton.BackColor = Color.LightGray;
            m_gripperExtendButton.Location = new Point(3, 3);
            m_gripperExtendButton.Name = "m_gripperExtendButton";
            m_gripperExtendButton.Size = new Size(150, 75);
            m_gripperExtendButton.TabIndex = 0;
            m_gripperExtendButton.Tag = "GRIPPER EXTEND";
            m_gripperExtendButton.Text = "Gripper Extend";
            m_gripperExtendButton.UseVisualStyleBackColor = false;
            m_gripperExtendButton.Click += OnSimpleCommandButton;
            m_gripperRetractButton.BackColor = Color.LightGray;
            m_gripperRetractButton.Location = new Point(159, 3);
            m_gripperRetractButton.Name = "m_gripperRetractButton";
            m_gripperRetractButton.Size = new Size(150, 75);
            m_gripperRetractButton.TabIndex = 1;
            m_gripperRetractButton.Tag = "GRIPPER RETRACT";
            m_gripperRetractButton.Text = "Gripper Retract";
            m_gripperRetractButton.UseVisualStyleBackColor = false;
            m_gripperRetractButton.Click += OnSimpleCommandButton;
            m_gripperOpenButton.BackColor = Color.LightGray;
            m_gripperOpenButton.Location = new Point(315, 3);
            m_gripperOpenButton.Name = "m_gripperOpenButton";
            m_gripperOpenButton.Size = new Size(150, 75);
            m_gripperOpenButton.TabIndex = 2;
            m_gripperOpenButton.Tag = "GRIPPER OPEN";
            m_gripperOpenButton.Text = "Gripper Open";
            m_gripperOpenButton.UseVisualStyleBackColor = false;
            m_gripperOpenButton.Click += OnSimpleCommandButton;
            m_gripperRentButton.BackColor = Color.LightGray;
            m_gripperRentButton.Location = new Point(3, 84);
            m_gripperRentButton.Name = "m_gripperRentButton";
            m_gripperRentButton.Size = new Size(150, 75);
            m_gripperRentButton.TabIndex = 3;
            m_gripperRentButton.Tag = "GRIPPER RENT";
            m_gripperRentButton.Text = "Gripper Rent";
            m_gripperRentButton.UseVisualStyleBackColor = false;
            m_gripperRentButton.Click += OnSimpleCommandButton;
            m_gripperCloseButton.BackColor = Color.LightGray;
            m_gripperCloseButton.Location = new Point(159, 84);
            m_gripperCloseButton.Name = "m_gripperCloseButton";
            m_gripperCloseButton.Size = new Size(150, 75);
            m_gripperCloseButton.TabIndex = 4;
            m_gripperCloseButton.Tag = "GRIPPER CLOSE";
            m_gripperCloseButton.Text = "Gripper Close";
            m_gripperCloseButton.UseVisualStyleBackColor = false;
            m_gripperCloseButton.Click += OnSimpleCommandButton;
            m_gripperPeekButton.BackColor = Color.LightGray;
            m_gripperPeekButton.Location = new Point(315, 84);
            m_gripperPeekButton.Name = "m_gripperPeekButton";
            m_gripperPeekButton.Size = new Size(150, 75);
            m_gripperPeekButton.TabIndex = 5;
            m_gripperPeekButton.Tag = "GRIPPER PEEK";
            m_gripperPeekButton.Text = "Gripper Peek";
            m_gripperPeekButton.UseVisualStyleBackColor = false;
            m_gripperPeekButton.Click += OnSimpleCommandButton;
            m_gripperPushButton.BackColor = Color.LightGray;
            m_gripperPushButton.Location = new Point(3, 165);
            m_gripperPushButton.Name = "m_gripperPushButton";
            m_gripperPushButton.Size = new Size(150, 75);
            m_gripperPushButton.TabIndex = 6;
            m_gripperPushButton.Tag = "GRIPPER PUSH";
            m_gripperPushButton.Text = "Gripper Push";
            m_gripperPushButton.UseVisualStyleBackColor = false;
            m_gripperPushButton.Click += OnSimpleCommandButton;
            m_gripperStatusButton.BackColor = Color.LightGray;
            m_gripperStatusButton.Location = new Point(159, 165);
            m_gripperStatusButton.Name = "m_gripperStatusButton";
            m_gripperStatusButton.Size = new Size(150, 75);
            m_gripperStatusButton.TabIndex = 7;
            m_gripperStatusButton.Tag = "GRIPPER STATUS";
            m_gripperStatusButton.Text = "Gripper Status";
            m_gripperStatusButton.UseVisualStyleBackColor = false;
            m_gripperStatusButton.Click += OnSimpleCommandButton;
            m_gripperClearButton.BackColor = Color.LightGray;
            m_gripperClearButton.Location = new Point(315, 165);
            m_gripperClearButton.Name = "m_gripperClearButton";
            m_gripperClearButton.Size = new Size(150, 75);
            m_gripperClearButton.TabIndex = 8;
            m_gripperClearButton.Tag = "GRIPPER CLEAR";
            m_gripperClearButton.Text = "Gripper Clear";
            m_gripperClearButton.UseVisualStyleBackColor = false;
            m_gripperClearButton.Click += OnSimpleCommandButton;
            m_gripperPushDvdInButton.BackColor = Color.LightGray;
            m_gripperPushDvdInButton.Location = new Point(3, 246);
            m_gripperPushDvdInButton.Name = "m_gripperPushDvdInButton";
            m_gripperPushDvdInButton.Size = new Size(150, 75);
            m_gripperPushDvdInButton.TabIndex = 9;
            m_gripperPushDvdInButton.Tag = "Push DVD In";
            m_gripperPushDvdInButton.Text = "Push DVD In";
            m_gripperPushDvdInButton.UseVisualStyleBackColor = false;
            m_gripperPushDvdInButton.Click += OnPushDVDIn;
            m_cameraTabPage.BackColor = SystemColors.AppWorkspace;
            m_cameraTabPage.Controls.Add(m_cameraFlowLayoutPanel);
            m_cameraTabPage.Location = new Point(4, 79);
            m_cameraTabPage.Name = "m_cameraTabPage";
            m_cameraTabPage.Size = new Size(612, 537);
            m_cameraTabPage.TabIndex = 2;
            m_cameraTabPage.Tag = "Camera";
            m_cameraTabPage.Text = "Camera";
            m_cameraFlowLayoutPanel.BackColor = Color.WhiteSmoke;
            m_cameraFlowLayoutPanel.Controls.Add(m_cameraRingOnButton);
            m_cameraFlowLayoutPanel.Controls.Add(m_cameraRingOffButton);
            m_cameraFlowLayoutPanel.Controls.Add(m_cameraStartButton);
            m_cameraFlowLayoutPanel.Controls.Add(m_cameraSnap);
            m_cameraFlowLayoutPanel.Controls.Add(m_cameraStopButton);
            m_cameraFlowLayoutPanel.Controls.Add(m_cameraStatusButton);
            m_cameraFlowLayoutPanel.Dock = DockStyle.Fill;
            m_cameraFlowLayoutPanel.Location = new Point(0, 0);
            m_cameraFlowLayoutPanel.Name = "m_cameraFlowLayoutPanel";
            m_cameraFlowLayoutPanel.Size = new Size(612, 537);
            m_cameraFlowLayoutPanel.TabIndex = 0;
            m_cameraRingOnButton.BackColor = Color.LightGray;
            m_cameraRingOnButton.Location = new Point(3, 3);
            m_cameraRingOnButton.Name = "m_cameraRingOnButton";
            m_cameraRingOnButton.Size = new Size(150, 75);
            m_cameraRingOnButton.TabIndex = 0;
            m_cameraRingOnButton.Tag = "RINGLIGHT ON";
            m_cameraRingOnButton.Text = "Ring Light On";
            m_cameraRingOnButton.UseVisualStyleBackColor = false;
            m_cameraRingOnButton.Click += OnSimpleCommandButton;
            m_cameraRingOffButton.BackColor = Color.LightGray;
            m_cameraRingOffButton.Location = new Point(159, 3);
            m_cameraRingOffButton.Name = "m_cameraRingOffButton";
            m_cameraRingOffButton.Size = new Size(150, 75);
            m_cameraRingOffButton.TabIndex = 1;
            m_cameraRingOffButton.Tag = "RINGLIGHT OFF";
            m_cameraRingOffButton.Text = "Ring Light Off";
            m_cameraRingOffButton.UseVisualStyleBackColor = false;
            m_cameraRingOffButton.Click += OnSimpleCommandButton;
            m_cameraStartButton.BackColor = Color.LightGray;
            m_cameraStartButton.Location = new Point(315, 3);
            m_cameraStartButton.Name = "m_cameraStartButton";
            m_cameraStartButton.Size = new Size(150, 75);
            m_cameraStartButton.TabIndex = 2;
            m_cameraStartButton.Tag = "CAMERA START CENTER=TRUE";
            m_cameraStartButton.Text = "Camera Start";
            m_cameraStartButton.UseVisualStyleBackColor = false;
            m_cameraStartButton.Click += OnSimpleCommandButton;
            m_cameraSnap.BackColor = Color.LightGray;
            m_cameraSnap.Location = new Point(3, 84);
            m_cameraSnap.Name = "m_cameraSnap";
            m_cameraSnap.Size = new Size(150, 75);
            m_cameraSnap.TabIndex = 3;
            m_cameraSnap.Tag = "CAMERA SNAP";
            m_cameraSnap.Text = "Camera Snap";
            m_cameraSnap.UseVisualStyleBackColor = false;
            m_cameraSnap.Click += OnSimpleCommandButton;
            m_cameraStopButton.BackColor = Color.LightGray;
            m_cameraStopButton.Location = new Point(159, 84);
            m_cameraStopButton.Name = "m_cameraStopButton";
            m_cameraStopButton.Size = new Size(150, 75);
            m_cameraStopButton.TabIndex = 4;
            m_cameraStopButton.Tag = "CAMERA STOP";
            m_cameraStopButton.Text = "Camera Stop";
            m_cameraStopButton.UseVisualStyleBackColor = false;
            m_cameraStopButton.Click += OnSimpleCommandButton;
            m_cameraStatusButton.BackColor = Color.LightGray;
            m_cameraStatusButton.Location = new Point(315, 84);
            m_cameraStatusButton.Name = "m_cameraStatusButton";
            m_cameraStatusButton.Size = new Size(150, 75);
            m_cameraStatusButton.TabIndex = 5;
            m_cameraStatusButton.Tag = "CAMERA STATUS";
            m_cameraStatusButton.Text = "Camera Status";
            m_cameraStatusButton.UseVisualStyleBackColor = false;
            m_cameraStatusButton.Click += OnSimpleCommandButton;
            m_vendDoorTabPage.BackColor = SystemColors.AppWorkspace;
            m_vendDoorTabPage.Controls.Add(m_vendDoorFlowLayoutPanel);
            m_vendDoorTabPage.Location = new Point(4, 79);
            m_vendDoorTabPage.Name = "m_vendDoorTabPage";
            m_vendDoorTabPage.Size = new Size(612, 537);
            m_vendDoorTabPage.TabIndex = 3;
            m_vendDoorTabPage.Tag = "VendDoor";
            m_vendDoorTabPage.Text = "VendDoor";
            m_vendDoorFlowLayoutPanel.BackColor = Color.WhiteSmoke;
            m_vendDoorFlowLayoutPanel.Controls.Add(m_vendDoorRentButton);
            m_vendDoorFlowLayoutPanel.Controls.Add(m_vendDoorStatusButton);
            m_vendDoorFlowLayoutPanel.Controls.Add(m_vendDoorCloseButton);
            m_vendDoorFlowLayoutPanel.Dock = DockStyle.Fill;
            m_vendDoorFlowLayoutPanel.Location = new Point(0, 0);
            m_vendDoorFlowLayoutPanel.Name = "m_vendDoorFlowLayoutPanel";
            m_vendDoorFlowLayoutPanel.Size = new Size(612, 537);
            m_vendDoorFlowLayoutPanel.TabIndex = 0;
            m_vendDoorRentButton.BackColor = Color.LightGray;
            m_vendDoorRentButton.Location = new Point(3, 3);
            m_vendDoorRentButton.Name = "m_vendDoorRentButton";
            m_vendDoorRentButton.Size = new Size(150, 75);
            m_vendDoorRentButton.TabIndex = 1;
            m_vendDoorRentButton.Tag = "VENDDOOR RENT";
            m_vendDoorRentButton.Text = "Vend Door Rent";
            m_vendDoorRentButton.UseVisualStyleBackColor = false;
            m_vendDoorRentButton.Click += OnSimpleCommandButton;
            m_vendDoorStatusButton.BackColor = Color.LightGray;
            m_vendDoorStatusButton.Location = new Point(159, 3);
            m_vendDoorStatusButton.Name = "m_vendDoorStatusButton";
            m_vendDoorStatusButton.Size = new Size(150, 75);
            m_vendDoorStatusButton.TabIndex = 2;
            m_vendDoorStatusButton.Tag = "VENDDOOR STATUS";
            m_vendDoorStatusButton.Text = "Vend Door Status";
            m_vendDoorStatusButton.UseVisualStyleBackColor = false;
            m_vendDoorStatusButton.Click += OnSimpleCommandButton;
            m_vendDoorCloseButton.BackColor = Color.LightGray;
            m_vendDoorCloseButton.Location = new Point(315, 3);
            m_vendDoorCloseButton.Name = "m_vendDoorCloseButton";
            m_vendDoorCloseButton.Size = new Size(150, 75);
            m_vendDoorCloseButton.TabIndex = 3;
            m_vendDoorCloseButton.Tag = "VENDDOOR CLOSE";
            m_vendDoorCloseButton.Text = "Vend Door Close";
            m_vendDoorCloseButton.UseVisualStyleBackColor = false;
            m_vendDoorCloseButton.Click += OnSimpleCommandButton;
            m_trackTabPage.BackColor = SystemColors.AppWorkspace;
            m_trackTabPage.Controls.Add(m_trackFlowLayoutPanel);
            m_trackTabPage.Location = new Point(4, 79);
            m_trackTabPage.Name = "m_trackTabPage";
            m_trackTabPage.Size = new Size(612, 537);
            m_trackTabPage.TabIndex = 4;
            m_trackTabPage.Tag = "Track";
            m_trackTabPage.Text = "Track";
            m_trackFlowLayoutPanel.BackColor = Color.WhiteSmoke;
            m_trackFlowLayoutPanel.Controls.Add(m_trackOpenButton);
            m_trackFlowLayoutPanel.Controls.Add(m_trackCloseButton);
            m_trackFlowLayoutPanel.Controls.Add(m_trackStatusButton);
            m_trackFlowLayoutPanel.Dock = DockStyle.Fill;
            m_trackFlowLayoutPanel.Location = new Point(0, 0);
            m_trackFlowLayoutPanel.Name = "m_trackFlowLayoutPanel";
            m_trackFlowLayoutPanel.Size = new Size(612, 537);
            m_trackFlowLayoutPanel.TabIndex = 0;
            m_trackOpenButton.BackColor = Color.LightGray;
            m_trackOpenButton.Location = new Point(3, 3);
            m_trackOpenButton.Name = "m_trackOpenButton";
            m_trackOpenButton.Size = new Size(150, 75);
            m_trackOpenButton.TabIndex = 0;
            m_trackOpenButton.Tag = "TRACK OPEN";
            m_trackOpenButton.Text = "Track Open";
            m_trackOpenButton.UseVisualStyleBackColor = false;
            m_trackOpenButton.Click += OnSimpleCommandButton;
            m_trackCloseButton.BackColor = Color.LightGray;
            m_trackCloseButton.Location = new Point(159, 3);
            m_trackCloseButton.Name = "m_trackCloseButton";
            m_trackCloseButton.Size = new Size(150, 75);
            m_trackCloseButton.TabIndex = 1;
            m_trackCloseButton.Tag = "TRACK CLOSE";
            m_trackCloseButton.Text = "Track Close";
            m_trackCloseButton.UseVisualStyleBackColor = false;
            m_trackCloseButton.Click += OnSimpleCommandButton;
            m_trackStatusButton.BackColor = Color.LightGray;
            m_trackStatusButton.Location = new Point(315, 3);
            m_trackStatusButton.Name = "m_trackStatusButton";
            m_trackStatusButton.Size = new Size(150, 75);
            m_trackStatusButton.TabIndex = 2;
            m_trackStatusButton.Tag = "TRACK STATUS";
            m_trackStatusButton.Text = "Track Status";
            m_trackStatusButton.UseVisualStyleBackColor = false;
            m_trackStatusButton.Click += OnSimpleCommandButton;
            m_sensorTabPage.BackColor = SystemColors.AppWorkspace;
            m_sensorTabPage.Controls.Add(m_sensorFlowLayoutPanel);
            m_sensorTabPage.Location = new Point(4, 79);
            m_sensorTabPage.Name = "m_sensorTabPage";
            m_sensorTabPage.Size = new Size(612, 537);
            m_sensorTabPage.TabIndex = 5;
            m_sensorTabPage.Tag = "Sensor";
            m_sensorTabPage.Text = "Sensor";
            m_sensorFlowLayoutPanel.BackColor = Color.WhiteSmoke;
            m_sensorFlowLayoutPanel.Controls.Add(m_sensorPickerOnButton);
            m_sensorFlowLayoutPanel.Controls.Add(m_sensorPickerOffButton);
            m_sensorFlowLayoutPanel.Controls.Add(m_sensorPickerReadButton);
            m_sensorFlowLayoutPanel.Controls.Add(m_sensorPickerWaitClearButton);
            m_sensorFlowLayoutPanel.Controls.Add(m_sensorPickerWaitBlockButton);
            m_sensorFlowLayoutPanel.Controls.Add(m_sensorReadPickerInputsButton);
            m_sensorFlowLayoutPanel.Controls.Add(m_sensorReadAuxInputsButton);
            m_sensorFlowLayoutPanel.Dock = DockStyle.Fill;
            m_sensorFlowLayoutPanel.Location = new Point(0, 0);
            m_sensorFlowLayoutPanel.Name = "m_sensorFlowLayoutPanel";
            m_sensorFlowLayoutPanel.Size = new Size(612, 537);
            m_sensorFlowLayoutPanel.TabIndex = 0;
            m_sensorPickerOnButton.BackColor = Color.LightGray;
            m_sensorPickerOnButton.Location = new Point(3, 3);
            m_sensorPickerOnButton.Name = "m_sensorPickerOnButton";
            m_sensorPickerOnButton.Size = new Size(150, 75);
            m_sensorPickerOnButton.TabIndex = 0;
            m_sensorPickerOnButton.Tag = "SENSOR PICKER-ON";
            m_sensorPickerOnButton.Text = "Picker Sensors On";
            m_sensorPickerOnButton.UseVisualStyleBackColor = false;
            m_sensorPickerOnButton.Click += OnSimpleCommandButton;
            m_sensorPickerOffButton.BackColor = Color.LightGray;
            m_sensorPickerOffButton.Location = new Point(159, 3);
            m_sensorPickerOffButton.Name = "m_sensorPickerOffButton";
            m_sensorPickerOffButton.Size = new Size(150, 75);
            m_sensorPickerOffButton.TabIndex = 1;
            m_sensorPickerOffButton.Tag = "SENSOR PICKER-OFF";
            m_sensorPickerOffButton.Text = "Picker Sensors Off";
            m_sensorPickerOffButton.UseVisualStyleBackColor = false;
            m_sensorPickerOffButton.Click += OnSimpleCommandButton;
            m_sensorPickerReadButton.BackColor = Color.LightGray;
            m_sensorPickerReadButton.Location = new Point(315, 3);
            m_sensorPickerReadButton.Name = "m_sensorPickerReadButton";
            m_sensorPickerReadButton.Size = new Size(150, 75);
            m_sensorPickerReadButton.TabIndex = 2;
            m_sensorPickerReadButton.Tag = "Read Picker Sensors";
            m_sensorPickerReadButton.Text = "Read Picker Sensors";
            m_sensorPickerReadButton.UseVisualStyleBackColor = false;
            m_sensorPickerReadButton.Click += OnReadPickerSensors;
            m_sensorPickerWaitClearButton.BackColor = Color.LightGray;
            m_sensorPickerWaitClearButton.Location = new Point(3, 84);
            m_sensorPickerWaitClearButton.Name = "m_sensorPickerWaitClearButton";
            m_sensorPickerWaitClearButton.Size = new Size(150, 75);
            m_sensorPickerWaitClearButton.TabIndex = 3;
            m_sensorPickerWaitClearButton.Tag = "Wait for Picker Sensors to Clear";
            m_sensorPickerWaitClearButton.Text = "Wait for Picker Sensors to Clear";
            m_sensorPickerWaitClearButton.UseVisualStyleBackColor = false;
            m_sensorPickerWaitClearButton.Click += OnWaitPickerSensorsClear;
            m_sensorPickerWaitBlockButton.BackColor = Color.LightGray;
            m_sensorPickerWaitBlockButton.Location = new Point(159, 84);
            m_sensorPickerWaitBlockButton.Name = "m_sensorPickerWaitBlockButton";
            m_sensorPickerWaitBlockButton.Size = new Size(150, 75);
            m_sensorPickerWaitBlockButton.TabIndex = 4;
            m_sensorPickerWaitBlockButton.Tag = "Wait for Picker Sensors To Block";
            m_sensorPickerWaitBlockButton.Text = "Wait for Picker Sensors To Block";
            m_sensorPickerWaitBlockButton.UseVisualStyleBackColor = false;
            m_sensorPickerWaitBlockButton.Click += OnWaitPickerSensorsBlock;
            m_sensorReadPickerInputsButton.BackColor = Color.LightGray;
            m_sensorReadPickerInputsButton.Location = new Point(315, 84);
            m_sensorReadPickerInputsButton.Name = "m_sensorReadPickerInputsButton";
            m_sensorReadPickerInputsButton.Size = new Size(150, 75);
            m_sensorReadPickerInputsButton.TabIndex = 5;
            m_sensorReadPickerInputsButton.Tag = "SENSOR READ-ALL-PICKER";
            m_sensorReadPickerInputsButton.Text = "Read Picker Inputs";
            m_sensorReadPickerInputsButton.UseVisualStyleBackColor = false;
            m_sensorReadPickerInputsButton.Click += OnSimpleCommandButton;
            m_sensorReadAuxInputsButton.BackColor = Color.LightGray;
            m_sensorReadAuxInputsButton.Location = new Point(3, 165);
            m_sensorReadAuxInputsButton.Name = "m_sensorReadAuxInputsButton";
            m_sensorReadAuxInputsButton.Size = new Size(150, 75);
            m_sensorReadAuxInputsButton.TabIndex = 7;
            m_sensorReadAuxInputsButton.Tag = "SENSOR READ-ALL-AUX";
            m_sensorReadAuxInputsButton.Text = "Read Aux Inputs";
            m_sensorReadAuxInputsButton.UseVisualStyleBackColor = false;
            m_sensorReadAuxInputsButton.Click += OnSimpleCommandButton;
            m_qlmTabPage.BackColor = SystemColors.AppWorkspace;
            m_qlmTabPage.Controls.Add(m_qlmFlowLayoutPanel);
            m_qlmTabPage.Location = new Point(4, 79);
            m_qlmTabPage.Name = "m_qlmTabPage";
            m_qlmTabPage.Size = new Size(612, 537);
            m_qlmTabPage.TabIndex = 6;
            m_qlmTabPage.Tag = "Qlm";
            m_qlmTabPage.Text = "Qlm";
            m_qlmFlowLayoutPanel.BackColor = Color.WhiteSmoke;
            m_qlmFlowLayoutPanel.Controls.Add(m_qlmEngageButton);
            m_qlmFlowLayoutPanel.Controls.Add(m_qlmDisengageButton);
            m_qlmFlowLayoutPanel.Controls.Add(m_qlmStatusButton);
            m_qlmFlowLayoutPanel.Controls.Add(m_qlmLiftButton);
            m_qlmFlowLayoutPanel.Controls.Add(m_qlmDropButton);
            m_qlmFlowLayoutPanel.Controls.Add(m_qlmHaltButton);
            m_qlmFlowLayoutPanel.Dock = DockStyle.Fill;
            m_qlmFlowLayoutPanel.Location = new Point(0, 0);
            m_qlmFlowLayoutPanel.Name = "m_qlmFlowLayoutPanel";
            m_qlmFlowLayoutPanel.Size = new Size(612, 537);
            m_qlmFlowLayoutPanel.TabIndex = 0;
            m_qlmEngageButton.BackColor = Color.LightGray;
            m_qlmEngageButton.Location = new Point(3, 3);
            m_qlmEngageButton.Name = "m_qlmEngageButton";
            m_qlmEngageButton.Size = new Size(150, 75);
            m_qlmEngageButton.TabIndex = 3;
            m_qlmEngageButton.Tag = "QLM ENGAGE";
            m_qlmEngageButton.Text = "QLM Engage";
            m_qlmEngageButton.UseVisualStyleBackColor = false;
            m_qlmEngageButton.Click += OnSimpleCommandButton;
            m_qlmDisengageButton.BackColor = Color.LightGray;
            m_qlmDisengageButton.Location = new Point(159, 3);
            m_qlmDisengageButton.Name = "m_qlmDisengageButton";
            m_qlmDisengageButton.Size = new Size(150, 75);
            m_qlmDisengageButton.TabIndex = 4;
            m_qlmDisengageButton.Tag = "QLM DISENGAGE";
            m_qlmDisengageButton.Text = "QLM Disengage";
            m_qlmDisengageButton.UseVisualStyleBackColor = false;
            m_qlmDisengageButton.Click += OnSimpleCommandButton;
            m_qlmStatusButton.BackColor = Color.LightGray;
            m_qlmStatusButton.Location = new Point(315, 3);
            m_qlmStatusButton.Name = "m_qlmStatusButton";
            m_qlmStatusButton.Size = new Size(150, 75);
            m_qlmStatusButton.TabIndex = 5;
            m_qlmStatusButton.Tag = "QLM STATUS";
            m_qlmStatusButton.Text = "QLM Status";
            m_qlmStatusButton.UseVisualStyleBackColor = false;
            m_qlmStatusButton.Click += OnSimpleCommandButton;
            m_qlmLiftButton.BackColor = Color.LightGray;
            m_qlmLiftButton.Location = new Point(3, 84);
            m_qlmLiftButton.Name = "m_qlmLiftButton";
            m_qlmLiftButton.Size = new Size(150, 75);
            m_qlmLiftButton.TabIndex = 6;
            m_qlmLiftButton.Tag = "QLM LIFT";
            m_qlmLiftButton.Text = "QLM Lift";
            m_qlmLiftButton.UseVisualStyleBackColor = false;
            m_qlmLiftButton.Click += OnSimpleCommandButton;
            m_qlmDropButton.BackColor = Color.LightGray;
            m_qlmDropButton.Location = new Point(159, 84);
            m_qlmDropButton.Name = "m_qlmDropButton";
            m_qlmDropButton.Size = new Size(150, 75);
            m_qlmDropButton.TabIndex = 7;
            m_qlmDropButton.Tag = "QLM DROP";
            m_qlmDropButton.Text = "QLM Drop";
            m_qlmDropButton.UseVisualStyleBackColor = false;
            m_qlmDropButton.Click += OnSimpleCommandButton;
            m_qlmHaltButton.BackColor = Color.LightGray;
            m_qlmHaltButton.Location = new Point(315, 84);
            m_qlmHaltButton.Name = "m_qlmHaltButton";
            m_qlmHaltButton.Size = new Size(150, 75);
            m_qlmHaltButton.TabIndex = 8;
            m_qlmHaltButton.Tag = "QLM Halt";
            m_qlmHaltButton.Text = "QLM Halt";
            m_qlmHaltButton.UseVisualStyleBackColor = false;
            m_qlmHaltButton.Click += OnSimpleCommandButton;
            m_systemTabPage.BackColor = SystemColors.AppWorkspace;
            m_systemTabPage.Controls.Add(m_systemFlowLayoutPanel);
            m_systemTabPage.Location = new Point(4, 79);
            m_systemTabPage.Name = "m_systemTabPage";
            m_systemTabPage.Size = new Size(612, 537);
            m_systemTabPage.TabIndex = 7;
            m_systemTabPage.Tag = "System";
            m_systemTabPage.Text = "System";
            m_systemFlowLayoutPanel.BackColor = Color.WhiteSmoke;
            m_systemFlowLayoutPanel.Controls.Add(m_systemResetButton);
            m_systemFlowLayoutPanel.Controls.Add(m_systemVersionButton);
            m_systemFlowLayoutPanel.Controls.Add(button1);
            m_systemFlowLayoutPanel.Dock = DockStyle.Fill;
            m_systemFlowLayoutPanel.Location = new Point(0, 0);
            m_systemFlowLayoutPanel.Name = "m_systemFlowLayoutPanel";
            m_systemFlowLayoutPanel.Size = new Size(612, 537);
            m_systemFlowLayoutPanel.TabIndex = 0;
            m_systemResetButton.BackColor = Color.LightGray;
            m_systemResetButton.Location = new Point(3, 3);
            m_systemResetButton.Name = "m_systemResetButton";
            m_systemResetButton.Size = new Size(150, 75);
            m_systemResetButton.TabIndex = 0;
            m_systemResetButton.Tag = "SERIALBOARD RESET";
            m_systemResetButton.Text = "Reset";
            m_systemResetButton.UseVisualStyleBackColor = false;
            m_systemResetButton.Click += OnSimpleCommandButton;
            m_systemVersionButton.BackColor = Color.LightGray;
            m_systemVersionButton.Location = new Point(159, 3);
            m_systemVersionButton.Name = "m_systemVersionButton";
            m_systemVersionButton.Size = new Size(150, 75);
            m_systemVersionButton.TabIndex = 1;
            m_systemVersionButton.Tag = "VERSION";
            m_systemVersionButton.Text = "Version";
            m_systemVersionButton.UseVisualStyleBackColor = false;
            m_systemVersionButton.Click += OnSimpleCommandButton;
            button1.BackColor = Color.LightGray;
            button1.Location = new Point(315, 3);
            button1.Name = "button1";
            button1.Size = new Size(150, 75);
            button1.TabIndex = 2;
            button1.Tag = "SERIALBOARD CLOSEPORT";
            button1.Text = "Close Serial Board Port";
            button1.UseVisualStyleBackColor = false;
            button1.Click += OnSimpleCommandButton;
            m_audioTabPage.BackColor = SystemColors.AppWorkspace;
            m_audioTabPage.Location = new Point(0, 0);
            m_audioTabPage.Name = "m_audioTabPage";
            m_audioTabPage.Size = new Size(200, 100);
            m_audioTabPage.TabIndex = 0;
            m_audioTabPage.Tag = "Audio";
            m_audioTabPage.Text = "Audio";
            AutoScaleMode = AutoScaleMode.Inherit;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(m_touchScreenAccessTabControl);
            Enabled = false;
            Name = nameof(TouchScreenAccess);
            Size = new Size(620, 620);
            m_touchScreenAccessTabControl.ResumeLayout(false);
            m_movementTabPage.ResumeLayout(false);
            m_movementFlowLayoutPanel.ResumeLayout(false);
            m_inventoryTabPage.ResumeLayout(false);
            m_inventoryFlowLayoutPanel.ResumeLayout(false);
            m_rollerTabPage.ResumeLayout(false);
            m_rollerFlowLayoutPanel.ResumeLayout(false);
            m_gripperTabPage.ResumeLayout(false);
            m_gripperFlowLayoutPanel.ResumeLayout(false);
            m_cameraTabPage.ResumeLayout(false);
            m_cameraFlowLayoutPanel.ResumeLayout(false);
            m_vendDoorTabPage.ResumeLayout(false);
            m_vendDoorFlowLayoutPanel.ResumeLayout(false);
            m_trackTabPage.ResumeLayout(false);
            m_trackFlowLayoutPanel.ResumeLayout(false);
            m_sensorTabPage.ResumeLayout(false);
            m_sensorFlowLayoutPanel.ResumeLayout(false);
            m_qlmTabPage.ResumeLayout(false);
            m_qlmFlowLayoutPanel.ResumeLayout(false);
            m_systemTabPage.ResumeLayout(false);
            m_systemFlowLayoutPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}