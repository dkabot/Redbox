using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Common.GUI.Functions
{
    public class ConfiguredDevicesForm : Form
    {
        private readonly OutputBox AirExchangerOutput;
        private readonly byte[] CenterItemProgram;
        private readonly IDictionary<string, string> FormProperties;
        private readonly OutputBox FraudSensorOutput;
        private readonly bool HasAirExchanger;
        private readonly bool HasCortexCamera;
        private readonly OutputBox KioskTestOutput;
        private readonly ButtonAspectsManager Manager;
        private readonly OutputBox OutputBox;
        private readonly QRInstallHelper QuickReturnControl;
        private readonly HardwareService Service;
        private Button button1;
        private Button button10;
        private Button button11;
        private Button button12;
        private Button button13;
        private Button button14;
        private Button button15;
        private Button button16;
        private Button button17;
        private Button button18;
        private Button button19;
        private Button button2;
        private Button button20;
        private Button button21;
        private Button button22;
        private Button button3;
        private Button button4;
        private Button button5;
        private Button button6;
        private Button button7;
        private Button button8;
        private Button button9;
        private IContainer components;
        private ErrorProvider errorProvider1;
        private KioskFunctionTest FunctionTest;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private bool HasFraudSensor;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private ListView listView1;
        private TabPage m_airExchangerTab;
        private TextBox m_cameraSnapTB;
        private TextBox m_checkDriversStatusTB;
        private Button m_configureFraudSensor;
        private Button m_configureQubeButton;
        private ListBox m_cortexStatusBox;
        private TabPage m_cortexTab;
        private TabControl m_devicesTab;
        private Button m_displaySessionsButton;
        private Button m_exitButton;
        private TextBox m_fraudDeckTB;
        private Button m_fraudPOSTButton;
        private ListBox m_fraudSensorOutput;
        private TabPage m_fraudSensorTab;
        private TextBox m_fraudSlotTB;
        private Label m_iceQubeExchangerLabel;
        private ListBox m_iceQubeOutput;
        private TextBox m_initTB;
        private Button m_installCortexButton;
        private TabPage m_kioskFunctionCheck;
        private ListBox m_kioskTestOutput;
        private Button m_powerOffFraudSensor;
        private Button m_powerOnFraudSensor;
        private Button m_qubeStatusButton;
        private TabPage m_quickReturnTab;
        private TextBox m_readPauseTB;
        private Button m_resetFraudButton;
        private Button m_resetPersistentCounter;
        private Button m_resetQubeBoard;
        private TextBox m_scanPauseTB;
        private TextBox m_sourceDeckTB;
        private TextBox m_sourceSlotTB;
        private Button m_startCortexButton;
        private Button m_startFraudScan;
        private Button m_stopFraudScan;
        private TextBox m_stressIterationsTB;
        private Button m_testDiskForMarkers;
        private Button m_testFraudButton;
        private TextBox m_testVendDoorTB;
        private Button m_uninstallCortexButton;
        private TextBox m_unknownCountTB;
        private TextBox m_userName;
        private TextBox m_verticalTestTB;
        private TabPage tabPage1;

        public ConfiguredDevicesForm(HardwareService service, IDictionary<string, string> properties)
        {
            InitializeComponent();
            OutputBox = new ConfiguredDeviceOuptutBox(m_cortexStatusBox);
            Service = service;
            m_devicesTab.Selected += OnTabSelectedChange;
            Manager = new ButtonAspectsManager();
            AirExchangerOutput = new ConfiguredDeviceOuptutBox(m_iceQubeOutput);
            FraudSensorOutput = new ConfiguredDeviceOuptutBox(m_fraudSensorOutput);
            KioskTestOutput = new ConfiguredDeviceOuptutBox(m_kioskTestOutput);
            using (var machineConfiguration = new MachineConfiguration(Service))
            {
                machineConfiguration.Run();
                HasAirExchanger = machineConfiguration.AirExchangerConfigured;
                HasFraudSensor = machineConfiguration.HasFraudDevice;
                HasCortexCamera = !machineConfiguration.LegacyCamera;
                QuickReturnControl = new QRInstallHelper(Service, machineConfiguration.QuickReturnStatus);
            }

            QuickReturnControl.Dock = DockStyle.Fill;
            m_quickReturnTab.Controls.Add(QuickReturnControl);
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("ROLLER POS=6 TIMEOUT=4000 WAIT=TRUE ");
            stringBuilder.AppendLine(" ROLLER POS=3 TIMEOUT=4000 WAIT=TRUE ");
            stringBuilder.AppendLine(" CLEAR");
            CenterItemProgram = Encoding.ASCII.GetBytes(stringBuilder.ToString());
            FormProperties = properties;
            var flag = false;
            if (FormProperties.ContainsKey("AllowSessionDisplay"))
                try
                {
                    flag = bool.Parse(FormProperties["AllowSessionDisplay"]);
                }
                catch
                {
                    flag = false;
                }

            m_displaySessionsButton.Enabled = m_displaySessionsButton.Visible = flag;
            listView1.Columns.Add("Component", 200);
            listView1.Columns.Add("Value", 200);
        }

        private void ConfigureCortexPanel()
        {
            m_installCortexButton.Enabled = !HasCortexCamera;
            m_uninstallCortexButton.Enabled = HasCortexCamera;
            WriteToBox(string.Format("The camera {0} enabled.", HasCortexCamera ? "is" : (object)"is not"));
        }

        private void OnConfigureIceQubeBoard()
        {
            m_iceQubeExchangerLabel.Text =
                string.Format("The board {0} configured.", HasAirExchanger ? "is" : (object)"is not");
            m_configureQubeButton.Text = HasAirExchanger ? "Remove" : "Configure";
            m_resetQubeBoard.Enabled = HasAirExchanger;
            m_qubeStatusButton.Enabled = HasAirExchanger;
            m_resetPersistentCounter.Enabled = HasAirExchanger;
        }

        private void OnTabSelectedChange(object sender, EventArgs e)
        {
            switch (m_devicesTab.SelectedIndex)
            {
                case 1:
                    OnConfigureIceQubeBoard();
                    break;
                case 2:
                    ConfigureCortexPanel();
                    break;
                case 3:
                    OnConfigureFraudSensor();
                    break;
                case 4:
                    OnConfigureKioskTest();
                    break;
            }
        }

        private void OnConfigureKioskTest()
        {
            var currentSession = ServiceLocator.Instance.GetService<ISessionUserService>().GetCurrentSession();
            FunctionTest = new KioskFunctionTest(Service, KioskTestOutput, HasCortexCamera, currentSession.User);
            m_testVendDoorTB.Text = m_checkDriversStatusTB.Text = m_cameraSnapTB.Text =
                m_initTB.Text = m_verticalTestTB.Text = m_unknownCountTB.Text = "Not Started";
            m_userName.Text = currentSession.User;
        }

        private void OnConfigureFraudSensor()
        {
            m_configureFraudSensor.Text = HasFraudSensor ? "Disable" : "Enable";
            m_fraudPOSTButton.Enabled = m_resetFraudButton.Enabled = HasFraudSensor;
            m_powerOnFraudSensor.Enabled = m_powerOffFraudSensor.Enabled = HasFraudSensor;
            m_startFraudScan.Enabled = m_stopFraudScan.Enabled = HasFraudSensor;
            m_testDiskForMarkers.Enabled = m_testFraudButton.Enabled = HasFraudSensor;
        }

        private void WriteToOutput(string msg)
        {
            ServiceLocator.Instance.GetService<ILogger>().Log(msg, LogEntryType.Info);
        }

        private void m_configureQubeButton_Click(object sender, EventArgs e)
        {
            var flag = !HasFraudSensor;
            var hardwareCommandResult = Service.ExecuteImmediate(
                string.Format("SETCFG \"EnableIceQubePolling\" \"{0}\" TYPE=CONTROLLER", flag.ToString()), out var _);
            if (!hardwareCommandResult.Success)
            {
                AirExchangerOutput.Write("Failed to update configuration.");
                foreach (object error in hardwareCommandResult.Errors)
                    AirExchangerOutput.Write(error.ToString());
            }
            else
            {
                HasFraudSensor = flag;
                AirExchangerOutput.Write("The air exchanger is {0}.",
                    HasFraudSensor ? "configured" : (object)"not configured");
                OnConfigureIceQubeBoard();
            }
        }

        private void m_configureAuxBoardButton_Click(object sender, EventArgs e)
        {
            m_iceQubeExchangerLabel.Text = "Power Relay board no longer supported.";
        }

        private void OnExecuteFraudSensorInstruction(object sender, EventArgs e)
        {
            ExecuteInstruction(sender, FraudSensorOutput, "FRAUDSENSOR");
        }

        private void OnExecuteIceQubeBoardCommand(object sender, EventArgs e)
        {
            ExecuteInstruction(sender, AirExchangerOutput, "AIRXCHGR");
        }

        private void ExecuteInstruction(object sender, OutputBox box, string mnemonic)
        {
            using (Manager.MakeAspect(sender))
            {
                var tagFromButton = GetTagFromButton(sender);
                if (string.IsNullOrEmpty(tagFromButton))
                {
                    box.Write("There is no operand associated with that button.");
                }
                else
                {
                    var job = CommonFunctions.ExecuteInstruction(Service,
                        string.Format("{0} {1}", mnemonic, tagFromButton));
                    if (job == null)
                        box.Write("Instruction execution failed.");
                    else
                        box.Write(job.GetTopOfStack());
                }
            }
        }

        private string GetTagFromButton(object sender)
        {
            return !(sender is Button button) || !(button.Tag is string tag) ? string.Empty : tag;
        }

        private void m_exitButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void m_installCortexButton_Click(object sender, EventArgs e)
        {
            ToggleCortex(sender, true);
        }

        private void m_uninstallCortexButton_Click(object sender, EventArgs e)
        {
            ToggleCortex(sender, false);
        }

        private void ToggleCortex(object sender, bool install)
        {
            using (Manager.MakeAspect(sender))
            {
                HardwareJob job;
                if (Service.ExecuteImmediate(
                        string.Format("SETCFG \"ScannerService\" \"{0}\" TYPE=CAMERA",
                            install ? "Cortex" : (object)"Legacy"), out job).Success)
                {
                    WriteToBox("Successfully changed camera configuration.");
                    PerfFunctions.Wait(2000);
                    if (install)
                    {
                        WriteToBox("Probing ports for device...");
                        job = CommonFunctions.ExecuteInstruction(Service, "CAMERA SCANFORPORT");
                        if (job != null)
                        {
                            var topOfStack = job.GetTopOfStack();
                            if (topOfStack == "NONE")
                                WriteToBox("Device probe failed; install unsuccessful.");
                            else if (Service.ExecuteImmediate(
                                         string.Format("SETCFG \"SnapDecodePort\" \"{0}\" TYPE=CAMERA", topOfStack),
                                         out job).Success)
                                WriteToBox(string.Format("The camera was successfully installed on port {0}",
                                    topOfStack));
                            else
                                WriteToBox("There was an error during installation.");
                        }
                    }
                    else
                    {
                        Service.ExecuteImmediate("SETCFG \"SnapDecodePort\" \"NONE\" TYPE=CAMERA", out job);
                    }

                    m_installCortexButton.Enabled = !install;
                    m_uninstallCortexButton.Enabled = m_startCortexButton.Enabled = install;
                }
                else
                {
                    WriteToBox("Unable to change camera configuration.");
                }
            }
        }

        private void WriteToBox(string msg)
        {
            OutputBox.Write(msg);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                WriteToBox("Waiting for camera to start - could take up to 15s.");
                WriteToBox(bool.Parse(CommonFunctions.ExecuteInstruction(Service, "CAMERA START").GetTopOfStack())
                    ? "Camera started."
                    : "Camera DIDN'T START.");
            }
        }

        private void RunPostTest()
        {
            using (var fraudSensorPost = new FraudSensorPost(Service))
            {
                fraudSensorPost.AddSink(ProcessPostEvent);
                fraudSensorPost.Run();
                foreach (var result in fraudSensorPost.Results)
                    FraudSensorOutput.Write("{0} : {1}", result.Code, result.Message);
            }

            BeginInvoke(new Action(() => m_fraudPOSTButton.Enabled = true));
        }

        private void ProcessPostEvent(HardwareJob job, DateTime time, string msg)
        {
            FraudSensorOutput.Write("[{0} {1}] {2}", time.ToShortDateString(), time.ToShortTimeString(), msg);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                var flag = !HasFraudSensor;
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(string.Format("SETCFG \"EnableSecureDiskValidator\" \"{0}\" TYPE=CONTROLLER",
                    flag.ToString()));
                stringBuilder.AppendLine(string.Format(" SETCFG \"EnableFraudSensorCheck\" \"{0}\" TYPE=CONTROLLER",
                    flag.ToString()));
                stringBuilder.AppendLine(string.Format(" SETCFG \"FraudScanJobBitmask2\" \"{0}\" TYPE=CONTROLLER",
                    flag ? 32 : 0));
                stringBuilder.AppendLine(" CLEAR");
                var hardwareCommandResult =
                    Service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
                if (!hardwareCommandResult.Success)
                {
                    FraudSensorOutput.Write("Failed to update configuration.");
                    foreach (object error in hardwareCommandResult.Errors)
                        FraudSensorOutput.Write(error.ToString());
                }
                else
                {
                    FraudSensorOutput.Write("Fraud sensor is {0}.", flag ? "enabled" : (object)"disabled");
                    HasFraudSensor = flag;
                    OnConfigureFraudSensor();
                }
            }
        }

        private void m_testFraudButton_Click(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                using (var fraudTestExecutor = new FraudTestExecutor(Service))
                {
                    fraudTestExecutor.Run();
                    foreach (var result in fraudTestExecutor.Results)
                        FraudSensorOutput.Write("Code: {0}, Message = {1}", result.Code, result.Message);
                }
            }
        }

        private void m_testUserDiskForFraud(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                using (var readReturnExecutor = new FraudTakeReadReturnExecutor(Service))
                {
                    readReturnExecutor.Run();
                    foreach (var result in readReturnExecutor.Results)
                        FraudSensorOutput.Write("Code: {0}, Message = {1}", result.Code, result.Message);
                }
            }
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                var integer1 = m_fraudDeckTB.GetInteger("Deck", FraudSensorOutput);
                var integer2 = m_fraudSlotTB.GetInteger("Slot", FraudSensorOutput);
                if (-1 == integer1 || -1 == integer2)
                {
                    FraudSensorOutput.Write("Check deck and slot!");
                }
                else
                {
                    CompositeFunctions.GetItem(sender, Manager, integer1, integer2, FraudSensorOutput, Service);
                    Service.ExecuteImmediateProgram(CenterItemProgram, out var _);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CompositeFunctions.VendDisk(sender, Service, Manager, FraudSensorOutput);
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            var integer1 = m_fraudDeckTB.GetInteger("Deck", FraudSensorOutput);
            var integer2 = m_fraudSlotTB.GetInteger("Slot", FraudSensorOutput);
            if (-1 == integer1 || -1 == integer2)
                FraudSensorOutput.Write("Check deck and slot!");
            else
                CompositeFunctions.PutItem(sender, Manager, Service, integer1, integer2, OutputBox);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                using (var takeDiskAtDoorJob = new TakeDiskAtDoorJob(Service))
                {
                    takeDiskAtDoorJob.Run();
                    foreach (var result in takeDiskAtDoorJob.Results)
                        OutputBox.Write("Code: {0} Message: {1}", result.Code, result.Message);
                }
            }
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            var locationNumberPad = new LocationNumberPad();
            if (locationNumberPad.ShowDialog() != DialogResult.OK)
                return;
            m_fraudDeckTB.Text = locationNumberPad.Number.ToString();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var locationNumberPad = new LocationNumberPad();
            if (locationNumberPad.ShowDialog() != DialogResult.OK)
                return;
            m_fraudSlotTB.Text = locationNumberPad.Number.ToString();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                GetInstructionFromButtonAndExecute(sender, FraudSensorOutput);
            }
        }

        private void GetInstructionFromButtonAndExecute(object sender, OutputBox box)
        {
            if (!(sender is Button button))
                return;
            var tag = button.Tag as string;
            if (string.IsNullOrEmpty(tag))
            {
                OutputBox.Write("Unable to parse instruction.");
            }
            else
            {
                var job = CommonFunctions.ExecuteInstruction(Service, tag, 120000);
                if (job != null)
                {
                    var topOfStack = job.GetTopOfStack();
                    box.Write(tag + " - " + topOfStack);
                }
                else
                {
                    box.Write("Command failed.");
                }
            }
        }

        private void RunUserReturn()
        {
            using (var returnExecutor = new ReturnExecutor(Service))
            {
                returnExecutor.AddSink(ProcessPostEvent);
                returnExecutor.Run();
                foreach (var result in returnExecutor.Results)
                    FraudSensorOutput.Write("{0} : {1}", result.Code, result.Message);
            }

            BeginInvoke(new Action(() => button7.Enabled = true));
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            FraudSensorOutput.Clear();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            AirExchangerOutput.Clear();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                GetInstructionFromButtonAndExecute(sender, FraudSensorOutput);
            }
        }

        private void RunStressTest()
        {
            var integer1 = m_scanPauseTB.GetInteger("Scan Pause", FraudSensorOutput);
            var integer2 = m_stressIterationsTB.GetInteger("iterations", FraudSensorOutput);
            var integer3 = m_readPauseTB.GetInteger("Iteration Pause", FraudSensorOutput);
            using (var stressTestExecutor = new FraudStressTestExecutor(Service, integer2, integer1, integer3))
            {
                stressTestExecutor.AddSink(ProcessPostEvent);
                stressTestExecutor.Run();
                foreach (var result in stressTestExecutor.Results)
                    FraudSensorOutput.Write("{0} : {1}", result.Code, result.Message);
            }

            BeginInvoke(new Action(() => button13.Enabled = true));
        }

        private void button14_Click(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                CommonFunctions.ExecuteInstruction(Service, "CONTROLSYSTEM CENTER");
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            var integer = m_scanPauseTB.GetInteger("Scan Pause", FraudSensorOutput);
            using (Manager.MakeAspect(sender))
            {
                using (var detectionTestExecutor = new FraudDetectionTestExecutor(Service, integer))
                {
                    detectionTestExecutor.Run();
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                m_checkDriversStatusTB.Text = FunctionTest.TestDrivers().ToString().ToUpper();
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                KioskTestOutput.Clear();
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                m_testVendDoorTB.Text = FunctionTest.TestVendDoor().ToString().ToUpper();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                m_initTB.Text = FunctionTest.RunInit().ToString().ToUpper();
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            var integer = m_sourceSlotTB.GetInteger("Test Slot", KioskTestOutput);
            if (-1 == integer)
                return;
            using (Manager.MakeAspect(sender))
            {
                m_verticalTestTB.Text = FunctionTest.RunVertialSlotTest(integer).ToString().ToUpper();
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                m_unknownCountTB.Text = FunctionTest.GetUnknownStats() ? "SUCCESS" : "FAILURE";
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            if (string.IsNullOrEmpty(m_userName.Text))
            {
                KioskTestOutput.Write("Please specify a user name.");
                errorProvider1.SetError(m_userName, "User name required.");
            }
            else
            {
                using (Manager.MakeAspect(sender))
                {
                    FunctionTest.SendData(m_userName.Text);
                }

                FunctionTest = null;
                OnConfigureKioskTest();
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            PopulateLocationBox(m_sourceSlotTB);
        }

        private void button20_Click(object sender, EventArgs e)
        {
            using (Manager.MakeAspect(sender))
            {
                var integer1 = m_sourceDeckTB.GetInteger("Sync deck", KioskTestOutput);
                var integer2 = m_sourceSlotTB.GetInteger("Sync slot", KioskTestOutput);
                if (-1 == integer1)
                    errorProvider1.SetError(m_sourceDeckTB, "Please enter valid deck.");
                else if (-1 == integer2)
                    errorProvider1.SetError(m_sourceSlotTB, "Please enter valid slot.");
                else
                    m_cameraSnapTB.Text = FunctionTest.TestCameraSnap(integer1, integer2).ToString().ToUpper();
            }
        }

        private void button21_Click(object sender, EventArgs e)
        {
            PopulateLocationBox(m_sourceDeckTB);
        }

        private void button22_Click(object sender, EventArgs e)
        {
            PopulateLocationBox(m_sourceSlotTB);
        }

        private void PopulateLocationBox(TextBox box)
        {
            var locationNumberPad = new LocationNumberPad();
            if (locationNumberPad.ShowDialog() != DialogResult.OK)
                return;
            box.Text = locationNumberPad.Number.ToString();
        }

        private void button16_Click_1(object sender, EventArgs e)
        {
            IList<IKioskFunctionCheckData> sessions;
            if (!Service.GetKioskFunctionCheckData(out sessions).Success)
                return;
            var num = 1;
            foreach (var session in sessions)
                WriteSessionData(session, num++);
        }

        private void WriteSessionData(IKioskFunctionCheckData session, int sessionCount)
        {
            KioskTestOutput.Write("   Vertical result = {0}", session.VerticalSlotTestResult);
            KioskTestOutput.Write("   Init result = {0}", session.InitTestResult);
            KioskTestOutput.Write("   Venddoor result = {0}", session.VendDoorTestResult);
            KioskTestOutput.Write("   Track result = {0}", session.TrackTestResult);
            KioskTestOutput.Write("   Snap and decode result = {0}", session.SnapDecodeTestResult);
            KioskTestOutput.Write("   Touchscreen driver result = {0}", session.TouchscreenDriverTestResult);
            KioskTestOutput.Write("   Camera driver result = {0}", session.CameraDriverTestResult);
            KioskTestOutput.Write("   Timestamp {0}", session.Timestamp.ToString());
            KioskTestOutput.Write("   User identifier {0}", session.UserIdentifier);
            KioskTestOutput.Write("Session {0}", sessionCount);
        }

        private void button11_Click(object sender, EventArgs e1)
        {
            using (Manager.MakeAspect(sender))
            {
                using (var hardwareSurveyExecutor = new HardwareSurveyExecutor(Service))
                {
                    using (var executionTimer = new ExecutionTimer())
                    {
                        hardwareSurveyExecutor.Run();
                        AddItem("Timestamp", hardwareSurveyExecutor.Timestamp.ToString());
                        AddItem("Camera", hardwareSurveyExecutor.CameraVersion);
                        var col2 = hardwareSurveyExecutor.TouchscreenFirmware;
                        try
                        {
                            var version = new Version(hardwareSurveyExecutor.TouchscreenFirmware);
                            var flag = version.Major == 4 && version.Minor == 30;
                            col2 = string.Format("{0} ( {1} CURRENT )", hardwareSurveyExecutor.TouchscreenFirmware,
                                flag ? "MOST" : (object)"NOT MOST");
                        }
                        catch (Exception ex)
                        {
                        }

                        AddItem("Touchscreen firmware", col2);
                        AddItem("Touchscreen model", hardwareSurveyExecutor.Touchscreen);
                        AddItem("ABE device", hardwareSurveyExecutor.ABEDevice.ToString());
                        AddItem("Fraud sensor", hardwareSurveyExecutor.FraudDevice.ToString());
                        AddItem("AuxRelay board", hardwareSurveyExecutor.HasAuxRelayBoard ? "Yes" : "No");
                        AddItem("Air exchanger", hardwareSurveyExecutor.AirExchanger.ToString());
                        AddItem("Quick return", hardwareSurveyExecutor.QuickReturn.ToString());
                        AddItem("Disk free space", string.Format("{0} GB", hardwareSurveyExecutor.FreeDiskSpace >> 30));
                        AddItem("Memory", string.Format("{0} GB", hardwareSurveyExecutor.Memory >> 20));
                        AddItem("PC Model", hardwareSurveyExecutor.PcModel);
                        AddItem("PC Manufacturer", hardwareSurveyExecutor.PcManufacturer);
                        AddItem("UPS", hardwareSurveyExecutor.UpsModel);
                        AddItem("Monitor Model", hardwareSurveyExecutor.Monitor);
                        AddItem("Serial controller version", hardwareSurveyExecutor.SerialControllerVersion);
                        executionTimer.Stop();
                        LogHelper.Instance.Log("[{0}] Hardware survey execution time = {1}", GetType().Name,
                            executionTimer.Elapsed);
                    }
                }
            }
        }

        private void AddItem(string col1, string col2)
        {
            listView1.Items.Add(new ListViewItem(new string[2]
            {
                col1,
                col2
            }));
        }

        private void button16_Click_2(object sender, EventArgs e)
        {
            listView1.Items.Clear();
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
            m_devicesTab = new TabControl();
            m_quickReturnTab = new TabPage();
            m_airExchangerTab = new TabPage();
            button10 = new Button();
            m_iceQubeOutput = new ListBox();
            m_resetPersistentCounter = new Button();
            m_qubeStatusButton = new Button();
            m_resetQubeBoard = new Button();
            m_configureQubeButton = new Button();
            m_iceQubeExchangerLabel = new Label();
            m_cortexTab = new TabPage();
            m_startCortexButton = new Button();
            m_cortexStatusBox = new ListBox();
            label3 = new Label();
            m_uninstallCortexButton = new Button();
            m_installCortexButton = new Button();
            m_fraudSensorTab = new TabPage();
            button15 = new Button();
            button14 = new Button();
            label5 = new Label();
            m_readPauseTB = new TextBox();
            m_scanPauseTB = new TextBox();
            label4 = new Label();
            label2 = new Label();
            m_stressIterationsTB = new TextBox();
            button13 = new Button();
            button3 = new Button();
            button7 = new Button();
            button6 = new Button();
            button5 = new Button();
            button4 = new Button();
            m_fraudSlotTB = new TextBox();
            m_fraudDeckTB = new TextBox();
            groupBox1 = new GroupBox();
            m_powerOffFraudSensor = new Button();
            m_powerOnFraudSensor = new Button();
            m_testDiskForMarkers = new Button();
            m_stopFraudScan = new Button();
            m_startFraudScan = new Button();
            m_resetFraudButton = new Button();
            m_testFraudButton = new Button();
            m_fraudPOSTButton = new Button();
            button1 = new Button();
            m_configureFraudSensor = new Button();
            m_fraudSensorOutput = new ListBox();
            m_kioskFunctionCheck = new TabPage();
            m_displaySessionsButton = new Button();
            m_sourceSlotTB = new TextBox();
            button19 = new Button();
            m_sourceDeckTB = new TextBox();
            button22 = new Button();
            groupBox2 = new GroupBox();
            m_unknownCountTB = new TextBox();
            m_cameraSnapTB = new TextBox();
            button20 = new Button();
            m_verticalTestTB = new TextBox();
            m_initTB = new TextBox();
            m_testVendDoorTB = new TextBox();
            m_checkDriversStatusTB = new TextBox();
            button18 = new Button();
            button17 = new Button();
            button12 = new Button();
            button9 = new Button();
            button8 = new Button();
            m_userName = new TextBox();
            button21 = new Button();
            label1 = new Label();
            button2 = new Button();
            m_kioskTestOutput = new ListBox();
            tabPage1 = new TabPage();
            button16 = new Button();
            button11 = new Button();
            m_exitButton = new Button();
            errorProvider1 = new ErrorProvider(components);
            listView1 = new ListView();
            m_devicesTab.SuspendLayout();
            m_airExchangerTab.SuspendLayout();
            m_cortexTab.SuspendLayout();
            m_fraudSensorTab.SuspendLayout();
            groupBox1.SuspendLayout();
            m_kioskFunctionCheck.SuspendLayout();
            groupBox2.SuspendLayout();
            tabPage1.SuspendLayout();
            ((ISupportInitialize)errorProvider1).BeginInit();
            SuspendLayout();
            m_devicesTab.Controls.Add(m_quickReturnTab);
            m_devicesTab.Controls.Add(m_airExchangerTab);
            m_devicesTab.Controls.Add(m_cortexTab);
            m_devicesTab.Controls.Add(m_fraudSensorTab);
            m_devicesTab.Controls.Add(m_kioskFunctionCheck);
            m_devicesTab.Controls.Add(tabPage1);
            m_devicesTab.Location = new Point(2, 12);
            m_devicesTab.Name = "m_devicesTab";
            m_devicesTab.Padding = new Point(10, 20);
            m_devicesTab.SelectedIndex = 0;
            m_devicesTab.Size = new Size(951, 626);
            m_devicesTab.TabIndex = 0;
            m_quickReturnTab.BackColor = Color.LightGray;
            m_quickReturnTab.Location = new Point(4, 56);
            m_quickReturnTab.Name = "m_quickReturnTab";
            m_quickReturnTab.Padding = new Padding(3);
            m_quickReturnTab.Size = new Size(943, 566);
            m_quickReturnTab.TabIndex = 0;
            m_quickReturnTab.Text = "Quick Return Device";
            m_airExchangerTab.BackColor = Color.LightGray;
            m_airExchangerTab.Controls.Add(button10);
            m_airExchangerTab.Controls.Add(m_iceQubeOutput);
            m_airExchangerTab.Controls.Add(m_resetPersistentCounter);
            m_airExchangerTab.Controls.Add(m_qubeStatusButton);
            m_airExchangerTab.Controls.Add(m_resetQubeBoard);
            m_airExchangerTab.Controls.Add(m_configureQubeButton);
            m_airExchangerTab.Controls.Add(m_iceQubeExchangerLabel);
            m_airExchangerTab.Location = new Point(4, 56);
            m_airExchangerTab.Name = "m_airExchangerTab";
            m_airExchangerTab.Padding = new Padding(3);
            m_airExchangerTab.Size = new Size(943, 566);
            m_airExchangerTab.TabIndex = 3;
            m_airExchangerTab.Text = "Ice Qube Air Exchanger";
            button10.Location = new Point(26, 473);
            button10.Name = "button10";
            button10.Size = new Size(120, 65);
            button10.TabIndex = 8;
            button10.Text = "Clear output";
            button10.UseVisualStyleBackColor = true;
            button10.Click += button10_Click;
            m_iceQubeOutput.FormattingEnabled = true;
            m_iceQubeOutput.Location = new Point(26, 164);
            m_iceQubeOutput.Name = "m_iceQubeOutput";
            m_iceQubeOutput.Size = new Size(436, 290);
            m_iceQubeOutput.TabIndex = 7;
            m_resetPersistentCounter.Location = new Point(528, 73);
            m_resetPersistentCounter.Name = "m_resetPersistentCounter";
            m_resetPersistentCounter.Size = new Size(120, 65);
            m_resetPersistentCounter.TabIndex = 6;
            m_resetPersistentCounter.Tag = "RESETFAILURECOUNTER";
            m_resetPersistentCounter.Text = "Clear Error Counter";
            m_resetPersistentCounter.UseVisualStyleBackColor = true;
            m_resetPersistentCounter.Click += OnExecuteIceQubeBoardCommand;
            m_qubeStatusButton.Location = new Point(188, 73);
            m_qubeStatusButton.Name = "m_qubeStatusButton";
            m_qubeStatusButton.Size = new Size(120, 65);
            m_qubeStatusButton.TabIndex = 3;
            m_qubeStatusButton.Tag = "BOARDSTATUS";
            m_qubeStatusButton.Text = "Exchanger Board Status";
            m_qubeStatusButton.UseVisualStyleBackColor = true;
            m_qubeStatusButton.Click += OnExecuteIceQubeBoardCommand;
            m_resetQubeBoard.Location = new Point(363, 73);
            m_resetQubeBoard.Name = "m_resetQubeBoard";
            m_resetQubeBoard.Size = new Size(120, 65);
            m_resetQubeBoard.TabIndex = 2;
            m_resetQubeBoard.Tag = "RESET";
            m_resetQubeBoard.Text = "Reset Board";
            m_resetQubeBoard.UseVisualStyleBackColor = true;
            m_resetQubeBoard.Click += OnExecuteIceQubeBoardCommand;
            m_configureQubeButton.Location = new Point(26, 73);
            m_configureQubeButton.Name = "m_configureQubeButton";
            m_configureQubeButton.Size = new Size(120, 65);
            m_configureQubeButton.TabIndex = 1;
            m_configureQubeButton.Text = "button1";
            m_configureQubeButton.UseVisualStyleBackColor = true;
            m_configureQubeButton.Click += m_configureQubeButton_Click;
            m_iceQubeExchangerLabel.AutoSize = true;
            m_iceQubeExchangerLabel.Location = new Point(23, 27);
            m_iceQubeExchangerLabel.Name = "m_iceQubeExchangerLabel";
            m_iceQubeExchangerLabel.Size = new Size(35, 13);
            m_iceQubeExchangerLabel.TabIndex = 0;
            m_iceQubeExchangerLabel.Text = "label1";
            m_cortexTab.BackColor = Color.LightGray;
            m_cortexTab.Controls.Add(m_startCortexButton);
            m_cortexTab.Controls.Add(m_cortexStatusBox);
            m_cortexTab.Controls.Add(label3);
            m_cortexTab.Controls.Add(m_uninstallCortexButton);
            m_cortexTab.Controls.Add(m_installCortexButton);
            m_cortexTab.Location = new Point(4, 56);
            m_cortexTab.Name = "m_cortexTab";
            m_cortexTab.Padding = new Padding(3);
            m_cortexTab.Size = new Size(943, 566);
            m_cortexTab.TabIndex = 4;
            m_cortexTab.Text = "Cortex Camera";
            m_startCortexButton.Enabled = false;
            m_startCortexButton.Location = new Point(375, 51);
            m_startCortexButton.Name = "m_startCortexButton";
            m_startCortexButton.Size = new Size(150, 75);
            m_startCortexButton.TabIndex = 4;
            m_startCortexButton.Text = "Start Camera";
            m_startCortexButton.UseVisualStyleBackColor = true;
            m_startCortexButton.Click += button1_Click;
            m_cortexStatusBox.FormattingEnabled = true;
            m_cortexStatusBox.Location = new Point(72, 161);
            m_cortexStatusBox.Name = "m_cortexStatusBox";
            m_cortexStatusBox.Size = new Size(453, 290);
            m_cortexStatusBox.TabIndex = 3;
            label3.AutoSize = true;
            label3.Location = new Point(16, 161);
            label3.Name = "label3";
            label3.Size = new Size(40, 13);
            label3.TabIndex = 2;
            label3.Text = "Status:";
            m_uninstallCortexButton.Location = new Point(189, 51);
            m_uninstallCortexButton.Name = "m_uninstallCortexButton";
            m_uninstallCortexButton.Size = new Size(150, 75);
            m_uninstallCortexButton.TabIndex = 1;
            m_uninstallCortexButton.Text = "Uninstall";
            m_uninstallCortexButton.UseVisualStyleBackColor = true;
            m_uninstallCortexButton.Click += m_uninstallCortexButton_Click;
            m_installCortexButton.Location = new Point(19, 51);
            m_installCortexButton.Name = "m_installCortexButton";
            m_installCortexButton.Size = new Size(150, 75);
            m_installCortexButton.TabIndex = 0;
            m_installCortexButton.Text = "Install";
            m_installCortexButton.UseVisualStyleBackColor = true;
            m_installCortexButton.Click += m_installCortexButton_Click;
            m_fraudSensorTab.BackColor = Color.LightGray;
            m_fraudSensorTab.Controls.Add(button15);
            m_fraudSensorTab.Controls.Add(button14);
            m_fraudSensorTab.Controls.Add(label5);
            m_fraudSensorTab.Controls.Add(m_readPauseTB);
            m_fraudSensorTab.Controls.Add(m_scanPauseTB);
            m_fraudSensorTab.Controls.Add(label4);
            m_fraudSensorTab.Controls.Add(label2);
            m_fraudSensorTab.Controls.Add(m_stressIterationsTB);
            m_fraudSensorTab.Controls.Add(button13);
            m_fraudSensorTab.Controls.Add(button3);
            m_fraudSensorTab.Controls.Add(button7);
            m_fraudSensorTab.Controls.Add(button6);
            m_fraudSensorTab.Controls.Add(button5);
            m_fraudSensorTab.Controls.Add(button4);
            m_fraudSensorTab.Controls.Add(m_fraudSlotTB);
            m_fraudSensorTab.Controls.Add(m_fraudDeckTB);
            m_fraudSensorTab.Controls.Add(groupBox1);
            m_fraudSensorTab.Controls.Add(button1);
            m_fraudSensorTab.Controls.Add(m_configureFraudSensor);
            m_fraudSensorTab.Controls.Add(m_fraudSensorOutput);
            m_fraudSensorTab.Location = new Point(4, 56);
            m_fraudSensorTab.Name = "m_fraudSensorTab";
            m_fraudSensorTab.Padding = new Padding(3);
            m_fraudSensorTab.Size = new Size(943, 566);
            m_fraudSensorTab.TabIndex = 5;
            m_fraudSensorTab.Text = "Fraud Sensor";
            button15.Enabled = false;
            button15.Location = new Point(670, 435);
            button15.Name = "button15";
            button15.Size = new Size(99, 45);
            button15.TabIndex = 26;
            button15.Text = "Run Detection Test";
            button15.UseVisualStyleBackColor = true;
            button15.Visible = false;
            button15.Click += button15_Click;
            button14.Location = new Point(510, 428);
            button14.Name = "button14";
            button14.Size = new Size(120, 52);
            button14.TabIndex = 25;
            button14.Text = "Center Disk";
            button14.UseVisualStyleBackColor = true;
            button14.Click += button14_Click;
            label5.AutoSize = true;
            label5.Enabled = false;
            label5.Location = new Point(801, 516);
            label5.Name = "label5";
            label5.Size = new Size(110, 13);
            label5.TabIndex = 24;
            label5.Text = "Pause between reads";
            label5.Visible = false;
            m_readPauseTB.Enabled = false;
            m_readPauseTB.Location = new Point(798, 532);
            m_readPauseTB.Name = "m_readPauseTB";
            m_readPauseTB.Size = new Size(100, 20);
            m_readPauseTB.TabIndex = 23;
            m_readPauseTB.Visible = false;
            m_scanPauseTB.Enabled = false;
            m_scanPauseTB.Location = new Point(798, 493);
            m_scanPauseTB.Name = "m_scanPauseTB";
            m_scanPauseTB.Size = new Size(100, 20);
            m_scanPauseTB.TabIndex = 22;
            m_scanPauseTB.Visible = false;
            label4.AutoSize = true;
            label4.Enabled = false;
            label4.Location = new Point(801, 477);
            label4.Name = "label4";
            label4.Size = new Size(65, 13);
            label4.TabIndex = 21;
            label4.Text = "Scan Pause";
            label4.Visible = false;
            label2.AutoSize = true;
            label2.Enabled = false;
            label2.Location = new Point(801, 435);
            label2.Name = "label2";
            label2.Size = new Size(76, 13);
            label2.TabIndex = 20;
            label2.Text = "Test iterations:";
            label2.Visible = false;
            m_stressIterationsTB.Enabled = false;
            m_stressIterationsTB.Location = new Point(798, 454);
            m_stressIterationsTB.Name = "m_stressIterationsTB";
            m_stressIterationsTB.Size = new Size(100, 20);
            m_stressIterationsTB.TabIndex = 19;
            m_stressIterationsTB.Visible = false;
            button13.Location = new Point(670, 495);
            button13.Name = "button13";
            button13.Size = new Size(99, 65);
            button13.TabIndex = 18;
            button13.Tag = "SERIALBOARD RESET";
            button13.Text = "Initialize Control System";
            button13.UseVisualStyleBackColor = true;
            button13.Click += button13_Click;
            button3.Location = new Point(6, 454);
            button3.Name = "button3";
            button3.Size = new Size(120, 65);
            button3.TabIndex = 17;
            button3.Text = "Clear Output";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click_1;
            button7.Location = new Point(510, 493);
            button7.Name = "button7";
            button7.Size = new Size(120, 65);
            button7.TabIndex = 16;
            button7.Tag = "SERIALBOARD CLOSEPORT";
            button7.Text = "Shutdown Control System";
            button7.UseVisualStyleBackColor = true;
            button7.Click += button7_Click;
            button6.Location = new Point(670, 372);
            button6.Name = "button6";
            button6.Size = new Size(75, 50);
            button6.TabIndex = 15;
            button6.Text = "Slot";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button6_Click;
            button5.Location = new Point(670, 316);
            button5.Name = "button5";
            button5.Size = new Size(75, 50);
            button5.TabIndex = 14;
            button5.Text = "Deck";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click_1;
            button4.Location = new Point(510, 372);
            button4.Name = "button4";
            button4.Size = new Size(120, 50);
            button4.TabIndex = 13;
            button4.Text = "Put";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click_1;
            m_fraudSlotTB.Location = new Point(751, 402);
            m_fraudSlotTB.Name = "m_fraudSlotTB";
            m_fraudSlotTB.Size = new Size(100, 20);
            m_fraudSlotTB.TabIndex = 12;
            m_fraudDeckTB.Location = new Point(751, 346);
            m_fraudDeckTB.Name = "m_fraudDeckTB";
            m_fraudDeckTB.Size = new Size(100, 20);
            m_fraudDeckTB.TabIndex = 8;
            groupBox1.Controls.Add(m_powerOffFraudSensor);
            groupBox1.Controls.Add(m_powerOnFraudSensor);
            groupBox1.Controls.Add(m_testDiskForMarkers);
            groupBox1.Controls.Add(m_stopFraudScan);
            groupBox1.Controls.Add(m_startFraudScan);
            groupBox1.Controls.Add(m_resetFraudButton);
            groupBox1.Controls.Add(m_testFraudButton);
            groupBox1.Controls.Add(m_fraudPOSTButton);
            groupBox1.Location = new Point(487, 6);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(390, 297);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Sensor Tests";
            m_powerOffFraudSensor.Location = new Point(222, 85);
            m_powerOffFraudSensor.Name = "m_powerOffFraudSensor";
            m_powerOffFraudSensor.Size = new Size(150, 60);
            m_powerOffFraudSensor.TabIndex = 10;
            m_powerOffFraudSensor.Tag = "SHUTDOWN";
            m_powerOffFraudSensor.Text = "Turn Off Power";
            m_powerOffFraudSensor.UseVisualStyleBackColor = true;
            m_powerOffFraudSensor.Click += OnExecuteFraudSensorInstruction;
            m_powerOnFraudSensor.Location = new Point(23, 85);
            m_powerOnFraudSensor.Name = "m_powerOnFraudSensor";
            m_powerOnFraudSensor.Size = new Size(150, 60);
            m_powerOnFraudSensor.TabIndex = 9;
            m_powerOnFraudSensor.Tag = "INITIALIZE";
            m_powerOnFraudSensor.Text = "Power Sensor";
            m_powerOnFraudSensor.UseVisualStyleBackColor = true;
            m_powerOnFraudSensor.Click += OnExecuteFraudSensorInstruction;
            m_testDiskForMarkers.Location = new Point(23, 218);
            m_testDiskForMarkers.Name = "m_testDiskForMarkers";
            m_testDiskForMarkers.Size = new Size(150, 60);
            m_testDiskForMarkers.TabIndex = 8;
            m_testDiskForMarkers.Text = "Test Disk In Picker";
            m_testDiskForMarkers.UseVisualStyleBackColor = true;
            m_testDiskForMarkers.Click += m_testFraudButton_Click;
            m_stopFraudScan.Location = new Point(222, 152);
            m_stopFraudScan.Name = "m_stopFraudScan";
            m_stopFraudScan.Size = new Size(150, 60);
            m_stopFraudScan.TabIndex = 7;
            m_stopFraudScan.Tag = "STOPSCAN";
            m_stopFraudScan.Text = "Stop Scan";
            m_stopFraudScan.UseVisualStyleBackColor = true;
            m_stopFraudScan.Click += OnExecuteFraudSensorInstruction;
            m_startFraudScan.Location = new Point(23, 152);
            m_startFraudScan.Name = "m_startFraudScan";
            m_startFraudScan.Size = new Size(150, 60);
            m_startFraudScan.TabIndex = 6;
            m_startFraudScan.Tag = "STARTSCAN";
            m_startFraudScan.Text = "Start Scan";
            m_startFraudScan.UseVisualStyleBackColor = true;
            m_startFraudScan.Click += OnExecuteFraudSensorInstruction;
            m_resetFraudButton.Location = new Point(222, 19);
            m_resetFraudButton.Name = "m_resetFraudButton";
            m_resetFraudButton.Size = new Size(150, 60);
            m_resetFraudButton.TabIndex = 5;
            m_resetFraudButton.Tag = "RESET";
            m_resetFraudButton.Text = "Reset";
            m_resetFraudButton.UseVisualStyleBackColor = true;
            m_resetFraudButton.Click += OnExecuteFraudSensorInstruction;
            m_testFraudButton.Location = new Point(222, 218);
            m_testFraudButton.Name = "m_testFraudButton";
            m_testFraudButton.Size = new Size(150, 60);
            m_testFraudButton.TabIndex = 3;
            m_testFraudButton.Text = "Take - Test - Return";
            m_testFraudButton.UseVisualStyleBackColor = true;
            m_testFraudButton.Click += m_testUserDiskForFraud;
            m_fraudPOSTButton.Location = new Point(23, 19);
            m_fraudPOSTButton.Name = "m_fraudPOSTButton";
            m_fraudPOSTButton.Size = new Size(150, 60);
            m_fraudPOSTButton.TabIndex = 4;
            m_fraudPOSTButton.Tag = "LASTPOSTRESULT";
            m_fraudPOSTButton.Text = "Last POST Result";
            m_fraudPOSTButton.UseVisualStyleBackColor = true;
            m_fraudPOSTButton.Click += OnExecuteFraudSensorInstruction;
            button1.Location = new Point(510, 309);
            button1.Name = "button1";
            button1.Size = new Size(120, 57);
            button1.TabIndex = 5;
            button1.Text = "Get";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_2;
            m_configureFraudSensor.Location = new Point(6, 62);
            m_configureFraudSensor.Name = "m_configureFraudSensor";
            m_configureFraudSensor.Size = new Size(150, 75);
            m_configureFraudSensor.TabIndex = 1;
            m_configureFraudSensor.Text = "Configure";
            m_configureFraudSensor.UseVisualStyleBackColor = true;
            m_configureFraudSensor.Click += button1_Click_1;
            m_fraudSensorOutput.FormattingEnabled = true;
            m_fraudSensorOutput.HorizontalScrollbar = true;
            m_fraudSensorOutput.Location = new Point(6, 158);
            m_fraudSensorOutput.Name = "m_fraudSensorOutput";
            m_fraudSensorOutput.ScrollAlwaysVisible = true;
            m_fraudSensorOutput.Size = new Size(450, 290);
            m_fraudSensorOutput.TabIndex = 0;
            m_kioskFunctionCheck.BackColor = Color.LightGray;
            m_kioskFunctionCheck.Controls.Add(m_displaySessionsButton);
            m_kioskFunctionCheck.Controls.Add(m_sourceSlotTB);
            m_kioskFunctionCheck.Controls.Add(button19);
            m_kioskFunctionCheck.Controls.Add(m_sourceDeckTB);
            m_kioskFunctionCheck.Controls.Add(button22);
            m_kioskFunctionCheck.Controls.Add(groupBox2);
            m_kioskFunctionCheck.Controls.Add(m_userName);
            m_kioskFunctionCheck.Controls.Add(button21);
            m_kioskFunctionCheck.Controls.Add(label1);
            m_kioskFunctionCheck.Controls.Add(button2);
            m_kioskFunctionCheck.Controls.Add(m_kioskTestOutput);
            m_kioskFunctionCheck.Location = new Point(4, 56);
            m_kioskFunctionCheck.Name = "m_kioskFunctionCheck";
            m_kioskFunctionCheck.Padding = new Padding(3);
            m_kioskFunctionCheck.Size = new Size(943, 566);
            m_kioskFunctionCheck.TabIndex = 6;
            m_kioskFunctionCheck.Text = "Kiosk Function Check";
            m_displaySessionsButton.Location = new Point(680, 501);
            m_displaySessionsButton.Name = "m_displaySessionsButton";
            m_displaySessionsButton.Size = new Size(104, 54);
            m_displaySessionsButton.TabIndex = 22;
            m_displaySessionsButton.Text = "Show Sessions";
            m_displaySessionsButton.UseVisualStyleBackColor = true;
            m_displaySessionsButton.Click += button16_Click_1;
            m_sourceSlotTB.Location = new Point(277, 83);
            m_sourceSlotTB.Name = "m_sourceSlotTB";
            m_sourceSlotTB.Size = new Size(75, 20);
            m_sourceSlotTB.TabIndex = 11;
            button19.Location = new Point(801, 501);
            button19.Name = "button19";
            button19.Size = new Size(100, 55);
            button19.TabIndex = 13;
            button19.Text = "Send Report";
            button19.UseVisualStyleBackColor = true;
            button19.Click += button19_Click;
            m_sourceDeckTB.Location = new Point(90, 83);
            m_sourceDeckTB.Name = "m_sourceDeckTB";
            m_sourceDeckTB.Size = new Size(68, 20);
            m_sourceDeckTB.TabIndex = 9;
            button22.Location = new Point(196, 65);
            button22.Name = "button22";
            button22.Size = new Size(75, 38);
            button22.TabIndex = 10;
            button22.Text = "Slot";
            button22.UseVisualStyleBackColor = true;
            button22.Click += button22_Click;
            groupBox2.Controls.Add(m_unknownCountTB);
            groupBox2.Controls.Add(m_cameraSnapTB);
            groupBox2.Controls.Add(button20);
            groupBox2.Controls.Add(m_verticalTestTB);
            groupBox2.Controls.Add(m_initTB);
            groupBox2.Controls.Add(m_testVendDoorTB);
            groupBox2.Controls.Add(m_checkDriversStatusTB);
            groupBox2.Controls.Add(button18);
            groupBox2.Controls.Add(button17);
            groupBox2.Controls.Add(button12);
            groupBox2.Controls.Add(button9);
            groupBox2.Controls.Add(button8);
            groupBox2.Location = new Point(9, 118);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(325, 437);
            groupBox2.TabIndex = 4;
            groupBox2.TabStop = false;
            groupBox2.Text = "Steps:";
            m_unknownCountTB.Location = new Point(133, 411);
            m_unknownCountTB.Name = "m_unknownCountTB";
            m_unknownCountTB.ReadOnly = true;
            m_unknownCountTB.Size = new Size(129, 20);
            m_unknownCountTB.TabIndex = 17;
            m_cameraSnapTB.Location = new Point(133, 129);
            m_cameraSnapTB.Name = "m_cameraSnapTB";
            m_cameraSnapTB.ReadOnly = true;
            m_cameraSnapTB.Size = new Size(129, 20);
            m_cameraSnapTB.TabIndex = 16;
            button20.Location = new Point(12, 89);
            button20.Name = "button20";
            button20.Size = new Size(100, 60);
            button20.TabIndex = 2;
            button20.Text = "Decode Test";
            button20.UseVisualStyleBackColor = true;
            button20.Click += button20_Click;
            m_verticalTestTB.Location = new Point(133, 338);
            m_verticalTestTB.Name = "m_verticalTestTB";
            m_verticalTestTB.ReadOnly = true;
            m_verticalTestTB.Size = new Size(129, 20);
            m_verticalTestTB.TabIndex = 13;
            m_initTB.Location = new Point(133, 272);
            m_initTB.Name = "m_initTB";
            m_initTB.ReadOnly = true;
            m_initTB.Size = new Size(129, 20);
            m_initTB.TabIndex = 12;
            m_testVendDoorTB.Location = new Point(133, 195);
            m_testVendDoorTB.Name = "m_testVendDoorTB";
            m_testVendDoorTB.ReadOnly = true;
            m_testVendDoorTB.Size = new Size(129, 20);
            m_testVendDoorTB.TabIndex = 11;
            m_checkDriversStatusTB.Location = new Point(133, 59);
            m_checkDriversStatusTB.Name = "m_checkDriversStatusTB";
            m_checkDriversStatusTB.ReadOnly = true;
            m_checkDriversStatusTB.Size = new Size(129, 20);
            m_checkDriversStatusTB.TabIndex = 9;
            button18.Location = new Point(12, 371);
            button18.Name = "button18";
            button18.Size = new Size(100, 60);
            button18.TabIndex = 7;
            button18.Text = "Unknown Count";
            button18.UseVisualStyleBackColor = true;
            button18.Click += button18_Click;
            button17.Location = new Point(12, 298);
            button17.Name = "button17";
            button17.Size = new Size(100, 60);
            button17.TabIndex = 6;
            button17.Text = "Vertical Slot Test";
            button17.UseVisualStyleBackColor = true;
            button17.Click += button17_Click;
            button12.Location = new Point(12, 155);
            button12.Name = "button12";
            button12.Size = new Size(100, 60);
            button12.TabIndex = 4;
            button12.Text = "Test Vend Door";
            button12.UseVisualStyleBackColor = true;
            button12.Click += button12_Click;
            button9.Location = new Point(12, 232);
            button9.Name = "button9";
            button9.Size = new Size(100, 60);
            button9.TabIndex = 5;
            button9.Text = "Init";
            button9.UseVisualStyleBackColor = true;
            button9.Click += button9_Click;
            button8.Location = new Point(12, 19);
            button8.Name = "button8";
            button8.Size = new Size(100, 60);
            button8.TabIndex = 0;
            button8.Text = "Check Drivers";
            button8.UseVisualStyleBackColor = true;
            button8.Click += button8_Click;
            m_userName.Location = new Point(6, 31);
            m_userName.Name = "m_userName";
            m_userName.ReadOnly = true;
            m_userName.Size = new Size(228, 20);
            m_userName.TabIndex = 3;
            button21.Location = new Point(9, 65);
            button21.Name = "button21";
            button21.Size = new Size(75, 38);
            button21.TabIndex = 8;
            button21.Text = "Deck";
            button21.UseVisualStyleBackColor = true;
            button21.Click += button21_Click;
            label1.AutoSize = true;
            label1.Location = new Point(6, 15);
            label1.Name = "label1";
            label1.Size = new Size(55, 13);
            label1.TabIndex = 2;
            label1.Text = "Username";
            button2.Location = new Point(460, 501);
            button2.Name = "button2";
            button2.Size = new Size(100, 55);
            button2.TabIndex = 12;
            button2.Text = "Clear Output";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click_1;
            m_kioskTestOutput.FormattingEnabled = true;
            m_kioskTestOutput.Location = new Point(460, 6);
            m_kioskTestOutput.Name = "m_kioskTestOutput";
            m_kioskTestOutput.Size = new Size(441, 485);
            m_kioskTestOutput.TabIndex = 0;
            tabPage1.BackColor = Color.LightGray;
            tabPage1.Controls.Add(listView1);
            tabPage1.Controls.Add(button16);
            tabPage1.Controls.Add(button11);
            tabPage1.Location = new Point(4, 56);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(943, 566);
            tabPage1.TabIndex = 7;
            tabPage1.Text = "Hardware Survey";
            button16.Location = new Point(656, 194);
            button16.Name = "button16";
            button16.Size = new Size(140, 80);
            button16.TabIndex = 2;
            button16.Text = "Clear";
            button16.UseVisualStyleBackColor = true;
            button16.Click += button16_Click_2;
            button11.Location = new Point(656, 74);
            button11.Name = "button11";
            button11.Size = new Size(140, 80);
            button11.TabIndex = 1;
            button11.Text = "Run Survey";
            button11.UseVisualStyleBackColor = true;
            button11.Click += button11_Click;
            m_exitButton.BackColor = Color.GreenYellow;
            m_exitButton.Location = new Point(12, 644);
            m_exitButton.Name = "m_exitButton";
            m_exitButton.Size = new Size(135, 59);
            m_exitButton.TabIndex = 14;
            m_exitButton.Text = "Exit";
            m_exitButton.UseVisualStyleBackColor = false;
            m_exitButton.Click += m_exitButton_Click;
            errorProvider1.ContainerControl = this;
            listView1.Location = new Point(20, 35);
            listView1.Name = "listView1";
            listView1.Size = new Size(502, 378);
            listView1.TabIndex = 3;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(975, 715);
            Controls.Add(m_exitButton);
            Controls.Add(m_devicesTab);
            MaximizeBox = false;
            Name = nameof(ConfiguredDevicesForm);
            Text = nameof(ConfiguredDevicesForm);
            m_devicesTab.ResumeLayout(false);
            m_airExchangerTab.ResumeLayout(false);
            m_airExchangerTab.PerformLayout();
            m_cortexTab.ResumeLayout(false);
            m_cortexTab.PerformLayout();
            m_fraudSensorTab.ResumeLayout(false);
            m_fraudSensorTab.PerformLayout();
            groupBox1.ResumeLayout(false);
            m_kioskFunctionCheck.ResumeLayout(false);
            m_kioskFunctionCheck.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            tabPage1.ResumeLayout(false);
            ((ISupportInitialize)errorProvider1).EndInit();
            ResumeLayout(false);
        }

        private sealed class ConfiguredDeviceOuptutBox : OutputBox
        {
            internal ConfiguredDeviceOuptutBox(ListBox box)
                : base(box)
            {
            }

            protected override string PrewriteFormat(string msg)
            {
                return string.Format("{0} {1}", DateTime.Now.ToLongTimeString(), msg);
            }
        }

        private class FraudTestExecutor : JobExecutor
        {
            internal FraudTestExecutor(HardwareService service)
                : base(service)
            {
            }

            protected override string JobName => "fraud-test-disk-in-picker";
        }

        private class FraudTakeReadReturnExecutor : JobExecutor
        {
            internal FraudTakeReadReturnExecutor(HardwareService service)
                : base(service)
            {
            }

            protected override string JobName => "fraud-test-take-read-return";
        }

        private class FraudStressTestExecutor : JobExecutor
        {
            private readonly int IterationPause;
            private readonly int Iterations;
            private readonly int ScanPause;

            internal FraudStressTestExecutor(
                HardwareService service,
                int iterations,
                int scanPause,
                int iterationPause)
                : base(service)
            {
                Iterations = iterations;
                ScanPause = scanPause;
                IterationPause = iterationPause;
            }

            protected override string JobName => "fraud-sensor-stress-test";

            protected override void SetupJob(HardwareJob job)
            {
                job.Push(IterationPause);
                job.Push(ScanPause);
                job.Push(Iterations);
            }
        }

        private class FraudDetectionTestExecutor : JobExecutor
        {
            private readonly int StartupPause;

            internal FraudDetectionTestExecutor(HardwareService service, int startupPause)
                : base(service)
            {
                StartupPause = startupPause;
            }

            protected override string JobName => "fraud-sensor-detect-test";

            protected override void SetupJob(HardwareJob job)
            {
                job.Push(StartupPause);
            }
        }
    }
}