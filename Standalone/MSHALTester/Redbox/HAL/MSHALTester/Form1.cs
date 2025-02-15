using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using log4net;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Client.Services;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Services;
using Redbox.HAL.Core;
using Redbox.HAL.MSHALTester.Properties;
using Redbox.IPC.Framework;

namespace Redbox.HAL.MSHALTester;

public class Form1 : Form, IDisposable, IClientOutputSink, ILogger
{
    private const int MaxMoveXUnits = 10;
    private const int MaxMoveYUnits = 200;
    private readonly AirExchangerState AirExchangerState;
    private readonly CameraAdapter CameraHelper;
    private readonly Button[] FormButtons;
    private readonly HardwareService HardwareService;
    private readonly ClientHelper Helper;
    private readonly ILog m_log;
    private readonly bool m_secure;
    private readonly List<HardwareJob> m_stoppedJobs = new();
    private readonly ButtonAspectsManager Manager;
    private readonly MoveHelper MoveHelper;
    private readonly OutputBox OutputBox;
    private readonly string Version;
    private Button button1;
    private Button button10;
    private Button button11;
    private Button button12;
    private Button button13;
    private Button button14;
    private Button button16;
    private Button button17;
    private Button button18;
    private Button button19;
    private Button button2;
    private Button button20;
    private Button button22;
    private Button button26;
    private Button button28;
    private Button button29;
    private Button button3;
    private Button button30;
    private Button button31;
    private Button button32;
    private Button button34;
    private Button button35;
    private Button button36;
    private Button button37;
    private Button button38;
    private Button button39;
    private Button button4;
    private Button button44;
    private Button button47;
    private Button button5;
    private Button button50;
    private Button button51;
    private Button button53;
    private Button button54;
    private Button button57;
    private Button button60;
    private Button button61;
    private Button button62;
    private Button button63;
    private Button button64;
    private Button button66;
    private Button button7;
    private Button button8;
    private IContainer components;
    private bool DoorSensorsConfigured;
    private bool FormDisposed;
    private GroupBox groupBox1;
    private GroupBox groupBox10;
    private GroupBox groupBox11;
    private GroupBox groupBox12;
    private GroupBox groupBox2;
    private GroupBox groupBox3;
    private GroupBox groupBox4;
    private GroupBox groupBox5;
    private GroupBox groupBox6;
    private GroupBox groupBox7;
    private GroupBox groupBox8;
    private ImageViewer ImageViewer;
    private Label label10;
    private Label label11;
    private Label label14;
    private Label label15;
    private Label label16;
    private Label label17;
    private Label label19;
    private Label label20;
    private Button m_cameraPreview;
    private Button m_cameraWorkingButton;
    private Button m_clearOutputButton;
    private Button m_closeSerialButton;
    private Button m_configureDevicesButton;
    private Button m_deckConfigurationButton;
    private TextBox m_destinationDeckText;
    private TextBox m_destinationSlotText;
    private TextBox m_encoderUnitsTextBox;
    private ErrorProvider m_errorProvider;
    private Button m_fanButton;
    private TextBox m_getOffsetTB;
    private RadioButton m_getOffXRadio;
    private RadioButton m_getOffYRadio;
    private Button m_getWithOffset;
    private Button m_gripperSellButton;
    private Button m_homeXButton;
    private Button m_homeYButton;
    private BackgroundWorker m_initWorker;
    private TextBox m_kioskIDTextBox;
    private RadioButton m_offsetGetRadioButton;
    private Button m_openLogsButton;
    private ListBox m_outputBox;
    private Button m_qlmCaseStatusButton;
    private Button m_qlmDisengageButton;
    private Button m_qlmDoorStatus;
    private Button m_qlmDownButton;
    private Button m_qlmEngageButton;
    private GroupBox m_qlmGroupBox;
    private Button m_qlmStopButton;
    private Button m_qlmTestButton;
    private Button m_qlmUpButton;
    private Button m_readLimitsButton;
    private Button m_readPosButton;
    private Button m_resetProteusButton;
    private TextBox m_rollerTimeoutTextBox;
    private TextBox m_rollToSensorText;
    private Button m_runInitButton;
    private CheckBox m_sensorCheckBox;
    private Button m_showTimeoutLogButton;
    private Button m_snapImageButton;
    private TextBox m_sourceDeckTextBox;
    private TextBox m_sourceSlotTextBox;
    private Button m_startButtonCamera;
    private BackgroundWorker m_startupWorker;
    private Button m_takeDiskButton;
    private Button m_testArcusCommButton;
    private Button m_updateConfigurationButton;
    private TextBox m_versionTextBox;
    private bool RinglightOn;
    private bool VMZConfigured;

    public Form1(bool secure, string user)
    {
        InitializeComponent();
        Version = typeof(Form1).Assembly.GetName().Version.ToString();
        var user1 = new TesterSession(user);
        ServiceLocator.Instance.AddService<ISessionUserService>(new TesterSessionImplemtation(user1));
        ServiceLocator.Instance.AddService<IDeviceSetupClassFactory>(new DeviceSetupClassFactory());
        ServiceLocator.Instance.AddService<IUsbDeviceService>(new UsbDeviceService(Settings.Default.UsbServiceDebug));
        OutputBox = new TesterOutputBox(m_outputBox, this);
        m_log = LogManager.GetLogger("TesterLog");
        m_secure = secure;
        FormButtons = new Button[63]
        {
            m_clearOutputButton,
            m_readPosButton,
            m_runInitButton,
            m_homeXButton,
            m_homeYButton,
            m_resetProteusButton,
            m_testArcusCommButton,
            button57,
            button5,
            button47,
            button50,
            button51,
            button53,
            button54,
            m_qlmTestButton,
            button22,
            button66,
            button20,
            m_showTimeoutLogButton,
            m_readLimitsButton,
            button36,
            button37,
            button38,
            button39,
            button64,
            button31,
            button32,
            m_gripperSellButton,
            button34,
            button35,
            button28,
            button29,
            button30,
            m_qlmEngageButton,
            m_qlmDisengageButton,
            m_qlmStopButton,
            m_qlmCaseStatusButton,
            m_qlmUpButton,
            m_qlmDownButton,
            m_qlmDoorStatus,
            m_snapImageButton,
            m_startButtonCamera,
            m_cameraPreview,
            m_cameraWorkingButton,
            button19,
            button26,
            button7,
            button8,
            button10,
            button11,
            button12,
            button13,
            button16,
            button14,
            button17,
            button18,
            button1,
            button2,
            button3,
            button4,
            m_closeSerialButton,
            m_getWithOffset,
            m_takeDiskButton
        };
        HardwareService = new HardwareService(IPCProtocol.Parse(Settings.Default.CommunicationURL));
        Helper = new ClientHelper(this, HardwareService);
        AirExchangerState = new AirExchangerState(HardwareService);
        CameraHelper = new CameraAdapter(HardwareService);
        MoveHelper = new MoveHelper(HardwareService);
        ServiceLocator.Instance.AddService<IControlSystem>(new ClientControlSystem(HardwareService));
        if (Settings.Default.TestCommOnStartup && !Helper.TestCommunication())
        {
            OutputBox.Write("Failed to communicate with HAL.");
            LogHelper.Instance.Log("Unable to contact service on {0}", Settings.Default.CommunicationURL);
        }
        else
        {
            m_offsetGetRadioButton = m_getOffXRadio;
            m_offsetGetRadioButton.Checked = true;
            ServiceLocator.Instance.AddService<IRuntimeService>(new RuntimeService());
            ServiceLocator.Instance.AddService<ILogger>(this);
            LogHelper.Instance.Log("Tester startup; form {0} in secure mode, user = {1}.",
                m_secure ? "is" : (object)"isn't", user1.User);
            FormClosing += OnFormClosing;
            var pickerSensorsBar = new PickerSensorsBar(HardwareService);
            pickerSensorsBar.Dock = DockStyle.Bottom;
            groupBox5.Controls.Add(pickerSensorsBar);
            pickerSensorsBar.ReadEvents += OnSensorReadDone;
            pickerSensorsBar.BarEvents += OnSensorOperation;
            Manager = new ButtonAspectsManager();
            m_versionTextBox.Text = Version;
            var id = "UNKNOWN";
            if (!HardwareService.GetKioskID(out id).Success)
                id = "UNKNOWN";
            m_kioskIDTextBox.Text = id;
            m_startupWorker.RunWorkerAsync();
        }
    }

    private int SourceDeck => m_sourceDeckTextBox.GetInteger("Source Deck", OutputBox);

    private int SourceSlot => m_sourceSlotTextBox.GetInteger("Source Slot", OutputBox);

    public void WriteMessage(string msg)
    {
        OutputBox.Write(msg);
    }

    public void WriteMessage(string fmt, params object[] stuff)
    {
        OutputBox.Write(fmt, stuff);
    }

    public void Log(string message, Exception e)
    {
        m_log.Error(message, e);
    }

    public void Log(string message, LogEntryType type)
    {
        switch (type)
        {
            case LogEntryType.Info:
                m_log.Info(message);
                break;
            case LogEntryType.Debug:
                m_log.Debug(message);
                break;
            case LogEntryType.Error:
                m_log.Error(message);
                break;
            case LogEntryType.Fatal:
                m_log.Fatal(message);
                break;
        }
    }

    public void Log(string message, Exception e, LogEntryType type)
    {
        switch (type)
        {
            case LogEntryType.Info:
                m_log.Info(message, e);
                break;
            case LogEntryType.Debug:
                m_log.Debug(message, e);
                break;
            case LogEntryType.Error:
                m_log.Error(message, e);
                break;
            case LogEntryType.Fatal:
                m_log.Fatal(message, e);
                break;
        }
    }

    public bool IsLevelEnabled(LogEntryType entryLogLevel)
    {
        switch (entryLogLevel)
        {
            case LogEntryType.Info:
                return m_log.IsInfoEnabled;
            case LogEntryType.Debug:
                return m_log.IsDebugEnabled;
            case LogEntryType.Error:
                return m_log.IsErrorEnabled;
            case LogEntryType.Fatal:
                return m_log.IsFatalEnabled;
            default:
                return false;
        }
    }

    private void OnSensorReadDone(bool readError)
    {
        OutputBox.Write(readError ? "Sensor Read Failure: PCB Not Responsive" : "Sensors read.");
    }

    private void OnSensorOperation(string msg, IControlResponse response)
    {
        var str = !response.CommError
            ? response.TimedOut ? ErrorCodes.Timeout.ToString().ToUpper() : ErrorCodes.Success.ToString().ToUpper()
            : response.Diagnostic;
        OutputBox.Write(string.Format("{0}: {1}", msg, str));
    }

    private void ChangeUIAfterconfig()
    {
        using (var machineConfiguration = new MachineConfiguration(HardwareService))
        {
            machineConfiguration.Run();
            VMZConfigured = machineConfiguration.VMZConfigured;
            DoorSensorsConfigured = machineConfiguration.DoorSensorsConfigured;
            if (VMZConfigured)
                OutputBox.Write("The kiosk is configured for a VMZ.");
            if (DoorSensorsConfigured)
            {
                OutputBox.Write("The kiosk is configured for door sensors.");
                if (machineConfiguration.DoorSensorStatus != "OK")
                    OutputBox.Write(string.Format("WARNING: DoorSensor status returned {0} (door may be open!)",
                        machineConfiguration.DoorSensorStatus));
            }
            else
            {
                OutputBox.Write("Door sensor configuration shows: NOT CONFIGURED.");
            }

            if ((machineConfiguration.QuickReturnStatus & DeviceStatus.Found) != DeviceStatus.None)
                OutputBox.Write("A Quick Return device is attached; it {0} configured.",
                    (machineConfiguration.QuickReturnStatus & DeviceStatus.Configured) != DeviceStatus.None
                        ? "is"
                        : (object)"is not");
            if (machineConfiguration.HasFraudDevice)
            {
                m_gripperSellButton.Enabled = false;
                OutputBox.Write("The kiosk is configured with a fraud sensor.");
            }

            CameraHelper.Reset(machineConfiguration.LegacyCamera);
            if (!CameraHelper.LegacyCamera)
            {
                m_cameraPreview.Enabled = false;
                m_startButtonCamera.Enabled = true;
                ToggleNonLegacyStart();
            }

            ChangeQlmPanelState();
            SetFanButtonState(AirExchangerState.Configure(machineConfiguration.AirExchangerStatus,
                machineConfiguration.AirExchangerFanStatus));
            m_cameraWorkingButton.Enabled = CameraHelper.CameraInError();
        }
    }

    private void ToggleNonLegacyStart()
    {
        var cameraStatus = CameraHelper.GetCameraStatus();
        if (cameraStatus == CameraState.Unknown)
            OutputBox.Write("Could not find current camera status.");
        else
            m_startButtonCamera.Text = CameraState.Stopped == cameraStatus ? "Start Camera" : "Stop Camera";
    }

    private void DisplayInventoryStats()
    {
        using (var inventoryStatsJob = new InventoryStatsJob(HardwareService))
        {
            inventoryStatsJob.Run();
            inventoryStatsJob.Results.ForEach(result =>
            {
                if (result.Code == "TotalEmptyCount")
                {
                    OutputBox.Write("  EMPTY slots: {0}", result.Message);
                }
                else if (result.Code == "UnknownCount")
                {
                    OutputBox.Write("  UNKNOWN slots: {0}", result.Message);
                }
                else
                {
                    if (!(result.Code == "InventoryStoreError"))
                        return;
                    OutputBox.Write(result.Message);
                }
            });
            OutputBox.Write("Inventory Stats: ");
        }
    }

    private void CompileScripts()
    {
        var path = ServiceLocator.Instance.GetService<IRuntimeService>().RuntimePath("Scripts");
        if (!Directory.Exists(path))
            return;
        foreach (var file in Directory.GetFiles(path, "*.hs"))
        {
            var withoutExtension = Path.GetFileNameWithoutExtension(file);
            if (!HardwareService.CompileProgram(file, withoutExtension, false).Success)
                LogHelper.Instance.Log("Failed to compile script {0} ", withoutExtension);
        }
    }

    private void OnDropQLM(object sender, EventArgs args)
    {
        using (var aspect = Manager.MakeAspect(sender))
        {
            using (var instructionHelper = new InstructionHelper(HardwareService))
            {
                var str = instructionHelper.ExecuteGeneric("HOMEY CLEARGRIPPER=TRUE");
                if (string.IsNullOrEmpty(str))
                    return;
                if (str != "SUCCESS")
                    OutputBox.Write(string.Format("The HOMEY function failed with error {0}", str));
                else
                    OnExecuteInstructionBlind(aspect);
            }
        }
    }

    private void OnDoSnap(object sender, EventArgs args)
    {
        using (Manager.MakeAspect(sender))
        {
            var snapResult = CameraHelper.Snap();
            if (snapResult == null)
            {
                OutputBox.Write("Command error.");
            }
            else if (snapResult.SnapOk)
            {
                if (ImageViewer == null)
                {
                    ImageViewer = new ImageViewer();
                    ImageViewer.FormClosing += OnImageViewClosing;
                }
                else
                {
                    ImageViewer.Hide();
                }

                ImageViewer.DisplayFile(snapResult.Path);
                ImageViewer.Show();
            }
            else
            {
                OutputBox.Write("CAMERA CAPTURE error. I'm sorry!");
            }
        }
    }

    private void OnImageViewClosing(object sender, FormClosingEventArgs args)
    {
        ImageViewer = null;
    }

    private void OnStartCamera(object sender, EventArgs args)
    {
        using (var buttonAspects = Manager.MakeAspect(sender))
        {
            if (CameraHelper.LegacyCamera)
            {
                var on = !RinglightOn;
                if (!ServiceLocator.Instance.GetService<IControlSystem>().ToggleRingLight(on, new int?()).Success)
                    return;
                RinglightOn = on;
                buttonAspects.Button.Text = RinglightOn ? "Turn Ringlight Off" : "Turn Ringlight On";
            }
            else
            {
                var cameraStatus = CameraHelper.GetCameraStatus();
                if (cameraStatus == CameraState.Unknown)
                    return;
                var cameraState = CameraHelper.ToggleState();
                if (cameraState != cameraStatus)
                {
                    OutputBox.Write(string.Format("Changed the camera state to {0}.", cameraState.ToString()));
                    ToggleNonLegacyStart();
                }
                else
                {
                    OutputBox.Write(string.Format("Couldn't change camera state to {0}.I'm sorry!",
                        cameraState.ToString()));
                }
            }
        }
    }

    private void OnExecuteInstructionBlind(object sender, EventArgs e)
    {
        using (var aspect = Manager.MakeAspect(sender))
        {
            OnExecuteInstructionBlind(aspect);
        }
    }

    private void OnExecuteInstructionBlind(ButtonAspects aspect)
    {
        var tagInstruction = aspect.GetTagInstruction();
        if (!string.IsNullOrEmpty(tagInstruction))
        {
            var result = HardwareService.ExecuteImmediate(tagInstruction, out var _);
            result.Dump();
            if (result.Success)
                OutputBox.Write("Sent instruction " + tagInstruction);
            else
                OutputBox.Write("Sending instruction " + tagInstruction + " failed.");
        }
        else
        {
            OutputBox.Write("{0} failed.", tagInstruction);
        }
    }

    private void OnExecuteErrorCodeInstruction(object sender, EventArgs e)
    {
        using (var aspect = Manager.MakeAspect(sender))
        {
            OnExecuteErrorCodeInstruction(aspect);
        }
    }

    private void OnExecuteErrorCodeInstruction(ButtonAspects aspect)
    {
        var tagInstruction = aspect.GetTagInstruction();
        using (var instructionHelper = new InstructionHelper(HardwareService))
        {
            OutputBox.Write("{0} - {1}", tagInstruction, instructionHelper.ExecuteErrorCode(tagInstruction));
        }
    }

    private void OnExecuteResponseInstruction(object sender, EventArgs e)
    {
        using (var aspect = Manager.MakeAspect(sender))
        {
            OnExecuteResponseInstruction(aspect);
        }
    }

    private void OnExecuteResponseInstruction(ButtonAspects aspect)
    {
        var tagInstruction = aspect.GetTagInstruction();
        using (var instructionHelper = new InstructionHelper(HardwareService))
        {
            LogResponse(instructionHelper.ExecuteWithResponse(tagInstruction), tagInstruction);
        }
    }

    private void OnExecuteInstructionRaw(object s, EventArgs e)
    {
        using (var buttonAspects = Manager.MakeAspect(s))
        {
            var tagInstruction = buttonAspects.GetTagInstruction();
            using (var instructionHelper = new InstructionHelper(HardwareService))
            {
                var str = instructionHelper.ExecuteGeneric(tagInstruction);
                if (string.IsNullOrEmpty(str))
                    str = ErrorCodes.ServiceChannelError.ToString().ToUpper();
                OutputBox.Write("{0} - {1}", tagInstruction, str);
            }
        }
    }

    private void LogResponse(IControlResponse response, string tag)
    {
        var fmt = "{0} - {1}";
        if (response.CommError)
            OutputBox.Write(fmt, tag, response.Diagnostic);
        else if (response.TimedOut)
            OutputBox.Write(fmt, tag, ErrorCodes.Timeout.ToString().ToUpper());
        else
            OutputBox.Write(fmt, tag, ErrorCodes.Success.ToString().ToUpper());
    }

    private void OnRunQlmDisengage(object sender, EventArgs e)
    {
        using (var aspect = Manager.MakeAspect(sender))
        {
            if (!VMZConfigured)
            {
                OnExecuteResponseInstruction(aspect);
            }
            else
            {
                if (new ConfirmationDialog().ShowDialog() != DialogResult.OK)
                    return;
                using (var testRetrofitDeck = new TestRetrofitDeck(HardwareService))
                {
                    testRetrofitDeck.Run();
                    if (HardwareJobStatus.Completed == testRetrofitDeck.EndStatus)
                        OutputBox.Write("Test succeeded.");
                    else
                        OutputBox.Write("The test failed.");
                }
            }
        }
    }

    private void OnTestBoards(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            OutputBox.Write("Test boards:");
            if (TestBoards())
            {
                OutputBox.Write("Board test ok.");
            }
            else
            {
                OutputBox.Write(" One or more boards not responsive - issue RESET.");
                using (var controlSystemJob = new ResetControlSystemJob(HardwareService))
                {
                    controlSystemJob.Run();
                    foreach (var result in controlSystemJob.Results)
                        OutputBox.Write(result.Message);
                    if (controlSystemJob.EndStatus != HardwareJobStatus.Completed)
                        return;
                    TestBoards();
                }
            }
        }
    }

    private bool TestBoards()
    {
        using (var boardTestJob = new BoardTestJob(HardwareService))
        {
            boardTestJob.Run();
            foreach (var result in boardTestJob.Results)
                if (result.Code == "VersionResult")
                    OutputBox.Write(result.Message);
            return boardTestJob.EndStatus == HardwareJobStatus.Completed;
        }
    }

    private Direction DirectionFromButton(object sender)
    {
        var direction = Direction.Unknown;
        if (!(sender is Button button))
            return direction;
        if (!(button.Tag is string tag))
            return direction;
        try
        {
            return (Direction)Enum.Parse(typeof(Direction), tag, true);
        }
        catch (Exception ex)
        {
            return Direction.Unknown;
        }
    }

    private void OnDirectionMove(object sender, EventArgs args)
    {
        using (Manager.MakeAspect(sender))
        {
            var direction = DirectionFromButton(sender);
            if (direction == Direction.Unknown)
                return;
            var flag = false;
            if (m_secure && m_sensorCheckBox.Checked)
            {
                flag = true;
                m_sensorCheckBox.Checked = false;
            }

            m_errorProvider.SetError(m_encoderUnitsTextBox, string.Empty);
            Application.DoEvents();
            var text = m_encoderUnitsTextBox.Text;
            var num1 = 0;
            ref var local = ref num1;
            if (int.TryParse(text, out local))
            {
                if (direction == Direction.Left || direction == Direction.Right)
                {
                    if (Math.Abs(num1) > 10)
                    {
                        m_errorProvider.SetError(m_encoderUnitsTextBox, string.Format("Cannot exceed {0} units.", 10));
                        OutputBox.Write("ERROR: move cannot exceed {0} units on X axis.", 10);
                        Application.DoEvents();
                        return;
                    }
                }
                else if ((direction == Direction.Up || direction == Direction.Down) && Math.Abs(num1) > 200)
                {
                    m_errorProvider.SetError(m_encoderUnitsTextBox, string.Format("Cannot exceed {0} units.", 200));
                    OutputBox.Write("ERROR: move cannot exceed {0} units on Y axis.", 200);
                    Application.DoEvents();
                    return;
                }

                if (direction == Direction.Left || direction == Direction.Down)
                {
                    num1 = -num1;
                    OutputBox.Write(string.Format("{0} moving {1} eu", direction.ToString(), num1));
                }

                var position = MoveHelper.GetPosition();
                if (!position.ReadOk)
                {
                    OutputBox.Write("Unable to query motor positions.");
                }
                else
                {
                    var num2 = position.XCoordinate.Value;
                    var num3 = position.YCoordinate.Value;
                    var axis = direction == Direction.Up || direction == Direction.Down ? Axis.Y : Axis.X;
                    var units = Axis.Y == axis ? num3 + num1 : num2 + num1;
                    OutputBox.Write(MoveHelper.MoveAbs(axis, units, !(m_secure & flag)).ToString().ToUpper());
                }
            }
            else
            {
                m_errorProvider.SetError(m_encoderUnitsTextBox, "Must specify an integer!");
            }
        }
    }

    private void OnReadPositions(object sender, EventArgs ea)
    {
        using (Manager.MakeAspect(sender))
        {
            using (var controlDataExecutor = new MotionControlDataExecutor(HardwareService))
            {
                controlDataExecutor.Run();
                if (HardwareJobStatus.Errored == controlDataExecutor.EndStatus)
                {
                    OutputBox.Write("Unable to run position job.");
                }
                else
                {
                    OutputBox.Write(controlDataExecutor.CurrentLocation);
                    if (controlDataExecutor.Position.ReadOk)
                        OutputBox.Write(string.Format("X = {0}, Y = {1}",
                            controlDataExecutor.Position.XCoordinate.Value,
                            controlDataExecutor.Position.YCoordinate.Value));
                    else
                        OutputBox.Write("Unable to obtain position information.");
                }
            }
        }
    }

    private void StopJobs()
    {
        var num = 0;
        HardwareJob[] jobs;
        if (!HardwareService.GetJobs(out jobs).Success)
        {
            OutputBox.Write("Unable to read job list - JOBS MAY BE ACTIVE!");
        }
        else
        {
            foreach (var hardwareJob in jobs)
                if ((hardwareJob.Status == HardwareJobStatus.Pending ||
                     hardwareJob.Status == HardwareJobStatus.Running) &&
                    !hardwareJob.ProgramName.Equals("job-watchdog"))
                    ++num;
            if (num > 0)
                OutputBox.Write("WARNING: jobs are currently running or pending.");
            else
                OutputBox.Write("There are no jobs running or pending.");
            HardwareService.ExecuteServiceCommand("SERVICE diagnostic-mode status: true");
        }
    }

    private void OnRollerToPosition(object sender, EventArgs e)
    {
        m_errorProvider.Clear();
        using (Manager.MakeAspect(sender))
        {
            if (string.IsNullOrEmpty(m_rollToSensorText.Text))
                return;
            var integer1 = m_rollToSensorText.GetInteger("Roller Position", OutputBox);
            if (-1 == integer1)
            {
                OutputBox.Write("Invalid sensor specified for position.");
                m_errorProvider.SetError(m_rollToSensorText, "Invalid position.");
            }
            else
            {
                var integer2 = m_rollerTimeoutTextBox.GetInteger("Roller Timeout", OutputBox);
                var num = integer2 != -1 ? integer2 : 5000;
                var str = string.Format("ROLLER POS={0} TIMEOUT={1} WAIT=TRUE", integer1, num);
                using (var instructionHelper = new InstructionHelper(HardwareService))
                {
                    LogResponse(instructionHelper.ExecuteWithResponse(str), str);
                }
            }
        }
    }

    private void OnReadDiskInPicker(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var scanResult = ScanResult.ReadBarcodeOfDiskInPicker(HardwareService);
            OutputBox.Write(string.Format("Found {0} secure codes.", scanResult.SecureCount));
            OutputBox.Write(scanResult.ToString());
        }
    }

    private void OnLaunchCameraProperties(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            TunerLaunchService.LaunchTunerAndWait(HardwareService, Manager);
        }
    }

    private void OnFormClosing(object sender, FormClosingEventArgs e)
    {
        CloseForm();
    }

    private void OnCloseForm(object sender, EventArgs e)
    {
        CloseForm();
    }

    private void CloseForm()
    {
        if (FormDisposed)
            return;
        FormDisposed = true;
        if (ImageViewer != null)
            ImageViewer.Dispose();
        OutputBox.Write("Cleanup Hardware.");
        HardwareService.ExecuteServiceCommand("SERVICE diagnostic-mode status: false");
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(" AIRXCHGR FANON");
        stringBuilder.AppendLine(" VENDDOOR CLOSE");
        stringBuilder.AppendLine(" GRIPPER RENT");
        stringBuilder.AppendLine(" GRIPPER RETRACT");
        stringBuilder.AppendLine(" SENSOR PICKER-OFF");
        stringBuilder.AppendLine(" ROLLER STOP");
        stringBuilder.AppendLine(" RINGLIGHT OFF");
        stringBuilder.AppendLine(" CLEAR");
        HardwareService.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
        OutputBox.Write("Restart suspended jobs.");
        m_stoppedJobs.ForEach(each => each.Pend());
        Application.Exit();
    }

    private void OnRunInit(object sender, EventArgs e)
    {
        m_runInitButton.BackColor = Color.Red;
        Array.ForEach(FormButtons, button => button.Enabled = false);
        m_initWorker.RunWorkerAsync();
    }

    private void OnClearOutput(object sender, EventArgs e)
    {
        OutputBox.Clear();
    }

    private void OnGotoSlot(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var sourceDeck = SourceDeck;
            var sourceSlot = SourceSlot;
            if (-1 == sourceDeck || -1 == sourceSlot)
                OutputBox.Write(Resources.SourceDeckSlotInvalid);
            else
                using (var moveToSlotExecutor = new TesterMoveToSlotExecutor(HardwareService, sourceDeck, sourceSlot))
                {
                    moveToSlotExecutor.Run();
                    moveToSlotExecutor.Results.ForEach(pr => OutputBox.Write(pr.Message));
                }
        }
    }

    private void OnSyncSlot(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var sourceDeck = SourceDeck;
            var sourceSlot = SourceSlot;
            if (-1 == sourceDeck || -1 == sourceSlot)
            {
                OutputBox.Write(Resources.SourceDeckSlotInvalid);
            }
            else
            {
                var locationList = new List<Location>
                {
                    new() { Deck = sourceDeck, Slot = sourceSlot }
                };
                var hardwareService = HardwareService;
                var locations = locationList;
                var schedule = new HardwareJobSchedule();
                schedule.Priority = HardwareJobPriority.Highest;
                HardwareJob hardwareJob;
                if (!hardwareService.HardSync(locations, "Tester Location Sync", schedule, out hardwareJob).Success)
                {
                    OutputBox.Write("Unable to communicate with HAL.");
                }
                else
                {
                    hardwareJob.Pend();
                    hardwareJob.WaitForCompletion(300000);
                    IDictionary<string, string> symbols;
                    if (!hardwareJob.GetSymbols(out symbols).Success)
                        return;
                    foreach (var key in symbols.Keys)
                        if (key.StartsWith("MSTESTER-SYMBOL"))
                            OutputBox.Write(symbols[key]);
                }
            }
        }
    }

    private void OnPut(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var sourceDeck = SourceDeck;
            var sourceSlot = SourceSlot;
            if (-1 == sourceDeck || -1 == sourceSlot)
                OutputBox.Write(Resources.SourceDeckSlotInvalid);
            else
                CompositeFunctions.PutItem(HardwareService, sourceDeck, sourceSlot, OutputBox);
        }
    }

    private void OnGetAndRead(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            GetDVD(sender);
        }
    }

    private void OnTransfer(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var sourceDeck = SourceDeck;
            var sourceSlot = SourceSlot;
            if (-1 == sourceDeck || -1 == sourceSlot)
            {
                OutputBox.Write(Resources.SourceDeckSlotInvalid);
            }
            else
            {
                var integer1 = m_destinationDeckText.GetInteger("Destination Deck", OutputBox);
                var integer2 = m_destinationSlotText.GetInteger("Destination Slot", OutputBox);
                if (-1 == integer1 || -1 == integer2)
                {
                    OutputBox.Write(Resources.DestDeckSlotInvalid);
                }
                else
                {
                    HardwareJob job;
                    var result = HardwareService.ExecuteImmediate("GRIPPER  STATUS", out job);
                    if (!result.Success)
                    {
                        result.Dump();
                        LogHelper.Instance.Log("Service call failure.");
                    }
                    else if (job.GetTopOfStack().Equals("FULL"))
                    {
                        OutputBox.Write("The gripper is full; no transfer. Sorry! Have a great day!");
                    }
                    else
                    {
                        OutputBox.Write("Transfer " + new TransferDisk(HardwareService).Transfer(
                            new TransferLocation(sourceDeck, sourceSlot), new TransferLocation(integer1, integer2)));
                        m_destinationDeckText.Text = string.Empty;
                        m_destinationSlotText.Text = string.Empty;
                    }
                }
            }
        }
    }

    private void OnVendDVD(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            CompositeFunctions.VendDisk(HardwareService, OutputBox);
        }
    }

    private void OnPushDVDInSlot(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            using (var pushInDvdJob = new PushInDvdJob(HardwareService))
            {
                pushInDvdJob.Run();
                var num = 0;
                foreach (var result in pushInDvdJob.Results)
                    if (result.Code == "MachineError")
                    {
                        ++num;
                        OutputBox.Write(result.Message);
                    }

                if (num == 0)
                    OutputBox.Write("push-in-dvd SUCCESS");
                else
                    OutputBox.Write(string.Format("push-in-dvd {0}",
                        pushInDvdJob.Symbols.ContainsKey("ERROR-STATE")
                            ? pushInDvdJob.Symbols["ERROR-STATE"]
                            : (object)"ERROR"));
            }
        }
    }

    private void OnPutInEmptySlot(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            using (var inEmptySlotResult = new PutInEmptySlotResult(HardwareService))
            {
                inEmptySlotResult.Run();
                foreach (var result in inEmptySlotResult.Results)
                    OutputBox.Write(result.Message);
            }
        }
    }

    private void OnReadPickerInputs(object sender, EventArgs args)
    {
        using (var buttonAspects = Manager.MakeAspect(sender))
        {
            if (buttonAspects.Button == null)
                return;
            WriteInputs(ServiceLocator.Instance.GetService<IControlSystem>().ReadPickerInputs(), "PICKER CONTROLLER");
        }
    }

    private void OnReadAuxInputs(object sender, EventArgs args)
    {
        using (var buttonAspects = Manager.MakeAspect(sender))
        {
            if (buttonAspects.Button == null)
                return;
            WriteInputs(ServiceLocator.Instance.GetService<IControlSystem>().ReadAuxInputs(), "AUX-QLM");
        }
    }

    private void WriteInputs<T>(IReadInputsResult<T> inputs, string header)
    {
        if (!inputs.Success)
        {
            OutputBox.Write("Read inputs error: {0}", inputs.Error);
        }
        else
        {
            inputs.Log();
            var _inputs = new char[inputs.InputCount];
            var count = 0;
            inputs.Foreach(each => _inputs[count++] = inputs.IsInputActive(each) ? '1' : '0');
            for (var index = _inputs.Length - 1; index >= 0; --index)
                OutputBox.Write("{0} I/O {1} = {2}", header, index + 1, _inputs[index]);
        }
    }

    private void button15_Click(object sender, EventArgs e)
    {
        var num = (int)new AdvancedMode(HardwareService).ShowDialog();
    }

    private void button58_Click(object sender, EventArgs e)
    {
        try
        {
            var num = (int)new DeckConfigurationForm(HardwareService).ShowDialog();
        }
        catch (Exception ex)
        {
            OutputBox.Write("Unable to run Decks form.");
            LogHelper.Instance.Log("Unable to run Decks form.");
            LogHelper.Instance.Log(ex.Message);
        }
    }

    private void button59_Click(object sender, EventArgs e)
    {
        try
        {
            new Process
            {
                StartInfo =
                {
                    Arguments = "c:\\Program Files\\Redbox\\KioskLogs\\ErrorLogs",
                    FileName = "explorer.exe"
                }
            }.Start();
        }
        catch (Exception ex)
        {
            OutputBox.Write("Unable to launch explorer.");
        }
    }

    private void OnSourceDeck_Click(object s, EventArgs e)
    {
        m_sourceDeckTextBox.InputNumber();
    }

    private void OnSourceSlot_Click(object s, EventArgs e)
    {
        m_sourceSlotTextBox.InputNumber();
    }

    private void OnDestDeck_Click(object s, EventArgs e)
    {
        m_destinationDeckText.InputNumber();
    }

    private void OnDestSlot_Click(object s, EventArgs e)
    {
        m_destinationSlotText.InputNumber();
    }

    private void button17_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            using (var msPullInDvdJob = new MSPullInDvdJob(HardwareService))
            {
                msPullInDvdJob.Run();
                if (msPullInDvdJob.EndStatus == HardwareJobStatus.Completed)
                    OutputBox.Write("pull-dvd-in SUCCESS");
                else
                    foreach (var result in msPullInDvdJob.Results)
                    {
                        if (result.Code == "InvalidJobUse")
                        {
                            OutputBox.Write("pull-dvd-in is intended for a disk stuck in the picker.");
                            OutputBox.Write("Please use the 'GET' function instead.");
                            break;
                        }

                        if (result.Code == "MachineError")
                            OutputBox.Write("pull-dvd-in " + result.Message);
                    }
            }
        }
    }

    private void button65_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            DisplayInventoryStats();
        }
    }

    private void button67_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var num = (int)new ConfiguredDevicesForm(HardwareService)
            {
                AllowSessionDisplay = Settings.Default.AllowSessionDisplay,
                RouterPowerCyclePause = Settings.Default.RouterRelayPause,
                DisplayEngineeringTab = Settings.Default.DisplayEngineeringTab
            }.ShowDialog();
            ChangeUIAfterconfig();
        }
    }

    private void ChangeQlmPanelState()
    {
        if (!VMZConfigured)
        {
            ToggleQLMButtons(true);
        }
        else
        {
            ToggleQLMButtons(false);
            m_qlmTestButton.Text = "Put In bin";
            m_qlmTestButton.Enabled = true;
            m_qlmGroupBox.Text = "Door sensors and others.";
            m_qlmEngageButton.Enabled = DoorSensorsConfigured;
            m_qlmEngageButton.Text = "Query Door Sensors";
            m_qlmDisengageButton.Enabled = true;
            m_qlmDisengageButton.Text = "Test deck 8.";
            m_qlmCaseStatusButton.Visible = m_qlmDownButton.Visible =
                m_qlmStopButton.Visible = m_qlmUpButton.Visible = m_qlmDoorStatus.Visible = false;
        }
    }

    private void ToggleQLMButtons(bool show)
    {
        m_qlmCaseStatusButton.Enabled = show;
        m_qlmDisengageButton.Enabled = show;
        m_qlmDownButton.Enabled = show;
        m_qlmEngageButton.Enabled = show;
        m_qlmStopButton.Enabled = show;
        m_qlmUpButton.Enabled = show;
        m_qlmDoorStatus.Enabled = show;
        m_qlmTestButton.Enabled = show;
    }

    private bool JobCompletedWithoutError(HardwareJob job)
    {
        return job.Status == HardwareJobStatus.Completed || job.Status == HardwareJobStatus.Stopped;
    }

    private bool JobCompleted(HardwareJob job)
    {
        return job.Status == HardwareJobStatus.Completed || job.Status == HardwareJobStatus.Stopped ||
               job.Status == HardwareJobStatus.Errored || job.Status == HardwareJobStatus.Garbage;
    }

    private void button66_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            RunHardwareStatus();
        }
    }

    private void RunHardwareStatus()
    {
        using (var hardwareStatusExecutor = new HardwareStatusExecutor(HardwareService))
        {
            hardwareStatusExecutor.Run();
            if (!hardwareStatusExecutor.HardwareOk)
                hardwareStatusExecutor.Results.ForEach(res =>
                {
                    if (!(res.Code != "HardwareStatusInError"))
                        return;
                    OutputBox.Write(res.Message);
                });
            OutputBox.Write(!hardwareStatusExecutor.HardwareOk
                ? "Hardware error status follows:"
                : "Hardware status ok.");
        }
    }

    private void button9_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            using (var motionControlExecutor = new ResetMotionControlExecutor(HardwareService))
            {
                motionControlExecutor.Run();
                motionControlExecutor.Results.ForEach(each => OutputBox.Write(each.Message));
                if (motionControlExecutor.EndStatus == HardwareJobStatus.Completed)
                    return;
                OutputBox.Write("ERROR: reset-arcus failed.");
            }
        }
    }

    private void ExecuteEngageOrOther(object sender, EventArgs e)
    {
        using (var aspect = Manager.MakeAspect(sender))
        {
            if (VMZConfigured)
                using (var instructionHelper = new InstructionHelper(HardwareService))
                {
                    var str = instructionHelper.ExecuteGeneric("DOORSENSORS TESTERQUERY");
                    OutputBox.Write(string.IsNullOrEmpty(str)
                        ? ErrorCodes.ServiceChannelError.ToString().ToUpper()
                        : str);
                }
            else
                OnExecuteResponseInstruction(aspect);
        }
    }

    private void button19_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var sourceDeck = SourceDeck;
            var sourceSlot = SourceSlot;
            if (-1 == sourceDeck || -1 == sourceSlot)
                OutputBox.Write(Resources.SourceDeckSlotInvalid);
            else
                using (var executor = new GetAndReadExecutor(HardwareService, sourceDeck, sourceSlot, true))
                {
                    executor.Run();
                    executor.Results.ForEach(result =>
                    {
                        if (!(result.Code == "ErrorMessage"))
                            return;
                        OutputBox.Write(result.Message);
                    });
                    if (HardwareJobStatus.Completed == executor.EndStatus)
                    {
                        var scanResult = ScanResult.From(executor);
                        OutputBox.Write(string.Format("found {0} secure codes.", scanResult.SecureCount));
                        OutputBox.Write(scanResult.ToString());
                    }

                    OutputBox.Write("Get and read ended with status {0}", executor.EndStatus.ToString());
                }
        }
    }

    private void GetDVD(object sender)
    {
        var sourceDeck = SourceDeck;
        var sourceSlot = SourceSlot;
        if (-1 == sourceDeck || -1 == sourceSlot)
            OutputBox.Write(Resources.SourceDeckSlotInvalid);
        else
            CompositeFunctions.GetItem(sourceDeck, sourceSlot, OutputBox, HardwareService);
    }

    private void button20_Click(object sender, EventArgs e)
    {
        m_errorProvider.Clear();
        using (Manager.MakeAspect(sender))
        {
            var sourceSlot = SourceSlot;
            if (-1 == sourceSlot)
                m_errorProvider.SetError(m_sourceSlotTextBox, "Need a valid slot!");
            else
                using (var verticalSync = new VerticalSync(HardwareService, sourceSlot))
                {
                    verticalSync.Run();
                    if (HardwareJobStatus.Completed == verticalSync.EndStatus)
                        OutputBox.Write("The job completed successfully.");
                    else
                        foreach (var result in verticalSync.Results)
                            OutputBox.Write(string.Format("Failure at Deck {0} Slot {1} MSG: {2}", result.Deck,
                                result.Slot, result.Message));
                }
        }
    }

    private void m_cameraWorkingButton_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            if (CameraHelper.ResetReturnCounter())
            {
                OutputBox.Write("Return counter reset.");
                m_cameraWorkingButton.Enabled = false;
            }
            else
            {
                OutputBox.Write("Command failed.");
            }
        }
    }

    private void m_showTimeoutLogButton_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var countersFile = Settings.Default.CountersFile;
            using (var timeoutsExecutor = new GetControllerTimeoutsExecutor(HardwareService))
            {
                timeoutsExecutor.Run();
                if (HardwareJobStatus.Completed == timeoutsExecutor.EndStatus)
                {
                    using (var log = new StreamWriter(File.Open(countersFile, FileMode.Append, FileAccess.Write,
                               FileShare.Read)))
                    {
                        timeoutsExecutor.Log(log);
                    }

                    Process.Start(countersFile);
                }
                else
                {
                    OutputBox.Write("Command failed.");
                }
            }
        }
    }

    private void m_readLimitsButton_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var controlLimitResponse = MoveHelper.ReadLimits();
            if (!controlLimitResponse.ReadOk)
                OutputBox.Write("Unable to read limits.");
            else
                foreach (var limit in controlLimitResponse.Limits)
                    OutputBox.Write(string.Format("{0} LIMIT: {1}", limit.Limit.ToString().ToUpper(),
                        limit.Blocked ? "BLOCKED" : (object)"CLEAR"));
        }
    }

    private void button22_Click(object sender, EventArgs e)
    {
        if (HardwareService.ServiceUnknownSync().Success)
            OutputBox.Write("Unknown sync scheduled.");
        else
            OutputBox.Write("Unknown sync schedule failed.");
    }

    private void button21_Click(object sender, EventArgs e)
    {
        m_errorProvider.Clear();
        using (Manager.MakeAspect(sender))
        {
            if (!VMZConfigured)
                using (var qlmTestSyncJob = new QlmTestSyncJob(HardwareService))
                {
                    qlmTestSyncJob.Run();
                    foreach (var result in qlmTestSyncJob.Results)
                        OutputBox.Write(string.Format("Failure at Deck {0} Slot {1} MSG: {2}", result.Deck, result.Slot,
                            result.Message));
                }
            else
                using (var inPickerInBinJob = new PutDiskInPickerInBinJob(HardwareService))
                {
                    inPickerInBinJob.Run();
                    OutputBox.Write(string.Format("Job ended with status = {0}",
                        inPickerInBinJob.EndStatus.ToString()));
                    inPickerInBinJob.Results.ForEach(res =>
                    {
                        if (res.Code == "PutToBinOK")
                            OutputBox.Write(string.Format("The disk ID = {0} was put into the bin.",
                                res.ItemID.IsUnknown() ? res.ItemID.Metadata : (object)res.ItemID.Barcode));
                        else
                            OutputBox.Write(string.Format("Code = {0} Message = {1}", res.Code, res.Message));
                    });
                }
        }
    }

    private void button6_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var fanStatus = AirExchangerState.ToggleFan();
            OutputBox.Write(ExchangerFanStatus.On == fanStatus
                ? "Xchanger fan should be on."
                : "Xchanger fan should be off.");
            SetFanButtonState(fanStatus);
        }
    }

    private void SetFanButtonState(ExchangerFanStatus fanStatus)
    {
        m_fanButton.Enabled = AirExchangerState.Configured;
        if (!AirExchangerState.Configured)
            return;
        m_fanButton.BackColor = ExchangerFanStatus.On == fanStatus ? Color.LightGray : Color.Red;
        m_fanButton.Text = ExchangerFanStatus.On == fanStatus ? "Turn Off Xchgr Fan" : "Turn On Xchgr Fan";
    }

    private void m_getWithOffset_Click(object sender, EventArgs e)
    {
        using (Manager.MakeAspect(sender))
        {
            var sourceDeck = SourceDeck;
            var sourceSlot = SourceSlot;
            if (-1 == sourceDeck || -1 == sourceSlot)
            {
                m_errorProvider.SetError(m_sourceDeckTextBox, "Must provide a deck/slot value.");
            }
            else if (string.IsNullOrEmpty(m_getOffsetTB.Text))
            {
                GetDVD(sender);
            }
            else
            {
                if (m_offsetGetRadioButton == null)
                {
                    OutputBox.Write("Please specify an axis.");
                    m_errorProvider.SetError(m_getOffsetTB, "Please specify an axis.");
                }

                HardwareJob job;
                HardwareService.ExecuteImmediate("CLEAR", out job);
                var result1 = HardwareService.ExecuteImmediate("GRIPPER STATUS", out job);
                if (!result1.Success)
                {
                    result1.Dump();
                    OutputBox.Write("Unable to communicate with service.");
                }
                else if ("FULL".Equals(job.GetTopOfStack(), StringComparison.CurrentCultureIgnoreCase))
                {
                    OutputBox.Write("The gripper is full - clear picker & try again.");
                }
                else
                {
                    var ignoringCase = Enum<Axis>.ParseIgnoringCase(m_offsetGetRadioButton.Tag as string, Axis.X);
                    int result2;
                    if (!int.TryParse(m_getOffsetTB.Text, out result2))
                        OutputBox.Write(string.Format("Could not decode {0} as offset.", m_getOffsetTB.Text));
                    else if (ignoringCase == Axis.X && Math.Abs(result2) > 50)
                        OutputBox.Write(string.Format("Offset {0} exceeds 50 units.", m_getOffsetTB.Text));
                    else if (Axis.Y == ignoringCase && Math.Abs(result2) > 500)
                        OutputBox.Write(string.Format("Offset {0} exceeds 500 units.", m_getOffsetTB.Text));
                    else
                        using (var atOffsetExecutor = new PickAtOffsetExecutor(HardwareService, sourceDeck, sourceSlot,
                                   ignoringCase, result2))
                        {
                            atOffsetExecutor.Run();
                            if (HardwareJobStatus.Completed == atOffsetExecutor.EndStatus)
                                OutputBox.Write("GET was successful.");
                            else
                                foreach (var result3 in atOffsetExecutor.Results)
                                    OutputBox.Write(result3.Message);
                        }
                }
            }
        }
    }

    private void radioButton_CheckedChanged(object sender, EventArgs e)
    {
        if (!(sender is RadioButton radioButton) || !radioButton.Checked)
            return;
        m_offsetGetRadioButton = radioButton;
    }

    private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
    {
        Helper.WaitforInit();
    }

    private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        StopJobs();
        CompileScripts();
        ToggleFormButtons(true);
        ChangeUIAfterconfig();
        RunHardwareStatus();
        DisplayInventoryStats();
    }

    private void ToggleFormButtons(bool enable)
    {
        m_configureDevicesButton.Enabled = m_secure & enable;
        m_deckConfigurationButton.Enabled = m_secure & enable;
        m_updateConfigurationButton.Enabled = m_secure & enable;
        m_sensorCheckBox.Enabled = m_secure & enable;
        m_openLogsButton.Enabled = m_secure & enable;
        Array.ForEach(FormButtons, each => each.Enabled = enable);
    }

    private void m_initWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        var initJob = new InitJob(HardwareService);
        initJob.Run();
        e.Result = initJob;
    }

    private void m_initWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        using (var result = e.Result as InitJob)
        {
            if (HardwareJobStatus.Completed == result.EndStatus)
            {
                OutputBox.Write("Init succeeded.");
            }
            else if (result.Errors.Count > 0)
            {
                result.Errors.ForEach(each => OutputBox.Write(each.Details));
                OutputBox.Write("Init didn't succeed; errors follow:");
            }

            Array.ForEach(FormButtons, button => button.Enabled = true);
            m_runInitButton.BackColor = Color.LightGray;
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
        groupBox2 = new GroupBox();
        m_testArcusCommButton = new Button();
        m_resetProteusButton = new Button();
        m_homeYButton = new Button();
        m_homeXButton = new Button();
        button44 = new Button();
        m_runInitButton = new Button();
        m_fanButton = new Button();
        groupBox3 = new GroupBox();
        m_closeSerialButton = new Button();
        button50 = new Button();
        button47 = new Button();
        button5 = new Button();
        button57 = new Button();
        groupBox4 = new GroupBox();
        button54 = new Button();
        button53 = new Button();
        button51 = new Button();
        groupBox5 = new GroupBox();
        groupBox6 = new GroupBox();
        m_rollerTimeoutTextBox = new TextBox();
        label17 = new Label();
        m_rollToSensorText = new TextBox();
        button39 = new Button();
        button38 = new Button();
        button37 = new Button();
        button36 = new Button();
        groupBox7 = new GroupBox();
        button64 = new Button();
        button35 = new Button();
        button34 = new Button();
        m_gripperSellButton = new Button();
        button32 = new Button();
        button31 = new Button();
        groupBox8 = new GroupBox();
        button30 = new Button();
        button29 = new Button();
        button28 = new Button();
        m_qlmGroupBox = new GroupBox();
        m_qlmDownButton = new Button();
        m_qlmUpButton = new Button();
        m_qlmDoorStatus = new Button();
        m_qlmCaseStatusButton = new Button();
        m_qlmStopButton = new Button();
        m_qlmDisengageButton = new Button();
        m_qlmEngageButton = new Button();
        groupBox10 = new GroupBox();
        button63 = new Button();
        button62 = new Button();
        button61 = new Button();
        button60 = new Button();
        m_destinationSlotText = new TextBox();
        m_destinationDeckText = new TextBox();
        label11 = new Label();
        label10 = new Label();
        button18 = new Button();
        button17 = new Button();
        button16 = new Button();
        button14 = new Button();
        button13 = new Button();
        button12 = new Button();
        button11 = new Button();
        button10 = new Button();
        button8 = new Button();
        button7 = new Button();
        m_sourceSlotTextBox = new TextBox();
        m_sourceDeckTextBox = new TextBox();
        groupBox11 = new GroupBox();
        m_getOffXRadio = new RadioButton();
        m_getOffYRadio = new RadioButton();
        m_getOffsetTB = new TextBox();
        m_getWithOffset = new Button();
        m_sensorCheckBox = new CheckBox();
        label16 = new Label();
        label15 = new Label();
        button4 = new Button();
        button3 = new Button();
        button2 = new Button();
        m_encoderUnitsTextBox = new TextBox();
        button1 = new Button();
        m_readPosButton = new Button();
        groupBox12 = new GroupBox();
        m_cameraWorkingButton = new Button();
        button19 = new Button();
        m_cameraPreview = new Button();
        button26 = new Button();
        m_startButtonCamera = new Button();
        m_snapImageButton = new Button();
        m_outputBox = new ListBox();
        m_clearOutputButton = new Button();
        label14 = new Label();
        m_updateConfigurationButton = new Button();
        m_versionTextBox = new TextBox();
        label19 = new Label();
        label20 = new Label();
        m_kioskIDTextBox = new TextBox();
        groupBox1 = new GroupBox();
        button22 = new Button();
        m_qlmTestButton = new Button();
        m_readLimitsButton = new Button();
        m_showTimeoutLogButton = new Button();
        button20 = new Button();
        button66 = new Button();
        m_errorProvider = new ErrorProvider(components);
        m_deckConfigurationButton = new Button();
        m_takeDiskButton = new Button();
        m_openLogsButton = new Button();
        m_configureDevicesButton = new Button();
        m_startupWorker = new BackgroundWorker();
        m_initWorker = new BackgroundWorker();
        groupBox2.SuspendLayout();
        groupBox3.SuspendLayout();
        groupBox4.SuspendLayout();
        groupBox6.SuspendLayout();
        groupBox7.SuspendLayout();
        groupBox8.SuspendLayout();
        m_qlmGroupBox.SuspendLayout();
        groupBox10.SuspendLayout();
        groupBox11.SuspendLayout();
        groupBox12.SuspendLayout();
        groupBox1.SuspendLayout();
        ((ISupportInitialize)m_errorProvider).BeginInit();
        SuspendLayout();
        groupBox2.Controls.Add(m_testArcusCommButton);
        groupBox2.Controls.Add(m_resetProteusButton);
        groupBox2.Controls.Add(m_homeYButton);
        groupBox2.Controls.Add(m_homeXButton);
        groupBox2.Controls.Add(button44);
        groupBox2.Controls.Add(m_runInitButton);
        groupBox2.Location = new Point(13, 219);
        groupBox2.Name = "groupBox2";
        groupBox2.Size = new Size(309, 130);
        groupBox2.TabIndex = 1;
        groupBox2.TabStop = false;
        groupBox2.Text = "Init";
        m_testArcusCommButton.BackColor = Color.LightGray;
        m_testArcusCommButton.Enabled = false;
        m_testArcusCommButton.Location = new Point(204, 21);
        m_testArcusCommButton.Name = "m_testArcusCommButton";
        m_testArcusCommButton.Size = new Size(93, 43);
        m_testArcusCommButton.TabIndex = 6;
        m_testArcusCommButton.Tag = "MOTIONCONTROL COMSTATUS";
        m_testArcusCommButton.Text = "Test Proteus Comm";
        m_testArcusCommButton.UseVisualStyleBackColor = false;
        m_testArcusCommButton.Click += OnExecuteInstructionRaw;
        m_resetProteusButton.BackColor = Color.LightGray;
        m_resetProteusButton.Enabled = false;
        m_resetProteusButton.Location = new Point(204, 79);
        m_resetProteusButton.Name = "m_resetProteusButton";
        m_resetProteusButton.Size = new Size(93, 45);
        m_resetProteusButton.TabIndex = 5;
        m_resetProteusButton.Text = "Reset Proteus";
        m_resetProteusButton.UseVisualStyleBackColor = false;
        m_resetProteusButton.Click += button9_Click;
        m_homeYButton.BackColor = Color.LightGray;
        m_homeYButton.Enabled = false;
        m_homeYButton.Location = new Point(105, 79);
        m_homeYButton.Name = "m_homeYButton";
        m_homeYButton.Size = new Size(75, 45);
        m_homeYButton.TabIndex = 3;
        m_homeYButton.Tag = "HOMEY";
        m_homeYButton.Text = "Home Y";
        m_homeYButton.UseVisualStyleBackColor = false;
        m_homeYButton.Click += OnExecuteErrorCodeInstruction;
        m_homeXButton.BackColor = Color.LightGray;
        m_homeXButton.Enabled = false;
        m_homeXButton.Location = new Point(6, 79);
        m_homeXButton.Name = "m_homeXButton";
        m_homeXButton.Size = new Size(75, 45);
        m_homeXButton.TabIndex = 2;
        m_homeXButton.Tag = "HOMEX";
        m_homeXButton.Text = "Home X";
        m_homeXButton.UseVisualStyleBackColor = false;
        m_homeXButton.Click += OnExecuteErrorCodeInstruction;
        button44.BackColor = Color.GreenYellow;
        button44.Location = new Point(105, 19);
        button44.Name = "button44";
        button44.Size = new Size(75, 45);
        button44.TabIndex = 1;
        button44.Text = "Exit";
        button44.UseVisualStyleBackColor = false;
        button44.Click += OnCloseForm;
        m_runInitButton.BackColor = Color.LightGray;
        m_runInitButton.Enabled = false;
        m_runInitButton.Location = new Point(6, 19);
        m_runInitButton.Name = "m_runInitButton";
        m_runInitButton.Size = new Size(75, 45);
        m_runInitButton.TabIndex = 0;
        m_runInitButton.Text = "Init";
        m_runInitButton.UseVisualStyleBackColor = false;
        m_runInitButton.Click += OnRunInit;
        m_fanButton.BackColor = Color.LightGray;
        m_fanButton.Enabled = false;
        m_fanButton.Location = new Point(6, 76);
        m_fanButton.Name = "m_fanButton";
        m_fanButton.Size = new Size(93, 42);
        m_fanButton.TabIndex = 6;
        m_fanButton.Text = "Turn Off Xchgr Fan";
        m_fanButton.UseVisualStyleBackColor = false;
        m_fanButton.Click += button6_Click;
        groupBox3.Controls.Add(m_fanButton);
        groupBox3.Controls.Add(m_closeSerialButton);
        groupBox3.Controls.Add(button50);
        groupBox3.Controls.Add(button47);
        groupBox3.Controls.Add(button5);
        groupBox3.Controls.Add(button57);
        groupBox3.Location = new Point(13, 355);
        groupBox3.Name = "groupBox3";
        groupBox3.Size = new Size(309, 135);
        groupBox3.TabIndex = 2;
        groupBox3.TabStop = false;
        groupBox3.Text = "Boards";
        m_closeSerialButton.BackColor = Color.LightGray;
        m_closeSerialButton.Enabled = false;
        m_closeSerialButton.Location = new Point(210, 19);
        m_closeSerialButton.Name = "m_closeSerialButton";
        m_closeSerialButton.Size = new Size(87, 45);
        m_closeSerialButton.TabIndex = 4;
        m_closeSerialButton.Tag = "SERIALBOARD CLOSEPORT";
        m_closeSerialButton.Text = "Close Serial Port";
        m_closeSerialButton.UseVisualStyleBackColor = false;
        m_closeSerialButton.Click += OnExecuteInstructionRaw;
        button50.BackColor = Color.LightGray;
        button50.Enabled = false;
        button50.Location = new Point(210, 75);
        button50.Name = "button50";
        button50.Size = new Size(93, 45);
        button50.TabIndex = 3;
        button50.Tag = "SERIALBOARD RESET";
        button50.Text = "Reset";
        button50.UseVisualStyleBackColor = false;
        button50.Click += OnExecuteResponseInstruction;
        button47.BackColor = Color.LightGray;
        button47.Enabled = false;
        button47.Location = new Point(111, 76);
        button47.Name = "button47";
        button47.Size = new Size(93, 45);
        button47.TabIndex = 0;
        button47.Tag = "";
        button47.Text = "Board Test";
        button47.UseVisualStyleBackColor = false;
        button47.Click += OnTestBoards;
        button5.BackColor = Color.LightGray;
        button5.Enabled = false;
        button5.Location = new Point(111, 19);
        button5.Name = "button5";
        button5.Size = new Size(93, 45);
        button5.TabIndex = 0;
        button5.Tag = "AUX";
        button5.Text = "Read Aux Inputs";
        button5.UseVisualStyleBackColor = false;
        button5.Click += OnReadAuxInputs;
        button57.BackColor = Color.LightGray;
        button57.Enabled = false;
        button57.Location = new Point(6, 19);
        button57.Name = "button57";
        button57.Size = new Size(93, 45);
        button57.TabIndex = 1;
        button57.Tag = "PICKER";
        button57.Text = "Read Picker Controller Inputs";
        button57.UseVisualStyleBackColor = false;
        button57.Click += OnReadPickerInputs;
        groupBox4.Controls.Add(button54);
        groupBox4.Controls.Add(button53);
        groupBox4.Controls.Add(button51);
        groupBox4.Location = new Point(13, 496);
        groupBox4.Name = "groupBox4";
        groupBox4.Size = new Size(309, 93);
        groupBox4.TabIndex = 3;
        groupBox4.TabStop = false;
        groupBox4.Text = "Vend Door";
        button54.BackColor = Color.LightGray;
        button54.Enabled = false;
        button54.Location = new Point(204, 25);
        button54.Name = "button54";
        button54.Size = new Size(75, 45);
        button54.TabIndex = 3;
        button54.Tag = "VENDDOOR STATUS";
        button54.Text = "Check Status";
        button54.UseVisualStyleBackColor = false;
        button54.Click += OnExecuteInstructionRaw;
        button53.BackColor = Color.LightGray;
        button53.Enabled = false;
        button53.Location = new Point(105, 25);
        button53.Name = "button53";
        button53.Size = new Size(75, 45);
        button53.TabIndex = 2;
        button53.Tag = "VENDDOOR CLOSE";
        button53.Text = "Close";
        button53.UseVisualStyleBackColor = false;
        button53.Click += OnExecuteResponseInstruction;
        button51.BackColor = Color.LightGray;
        button51.Enabled = false;
        button51.Location = new Point(6, 25);
        button51.Name = "button51";
        button51.Size = new Size(75, 45);
        button51.TabIndex = 0;
        button51.Tag = "VENDDOOR RENT";
        button51.Text = "Rent";
        button51.UseVisualStyleBackColor = false;
        button51.Click += OnExecuteResponseInstruction;
        groupBox5.Location = new Point(328, 9);
        groupBox5.Name = "groupBox5";
        groupBox5.Size = new Size(373, 146);
        groupBox5.TabIndex = 4;
        groupBox5.TabStop = false;
        groupBox5.Text = "Sensors";
        groupBox6.Controls.Add(m_rollerTimeoutTextBox);
        groupBox6.Controls.Add(label17);
        groupBox6.Controls.Add(m_rollToSensorText);
        groupBox6.Controls.Add(button39);
        groupBox6.Controls.Add(button38);
        groupBox6.Controls.Add(button37);
        groupBox6.Controls.Add(button36);
        groupBox6.Location = new Point(328, 161);
        groupBox6.Name = "groupBox6";
        groupBox6.Size = new Size(373, 119);
        groupBox6.TabIndex = 5;
        groupBox6.TabStop = false;
        groupBox6.Text = "Rollers";
        m_rollerTimeoutTextBox.Location = new Point(243, 35);
        m_rollerTimeoutTextBox.MaxLength = 5;
        m_rollerTimeoutTextBox.Name = "m_rollerTimeoutTextBox";
        m_rollerTimeoutTextBox.Size = new Size(87, 20);
        m_rollerTimeoutTextBox.TabIndex = 6;
        label17.AutoSize = true;
        label17.Location = new Point(240, 20);
        label17.Name = "label17";
        label17.Size = new Size(101, 13);
        label17.TabIndex = 5;
        label17.Text = "Roll To Timeout(ms)";
        m_rollToSensorText.Location = new Point(240, 93);
        m_rollToSensorText.Name = "m_rollToSensorText";
        m_rollToSensorText.Size = new Size(50, 20);
        m_rollToSensorText.TabIndex = 4;
        button39.BackColor = Color.LightGray;
        button39.Enabled = false;
        button39.Location = new Point(146, 68);
        button39.Name = "button39";
        button39.Size = new Size(88, 45);
        button39.TabIndex = 3;
        button39.Text = "Roll To Sensor";
        button39.UseVisualStyleBackColor = false;
        button39.Click += OnRollerToPosition;
        button38.BackColor = Color.LightGray;
        button38.Enabled = false;
        button38.Location = new Point(23, 69);
        button38.Name = "button38";
        button38.Size = new Size(105, 45);
        button38.TabIndex = 2;
        button38.Tag = "ROLLER STOP";
        button38.Text = "Stop";
        button38.UseVisualStyleBackColor = false;
        button38.Click += OnExecuteResponseInstruction;
        button37.BackColor = Color.LightGray;
        button37.Enabled = false;
        button37.Location = new Point(146, 18);
        button37.Name = "button37";
        button37.Size = new Size(88, 45);
        button37.TabIndex = 1;
        button37.Tag = "ROLLER OUT";
        button37.Text = "Out";
        button37.UseVisualStyleBackColor = false;
        button37.Click += OnExecuteResponseInstruction;
        button36.BackColor = Color.LightGray;
        button36.Enabled = false;
        button36.Location = new Point(23, 18);
        button36.Name = "button36";
        button36.Size = new Size(105, 45);
        button36.TabIndex = 0;
        button36.Tag = "ROLLER IN";
        button36.Text = "In";
        button36.UseVisualStyleBackColor = false;
        button36.Click += OnExecuteResponseInstruction;
        groupBox7.Controls.Add(button64);
        groupBox7.Controls.Add(button35);
        groupBox7.Controls.Add(button34);
        groupBox7.Controls.Add(m_gripperSellButton);
        groupBox7.Controls.Add(button32);
        groupBox7.Controls.Add(button31);
        groupBox7.Location = new Point(328, 286);
        groupBox7.Name = "groupBox7";
        groupBox7.Size = new Size(373, 125);
        groupBox7.TabIndex = 6;
        groupBox7.TabStop = false;
        groupBox7.Text = "Gripper";
        button64.BackColor = Color.LightGray;
        button64.Enabled = false;
        button64.Location = new Point(10, 19);
        button64.Name = "button64";
        button64.Size = new Size(97, 45);
        button64.TabIndex = 5;
        button64.Tag = "GRIPPER PEEK";
        button64.Text = "Check for DVD In Slot";
        button64.UseVisualStyleBackColor = false;
        button64.Click += OnExecuteInstructionRaw;
        button35.BackColor = Color.LightGray;
        button35.Enabled = false;
        button35.Location = new Point(241, 70);
        button35.Name = "button35";
        button35.Size = new Size(105, 45);
        button35.TabIndex = 4;
        button35.Tag = "GRIPPER CLOSE";
        button35.Text = "Close";
        button35.UseVisualStyleBackColor = false;
        button35.Click += OnExecuteResponseInstruction;
        button34.BackColor = Color.LightGray;
        button34.Enabled = false;
        button34.Location = new Point(10, 70);
        button34.Name = "button34";
        button34.Size = new Size(97, 45);
        button34.TabIndex = 3;
        button34.Tag = "GRIPPER RENT";
        button34.Text = "Rent";
        button34.UseVisualStyleBackColor = false;
        button34.Click += OnExecuteResponseInstruction;
        m_gripperSellButton.BackColor = Color.LightGray;
        m_gripperSellButton.Enabled = false;
        m_gripperSellButton.Location = new Point(125, 70);
        m_gripperSellButton.Name = "m_gripperSellButton";
        m_gripperSellButton.Size = new Size(96, 45);
        m_gripperSellButton.TabIndex = 2;
        m_gripperSellButton.Tag = "GRIPPER OPEN";
        m_gripperSellButton.Text = "Sell";
        m_gripperSellButton.UseVisualStyleBackColor = false;
        m_gripperSellButton.Click += OnExecuteResponseInstruction;
        button32.BackColor = Color.LightGray;
        button32.Enabled = false;
        button32.Location = new Point(241, 19);
        button32.Name = "button32";
        button32.Size = new Size(105, 45);
        button32.TabIndex = 1;
        button32.Tag = "GRIPPER RETRACT";
        button32.Text = "Retract";
        button32.UseVisualStyleBackColor = false;
        button32.Click += OnExecuteResponseInstruction;
        button31.BackColor = Color.LightGray;
        button31.Enabled = false;
        button31.Location = new Point(125, 19);
        button31.Name = "button31";
        button31.Size = new Size(97, 45);
        button31.TabIndex = 0;
        button31.Tag = "GRIPPER EXTEND";
        button31.Text = "Extend";
        button31.UseVisualStyleBackColor = false;
        button31.Click += OnExecuteResponseInstruction;
        groupBox8.Controls.Add(button30);
        groupBox8.Controls.Add(button29);
        groupBox8.Controls.Add(button28);
        groupBox8.Location = new Point(328, 417);
        groupBox8.Name = "groupBox8";
        groupBox8.Size = new Size(373, 73);
        groupBox8.TabIndex = 7;
        groupBox8.TabStop = false;
        groupBox8.Text = "Track";
        button30.BackColor = Color.LightGray;
        button30.Enabled = false;
        button30.Location = new Point(249, 19);
        button30.Name = "button30";
        button30.Size = new Size(97, 45);
        button30.TabIndex = 2;
        button30.Tag = "TRACK STATUS";
        button30.Text = "Status";
        button30.UseVisualStyleBackColor = false;
        button30.Click += OnExecuteInstructionRaw;
        button29.BackColor = Color.LightGray;
        button29.Enabled = false;
        button29.Location = new Point(125, 19);
        button29.Name = "button29";
        button29.Size = new Size(108, 45);
        button29.TabIndex = 1;
        button29.Tag = "TRACK CLOSE";
        button29.Text = "Close";
        button29.UseVisualStyleBackColor = false;
        button29.Click += OnExecuteResponseInstruction;
        button28.BackColor = Color.LightGray;
        button28.Enabled = false;
        button28.Location = new Point(10, 19);
        button28.Name = "button28";
        button28.Size = new Size(97, 45);
        button28.TabIndex = 0;
        button28.Tag = "TRACK OPEN";
        button28.Text = "Open";
        button28.UseVisualStyleBackColor = false;
        button28.Click += OnExecuteResponseInstruction;
        m_qlmGroupBox.Controls.Add(m_qlmDownButton);
        m_qlmGroupBox.Controls.Add(m_qlmUpButton);
        m_qlmGroupBox.Controls.Add(m_qlmDoorStatus);
        m_qlmGroupBox.Controls.Add(m_qlmCaseStatusButton);
        m_qlmGroupBox.Controls.Add(m_qlmStopButton);
        m_qlmGroupBox.Controls.Add(m_qlmDisengageButton);
        m_qlmGroupBox.Controls.Add(m_qlmEngageButton);
        m_qlmGroupBox.Location = new Point(328, 497);
        m_qlmGroupBox.Name = "m_qlmGroupBox";
        m_qlmGroupBox.Size = new Size(373, 118);
        m_qlmGroupBox.TabIndex = 8;
        m_qlmGroupBox.TabStop = false;
        m_qlmGroupBox.Text = "QLM";
        m_qlmDownButton.BackColor = Color.LightGray;
        m_qlmDownButton.Enabled = false;
        m_qlmDownButton.Location = new Point(90, 70);
        m_qlmDownButton.Name = "m_qlmDownButton";
        m_qlmDownButton.Size = new Size(75, 40);
        m_qlmDownButton.TabIndex = 6;
        m_qlmDownButton.Tag = "QLM DROP";
        m_qlmDownButton.Text = "Down";
        m_qlmDownButton.UseVisualStyleBackColor = false;
        m_qlmDownButton.Click += OnDropQLM;
        m_qlmUpButton.BackColor = Color.LightGray;
        m_qlmUpButton.Enabled = false;
        m_qlmUpButton.Location = new Point(10, 70);
        m_qlmUpButton.Name = "m_qlmUpButton";
        m_qlmUpButton.Size = new Size(75, 40);
        m_qlmUpButton.TabIndex = 5;
        m_qlmUpButton.Tag = "QLM LIFT";
        m_qlmUpButton.Text = "Up";
        m_qlmUpButton.UseVisualStyleBackColor = false;
        m_qlmUpButton.Click += OnExecuteInstructionBlind;
        m_qlmDoorStatus.BackColor = Color.LightGray;
        m_qlmDoorStatus.Enabled = false;
        m_qlmDoorStatus.Location = new Point(262, 65);
        m_qlmDoorStatus.Name = "m_qlmDoorStatus";
        m_qlmDoorStatus.Size = new Size(75, 45);
        m_qlmDoorStatus.TabIndex = 4;
        m_qlmDoorStatus.Tag = "QLMDOOR STATUS";
        m_qlmDoorStatus.Text = "Door Status";
        m_qlmDoorStatus.UseVisualStyleBackColor = false;
        m_qlmDoorStatus.Visible = false;
        m_qlmDoorStatus.Click += OnExecuteInstructionRaw;
        m_qlmCaseStatusButton.BackColor = Color.LightGray;
        m_qlmCaseStatusButton.Enabled = false;
        m_qlmCaseStatusButton.Location = new Point(byte.MaxValue, 18);
        m_qlmCaseStatusButton.Name = "m_qlmCaseStatusButton";
        m_qlmCaseStatusButton.Size = new Size(75, 45);
        m_qlmCaseStatusButton.TabIndex = 3;
        m_qlmCaseStatusButton.Tag = "QLM STATUS";
        m_qlmCaseStatusButton.Text = "Case Status";
        m_qlmCaseStatusButton.UseVisualStyleBackColor = false;
        m_qlmCaseStatusButton.Click += OnExecuteInstructionRaw;
        m_qlmStopButton.BackColor = Color.LightGray;
        m_qlmStopButton.Enabled = false;
        m_qlmStopButton.Location = new Point(172, 19);
        m_qlmStopButton.Name = "m_qlmStopButton";
        m_qlmStopButton.Size = new Size(75, 45);
        m_qlmStopButton.TabIndex = 2;
        m_qlmStopButton.Tag = "QLM HALT";
        m_qlmStopButton.Text = "Stop";
        m_qlmStopButton.UseVisualStyleBackColor = false;
        m_qlmStopButton.Click += OnExecuteInstructionBlind;
        m_qlmDisengageButton.BackColor = Color.LightGray;
        m_qlmDisengageButton.Enabled = false;
        m_qlmDisengageButton.Location = new Point(91, 19);
        m_qlmDisengageButton.Name = "m_qlmDisengageButton";
        m_qlmDisengageButton.Size = new Size(75, 45);
        m_qlmDisengageButton.TabIndex = 1;
        m_qlmDisengageButton.Tag = "QLM DISENGAGE";
        m_qlmDisengageButton.Text = "Disengage";
        m_qlmDisengageButton.UseVisualStyleBackColor = false;
        m_qlmDisengageButton.Click += OnRunQlmDisengage;
        m_qlmEngageButton.BackColor = Color.LightGray;
        m_qlmEngageButton.Enabled = false;
        m_qlmEngageButton.Location = new Point(10, 19);
        m_qlmEngageButton.Name = "m_qlmEngageButton";
        m_qlmEngageButton.Size = new Size(75, 45);
        m_qlmEngageButton.TabIndex = 0;
        m_qlmEngageButton.Tag = "QLM ENGAGE";
        m_qlmEngageButton.Text = "Engage";
        m_qlmEngageButton.UseVisualStyleBackColor = false;
        m_qlmEngageButton.Click += ExecuteEngageOrOther;
        groupBox10.Controls.Add(button63);
        groupBox10.Controls.Add(button62);
        groupBox10.Controls.Add(button61);
        groupBox10.Controls.Add(button60);
        groupBox10.Controls.Add(m_destinationSlotText);
        groupBox10.Controls.Add(m_destinationDeckText);
        groupBox10.Controls.Add(label11);
        groupBox10.Controls.Add(label10);
        groupBox10.Controls.Add(button18);
        groupBox10.Controls.Add(button17);
        groupBox10.Controls.Add(button16);
        groupBox10.Controls.Add(button14);
        groupBox10.Controls.Add(button13);
        groupBox10.Controls.Add(button12);
        groupBox10.Controls.Add(button11);
        groupBox10.Controls.Add(button10);
        groupBox10.Controls.Add(button8);
        groupBox10.Controls.Add(button7);
        groupBox10.Controls.Add(m_sourceSlotTextBox);
        groupBox10.Controls.Add(m_sourceDeckTextBox);
        groupBox10.Location = new Point(720, 12);
        groupBox10.Name = "groupBox10";
        groupBox10.Size = new Size(271, 358);
        groupBox10.TabIndex = 9;
        groupBox10.TabStop = false;
        groupBox10.Text = "Movements";
        button63.BackColor = Color.LightGray;
        button63.Location = new Point(133, 89);
        button63.Name = "button63";
        button63.Size = new Size(56, 29);
        button63.TabIndex = 25;
        button63.Tag = "DestSlot";
        button63.Text = "Slot";
        button63.UseVisualStyleBackColor = false;
        button63.Click += OnDestSlot_Click;
        button62.BackColor = Color.LightGray;
        button62.Location = new Point(12, 89);
        button62.Name = "button62";
        button62.Size = new Size(60, 29);
        button62.TabIndex = 24;
        button62.Tag = "DestDeck";
        button62.Text = "Deck";
        button62.UseVisualStyleBackColor = false;
        button62.Click += OnDestDeck_Click;
        button61.BackColor = Color.LightGray;
        button61.Location = new Point(133, 32);
        button61.Name = "button61";
        button61.Size = new Size(56, 32);
        button61.TabIndex = 23;
        button61.Tag = "SourceSlot";
        button61.Text = "Slot";
        button61.UseVisualStyleBackColor = false;
        button61.Click += OnSourceSlot_Click;
        button60.BackColor = Color.LightGray;
        button60.Location = new Point(13, 32);
        button60.Name = "button60";
        button60.Size = new Size(59, 32);
        button60.TabIndex = 22;
        button60.Tag = "SourceDeck";
        button60.Text = "Deck";
        button60.UseVisualStyleBackColor = false;
        button60.Click += OnSourceDeck_Click;
        m_destinationSlotText.Location = new Point(203, 94);
        m_destinationSlotText.Name = "m_destinationSlotText";
        m_destinationSlotText.Size = new Size(49, 20);
        m_destinationSlotText.TabIndex = 21;
        m_destinationDeckText.Location = new Point(81, 95);
        m_destinationDeckText.Name = "m_destinationDeckText";
        m_destinationDeckText.Size = new Size(40, 20);
        m_destinationDeckText.TabIndex = 20;
        label11.AutoSize = true;
        label11.Location = new Point(11, 73);
        label11.Name = "label11";
        label11.Size = new Size(102, 13);
        label11.TabIndex = 17;
        label11.Text = "Transfer Destination";
        label10.AutoSize = true;
        label10.Location = new Point(18, 19);
        label10.Name = "label10";
        label10.Size = new Size(41, 13);
        label10.TabIndex = 16;
        label10.Text = "Source";
        button18.BackColor = Color.LightGray;
        button18.Enabled = false;
        button18.Location = new Point(133, 308);
        button18.Name = "button18";
        button18.Size = new Size(119, 40);
        button18.TabIndex = 15;
        button18.Text = "Put in Empty Slot";
        button18.UseVisualStyleBackColor = false;
        button18.Click += OnPutInEmptySlot;
        button17.BackColor = Color.LightGray;
        button17.Enabled = false;
        button17.Location = new Point(14, 308);
        button17.Name = "button17";
        button17.Size = new Size(102, 40);
        button17.TabIndex = 14;
        button17.Tag = "";
        button17.Text = "Pull DVD Into Picker";
        button17.UseVisualStyleBackColor = false;
        button17.Click += button17_Click;
        button16.BackColor = Color.LightGray;
        button16.Enabled = false;
        button16.Location = new Point(14, 262);
        button16.Name = "button16";
        button16.Size = new Size(102, 40);
        button16.TabIndex = 13;
        button16.Text = "Push DVD In Slot";
        button16.UseVisualStyleBackColor = false;
        button16.Click += OnPushDVDInSlot;
        button14.BackColor = Color.LightGray;
        button14.Enabled = false;
        button14.Location = new Point(133, 262);
        button14.Name = "button14";
        button14.Size = new Size(119, 40);
        button14.TabIndex = 11;
        button14.Text = "Vend DVD in Picker";
        button14.UseVisualStyleBackColor = false;
        button14.Click += OnVendDVD;
        button13.BackColor = Color.LightGray;
        button13.Enabled = false;
        button13.Location = new Point(14, 216);
        button13.Name = "button13";
        button13.Size = new Size(102, 40);
        button13.TabIndex = 10;
        button13.Tag = "MOVEVEND";
        button13.Text = "Move to Vend";
        button13.UseVisualStyleBackColor = false;
        button13.Click += OnExecuteErrorCodeInstruction;
        button12.BackColor = Color.LightGray;
        button12.Enabled = false;
        button12.Location = new Point(133, 216);
        button12.Name = "button12";
        button12.Size = new Size(119, 40);
        button12.TabIndex = 9;
        button12.Text = "Transfer";
        button12.UseVisualStyleBackColor = false;
        button12.Click += OnTransfer;
        button11.BackColor = Color.LightGray;
        button11.Enabled = false;
        button11.Location = new Point(14, 170);
        button11.Name = "button11";
        button11.Size = new Size(102, 40);
        button11.TabIndex = 8;
        button11.Text = "Get";
        button11.UseVisualStyleBackColor = false;
        button11.Click += OnGetAndRead;
        button10.BackColor = Color.LightGray;
        button10.Enabled = false;
        button10.Location = new Point(133, 124);
        button10.Name = "button10";
        button10.Size = new Size(119, 40);
        button10.TabIndex = 7;
        button10.Text = "Put";
        button10.UseVisualStyleBackColor = false;
        button10.Click += OnPut;
        button8.BackColor = Color.LightGray;
        button8.Enabled = false;
        button8.Location = new Point(133, 170);
        button8.Name = "button8";
        button8.Size = new Size(119, 40);
        button8.TabIndex = 5;
        button8.Text = "Sync Slot";
        button8.UseVisualStyleBackColor = false;
        button8.Click += OnSyncSlot;
        button7.BackColor = Color.LightGray;
        button7.Enabled = false;
        button7.Location = new Point(14, 124);
        button7.Name = "button7";
        button7.Size = new Size(102, 40);
        button7.TabIndex = 4;
        button7.Text = "Go To";
        button7.UseVisualStyleBackColor = false;
        button7.Click += OnGotoSlot;
        m_sourceSlotTextBox.Location = new Point(203, 39);
        m_sourceSlotTextBox.Name = "m_sourceSlotTextBox";
        m_sourceSlotTextBox.Size = new Size(49, 20);
        m_sourceSlotTextBox.TabIndex = 3;
        m_sourceDeckTextBox.Location = new Point(81, 39);
        m_sourceDeckTextBox.Name = "m_sourceDeckTextBox";
        m_sourceDeckTextBox.Size = new Size(38, 20);
        m_sourceDeckTextBox.TabIndex = 0;
        groupBox11.Controls.Add(m_getOffXRadio);
        groupBox11.Controls.Add(m_getOffYRadio);
        groupBox11.Controls.Add(m_getOffsetTB);
        groupBox11.Controls.Add(m_getWithOffset);
        groupBox11.Controls.Add(m_sensorCheckBox);
        groupBox11.Controls.Add(label16);
        groupBox11.Controls.Add(label15);
        groupBox11.Controls.Add(button4);
        groupBox11.Controls.Add(button3);
        groupBox11.Controls.Add(button2);
        groupBox11.Controls.Add(m_encoderUnitsTextBox);
        groupBox11.Controls.Add(button1);
        groupBox11.Location = new Point(720, 381);
        groupBox11.Name = "groupBox11";
        groupBox11.Size = new Size(271, 193);
        groupBox11.TabIndex = 10;
        groupBox11.TabStop = false;
        groupBox11.Text = "X/Y Moves";
        m_getOffXRadio.AutoSize = true;
        m_getOffXRadio.Location = new Point(93, 149);
        m_getOffXRadio.Name = "m_getOffXRadio";
        m_getOffXRadio.Size = new Size(32, 17);
        m_getOffXRadio.TabIndex = 18;
        m_getOffXRadio.TabStop = true;
        m_getOffXRadio.Tag = "X";
        m_getOffXRadio.Text = "X";
        m_getOffXRadio.UseVisualStyleBackColor = true;
        m_getOffXRadio.CheckedChanged += radioButton_CheckedChanged;
        m_getOffYRadio.AutoSize = true;
        m_getOffYRadio.Location = new Point(93, 170);
        m_getOffYRadio.Name = "m_getOffYRadio";
        m_getOffYRadio.Size = new Size(32, 17);
        m_getOffYRadio.TabIndex = 17;
        m_getOffYRadio.TabStop = true;
        m_getOffYRadio.Tag = "Y";
        m_getOffYRadio.Text = "Y";
        m_getOffYRadio.UseVisualStyleBackColor = true;
        m_getOffYRadio.CheckedChanged += radioButton_CheckedChanged;
        m_getOffsetTB.Location = new Point(6, 165);
        m_getOffsetTB.Name = "m_getOffsetTB";
        m_getOffsetTB.Size = new Size(81, 20);
        m_getOffsetTB.TabIndex = 16;
        m_getWithOffset.BackColor = Color.LightGray;
        m_getWithOffset.Enabled = false;
        m_getWithOffset.Location = new Point(177, 146);
        m_getWithOffset.Name = "m_getWithOffset";
        m_getWithOffset.Size = new Size(84, 41);
        m_getWithOffset.TabIndex = 15;
        m_getWithOffset.Text = "Get With Offset";
        m_getWithOffset.UseVisualStyleBackColor = false;
        m_getWithOffset.Click += m_getWithOffset_Click;
        m_sensorCheckBox.AutoSize = true;
        m_sensorCheckBox.Location = new Point(6, 134);
        m_sensorCheckBox.Name = "m_sensorCheckBox";
        m_sensorCheckBox.Size = new Size(133, 17);
        m_sensorCheckBox.TabIndex = 14;
        m_sensorCheckBox.Text = "Without Sensor Check";
        m_sensorCheckBox.UseVisualStyleBackColor = true;
        label16.AutoSize = true;
        label16.Location = new Point(174, 37);
        label16.Name = "label16";
        label16.Size = new Size(94, 13);
        label16.TabIndex = 11;
        label16.Text = "Counter-clockwise";
        label15.AutoSize = true;
        label15.Location = new Point(11, 37);
        label15.Name = "label15";
        label15.Size = new Size(55, 13);
        label15.TabIndex = 10;
        label15.Text = "Clockwise";
        button4.BackColor = Color.LightGray;
        button4.Enabled = false;
        button4.Location = new Point(87, 91);
        button4.Name = "button4";
        button4.Size = new Size(75, 40);
        button4.TabIndex = 4;
        button4.Tag = "Down";
        button4.Text = "Down";
        button4.UseVisualStyleBackColor = false;
        button4.Click += OnDirectionMove;
        button3.BackColor = Color.LightGray;
        button3.Enabled = false;
        button3.Location = new Point(87, 18);
        button3.Name = "button3";
        button3.Size = new Size(75, 40);
        button3.TabIndex = 3;
        button3.Tag = "Up";
        button3.Text = "Up";
        button3.UseVisualStyleBackColor = false;
        button3.Click += OnDirectionMove;
        button2.BackColor = Color.LightGray;
        button2.Enabled = false;
        button2.Location = new Point(177, 54);
        button2.Name = "button2";
        button2.Size = new Size(75, 40);
        button2.TabIndex = 2;
        button2.Tag = "Right";
        button2.Text = "Right";
        button2.UseVisualStyleBackColor = false;
        button2.Click += OnDirectionMove;
        m_encoderUnitsTextBox.Location = new Point(87, 65);
        m_encoderUnitsTextBox.Name = "m_encoderUnitsTextBox";
        m_encoderUnitsTextBox.Size = new Size(75, 20);
        m_encoderUnitsTextBox.TabIndex = 1;
        button1.BackColor = Color.LightGray;
        button1.Enabled = false;
        button1.Location = new Point(0, 55);
        button1.Name = "button1";
        button1.Size = new Size(75, 40);
        button1.TabIndex = 0;
        button1.Tag = "Left";
        button1.Text = "Left";
        button1.UseVisualStyleBackColor = false;
        button1.Click += OnDirectionMove;
        m_readPosButton.BackColor = Color.LightGray;
        m_readPosButton.Enabled = false;
        m_readPosButton.Location = new Point(118, 161);
        m_readPosButton.Name = "m_readPosButton";
        m_readPosButton.Size = new Size(99, 45);
        m_readPosButton.TabIndex = 7;
        m_readPosButton.Tag = "";
        m_readPosButton.Text = "Read Positions";
        m_readPosButton.UseVisualStyleBackColor = false;
        m_readPosButton.Click += OnReadPositions;
        groupBox12.Controls.Add(m_cameraWorkingButton);
        groupBox12.Controls.Add(button19);
        groupBox12.Controls.Add(m_cameraPreview);
        groupBox12.Controls.Add(button26);
        groupBox12.Controls.Add(m_startButtonCamera);
        groupBox12.Controls.Add(m_snapImageButton);
        groupBox12.Location = new Point(328, 621);
        groupBox12.Name = "groupBox12";
        groupBox12.Size = new Size(370, 107);
        groupBox12.TabIndex = 11;
        groupBox12.TabStop = false;
        groupBox12.Text = "Camera";
        m_cameraWorkingButton.BackColor = Color.LightGray;
        m_cameraWorkingButton.Enabled = false;
        m_cameraWorkingButton.Location = new Point(128, 62);
        m_cameraWorkingButton.Name = "m_cameraWorkingButton";
        m_cameraWorkingButton.Size = new Size(93, 40);
        m_cameraWorkingButton.TabIndex = 5;
        m_cameraWorkingButton.Text = "Reset CCF Counter";
        m_cameraWorkingButton.UseVisualStyleBackColor = false;
        m_cameraWorkingButton.Click += m_cameraWorkingButton_Click;
        button19.BackColor = Color.LightGray;
        button19.Enabled = false;
        button19.Location = new Point(262, 13);
        button19.Name = "button19";
        button19.Size = new Size(93, 40);
        button19.TabIndex = 4;
        button19.Text = "Get and Read";
        button19.UseVisualStyleBackColor = false;
        button19.Click += button19_Click;
        m_cameraPreview.BackColor = Color.LightGray;
        m_cameraPreview.Enabled = false;
        m_cameraPreview.Location = new Point(128, 13);
        m_cameraPreview.Name = "m_cameraPreview";
        m_cameraPreview.Size = new Size(93, 40);
        m_cameraPreview.TabIndex = 3;
        m_cameraPreview.Text = "Camera Settings and Preview";
        m_cameraPreview.UseVisualStyleBackColor = false;
        m_cameraPreview.Click += OnLaunchCameraProperties;
        button26.BackColor = Color.LightGray;
        button26.Enabled = false;
        button26.Location = new Point(262, 62);
        button26.Name = "button26";
        button26.Size = new Size(93, 40);
        button26.TabIndex = 2;
        button26.Text = "Read Disk In Picker";
        button26.UseVisualStyleBackColor = false;
        button26.Click += OnReadDiskInPicker;
        m_startButtonCamera.BackColor = Color.LightGray;
        m_startButtonCamera.Enabled = false;
        m_startButtonCamera.Location = new Point(10, 62);
        m_startButtonCamera.Name = "m_startButtonCamera";
        m_startButtonCamera.Size = new Size(93, 40);
        m_startButtonCamera.TabIndex = 1;
        m_startButtonCamera.Tag = "";
        m_startButtonCamera.Text = "Turn Ringlight On";
        m_startButtonCamera.UseVisualStyleBackColor = false;
        m_startButtonCamera.Click += OnStartCamera;
        m_snapImageButton.BackColor = Color.LightGray;
        m_snapImageButton.Enabled = false;
        m_snapImageButton.Location = new Point(10, 16);
        m_snapImageButton.Name = "m_snapImageButton";
        m_snapImageButton.Size = new Size(93, 40);
        m_snapImageButton.TabIndex = 0;
        m_snapImageButton.Tag = "";
        m_snapImageButton.Text = "Snap";
        m_snapImageButton.UseVisualStyleBackColor = false;
        m_snapImageButton.Click += OnDoSnap;
        m_outputBox.Font = new Font("Arial", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
        m_outputBox.FormattingEnabled = true;
        m_outputBox.HorizontalScrollbar = true;
        m_outputBox.ItemHeight = 14;
        m_outputBox.Location = new Point(13, 25);
        m_outputBox.Name = "m_outputBox";
        m_outputBox.Size = new Size(301, 130);
        m_outputBox.TabIndex = 12;
        m_clearOutputButton.BackColor = Color.LightGray;
        m_clearOutputButton.Enabled = false;
        m_clearOutputButton.Location = new Point(13, 161);
        m_clearOutputButton.Name = "m_clearOutputButton";
        m_clearOutputButton.Size = new Size(99, 45);
        m_clearOutputButton.TabIndex = 14;
        m_clearOutputButton.Text = "Clear Output";
        m_clearOutputButton.UseVisualStyleBackColor = false;
        m_clearOutputButton.Click += OnClearOutput;
        label14.AutoSize = true;
        label14.Location = new Point(10, 9);
        label14.Name = "label14";
        label14.Size = new Size(39, 13);
        label14.TabIndex = 15;
        label14.Text = "Output";
        m_updateConfigurationButton.BackColor = Color.LightGray;
        m_updateConfigurationButton.Enabled = false;
        m_updateConfigurationButton.Location = new Point(720, 580);
        m_updateConfigurationButton.Name = "m_updateConfigurationButton";
        m_updateConfigurationButton.Size = new Size(132, 40);
        m_updateConfigurationButton.TabIndex = 16;
        m_updateConfigurationButton.Text = "Update Configuration";
        m_updateConfigurationButton.UseVisualStyleBackColor = false;
        m_updateConfigurationButton.Click += button15_Click;
        m_versionTextBox.Location = new Point(859, 676);
        m_versionTextBox.Name = "m_versionTextBox";
        m_versionTextBox.ReadOnly = true;
        m_versionTextBox.Size = new Size(100, 20);
        m_versionTextBox.TabIndex = 17;
        label19.AutoSize = true;
        label19.Location = new Point(717, 683);
        label19.Name = "label19";
        label19.Size = new Size(78, 13);
        label19.TabIndex = 18;
        label19.Text = "Tester Version:";
        label20.AutoSize = true;
        label20.Location = new Point(717, 709);
        label20.Name = "label20";
        label20.Size = new Size(50, 13);
        label20.TabIndex = 19;
        label20.Text = "Kiosk ID:";
        m_kioskIDTextBox.Location = new Point(859, 702);
        m_kioskIDTextBox.Name = "m_kioskIDTextBox";
        m_kioskIDTextBox.ReadOnly = true;
        m_kioskIDTextBox.Size = new Size(100, 20);
        m_kioskIDTextBox.TabIndex = 20;
        m_kioskIDTextBox.Text = "UNKNOWN";
        groupBox1.Controls.Add(button22);
        groupBox1.Controls.Add(m_qlmTestButton);
        groupBox1.Controls.Add(m_readLimitsButton);
        groupBox1.Controls.Add(m_showTimeoutLogButton);
        groupBox1.Controls.Add(button20);
        groupBox1.Controls.Add(button66);
        groupBox1.Location = new Point(13, 596);
        groupBox1.Name = "groupBox1";
        groupBox1.Size = new Size(309, 132);
        groupBox1.TabIndex = 21;
        groupBox1.TabStop = false;
        groupBox1.Text = "Other";
        button22.BackColor = Color.LightGray;
        button22.Enabled = false;
        button22.Location = new Point(107, 17);
        button22.Name = "button22";
        button22.Size = new Size(75, 46);
        button22.TabIndex = 7;
        button22.Text = "Sync Unknowns";
        button22.UseVisualStyleBackColor = false;
        button22.Click += button22_Click;
        m_qlmTestButton.BackColor = Color.LightGray;
        m_qlmTestButton.Enabled = false;
        m_qlmTestButton.Location = new Point(8, 17);
        m_qlmTestButton.Name = "m_qlmTestButton";
        m_qlmTestButton.Size = new Size(75, 46);
        m_qlmTestButton.TabIndex = 6;
        m_qlmTestButton.Text = "Qlm Slot Test";
        m_qlmTestButton.UseVisualStyleBackColor = false;
        m_qlmTestButton.Click += button21_Click;
        m_readLimitsButton.BackColor = Color.LightGray;
        m_readLimitsButton.Enabled = false;
        m_readLimitsButton.Location = new Point(204, 69);
        m_readLimitsButton.Name = "m_readLimitsButton";
        m_readLimitsButton.Size = new Size(75, 57);
        m_readLimitsButton.TabIndex = 5;
        m_readLimitsButton.Text = "Read Controller Limits";
        m_readLimitsButton.UseVisualStyleBackColor = false;
        m_readLimitsButton.Click += m_readLimitsButton_Click;
        m_showTimeoutLogButton.BackColor = Color.LightGray;
        m_showTimeoutLogButton.Enabled = false;
        m_showTimeoutLogButton.Location = new Point(105, 69);
        m_showTimeoutLogButton.Name = "m_showTimeoutLogButton";
        m_showTimeoutLogButton.Size = new Size(75, 57);
        m_showTimeoutLogButton.TabIndex = 4;
        m_showTimeoutLogButton.Text = "Show Timeouts";
        m_showTimeoutLogButton.UseVisualStyleBackColor = false;
        m_showTimeoutLogButton.Click += m_showTimeoutLogButton_Click;
        button20.BackColor = Color.LightGray;
        button20.Enabled = false;
        button20.Location = new Point(6, 69);
        button20.Name = "button20";
        button20.Size = new Size(75, 57);
        button20.TabIndex = 3;
        button20.Text = "Vertical Slot Test";
        button20.UseVisualStyleBackColor = false;
        button20.Click += button20_Click;
        button66.BackColor = Color.LightGray;
        button66.Enabled = false;
        button66.Location = new Point(204, 18);
        button66.Name = "button66";
        button66.Size = new Size(75, 45);
        button66.TabIndex = 2;
        button66.Text = "Hardware Check";
        button66.UseVisualStyleBackColor = false;
        button66.Click += button66_Click;
        m_errorProvider.ContainerControl = this;
        m_deckConfigurationButton.BackColor = Color.LightGray;
        m_deckConfigurationButton.Enabled = false;
        m_deckConfigurationButton.Location = new Point(859, 580);
        m_deckConfigurationButton.Name = "m_deckConfigurationButton";
        m_deckConfigurationButton.Size = new Size(132, 40);
        m_deckConfigurationButton.TabIndex = 23;
        m_deckConfigurationButton.Text = "Deck Configuration";
        m_deckConfigurationButton.UseVisualStyleBackColor = false;
        m_deckConfigurationButton.Click += button58_Click;
        m_takeDiskButton.BackColor = Color.LightGray;
        m_takeDiskButton.Enabled = false;
        m_takeDiskButton.Location = new Point(721, 626);
        m_takeDiskButton.Name = "m_takeDiskButton";
        m_takeDiskButton.Size = new Size(132, 44);
        m_takeDiskButton.TabIndex = 27;
        m_takeDiskButton.Text = "Unknown Count";
        m_takeDiskButton.UseVisualStyleBackColor = false;
        m_takeDiskButton.Click += button65_Click;
        m_openLogsButton.BackColor = Color.LightGray;
        m_openLogsButton.Enabled = false;
        m_openLogsButton.Location = new Point(223, 161);
        m_openLogsButton.Name = "m_openLogsButton";
        m_openLogsButton.Size = new Size(99, 45);
        m_openLogsButton.TabIndex = 28;
        m_openLogsButton.Text = "Open Error Logs";
        m_openLogsButton.UseVisualStyleBackColor = false;
        m_openLogsButton.Click += button59_Click;
        m_configureDevicesButton.BackColor = Color.LightGray;
        m_configureDevicesButton.Enabled = false;
        m_configureDevicesButton.Location = new Point(859, 626);
        m_configureDevicesButton.Name = "m_configureDevicesButton";
        m_configureDevicesButton.Size = new Size(132, 44);
        m_configureDevicesButton.TabIndex = 29;
        m_configureDevicesButton.Text = "Configure Devices";
        m_configureDevicesButton.UseVisualStyleBackColor = false;
        m_configureDevicesButton.Click += button67_Click;
        m_startupWorker.DoWork += backgroundWorker1_DoWork;
        m_startupWorker.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
        m_initWorker.DoWork += m_initWorker_DoWork;
        m_initWorker.RunWorkerCompleted += m_initWorker_RunWorkerCompleted;
        AutoScaleDimensions = new SizeF(6f, 13f);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.WhiteSmoke;
        ClientSize = new Size(1013, 742);
        Controls.Add(m_configureDevicesButton);
        Controls.Add(m_openLogsButton);
        Controls.Add(m_takeDiskButton);
        Controls.Add(m_deckConfigurationButton);
        Controls.Add(m_readPosButton);
        Controls.Add(groupBox1);
        Controls.Add(m_kioskIDTextBox);
        Controls.Add(label20);
        Controls.Add(label19);
        Controls.Add(m_versionTextBox);
        Controls.Add(m_updateConfigurationButton);
        Controls.Add(label14);
        Controls.Add(m_clearOutputButton);
        Controls.Add(m_outputBox);
        Controls.Add(groupBox12);
        Controls.Add(groupBox11);
        Controls.Add(groupBox10);
        Controls.Add(m_qlmGroupBox);
        Controls.Add(groupBox8);
        Controls.Add(groupBox7);
        Controls.Add(groupBox6);
        Controls.Add(groupBox5);
        Controls.Add(groupBox4);
        Controls.Add(groupBox3);
        Controls.Add(groupBox2);
        Name = nameof(Form1);
        Text = "HAL Tester";
        groupBox2.ResumeLayout(false);
        groupBox3.ResumeLayout(false);
        groupBox4.ResumeLayout(false);
        groupBox6.ResumeLayout(false);
        groupBox6.PerformLayout();
        groupBox7.ResumeLayout(false);
        groupBox8.ResumeLayout(false);
        m_qlmGroupBox.ResumeLayout(false);
        groupBox10.ResumeLayout(false);
        groupBox10.PerformLayout();
        groupBox11.ResumeLayout(false);
        groupBox11.PerformLayout();
        groupBox12.ResumeLayout(false);
        groupBox1.ResumeLayout(false);
        ((ISupportInitialize)m_errorProvider).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    private enum Direction
    {
        Unknown,
        Left,
        Right,
        Up,
        Down
    }
}