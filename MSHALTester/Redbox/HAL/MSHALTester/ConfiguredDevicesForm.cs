using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Client.Services;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.MSHALTester;

public class ConfiguredDevicesForm : Form
{
    private readonly OutputBox AirExchangerOutput;
    private readonly byte[] CenterItemProgram;
    private readonly OutputBox EngineeringOutput;
    private readonly AirExchangerState ExchangerState;
    private readonly OutputBox FraudSensorOutput;
    private readonly OutputBox HardwareStatsOutput;
    private readonly OutputBox IrOutput;
    private readonly OutputBox KioskTestOutput;
    private readonly ButtonAspectsManager Manager;
    private readonly OutputBox OutputBox;
    private readonly HardwareService Service;
    private bool ArcusResetConfigured;
    private Button btn_KFCCheckDrivers;
    private Button btn_KFCDecodeTest;
    private Button btn_KFCInit;
    private Button btn_KFCTestVendDoor;
    private Button btn_KFCUnknownCount;
    private Button btn_KFCVerticalSlotTest;
    private Button button1;
    private Button button10;
    private Button button11;
    private Button button13;
    private Button button14;
    private Button button15;
    private Button button16;
    private Button button19;
    private Button button2;
    private Button button21;
    private Button button22;
    private Button button23;
    private Button button24;
    private Button button25;
    private Button button26;
    private Button button27;
    private Button button28;
    private Button button29;
    private Button button3;
    private Button button30;
    private Button button31;
    private Button button32;
    private Button button33;
    private Button button34;
    private Button button35;
    private Button button36;
    private Button button37;
    private Button button38;
    private Button button39;
    private Button button4;
    private Button button40;
    private Button button41;
    private Button button42;
    private Button button43;
    private Button button44;
    private Button button45;
    private Button button46;
    private Button button47;
    private Button button5;
    private Button button6;
    private Button button7;
    private IContainer components;
    private ErrorProvider errorProvider1;
    private KioskFunctionTest FunctionTest;
    private GroupBox groupBox1;
    private GroupBox groupBox2;
    private GroupBox groupBox3;
    private GroupBox groupBox4;
    private GroupBox groupBox5;
    private GroupBox groupBox6;
    private GroupBox groupBox7;
    private GroupBox groupBox8;
    private bool HasFraudSensor;
    private bool HasRouterPowerRelay;
    private Label label1;
    private Label label2;
    private Label label3;
    private Label label4;
    private Label label5;
    private ListView listView1;
    private TabPage m_airExchangerTab;
    private bool m_allowSessionDisplay;
    private TextBox m_cameraSnapTB;
    private TextBox m_checkDriversStatusTB;
    private Button m_configureArcus;
    private Button m_configureFraudSensor;
    private Button m_configureQubeButton;
    private Button m_configureRouterRelay;
    private ListBox m_cortexStatusBox;
    private TabPage m_cortexTab;
    private TabControl m_devicesTab;
    private bool m_displayEngineeringTab;
    private Button m_displaySessionsButton;
    private TextBox m_engDeckTB;
    private TextBox m_engEndDeckTB;
    private TextBox m_engEndSlotTB;
    private ListBox m_engineeringOutput;
    private TabPage m_engineeringTab;
    private TextBox m_engSlotTB;
    private Button m_exitButton;
    private TextBox m_fraudDeckTB;
    private Button m_fraudPOSTButton;
    private ListBox m_fraudSensorOutput;
    private TabPage m_fraudSensorTab;
    private TextBox m_fraudSlotTB;
    private TabPage m_hwCorrections;
    private GroupBox m_hwCorrectiontypeGB;
    private ListBox m_hwStatsOutput;
    private Label m_iceQubeExchangerLabel;
    private ListBox m_iceQubeOutput;
    private TextBox m_initTB;
    private Button m_installCortexButton;
    private ListBox m_irCameraOutput;
    private TabPage m_irConfigurationTab;
    private bool m_irConfigured;
    private TabPage m_kioskFunctionCheck;
    private ListBox m_kioskTestOutput;
    private Button m_powerOffFraudSensor;
    private Button m_powerOnFraudSensor;
    private Button m_qubeStatusButton;
    private TextBox m_readPauseTB;
    private Button m_resetFraudButton;
    private Button m_resetPersistentCounter;
    private Button m_resetQubeBoard;
    private Button m_restartArcus;
    private Button m_restartTouchscreen;
    private BackgroundWorker m_runDecodeWorker;
    private BackgroundWorker m_runInitWorker;
    private TextBox m_scanPauseTB;
    private TextBox m_sourceDeckTB;
    private TextBox m_sourceSlotTB;
    private Button m_startCortexButton;
    private Button m_startFraudScan;
    private Button m_stopFraudScan;
    private TextBox m_stressIterationsTB;
    private Button m_testDiskForMarkers;
    private Button m_testFraudButton;
    private Button m_testRouterRelay;
    private TextBox m_testVendDoorTB;
    private Button m_uninstallCortexButton;
    private TextBox m_unknownCountTB;
    private TextBox m_userName;
    private BackgroundWorker m_verticalSyncWorker;
    private TextBox m_verticalTestTB;
    private RadioButton radioButton1;
    private RadioButton radioButton2;
    private RadioButton radioButton3;
    private RadioButton radioButton4;
    private RadioButton radioButton5;
    private ScannerServices ScannerService = ScannerServices.Emulated;
    private TabPage tabPage1;
    private TextBox textBox1;
    private TextBox textBox2;

    public ConfiguredDevicesForm(HardwareService service)
    {
        InitializeComponent();
        OutputBox = new ConfiguredDeviceOuptutBox(m_cortexStatusBox);
        Service = service;
        m_devicesTab.Selected += OnTabSelectedChange;
        Manager = new ButtonAspectsManager();
        AirExchangerOutput = new ConfiguredDeviceOuptutBox(m_iceQubeOutput);
        FraudSensorOutput = new ConfiguredDeviceOuptutBox(m_fraudSensorOutput);
        KioskTestOutput = new ConfiguredDeviceOuptutBox(m_kioskTestOutput);
        HardwareStatsOutput = new ConfiguredDeviceOuptutBox(m_hwStatsOutput);
        EngineeringOutput = new ConfiguredDeviceOuptutBox(m_engineeringOutput);
        ExchangerState = new AirExchangerState(service);
        IrOutput = new ConfiguredDeviceOuptutBox(m_irCameraOutput);
        using (var machineConfiguration = new MachineConfiguration(Service))
        {
            machineConfiguration.Run();
            var num = (int)ExchangerState.Configure(machineConfiguration.AirExchangerStatus,
                machineConfiguration.AirExchangerFanStatus);
            HasFraudSensor = machineConfiguration.HasFraudDevice;
            ScannerService = machineConfiguration.ConfiguredCamera;
            HasRouterPowerRelay = machineConfiguration.HasRouterPowerRelay;
            ArcusResetConfigured = machineConfiguration.ArcusResetConfigured;
            m_irConfigured = machineConfiguration.HasIRHardware;
            btn_KFCCheckDrivers.Enabled = !machineConfiguration.KFCDisableCheckDrivers;
            btn_KFCDecodeTest.Enabled = !machineConfiguration.KFCDisableDecodeTest;
            btn_KFCTestVendDoor.Enabled = !machineConfiguration.KFCDisableVendDoorTest;
            btn_KFCInit.Enabled = !machineConfiguration.KFCDisableInit;
            btn_KFCVerticalSlotTest.Enabled = !machineConfiguration.KFCDisableVerticalSlotTest;
            btn_KFCUnknownCount.Enabled = !machineConfiguration.KFCDisableUnknownCount;
        }

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("ROLLER POS=6 TIMEOUT=4000 WAIT=TRUE ");
        stringBuilder.AppendLine(" ROLLER POS=3 TIMEOUT=4000 WAIT=TRUE ");
        stringBuilder.AppendLine(" CLEAR");
        CenterItemProgram = Encoding.ASCII.GetBytes(stringBuilder.ToString());
        listView1.Columns.Add("Component", 200);
        listView1.Columns.Add("Value", 200);
        RouterPowerCyclePause = 1500;
    }

    public bool DisplayEngineeringTab
    {
        get => m_displayEngineeringTab;
        set
        {
            m_displayEngineeringTab = value;
            if (m_displayEngineeringTab)
                return;
            m_devicesTab.TabPages.Remove(m_engineeringTab);
        }
    }

    public int RouterPowerCyclePause { get; set; }

    public bool AllowSessionDisplay
    {
        get => m_allowSessionDisplay;
        set
        {
            m_allowSessionDisplay = value;
            m_displaySessionsButton.Enabled = m_displaySessionsButton.Visible = m_allowSessionDisplay;
        }
    }

    private int SourceDeck => m_engDeckTB.GetInteger("Source Deck", EngineeringOutput);

    private int SourceSlot => m_engSlotTB.GetInteger("Source Slot", EngineeringOutput);

    private int DestinationDeck => m_engEndDeckTB.GetInteger("Source Deck", EngineeringOutput);

    private int DestinationSlot => m_engEndSlotTB.GetInteger("Source Slot", EngineeringOutput);

    private int? GetDeck
    {
        get
        {
            var integer = textBox1.GetInteger("Deck", IrOutput);
            if (-1 != integer)
                return integer;
            errorProvider1.SetError(textBox1, "Please specify a valid deck");
            return new int?();
        }
    }

    private int? GetSlot
    {
        get
        {
            var integer = textBox2.GetInteger("Slot", IrOutput);
            if (-1 != integer)
                return integer;
            errorProvider1.SetError(textBox2, "Please specify a valid deck");
            return new int?();
        }
    }

    private void ConfigureCortexPanel()
    {
        var flag = ScannerServices.Cortex == ScannerService;
        m_installCortexButton.Enabled = !flag;
        m_uninstallCortexButton.Enabled = flag;
        WriteToBox(string.Format("The camera {0} enabled.", flag ? "is" : (object)"is not"));
    }

    private void OnConfigureIceQubeBoard()
    {
        var configured = ExchangerState.Configured;
        m_iceQubeExchangerLabel.Text = string.Format("The board {0} configured.", configured ? "is" : (object)"is not");
        m_configureQubeButton.Text = configured ? "Remove" : "Configure";
        m_resetQubeBoard.Enabled = configured;
        m_qubeStatusButton.Enabled = configured;
        m_resetPersistentCounter.Enabled = configured;
    }

    private void OnTabSelectedChange(object sender, EventArgs e)
    {
        switch (m_devicesTab.SelectedIndex)
        {
            case 0:
                OnConfigureIceQubeBoard();
                break;
            case 1:
                ConfigureCortexPanel();
                break;
            case 2:
                OnConfigureFraudSensor();
                break;
            case 3:
                OnConfigureKioskTest();
                break;
            case 5:
                ConfigureRouterGroup();
                ConfigureArcusGroup();
                break;
            case 6:
                ConfigureIrCameraGroup();
                break;
        }
    }

    private void ConfigureIrCameraGroup()
    {
        var flag = ScannerServices.Cortex == ScannerService;
        if (!flag)
            button37.Text = m_irConfigured ? "Un-configure" : "Configure";
        else
            button37.Text = "Unavailable";
        button38.Enabled = button39.Enabled = button40.Enabled = button41.Enabled = !flag;
        button42.Enabled = button43.Enabled = button46.Enabled = button47.Enabled = !flag;
        button44.Enabled = button45.Enabled = button37.Enabled = !flag;
    }

    private void OnConfigureKioskTest()
    {
        var currentSession = ServiceLocator.Instance.GetService<ISessionUserService>().GetCurrentSession();
        FunctionTest = new KioskFunctionTest(Service, KioskTestOutput, ScannerServices.Cortex == ScannerService,
            currentSession.User);
        m_testVendDoorTB.Text = m_checkDriversStatusTB.Text = m_cameraSnapTB.Text =
            m_initTB.Text = m_verticalTestTB.Text = m_unknownCountTB.Text = "Not Started";
        m_userName.Text = currentSession.User;
    }

    private void OnConfigureFraudSensor()
    {
        FraudSensorOutput.Write("Fraud sensor is {0}.", HasFraudSensor ? "enabled" : (object)"disabled");
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
        if (!ExchangerState.ToggleConfiguration())
        {
            AirExchangerOutput.Write("Failed to update configuration.");
        }
        else
        {
            AirExchangerOutput.Write("The air exchanger is {0}.",
                ExchangerState.Configured ? "configured" : (object)"not configured");
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
        using (var buttonAspects = Manager.MakeAspect(sender))
        {
            var tagInstruction = buttonAspects.GetTagInstruction();
            if (string.IsNullOrEmpty(tagInstruction))
                box.Write("There is no operand associated with that button.");
            else
                using (var instructionHelper = new InstructionHelper(Service))
                {
                    var instruction = string.Format("{0} {1}", mnemonic, tagInstruction);
                    var str = instructionHelper.ExecuteGeneric(instruction);
                    box.Write(string.IsNullOrEmpty(str) ? "Instruction execution failed." : str);
                }
        }
    }

    private void m_exitButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void m_installCortexButton_Click(object sender, EventArgs e)
    {
        ToggleCortex(sender);
    }

    private void m_uninstallCortexButton_Click(object sender, EventArgs e)
    {
        ToggleCortex(sender);
    }

    private void ToggleCortex(object sender)
    {
        var newService = ScannerServices.Cortex == ScannerService ? ScannerServices.Legacy : ScannerServices.Cortex;
        ServiceLocator.Instance.GetService<IRuntimeService>();
        using (Manager.MakeAspect(sender))
        {
            using (var configurationExecutor = new ChangeCameraConfigurationExecutor(Service, newService))
            {
                configurationExecutor.Run();
                if (HardwareJobStatus.Completed == configurationExecutor.EndStatus)
                {
                    ScannerService = newService;
                    var flag = ScannerServices.Cortex == ScannerService;
                    WriteToBox("The camera was successfully installed");
                    m_installCortexButton.Enabled = !flag;
                    m_uninstallCortexButton.Enabled = m_startCortexButton.Enabled = flag;
                }
                else
                {
                    configurationExecutor.Errors.ForEach(each => WriteToBox(each.Details));
                    WriteToBox("Unable to change camera configuration.");
                }
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
            using (var instructionHelper = new InstructionHelper(Service))
            {
                var str = instructionHelper.ExecuteGeneric("CAMERA START");
                if (string.IsNullOrEmpty(str))
                    OutputBox.Write(ErrorCodes.ServiceChannelError.ToString().ToUpper());
                else
                    WriteToBox(bool.Parse(str) ? "Camera started." : "Camera DIDN'T START.");
            }
        }
    }

    private void RunPostTest()
    {
        using (var fraudSensorPost = new FraudSensorPost(Service))
        {
            fraudSensorPost.AddSink((j, time, msg) =>
                FraudSensorOutput.Write("[{0} {1}] {2}", time.ToShortDateString(), time.ToShortTimeString(), msg));
            fraudSensorPost.Run();
            fraudSensorPost.Results.ForEach(each => FraudSensorOutput.Write("{0} : {1}", each.Code, each.Message));
        }

        BeginInvoke(() => m_fraudPOSTButton.Enabled = true);
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
            var result = Service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
            if (!result.Success)
            {
                FraudSensorOutput.Write("Failed to update configuration.");
                DumpResult(result, FraudSensorOutput);
            }
            else
            {
                HasFraudSensor = flag;
                OnConfigureFraudSensor();
            }
        }
    }

    private void DumpResult(HardwareCommandResult result, OutputBox box)
    {
        result.Errors.ForEach(err => box.Write(err.ToString()));
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
                CompositeFunctions.GetItem(integer1, integer2, FraudSensorOutput, Service);
                Service.ExecuteImmediateProgram(CenterItemProgram, out var _);
            }
        }
    }

    private void button2_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            CompositeFunctions.VendDisk(Service, FraudSensorOutput);
        }
    }

    private void button4_Click_1(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var integer1 = m_fraudDeckTB.GetInteger("Deck", FraudSensorOutput);
            var integer2 = m_fraudSlotTB.GetInteger("Slot", FraudSensorOutput);
            if (-1 == integer1 || -1 == integer2)
                FraudSensorOutput.Write("Check deck and slot!");
            else
                CompositeFunctions.PutItem(Service, integer1, integer2, OutputBox);
        }
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
        using (var buttonAspects = Manager.MakeAspect(sender))
        {
            using (var instructionHelper = new InstructionHelper(Service))
            {
                var tagInstruction = buttonAspects.GetTagInstruction();
                if (string.IsNullOrEmpty(tagInstruction))
                    FraudSensorOutput.Write("There is no instruction for that button.");
                else
                    instructionHelper.ExecuteGeneric(tagInstruction);
            }
        }
    }

    private void button3_Click_1(object sender, EventArgs e)
    {
        FraudSensorOutput.Clear();
    }

    private void button10_Click(object sender, EventArgs e)
    {
        AirExchangerOutput.Clear();
    }

    private void RunStressTest()
    {
        var integer1 = m_scanPauseTB.GetInteger("Scan Pause", FraudSensorOutput);
        var integer2 = m_stressIterationsTB.GetInteger("iterations", FraudSensorOutput);
        var integer3 = m_readPauseTB.GetInteger("Iteration Pause", FraudSensorOutput);
        using (var stressTestExecutor = new FraudStressTestExecutor(Service, integer2, integer1, integer3))
        {
            stressTestExecutor.AddSink((j, time, msg) =>
                FraudSensorOutput.Write("[{0} {1}] {2}", time.ToShortDateString(), time.ToShortTimeString(), msg));
            stressTestExecutor.Run();
            foreach (var result in stressTestExecutor.Results)
                FraudSensorOutput.Write("{0} : {1}", result.Code, result.Message);
        }

        BeginInvoke(() => button13.Enabled = true);
    }

    private void button14_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var num = (int)ServiceLocator.Instance.GetService<IControlSystem>()
                .Center(CenterDiskMethod.VendDoorAndBack);
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
        btn_KFCInit.BackColor = Color.Red;
        ToggleKfcButtons(false);
        m_runInitWorker.RunWorkerAsync();
    }

    private void button17_Click(object sender, EventArgs e)
    {
        errorProvider1.Clear();
        var integer = m_sourceSlotTB.GetInteger("Test Slot", KioskTestOutput);
        if (-1 == integer)
        {
            errorProvider1.SetError(m_sourceSlotTB, "Select slot");
        }
        else
        {
            ToggleKfcButtons(false);
            btn_KFCVerticalSlotTest.BackColor = Color.Red;
            m_verticalSyncWorker.RunWorkerAsync(integer);
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
        m_sourceSlotTB.InputNumber();
    }

    private void button20_Click(object sender, EventArgs e)
    {
        var integer1 = m_sourceDeckTB.GetInteger("Sync deck", KioskTestOutput);
        var integer2 = m_sourceSlotTB.GetInteger("Sync slot", KioskTestOutput);
        if (-1 == integer1)
        {
            errorProvider1.SetError(m_sourceDeckTB, "Please enter valid deck.");
        }
        else if (-1 == integer2)
        {
            errorProvider1.SetError(m_sourceSlotTB, "Please enter valid slot.");
        }
        else
        {
            ToggleKfcButtons(false);
            btn_KFCDecodeTest.BackColor = Color.Red;
            m_runDecodeWorker.RunWorkerAsync(new Location
            {
                Deck = integer1,
                Slot = integer2
            });
        }
    }

    private void button21_Click(object sender, EventArgs e)
    {
        m_sourceDeckTB.InputNumber();
    }

    private void button22_Click(object sender, EventArgs e)
    {
        m_sourceSlotTB.InputNumber();
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

    private void m_configureRouterRelay_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var flag = !HasRouterPowerRelay;
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Format("SETCFG \"RouterPowerCyclePause\" \"{0}\" TYPE=CONTROLLER",
                flag ? RouterPowerCyclePause : 0));
            if (flag)
                stringBuilder.AppendLine("SETCFG \"TrackHardwareCorrections\" \"TRUE\" TYPE=CONTROLLER");
            stringBuilder.AppendLine(" CLEAR");
            var result = Service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
            if (!result.Success)
            {
                DumpResult(result, HardwareStatsOutput);
                HardwareStatsOutput.Write("Failed to update configuration.");
            }
            else
            {
                HasRouterPowerRelay = flag;
                ConfigureRouterGroup();
            }
        }
    }

    private void ConfigureRouterGroup()
    {
        m_configureRouterRelay.Text = HasRouterPowerRelay ? "Disable" : "Enable";
        HardwareStatsOutput.Write("Router relay is {0}.",
            HasRouterPowerRelay ? "configured" : (object)"not configured");
        m_testRouterRelay.Enabled = HasRouterPowerRelay;
    }

    private void ConfigureArcusGroup()
    {
        m_restartArcus.Enabled = ArcusResetConfigured;
        m_configureArcus.Text = ArcusResetConfigured ? "Disable" : "Enable";
        HardwareStatsOutput.Write("Kiosk is {0} to reset the Arcus.",
            ArcusResetConfigured ? "configured" : (object)"not configured");
    }

    private void m_testRouterRelay_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var service = Service;
            var s = new HardwareJobSchedule();
            s.Priority = HardwareJobPriority.High;
            HardwareJob job;
            var result = service.PowerCycleRouter(s, out job);
            if (result.Success)
            {
                using (var clientHelper = new ClientHelper(Service))
                {
                    HardwareJobStatus endStatus;
                    clientHelper.WaitForJob(job, out endStatus);
                    HardwareStatsOutput.Write("Power cycle router job {0} ended with status {1}", job.ID, endStatus);
                }
            }
            else
            {
                DumpResult(result, HardwareStatsOutput);
                HardwareStatsOutput.Write("Failed to schedule router power cycle job.");
            }
        }
    }

    private void button24_Click(object sender, EventArgs e)
    {
        var stat = HardwareCorrectionStatistic.None;
        foreach (Control control in (ArrangedElementCollection)m_hwCorrectiontypeGB.Controls)
            if (control.GetType() == typeof(RadioButton))
            {
                var radioButton = (RadioButton)control;
                if (radioButton.Checked)
                {
                    stat = Enum<HardwareCorrectionStatistic>.ParseIgnoringCase((string)radioButton.Tag,
                        HardwareCorrectionStatistic.None);
                    break;
                }
            }

        if (stat != HardwareCorrectionStatistic.None)
            using (var correctionStatistics = new GetHardwareCorrectionStatistics(Service, stat))
            {
                correctionStatistics.Run();
                if (HardwareJobStatus.Completed != correctionStatistics.EndStatus)
                    HardwareStatsOutput.Write("Failed to get statistics from service.");
                else
                    DumpHardwareStats(correctionStatistics.Stats);
            }
        else
            using (var correctionStatistics = new GetAllHardwareCorrectionStatistics(Service))
            {
                correctionStatistics.Run();
                if (HardwareJobStatus.Completed != correctionStatistics.EndStatus)
                    HardwareStatsOutput.Write("Failed to get statistics from service.");
                else
                    DumpHardwareStats(correctionStatistics.Stats);
            }
    }

    private void DumpHardwareStats(List<IHardwareCorrectionStatistic> stats)
    {
        stats.ForEach(s => HardwareStatsOutput.Write("{0} {1}: {2} {3}", s.CorrectionTime, s.Statistic, s.ProgramName,
            s.CorrectionOk ? "SUCCESS" : (object)"FAILURE "));
        HardwareStatsOutput.Write("Total of {0} statistics", stats.Count);
    }

    private void button23_Click(object sender, EventArgs e)
    {
        HardwareStatsOutput.Clear();
    }

    private void button27_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            CompositeFunctions.VendDisk(Service, EngineeringOutput);
        }
    }

    private void button28_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            EngineeringOutput.Write(ScanResult.ReadBarcodeOfDiskInPicker(Service).ToString());
        }
    }

    private void button26_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var service = Service;
            var empty = string.Empty;
            var schedule = new HardwareJobSchedule();
            schedule.Priority = HardwareJobPriority.High;
            HardwareJob job;
            if (!service.ScheduleJob("ms-take-disk-at-door", empty, false, schedule, out job).Success)
            {
                EngineeringOutput.Write("Failed to schedule job");
            }
            else
            {
                job.Pend();
                using (var clientHelper = new ClientHelper(Service))
                {
                    HardwareJobStatus endStatus;
                    clientHelper.WaitForJob(job, out endStatus);
                    if (endStatus != HardwareJobStatus.Completed)
                        return;
                    ProgramResult[] results;
                    if (!job.GetResults(out results).Success)
                        EngineeringOutput.Write("Failed to get program results. I'm sorry!");
                    else
                        foreach (var programResult in results)
                            EngineeringOutput.Write(programResult.Message);
                }
            }
        }
    }

    private void button25_Click(object sender, EventArgs e)
    {
        EngineeringOutput.Clear();
    }

    private void button29_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var sourceDeck = SourceDeck;
            var sourceSlot = SourceSlot;
            if (-1 == sourceDeck || -1 == sourceSlot)
            {
                EngineeringOutput.Write("Please specify source deck/slot properly");
            }
            else
            {
                var destinationDeck = DestinationDeck;
                var destinationSlot = DestinationSlot;
                if (-1 == destinationDeck || -1 == destinationSlot)
                {
                    EngineeringOutput.Write("Please specify destination deck/slot properly");
                }
                else
                {
                    var syncRange = new SyncRange(sourceDeck, destinationDeck,
                        new SlotRange(sourceSlot, destinationSlot));
                    var service = Service;
                    var range = syncRange;
                    var schedule = new HardwareJobSchedule();
                    schedule.Priority = HardwareJobPriority.Highest;
                    HardwareJob hardwareJob;
                    if (!service.HardSync(range, schedule, out hardwareJob).Success)
                    {
                        EngineeringOutput.Write("Unable to communicate with HAL.");
                    }
                    else
                    {
                        hardwareJob.Pend();
                        hardwareJob.WaitForCompletion(300000);
                        IDictionary<string, string> symbols;
                        if (hardwareJob.GetSymbols(out symbols).Success)
                            foreach (var key in symbols.Keys)
                                if (key.StartsWith("MSTESTER-SYMBOL"))
                                    EngineeringOutput.Write(symbols[key]);

                        ProgramResult[] results;
                        if (!hardwareJob.GetResults(out results).Success)
                            EngineeringOutput.Write("Failed to get program results. I'm sorry!");
                        else
                            foreach (var programResult in results)
                                EngineeringOutput.Write(programResult.Message);
                    }
                }
            }
        }
    }

    private void button34_Click(object sender, EventArgs e)
    {
        m_engSlotTB.InputNumber();
    }

    private void button33_Click(object sender, EventArgs e)
    {
        m_engDeckTB.InputNumber();
    }

    private void button31_Click(object sender, EventArgs e)
    {
        m_engEndDeckTB.InputNumber();
    }

    private void button32_Click(object sender, EventArgs e)
    {
        m_engEndSlotTB.InputNumber();
    }

    private void button30_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var sourceDeck = SourceDeck;
            var sourceSlot = SourceSlot;
            if (-1 == sourceDeck || -1 == sourceSlot)
            {
                EngineeringOutput.Write("Please specify deck/slot properly");
            }
            else
            {
                var locationList = new List<Location>
                {
                    new() { Deck = sourceDeck, Slot = sourceSlot }
                };
                var service = Service;
                var locations = locationList;
                var schedule = new HardwareJobSchedule();
                schedule.Priority = HardwareJobPriority.Highest;
                HardwareJob hardwareJob;
                if (!service.HardSync(locations, "Engineering Location Sync", schedule, out hardwareJob).Success)
                {
                    EngineeringOutput.Write("Unable to communicate with HAL.");
                }
                else
                {
                    hardwareJob.Pend();
                    hardwareJob.WaitForCompletion(300000);
                    IDictionary<string, string> symbols;
                    if (hardwareJob.GetSymbols(out symbols).Success)
                        foreach (var key in symbols.Keys)
                            if (key.StartsWith("MSTESTER-SYMBOL"))
                                EngineeringOutput.Write(symbols[key]);

                    ProgramResult[] results;
                    if (!hardwareJob.GetResults(out results).Success)
                        EngineeringOutput.Write("Failed to get program results. I'm sorry!");
                    else
                        foreach (var programResult in results)
                            EngineeringOutput.Write(programResult.Message);
                }
            }
        }
    }

    private void button35_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var sourceDeck = SourceDeck;
            var sourceSlot = SourceSlot;
            if (-1 == sourceDeck || -1 == sourceSlot)
                EngineeringOutput.Write("Please specify deck/slot properly");
            else
                CompositeFunctions.GetItem(sourceDeck, sourceSlot, EngineeringOutput, Service);
        }
    }

    private void button36_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var sourceDeck = SourceDeck;
            var sourceSlot = SourceSlot;
            if (-1 == sourceDeck || -1 == sourceSlot)
                EngineeringOutput.Write("Please specify deck/slot properly");
            else
                CompositeFunctions.PutItem(Service, sourceDeck, sourceSlot, OutputBox);
        }
    }

    private void m_restartTouchscreen_Click(object sender, EventArgs e)
    {
        var service = Service;
        var s = new HardwareJobSchedule();
        s.Priority = HardwareJobPriority.High;
        HardwareJob job;
        if (service.ResetTouchscreenController(s, out job).Success)
            using (var clientHelper = new ClientHelper(Service))
            {
                HardwareJobStatus endStatus;
                clientHelper.WaitForJob(job, out endStatus);
                HardwareStatsOutput.Write("Reset touchscreen controller ended with status {0}", endStatus.ToString());
            }
        else
            HardwareStatsOutput.Write("Unable to schedule reset touchscreen job.");
    }

    private void m_restartArcus_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            using (var motionControlExecutor = new ResetMotionControlExecutor(Service))
            {
                motionControlExecutor.Run();
                if (motionControlExecutor.EndStatus != HardwareJobStatus.Completed)
                    HardwareStatsOutput.Write("ERROR: reset-arcus failed.");
                else
                    foreach (var result in motionControlExecutor.Results)
                        HardwareStatsOutput.Write(result.Message);
            }
        }
    }

    private void m_configureArcus_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var flag = !ArcusResetConfigured;
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Format("SETCFG \"RestartControllerDuringUserJobs\" \"{0}\" TYPE=CONTROLLER",
                flag));
            if (flag)
                stringBuilder.AppendLine("SETCFG \"TrackHardwareCorrections\" \"TRUE\" TYPE=CONTROLLER");
            stringBuilder.AppendLine("CLEAR");
            var result = Service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
            if (!result.Success)
            {
                DumpResult(result, HardwareStatsOutput);
                HardwareStatsOutput.Write("Failed to update configuration.");
            }
            else
            {
                ArcusResetConfigured = flag;
                ConfigureArcusGroup();
            }
        }
    }

    private void button37_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var flag = !HasRouterPowerRelay;
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Format("SETCFG \"RouterPowerCyclePause\" \"{0}\" TYPE=CONTROLLER",
                flag ? RouterPowerCyclePause : 0));
            if (flag)
                stringBuilder.AppendLine("SETCFG \"TrackHardwareCorrections\" \"TRUE\" TYPE=CONTROLLER");
            stringBuilder.AppendLine(" CLEAR");
            var result = Service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
            if (!result.Success)
            {
                DumpResult(result, HardwareStatsOutput);
                HardwareStatsOutput.Write("Failed to update configuration.");
            }
            else
            {
                HasRouterPowerRelay = flag;
                ConfigureRouterGroup();
            }
        }
    }

    private void button38_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var service = Service;
            var s = new HardwareJobSchedule();
            s.Priority = HardwareJobPriority.High;
            HardwareJob job;
            var result = service.PowerCycleRouter(s, out job);
            if (result.Success)
            {
                using (var clientHelper = new ClientHelper(Service))
                {
                    HardwareJobStatus endStatus;
                    clientHelper.WaitForJob(job, out endStatus);
                    HardwareStatsOutput.Write("Power cycle router job {0} ended with status {1}", job.ID, endStatus);
                }
            }
            else
            {
                DumpResult(result, HardwareStatsOutput);
                HardwareStatsOutput.Write("Failed to schedule router power cycle job.");
            }
        }
    }

    private void ToggleKfcButtons(bool enable)
    {
        btn_KFCCheckDrivers.Enabled = btn_KFCDecodeTest.Enabled = btn_KFCTestVendDoor.Enabled =
            btn_KFCInit.Enabled = btn_KFCVerticalSlotTest.Enabled = btn_KFCUnknownCount.Enabled = enable;
    }

    private void m_verticalSyncWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        var slot = (int)e.Argument;
        e.Result = FunctionTest.RunVertialSlotTest(slot).ToString().ToUpper();
    }

    private void m_verticalSyncWorker_RunWorkerCompleted(
        object sender,
        RunWorkerCompletedEventArgs e)
    {
        ToggleKfcButtons(true);
        m_verticalTestTB.Text = e.Result as string;
        btn_KFCVerticalSlotTest.BackColor = Color.LightGray;
    }

    private void m_runInitWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        e.Result = FunctionTest.RunInit().ToString().ToUpper();
    }

    private void m_runInitWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        ToggleKfcButtons(true);
        btn_KFCInit.BackColor = Color.LightGray;
        m_initTB.Text = e.Result as string;
    }

    private void m_runDecodeWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        var loc = e.Argument as Location;
        e.Result = FunctionTest.TestCameraSnap(loc).ToString().ToUpper();
    }

    private void m_runDecodeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        ToggleKfcButtons(true);
        btn_KFCDecodeTest.BackColor = Color.LightGray;
        m_cameraSnapTB.Text = e.Result as string;
    }

    private void button37_Click_1(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var flag = !m_irConfigured;
            var universalTime = DateTime.Now.ToUniversalTime();
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Format("SETCFG \"IRHardwareInstallDate\" \"{0}\" TYPE=KIOSK",
                flag ? universalTime.ToString() : (object)"NONE"));
            stringBuilder.AppendLine("CLEAR");
            var hardwareCommandResult =
                Service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
            if (!hardwareCommandResult.Success)
            {
                hardwareCommandResult.Errors.ForEach(err => IrOutput.Write(err.ToString()));
                IrOutput.Write("Failed to update configuration.");
            }
            else
            {
                m_irConfigured = flag;
                ConfigureIrCameraGroup();
            }
        }
    }

    private void OnExecuteResponseInstruction(object sender, EventArgs e)
    {
        using (var buttonAspects = Manager.MakeAspect(sender))
        {
            var tagInstruction = buttonAspects.GetTagInstruction();
            using (var instructionHelper = new InstructionHelper(Service))
            {
                var str = instructionHelper.ExecuteGeneric(tagInstruction);
                IrOutput.Write("{0} - {1}", tagInstruction, str);
            }
        }
    }

    private void button46_Click(object sender, EventArgs e)
    {
        textBox1.InputNumber();
    }

    private void button47_Click(object sender, EventArgs e)
    {
        textBox2.InputNumber();
    }

    private void button43_Click(object sender, EventArgs e)
    {
        var getDeck = GetDeck;
        var getSlot = GetSlot;
        if (!getDeck.HasValue || !getSlot.HasValue)
            return;
        CompositeFunctions.GetItem(getDeck.Value, getSlot.Value, IrOutput, Service);
        using (Manager.MakeAspect(sender))
        {
            var scanResult = ScanResult.ReadBarcodeOfDiskInPicker(Service);
            IrOutput.Write(string.Format("found {0} secure codes.", scanResult.SecureCount));
            IrOutput.Write(scanResult.ToString());
        }
    }

    private void button44_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var getDeck = GetDeck;
            var getSlot = GetSlot;
            if (!getDeck.HasValue || !getSlot.HasValue)
                return;
            CompositeFunctions.PutItem(Service, getDeck.Value, getSlot.Value, IrOutput);
        }
    }

    private void button42_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            TunerLaunchService.LaunchTunerAndWait(Service, Manager);
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
        components = new Container();
        m_devicesTab = new TabControl();
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
        btn_KFCDecodeTest = new Button();
        m_verticalTestTB = new TextBox();
        m_initTB = new TextBox();
        m_testVendDoorTB = new TextBox();
        m_checkDriversStatusTB = new TextBox();
        btn_KFCUnknownCount = new Button();
        btn_KFCVerticalSlotTest = new Button();
        btn_KFCTestVendDoor = new Button();
        btn_KFCInit = new Button();
        btn_KFCCheckDrivers = new Button();
        m_userName = new TextBox();
        button21 = new Button();
        label1 = new Label();
        button2 = new Button();
        m_kioskTestOutput = new ListBox();
        tabPage1 = new TabPage();
        listView1 = new ListView();
        button16 = new Button();
        button11 = new Button();
        m_hwCorrections = new TabPage();
        groupBox4 = new GroupBox();
        m_testRouterRelay = new Button();
        m_configureRouterRelay = new Button();
        groupBox6 = new GroupBox();
        m_restartArcus = new Button();
        m_configureArcus = new Button();
        groupBox5 = new GroupBox();
        m_restartTouchscreen = new Button();
        m_hwCorrectiontypeGB = new GroupBox();
        radioButton5 = new RadioButton();
        radioButton4 = new RadioButton();
        radioButton2 = new RadioButton();
        radioButton3 = new RadioButton();
        button24 = new Button();
        radioButton1 = new RadioButton();
        button23 = new Button();
        m_hwStatsOutput = new ListBox();
        m_irConfigurationTab = new TabPage();
        button37 = new Button();
        groupBox8 = new GroupBox();
        textBox2 = new TextBox();
        textBox1 = new TextBox();
        button47 = new Button();
        button46 = new Button();
        button45 = new Button();
        button44 = new Button();
        button43 = new Button();
        groupBox7 = new GroupBox();
        button42 = new Button();
        button41 = new Button();
        button40 = new Button();
        button39 = new Button();
        button38 = new Button();
        m_irCameraOutput = new ListBox();
        m_engineeringTab = new TabPage();
        groupBox3 = new GroupBox();
        button36 = new Button();
        button35 = new Button();
        button34 = new Button();
        button29 = new Button();
        button30 = new Button();
        button33 = new Button();
        button32 = new Button();
        m_engEndSlotTB = new TextBox();
        button31 = new Button();
        m_engEndDeckTB = new TextBox();
        m_engDeckTB = new TextBox();
        m_engSlotTB = new TextBox();
        button28 = new Button();
        button27 = new Button();
        button26 = new Button();
        button25 = new Button();
        m_engineeringOutput = new ListBox();
        m_exitButton = new Button();
        errorProvider1 = new ErrorProvider(components);
        m_verticalSyncWorker = new BackgroundWorker();
        m_runInitWorker = new BackgroundWorker();
        m_runDecodeWorker = new BackgroundWorker();
        m_devicesTab.SuspendLayout();
        m_airExchangerTab.SuspendLayout();
        m_cortexTab.SuspendLayout();
        m_fraudSensorTab.SuspendLayout();
        groupBox1.SuspendLayout();
        m_kioskFunctionCheck.SuspendLayout();
        groupBox2.SuspendLayout();
        tabPage1.SuspendLayout();
        m_hwCorrections.SuspendLayout();
        groupBox4.SuspendLayout();
        groupBox6.SuspendLayout();
        groupBox5.SuspendLayout();
        m_hwCorrectiontypeGB.SuspendLayout();
        m_irConfigurationTab.SuspendLayout();
        groupBox8.SuspendLayout();
        groupBox7.SuspendLayout();
        m_engineeringTab.SuspendLayout();
        groupBox3.SuspendLayout();
        ((ISupportInitialize)errorProvider1).BeginInit();
        SuspendLayout();
        m_devicesTab.Controls.Add(m_airExchangerTab);
        m_devicesTab.Controls.Add(m_cortexTab);
        m_devicesTab.Controls.Add(m_fraudSensorTab);
        m_devicesTab.Controls.Add(m_kioskFunctionCheck);
        m_devicesTab.Controls.Add(tabPage1);
        m_devicesTab.Controls.Add(m_hwCorrections);
        m_devicesTab.Controls.Add(m_irConfigurationTab);
        m_devicesTab.Controls.Add(m_engineeringTab);
        m_devicesTab.Location = new Point(2, 12);
        m_devicesTab.Name = "m_devicesTab";
        m_devicesTab.Padding = new Point(10, 20);
        m_devicesTab.SelectedIndex = 0;
        m_devicesTab.Size = new Size(1057, 626);
        m_devicesTab.TabIndex = 0;
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
        m_airExchangerTab.Size = new Size(1049, 566);
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
        m_cortexTab.Size = new Size(1049, 566);
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
        m_fraudSensorTab.Size = new Size(1049, 566);
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
        button13.Click += button7_Click;
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
        m_kioskFunctionCheck.Size = new Size(1049, 566);
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
        groupBox2.Controls.Add(btn_KFCDecodeTest);
        groupBox2.Controls.Add(m_verticalTestTB);
        groupBox2.Controls.Add(m_initTB);
        groupBox2.Controls.Add(m_testVendDoorTB);
        groupBox2.Controls.Add(m_checkDriversStatusTB);
        groupBox2.Controls.Add(btn_KFCUnknownCount);
        groupBox2.Controls.Add(btn_KFCVerticalSlotTest);
        groupBox2.Controls.Add(btn_KFCTestVendDoor);
        groupBox2.Controls.Add(btn_KFCInit);
        groupBox2.Controls.Add(btn_KFCCheckDrivers);
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
        btn_KFCDecodeTest.Location = new Point(12, 89);
        btn_KFCDecodeTest.Name = "btn_KFCDecodeTest";
        btn_KFCDecodeTest.Size = new Size(100, 60);
        btn_KFCDecodeTest.TabIndex = 2;
        btn_KFCDecodeTest.Text = "Decode Test";
        btn_KFCDecodeTest.UseVisualStyleBackColor = true;
        btn_KFCDecodeTest.Click += button20_Click;
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
        btn_KFCUnknownCount.Location = new Point(12, 371);
        btn_KFCUnknownCount.Name = "btn_KFCUnknownCount";
        btn_KFCUnknownCount.Size = new Size(100, 60);
        btn_KFCUnknownCount.TabIndex = 7;
        btn_KFCUnknownCount.Text = "Unknown Count";
        btn_KFCUnknownCount.UseVisualStyleBackColor = true;
        btn_KFCUnknownCount.Click += button18_Click;
        btn_KFCVerticalSlotTest.Location = new Point(12, 298);
        btn_KFCVerticalSlotTest.Name = "btn_KFCVerticalSlotTest";
        btn_KFCVerticalSlotTest.Size = new Size(100, 60);
        btn_KFCVerticalSlotTest.TabIndex = 6;
        btn_KFCVerticalSlotTest.Text = "Vertical Slot Test";
        btn_KFCVerticalSlotTest.UseVisualStyleBackColor = true;
        btn_KFCVerticalSlotTest.Click += button17_Click;
        btn_KFCTestVendDoor.Location = new Point(12, 155);
        btn_KFCTestVendDoor.Name = "btn_KFCTestVendDoor";
        btn_KFCTestVendDoor.Size = new Size(100, 60);
        btn_KFCTestVendDoor.TabIndex = 4;
        btn_KFCTestVendDoor.Text = "Test Vend Door";
        btn_KFCTestVendDoor.UseVisualStyleBackColor = true;
        btn_KFCTestVendDoor.Click += button12_Click;
        btn_KFCInit.Location = new Point(12, 232);
        btn_KFCInit.Name = "btn_KFCInit";
        btn_KFCInit.Size = new Size(100, 60);
        btn_KFCInit.TabIndex = 5;
        btn_KFCInit.Text = "Init";
        btn_KFCInit.UseVisualStyleBackColor = true;
        btn_KFCInit.Click += button9_Click;
        btn_KFCCheckDrivers.Location = new Point(12, 19);
        btn_KFCCheckDrivers.Name = "btn_KFCCheckDrivers";
        btn_KFCCheckDrivers.Size = new Size(100, 60);
        btn_KFCCheckDrivers.TabIndex = 0;
        btn_KFCCheckDrivers.Text = "Check Drivers";
        btn_KFCCheckDrivers.UseVisualStyleBackColor = true;
        btn_KFCCheckDrivers.Click += button8_Click;
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
        tabPage1.Size = new Size(1049, 566);
        tabPage1.TabIndex = 7;
        tabPage1.Text = "Hardware Survey";
        listView1.HideSelection = false;
        listView1.Location = new Point(20, 35);
        listView1.Name = "listView1";
        listView1.Size = new Size(502, 378);
        listView1.TabIndex = 3;
        listView1.UseCompatibleStateImageBehavior = false;
        listView1.View = View.Details;
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
        m_hwCorrections.BackColor = Color.LightGray;
        m_hwCorrections.Controls.Add(groupBox4);
        m_hwCorrections.Controls.Add(groupBox6);
        m_hwCorrections.Controls.Add(groupBox5);
        m_hwCorrections.Controls.Add(m_hwCorrectiontypeGB);
        m_hwCorrections.Controls.Add(button23);
        m_hwCorrections.Controls.Add(m_hwStatsOutput);
        m_hwCorrections.Location = new Point(4, 56);
        m_hwCorrections.Name = "m_hwCorrections";
        m_hwCorrections.Padding = new Padding(3);
        m_hwCorrections.Size = new Size(1049, 566);
        m_hwCorrections.TabIndex = 9;
        m_hwCorrections.Text = "Hardware Corrections";
        groupBox4.Controls.Add(m_testRouterRelay);
        groupBox4.Controls.Add(m_configureRouterRelay);
        groupBox4.Location = new Point(513, 279);
        groupBox4.Name = "groupBox4";
        groupBox4.Size = new Size(200, 175);
        groupBox4.TabIndex = 8;
        groupBox4.TabStop = false;
        groupBox4.Text = "Router ";
        m_testRouterRelay.Location = new Point(23, 100);
        m_testRouterRelay.Name = "m_testRouterRelay";
        m_testRouterRelay.Size = new Size(120, 50);
        m_testRouterRelay.TabIndex = 1;
        m_testRouterRelay.Text = "Power Cycle";
        m_testRouterRelay.UseVisualStyleBackColor = true;
        m_testRouterRelay.Click += button38_Click;
        m_configureRouterRelay.Location = new Point(23, 31);
        m_configureRouterRelay.Name = "m_configureRouterRelay";
        m_configureRouterRelay.Size = new Size(120, 50);
        m_configureRouterRelay.TabIndex = 0;
        m_configureRouterRelay.Text = "Configure";
        m_configureRouterRelay.UseVisualStyleBackColor = true;
        m_configureRouterRelay.Click += button37_Click;
        groupBox6.Controls.Add(m_restartArcus);
        groupBox6.Controls.Add(m_configureArcus);
        groupBox6.Location = new Point(782, 280);
        groupBox6.Name = "groupBox6";
        groupBox6.Size = new Size(200, 175);
        groupBox6.TabIndex = 7;
        groupBox6.TabStop = false;
        groupBox6.Text = "Arcus";
        m_restartArcus.Location = new Point(26, 99);
        m_restartArcus.Name = "m_restartArcus";
        m_restartArcus.Size = new Size(120, 50);
        m_restartArcus.TabIndex = 1;
        m_restartArcus.Text = "Restart";
        m_restartArcus.UseVisualStyleBackColor = true;
        m_restartArcus.Click += m_restartArcus_Click;
        m_configureArcus.Location = new Point(26, 30);
        m_configureArcus.Name = "m_configureArcus";
        m_configureArcus.Size = new Size(120, 50);
        m_configureArcus.TabIndex = 0;
        m_configureArcus.Text = "Configure";
        m_configureArcus.UseVisualStyleBackColor = true;
        m_configureArcus.Click += m_configureArcus_Click;
        groupBox5.Controls.Add(m_restartTouchscreen);
        groupBox5.Location = new Point(782, 51);
        groupBox5.Name = "groupBox5";
        groupBox5.Size = new Size(200, 117);
        groupBox5.TabIndex = 6;
        groupBox5.TabStop = false;
        groupBox5.Text = "Touchscreen";
        m_restartTouchscreen.Location = new Point(33, 33);
        m_restartTouchscreen.Name = "m_restartTouchscreen";
        m_restartTouchscreen.Size = new Size(120, 50);
        m_restartTouchscreen.TabIndex = 0;
        m_restartTouchscreen.Text = "Reset";
        m_restartTouchscreen.UseVisualStyleBackColor = true;
        m_restartTouchscreen.Click += m_restartTouchscreen_Click;
        m_hwCorrectiontypeGB.Controls.Add(radioButton5);
        m_hwCorrectiontypeGB.Controls.Add(radioButton4);
        m_hwCorrectiontypeGB.Controls.Add(radioButton2);
        m_hwCorrectiontypeGB.Controls.Add(radioButton3);
        m_hwCorrectiontypeGB.Controls.Add(button24);
        m_hwCorrectiontypeGB.Controls.Add(radioButton1);
        m_hwCorrectiontypeGB.Location = new Point(513, 43);
        m_hwCorrectiontypeGB.Name = "m_hwCorrectiontypeGB";
        m_hwCorrectiontypeGB.Size = new Size(200, 217);
        m_hwCorrectiontypeGB.TabIndex = 4;
        m_hwCorrectiontypeGB.TabStop = false;
        m_hwCorrectiontypeGB.Text = "Correction Type";
        radioButton5.AutoSize = true;
        radioButton5.Location = new Point(23, 93);
        radioButton5.Name = "radioButton5";
        radioButton5.Size = new Size(141, 17);
        radioButton5.TabIndex = 6;
        radioButton5.TabStop = true;
        radioButton5.Tag = "UnexpectedPowerLoss";
        radioButton5.Text = "Unexpected Power Loss";
        radioButton5.UseVisualStyleBackColor = true;
        radioButton4.AutoSize = true;
        radioButton4.Location = new Point(23, 70);
        radioButton4.Name = "radioButton4";
        radioButton4.Size = new Size(57, 17);
        radioButton4.TabIndex = 5;
        radioButton4.TabStop = true;
        radioButton4.Tag = "RouterRecycle";
        radioButton4.Text = "Router";
        radioButton4.UseVisualStyleBackColor = true;
        radioButton2.AutoSize = true;
        radioButton2.Location = new Point(23, 116);
        radioButton2.Name = "radioButton2";
        radioButton2.Size = new Size(36, 17);
        radioButton2.TabIndex = 4;
        radioButton2.TabStop = true;
        radioButton2.Tag = "None";
        radioButton2.Text = "All";
        radioButton2.UseVisualStyleBackColor = true;
        radioButton3.AutoSize = true;
        radioButton3.Location = new Point(23, 47);
        radioButton3.Name = "radioButton3";
        radioButton3.Size = new Size(88, 17);
        radioButton3.TabIndex = 2;
        radioButton3.TabStop = true;
        radioButton3.Tag = "Touchscreen";
        radioButton3.Text = "Touchscreen";
        radioButton3.UseVisualStyleBackColor = true;
        button24.Location = new Point(23, 150);
        button24.Name = "button24";
        button24.Size = new Size(120, 50);
        button24.TabIndex = 3;
        button24.Text = "Show Stats";
        button24.UseVisualStyleBackColor = true;
        button24.Click += button24_Click;
        radioButton1.AutoSize = true;
        radioButton1.Location = new Point(23, 24);
        radioButton1.Name = "radioButton1";
        radioButton1.Size = new Size(52, 17);
        radioButton1.TabIndex = 0;
        radioButton1.TabStop = true;
        radioButton1.Tag = "Arcus";
        radioButton1.Text = "Arcus";
        radioButton1.UseVisualStyleBackColor = true;
        button23.Location = new Point(21, 473);
        button23.Name = "button23";
        button23.Size = new Size(120, 57);
        button23.TabIndex = 1;
        button23.Text = "Clear Output";
        button23.UseVisualStyleBackColor = true;
        button23.Click += button23_Click;
        m_hwStatsOutput.FormattingEnabled = true;
        m_hwStatsOutput.Location = new Point(21, 35);
        m_hwStatsOutput.Name = "m_hwStatsOutput";
        m_hwStatsOutput.Size = new Size(422, 420);
        m_hwStatsOutput.TabIndex = 0;
        m_irConfigurationTab.BackColor = Color.LightGray;
        m_irConfigurationTab.Controls.Add(button37);
        m_irConfigurationTab.Controls.Add(groupBox8);
        m_irConfigurationTab.Controls.Add(groupBox7);
        m_irConfigurationTab.Controls.Add(m_irCameraOutput);
        m_irConfigurationTab.Location = new Point(4, 56);
        m_irConfigurationTab.Name = "m_irConfigurationTab";
        m_irConfigurationTab.Size = new Size(1049, 566);
        m_irConfigurationTab.TabIndex = 11;
        m_irConfigurationTab.Text = "IR Camera";
        button37.Location = new Point(352, 64);
        button37.Name = "button37";
        button37.Size = new Size(115, 60);
        button37.TabIndex = 3;
        button37.Text = "button37";
        button37.UseVisualStyleBackColor = true;
        button37.Click += button37_Click_1;
        groupBox8.Controls.Add(textBox2);
        groupBox8.Controls.Add(textBox1);
        groupBox8.Controls.Add(button47);
        groupBox8.Controls.Add(button46);
        groupBox8.Controls.Add(button45);
        groupBox8.Controls.Add(button44);
        groupBox8.Controls.Add(button43);
        groupBox8.Location = new Point(36, 299);
        groupBox8.Name = "groupBox8";
        groupBox8.Size = new Size(348, 205);
        groupBox8.TabIndex = 2;
        groupBox8.TabStop = false;
        groupBox8.Text = "Hardware Control";
        textBox2.Location = new Point(225, 81);
        textBox2.Name = "textBox2";
        textBox2.Size = new Size(100, 20);
        textBox2.TabIndex = 9;
        textBox1.Location = new Point(225, 38);
        textBox1.Name = "textBox1";
        textBox1.Size = new Size(100, 20);
        textBox1.TabIndex = 8;
        button47.Location = new Point(144, 73);
        button47.Name = "button47";
        button47.Size = new Size(75, 40);
        button47.TabIndex = 7;
        button47.Text = "Slot";
        button47.UseVisualStyleBackColor = true;
        button47.Click += button47_Click;
        button46.Location = new Point(144, 27);
        button46.Name = "button46";
        button46.Size = new Size(75, 40);
        button46.TabIndex = 6;
        button46.Text = "Deck";
        button46.UseVisualStyleBackColor = true;
        button46.Click += button46_Click;
        button45.Location = new Point(144, 124);
        button45.Name = "button45";
        button45.Size = new Size(115, 75);
        button45.TabIndex = 5;
        button45.Tag = "CONTROLSYSTEM CENTER";
        button45.Text = "Center disk";
        button45.UseVisualStyleBackColor = true;
        button45.Click += OnExecuteResponseInstruction;
        button44.Location = new Point(12, 124);
        button44.Name = "button44";
        button44.Size = new Size(115, 75);
        button44.TabIndex = 4;
        button44.Text = "Put";
        button44.UseVisualStyleBackColor = true;
        button44.Click += button44_Click;
        button43.Location = new Point(12, 26);
        button43.Name = "button43";
        button43.Size = new Size(115, 75);
        button43.TabIndex = 4;
        button43.Text = "Get and Read";
        button43.UseVisualStyleBackColor = true;
        button43.Click += button43_Click;
        groupBox7.Controls.Add(button42);
        groupBox7.Controls.Add(button41);
        groupBox7.Controls.Add(button40);
        groupBox7.Controls.Add(button39);
        groupBox7.Controls.Add(button38);
        groupBox7.Location = new Point(36, 34);
        groupBox7.Name = "groupBox7";
        groupBox7.Size = new Size(298, 238);
        groupBox7.TabIndex = 1;
        groupBox7.TabStop = false;
        groupBox7.Text = "Camera Control";
        button42.Location = new Point(18, 162);
        button42.Name = "button42";
        button42.Size = new Size(115, 60);
        button42.TabIndex = 4;
        button42.Text = "Launch Tuner";
        button42.UseVisualStyleBackColor = true;
        button42.Click += button42_Click;
        button41.Location = new Point(163, 96);
        button41.Name = "button41";
        button41.Size = new Size(115, 60);
        button41.TabIndex = 4;
        button41.Tag = "CONTROLSYSTEM RINGLIGHTOFF";
        button41.Text = "Ringlight Off";
        button41.UseVisualStyleBackColor = true;
        button41.Click += OnExecuteResponseInstruction;
        button40.Location = new Point(18, 96);
        button40.Name = "button40";
        button40.Size = new Size(115, 60);
        button40.TabIndex = 4;
        button40.Tag = "CAMERA STOP";
        button40.Text = "Stop Camera";
        button40.UseVisualStyleBackColor = true;
        button40.Click += OnExecuteResponseInstruction;
        button39.Location = new Point(163, 30);
        button39.Name = "button39";
        button39.Size = new Size(115, 60);
        button39.TabIndex = 4;
        button39.Tag = "CONTROLSYSTEM RINGLIGHTON";
        button39.Text = "Ringlight On";
        button39.UseVisualStyleBackColor = true;
        button39.Click += OnExecuteResponseInstruction;
        button38.Location = new Point(18, 30);
        button38.Name = "button38";
        button38.Size = new Size(115, 60);
        button38.TabIndex = 4;
        button38.Tag = "CAMERA START";
        button38.Text = "Start Camera";
        button38.UseVisualStyleBackColor = true;
        button38.Click += OnExecuteResponseInstruction;
        m_irCameraOutput.FormattingEnabled = true;
        m_irCameraOutput.Location = new Point(558, 48);
        m_irCameraOutput.Name = "m_irCameraOutput";
        m_irCameraOutput.Size = new Size(440, 420);
        m_irCameraOutput.TabIndex = 0;
        m_engineeringTab.BackColor = Color.LightGray;
        m_engineeringTab.Controls.Add(groupBox3);
        m_engineeringTab.Controls.Add(button28);
        m_engineeringTab.Controls.Add(button27);
        m_engineeringTab.Controls.Add(button26);
        m_engineeringTab.Controls.Add(button25);
        m_engineeringTab.Controls.Add(m_engineeringOutput);
        m_engineeringTab.Location = new Point(4, 56);
        m_engineeringTab.Name = "m_engineeringTab";
        m_engineeringTab.Padding = new Padding(3);
        m_engineeringTab.Size = new Size(1049, 566);
        m_engineeringTab.TabIndex = 10;
        m_engineeringTab.Text = "Engineering";
        groupBox3.Controls.Add(button36);
        groupBox3.Controls.Add(button35);
        groupBox3.Controls.Add(button34);
        groupBox3.Controls.Add(button29);
        groupBox3.Controls.Add(button30);
        groupBox3.Controls.Add(button33);
        groupBox3.Controls.Add(button32);
        groupBox3.Controls.Add(m_engEndSlotTB);
        groupBox3.Controls.Add(button31);
        groupBox3.Controls.Add(m_engEndDeckTB);
        groupBox3.Controls.Add(m_engDeckTB);
        groupBox3.Controls.Add(m_engSlotTB);
        groupBox3.Location = new Point(621, 46);
        groupBox3.Name = "groupBox3";
        groupBox3.Size = new Size(292, 392);
        groupBox3.TabIndex = 14;
        groupBox3.TabStop = false;
        groupBox3.Text = "Locations";
        button36.Location = new Point(161, 293);
        button36.Name = "button36";
        button36.Size = new Size(110, 60);
        button36.TabIndex = 18;
        button36.Text = "Put to";
        button36.UseVisualStyleBackColor = true;
        button36.Click += button36_Click;
        button35.Location = new Point(18, 293);
        button35.Name = "button35";
        button35.Size = new Size(110, 60);
        button35.TabIndex = 17;
        button35.Text = "Get from";
        button35.UseVisualStyleBackColor = true;
        button35.Click += button35_Click;
        button34.Location = new Point(171, 26);
        button34.Name = "button34";
        button34.Size = new Size(100, 40);
        button34.TabIndex = 16;
        button34.Text = "Start slot";
        button34.UseVisualStyleBackColor = true;
        button34.Click += button34_Click;
        button29.Location = new Point(161, 216);
        button29.Name = "button29";
        button29.Size = new Size(110, 60);
        button29.TabIndex = 9;
        button29.Text = "Sync slots";
        button29.UseVisualStyleBackColor = true;
        button29.Click += button29_Click;
        button30.Location = new Point(18, 216);
        button30.Name = "button30";
        button30.Size = new Size(110, 60);
        button30.TabIndex = 10;
        button30.Text = "Sync slot";
        button30.UseVisualStyleBackColor = true;
        button30.Click += button30_Click;
        button33.Location = new Point(18, 26);
        button33.Name = "button33";
        button33.Size = new Size(100, 40);
        button33.TabIndex = 15;
        button33.Text = "Start deck";
        button33.UseVisualStyleBackColor = true;
        button33.Click += button33_Click;
        button32.Location = new Point(171, 121);
        button32.Name = "button32";
        button32.Size = new Size(100, 40);
        button32.TabIndex = 14;
        button32.Text = "End slot";
        button32.UseVisualStyleBackColor = true;
        button32.Click += button32_Click;
        m_engEndSlotTB.Location = new Point(171, 167);
        m_engEndSlotTB.Name = "m_engEndSlotTB";
        m_engEndSlotTB.Size = new Size(100, 20);
        m_engEndSlotTB.TabIndex = 12;
        button31.Location = new Point(18, 121);
        button31.Name = "button31";
        button31.Size = new Size(100, 40);
        button31.TabIndex = 13;
        button31.Text = "End deck";
        button31.UseVisualStyleBackColor = true;
        button31.Click += button31_Click;
        m_engEndDeckTB.Location = new Point(18, 167);
        m_engEndDeckTB.Name = "m_engEndDeckTB";
        m_engEndDeckTB.Size = new Size(100, 20);
        m_engEndDeckTB.TabIndex = 11;
        m_engDeckTB.Location = new Point(18, 72);
        m_engDeckTB.Name = "m_engDeckTB";
        m_engDeckTB.Size = new Size(100, 20);
        m_engDeckTB.TabIndex = 5;
        m_engSlotTB.Location = new Point(171, 72);
        m_engSlotTB.Name = "m_engSlotTB";
        m_engSlotTB.Size = new Size(100, 20);
        m_engSlotTB.TabIndex = 6;
        button28.Location = new Point(441, 220);
        button28.Name = "button28";
        button28.Size = new Size(120, 61);
        button28.TabIndex = 4;
        button28.Text = "Read barcode";
        button28.UseVisualStyleBackColor = true;
        button28.Click += button28_Click;
        button27.Location = new Point(441, 133);
        button27.Name = "button27";
        button27.Size = new Size(120, 65);
        button27.TabIndex = 3;
        button27.Text = "Vend disk in picker";
        button27.UseVisualStyleBackColor = true;
        button27.Click += button27_Click;
        button26.Location = new Point(441, 46);
        button26.Name = "button26";
        button26.Size = new Size(120, 68);
        button26.TabIndex = 2;
        button26.Text = "Take Disk at door";
        button26.UseVisualStyleBackColor = true;
        button26.Click += button26_Click;
        button25.Location = new Point(21, 455);
        button25.Name = "button25";
        button25.Size = new Size(120, 58);
        button25.TabIndex = 1;
        button25.Text = "Clear Output";
        button25.UseVisualStyleBackColor = true;
        button25.Click += button25_Click;
        m_engineeringOutput.FormattingEnabled = true;
        m_engineeringOutput.Location = new Point(21, 31);
        m_engineeringOutput.Name = "m_engineeringOutput";
        m_engineeringOutput.Size = new Size(392, 407);
        m_engineeringOutput.TabIndex = 0;
        m_exitButton.BackColor = Color.GreenYellow;
        m_exitButton.Location = new Point(466, 644);
        m_exitButton.Name = "m_exitButton";
        m_exitButton.Size = new Size(135, 59);
        m_exitButton.TabIndex = 14;
        m_exitButton.Text = "Exit";
        m_exitButton.UseVisualStyleBackColor = false;
        m_exitButton.Click += m_exitButton_Click;
        errorProvider1.ContainerControl = this;
        m_verticalSyncWorker.DoWork += m_verticalSyncWorker_DoWork;
        m_verticalSyncWorker.RunWorkerCompleted += m_verticalSyncWorker_RunWorkerCompleted;
        m_runInitWorker.DoWork += m_runInitWorker_DoWork;
        m_runInitWorker.RunWorkerCompleted += m_runInitWorker_RunWorkerCompleted;
        m_runDecodeWorker.DoWork += m_runDecodeWorker_DoWork;
        m_runDecodeWorker.RunWorkerCompleted += m_runDecodeWorker_RunWorkerCompleted;
        AutoScaleDimensions = new SizeF(6f, 13f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1116, 715);
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
        m_hwCorrections.ResumeLayout(false);
        groupBox4.ResumeLayout(false);
        groupBox6.ResumeLayout(false);
        groupBox5.ResumeLayout(false);
        m_hwCorrectiontypeGB.ResumeLayout(false);
        m_hwCorrectiontypeGB.PerformLayout();
        m_irConfigurationTab.ResumeLayout(false);
        groupBox8.ResumeLayout(false);
        groupBox8.PerformLayout();
        groupBox7.ResumeLayout(false);
        m_engineeringTab.ResumeLayout(false);
        groupBox3.ResumeLayout(false);
        groupBox3.PerformLayout();
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

        protected override void SetupJob()
        {
            Job.Push(IterationPause);
            Job.Push(ScanPause);
            Job.Push(Iterations);
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

        protected override void SetupJob()
        {
            Job.Push(StartupPause);
        }
    }
}