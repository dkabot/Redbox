using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Common.GUI.Functions
{
    public class QRInstallHelper : UserControl
    {
        private const string StateFile = "C:\\Program Files\\Redbox\\HALService\\bin\\qr.state";
        private const string DefaultStateFile = "C:\\Program Files\\Redbox\\HALService\\bin\\qr.defs.default";
        private readonly HardwareService m_hardwareService;
        private Button button3;
        private IContainer components;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private GroupBox groupBox4;
        private GroupBox groupBox5;
        private GroupBox groupBox6;
        private Label label1;
        private Label label2;
        private Label label3;
        private ListBox listBox1;
        private Button m_blinkButton;
        private Button m_blinkGreenArrow;
        private Button m_blinkRedButton;
        private Button m_checkStatus;
        private Button m_clearStatus;
        private TextBox m_comPortTextBox;
        private Button m_greenArrowOff;
        private Button m_greenArrowOn;
        private Button m_installIntoHALButton;
        private TextBox m_outputTextBox;
        private Button m_redArrowBlink;
        private Button m_redArrowOff;
        private Button m_redArrowOn;
        private Button m_restartPollingButton;
        private Button m_resumeDaemonButton;
        private Button m_scheduleJobButton;
        private InstallState m_state = InstallState.NotInstalled;
        private Button m_suspendDaemonButton;
        private TextBox m_suspendJobTB;
        private Button m_turnOffBackLight;
        private Button m_turnOffButton;
        private Button m_turnOffRedButton;
        private Button m_turnOnBackLight;
        private Button m_turnOnButton;
        private Button m_turnOnRedButton;
        private Button m_uninstallButton;

        public QRInstallHelper(HardwareService service, DeviceStatus status)
        {
            InitializeComponent();
            m_hardwareService = service != null ? service : throw new ArgumentNullException("Hardware service");
            State = (status & DeviceStatus.Configured) != 0 ? InstallState.Installed : InstallState.NotInstalled;
            InstalledComPortNumber = LocatePortViaRegistry();
        }

        private string InstalledComPortNumber
        {
            get => m_comPortTextBox.Text;
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;
                m_comPortTextBox.Text = FindPortNumber(value);
            }
        }

        private InstallState State
        {
            get => m_state;
            set
            {
                m_state = value;
                m_uninstallButton.Enabled = m_state == InstallState.Installed;
                m_installIntoHALButton.Enabled = InstallState.NotInstalled == m_state;
            }
        }

        private void SetToolButtonState(bool enabled)
        {
            m_suspendDaemonButton.Enabled = enabled;
            m_resumeDaemonButton.Enabled = enabled;
            m_scheduleJobButton.Enabled = enabled;
            m_turnOnBackLight.Enabled = enabled;
            m_turnOffBackLight.Enabled = enabled;
            m_greenArrowOff.Enabled = enabled;
            m_greenArrowOn.Enabled = enabled;
            m_blinkGreenArrow.Enabled = enabled;
            m_redArrowOff.Enabled = enabled;
            m_redArrowOn.Enabled = enabled;
            m_redArrowBlink.Enabled = enabled;
            m_checkStatus.Enabled = enabled;
            m_clearStatus.Enabled = enabled;
            m_turnOffButton.Enabled = enabled;
            m_turnOnButton.Enabled = enabled;
            m_blinkButton.Enabled = enabled;
            m_blinkRedButton.Enabled = enabled;
            m_turnOnRedButton.Enabled = enabled;
            m_turnOffRedButton.Enabled = enabled;
            m_restartPollingButton.Enabled = enabled;
        }

        private void WriteToInstallOutput(string fmt, params object[] stuff)
        {
            WriteToInstallOutput(string.Format(fmt, stuff));
        }

        private void WriteToInstallOutput(string msg)
        {
            msg += Environment.NewLine;
            m_outputTextBox.Text += msg;
            Application.DoEvents();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_outputTextBox.Text = string.Empty;
            int result;
            if (!int.TryParse(InstalledComPortNumber, out result))
            {
                WriteToInstallOutput("Port not recognized - INSTALL FAILED.");
            }
            else if (!UpdateConfiguration(true, result))
            {
                WriteToInstallOutput("Writing installed devices failed - INSTALL FAILED.");
            }
            else
            {
                WriteToInstallOutput("Successfully installed Quick Return device in HAL.");
                StartDevice();
            }
        }

        private void m_uninstallButton_Click(object sender, EventArgs e)
        {
            StopDevice();
            if (!UpdateConfiguration(false, new int?()))
                WriteToInstallOutput("FAILED to uninstall device in HAL.xml - uninstall FAILED.");
            else
                WriteToInstallOutput("Successfully uninstalled Quick Return from HAL.");
        }

        private bool UpdateConfiguration(bool install, int? newComPort)
        {
            if (!m_hardwareService
                    .ExecuteImmediate(
                        string.Format("SETCFG \"QuickReturnComPort\" \"{0}\" TYPE=CONTROLLER",
                            newComPort.HasValue ? string.Format("COM{0}", newComPort.Value) : (object)"NONE"),
                        out var _).Success)
                return false;
            if (install)
            {
                if (!File.Exists("C:\\Program Files\\Redbox\\HALService\\bin\\qr.state"))
                    if (File.Exists("C:\\Program Files\\Redbox\\HALService\\bin\\qr.defs.default"))
                        try
                        {
                            File.Copy("C:\\Program Files\\Redbox\\HALService\\bin\\qr.defs.default",
                                "C:\\Program Files\\Redbox\\HALService\\bin\\qr.state", false);
                        }
                        catch (Exception ex)
                        {
                            WriteToInstallOutput("Unable to copy {0} to {1}: error = {2}",
                                "C:\\Program Files\\Redbox\\HALService\\bin\\qr.defs.default",
                                "C:\\Program Files\\Redbox\\HALService\\bin\\qr.state", ex.Message);
                        }
            }
            else
            {
                ServiceLocator.Instance.GetService<IRuntimeService>()
                    .SafeDelete("C:\\Program Files\\Redbox\\HALService\\bin\\qr.state");
            }

            return true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            StopDevice();
        }

        private void m_greenArrowOff_Click(object sender, EventArgs e)
        {
            ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"GreenArrow\" MODE=\"Off\"");
        }

        private void m_greenArrowOn_Click(object sender, EventArgs e)
        {
            ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"GreenArrow\" MODE=\"On\"");
        }

        private void m_blinkGreenArrow_Click(object sender, EventArgs e)
        {
            ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"GreenArrow\" MODE=\"Blink\"");
        }

        private void m_redArrowOff_Click(object sender, EventArgs e)
        {
            ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"RedArrow\" MODE=\"Off\"");
        }

        private void m_redArrowOn_Click(object sender, EventArgs e)
        {
            ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"RedArrow\" MODE=\"On\"");
        }

        private void m_redArrowBlink_Click(object sender, EventArgs e)
        {
            ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"RedArrow\" MODE=\"Blink\"");
        }

        private void m_checkStatus_Click(object sender, EventArgs e)
        {
            HardwareJob job;
            if (m_hardwareService.ExecuteImmediate("QUICKRETURN CHECKATTBUTTON", out job).Success)
            {
                Stack<string> stack;
                if (job.GetStack(out stack).Success)
                {
                    if (stack.Count > 1)
                    {
                        stack.Pop();
                        listBox1.Items.Add(ConvertMessage(stack.Peek()));
                    }
                    else
                    {
                        listBox1.Items.Add("COMMUNICATION ERROR");
                    }
                }
                else
                {
                    listBox1.Items.Add("FAILURE");
                }
            }
            else
            {
                listBox1.Items.Add("FAILURE");
            }

            Application.DoEvents();
        }

        private void m_clearStatus_Click(object sender, EventArgs e)
        {
            ExecuteInstruction("QUICKRETURN CLEARATTBUTTON");
        }

        private void m_turnOffButton_Click(object sender, EventArgs e)
        {
            ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"GreenButton\" MODE=\"Off\"");
        }

        private void m_turnOnButton_Click(object sender, EventArgs e)
        {
            ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"GreenButton\" MODE=\"On\"");
        }

        private void m_blinkButton_Click(object sender, EventArgs e)
        {
            ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"GreenButton\" MODE=\"Blink\"");
        }

        private void m_turnOnBackLight_Click(object sender, EventArgs e)
        {
            ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"Background\" MODE=\"On\"");
        }

        private void m_turnOffBackLight_Click(object sender, EventArgs e)
        {
            ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"Background\" MODE=\"Off\"");
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"RedButton\" MODE=\"Off\"");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"RedButton\" MODE=\"On\"");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ExecuteInstruction("QUICKRETURN LEDSTATE LED=\"RedButton\" MODE=\"Blink\"");
        }

        private void ExecuteInstruction(string instruction)
        {
            HardwareJob job;
            var result = m_hardwareService.ExecuteImmediate(instruction, out job);
            if (result.Success)
                WriteToCommandOutput(result, job);
            else
                listBox1.Items.Add("FAILURE");
        }

        private string ConvertMessage(string msg)
        {
            if ("true".Equals(msg, StringComparison.CurrentCultureIgnoreCase))
                return "SUCCESS";
            return "false".Equals(msg, StringComparison.CurrentCultureIgnoreCase) ? "FAILURE" : msg;
        }

        private void WriteToCommandOutput(HardwareCommandResult result, HardwareJob job)
        {
            Stack<string> stack;
            if (job.GetStack(out stack).Success)
            {
                if (stack.Count > 0)
                    listBox1.Items.Add(ConvertMessage(stack.Peek()));
                else
                    listBox1.Items.Add("FAILURE");
            }
            else
            {
                listBox1.Items.Add("FAILURE");
            }

            Application.DoEvents();
        }

        private void button3_Click_2(object sender, EventArgs e)
        {
            HardwareJob job;
            if (!m_hardwareService.ExecuteImmediate("QUICKRETURN POLLSTATUS", out job).Success)
            {
                m_suspendJobTB.Text = "COMM Error";
            }
            else
            {
                Stack<string> stack;
                if (job.GetStack(out stack).Success)
                    m_suspendJobTB.Text = stack.Pop();
                else
                    m_suspendJobTB.Text = "COMM Error";
            }
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            var startTimeForm = new StartTimeForm();
            if (startTimeForm.ShowDialog() == DialogResult.Cancel)
                return;
            var schedule = new HardwareJobSchedule
            {
                StartTime = startTimeForm.StartTime.Value,
                Priority = HardwareJobPriority.High
            };
            HardwareJob job;
            var hardwareCommandResult =
                m_hardwareService.ScheduleJob("quick-return-restart", string.Empty, false, schedule, out job);
            if (hardwareCommandResult.Success)
                job.Pend();
            m_suspendJobTB.Text = hardwareCommandResult.Success ? "SUCCESS" : "COMMAND ERROR";
            Application.DoEvents();
        }

        private void button3_Click_3(object sender, EventArgs e)
        {
            SetToolButtonState(true);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            StartDevice();
        }

        private string LocatePortViaRegistry()
        {
            var registryKey1 = (RegistryKey)null;
            try
            {
                registryKey1 = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Enum\\FTDIBUS");
                if (registryKey1 == null)
                    return null;
                foreach (var subKeyName in registryKey1.GetSubKeyNames())
                    if (subKeyName.StartsWith("VID_0403+PID_6001"))
                    {
                        var registryKey2 = registryKey1.OpenSubKey(string.Format("{0}\\0000\\Control", subKeyName));
                        if (registryKey2 != null)
                        {
                            registryKey2.Close();
                            var registryKey3 =
                                registryKey1.OpenSubKey(string.Format("{0}\\0000\\Device Parameters", subKeyName));
                            if (registryKey3 != null)
                            {
                                var str = (string)registryKey3.GetValue("PortName");
                                registryKey3.Close();
                                return str;
                            }
                        }
                    }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                registryKey1?.Close();
            }
        }

        private string FindPortNumber(string port)
        {
            return !port.StartsWith("COM", StringComparison.CurrentCultureIgnoreCase)
                ? port
                : port.Substring(3, port.Length - 3);
        }

        private void StartDevice()
        {
            HardwareJob job;
            if (!m_hardwareService.ExecuteImmediate("QUICKRETURN RESTART", out job).Success)
            {
                m_suspendJobTB.Text = "COMMAND FAILED.";
                Application.DoEvents();
            }
            else
            {
                Stack<string> stack;
                job.GetStack(out stack);
                m_suspendJobTB.Text = stack.Count > 0 ? stack.Pop() : "COMMAND ERROR";
            }
        }

        private void StopDevice()
        {
            m_suspendJobTB.Text = m_hardwareService.ExecuteImmediate("QUICKRETURN HALTPOLL", out var _).Success
                ? "STOPPED"
                : "RUNNING";
            m_comPortTextBox.Text = string.Empty;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_comPortTextBox = new TextBox();
            label1 = new Label();
            m_installIntoHALButton = new Button();
            m_outputTextBox = new TextBox();
            m_uninstallButton = new Button();
            groupBox1 = new GroupBox();
            button3 = new Button();
            groupBox2 = new GroupBox();
            label2 = new Label();
            listBox1 = new ListBox();
            groupBox6 = new GroupBox();
            m_redArrowOff = new Button();
            m_greenArrowOff = new Button();
            m_redArrowOn = new Button();
            m_greenArrowOn = new Button();
            m_redArrowBlink = new Button();
            m_blinkGreenArrow = new Button();
            groupBox5 = new GroupBox();
            m_turnOnBackLight = new Button();
            m_turnOffBackLight = new Button();
            groupBox3 = new GroupBox();
            m_blinkRedButton = new Button();
            m_turnOnRedButton = new Button();
            m_turnOffRedButton = new Button();
            m_blinkButton = new Button();
            m_turnOnButton = new Button();
            m_turnOffButton = new Button();
            m_clearStatus = new Button();
            m_checkStatus = new Button();
            m_suspendDaemonButton = new Button();
            m_suspendJobTB = new TextBox();
            groupBox4 = new GroupBox();
            m_restartPollingButton = new Button();
            label3 = new Label();
            m_scheduleJobButton = new Button();
            m_resumeDaemonButton = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox6.SuspendLayout();
            groupBox5.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            SuspendLayout();
            m_comPortTextBox.Location = new Point(165, 20);
            m_comPortTextBox.Name = "m_comPortTextBox";
            m_comPortTextBox.Size = new Size(52, 20);
            m_comPortTextBox.TabIndex = 0;
            label1.AutoSize = true;
            label1.Location = new Point(6, 23);
            label1.Name = "label1";
            label1.Size = new Size(139, 13);
            label1.TabIndex = 1;
            label1.Text = "Install on COM port (e.g., 6):";
            m_installIntoHALButton.BackColor = Color.LightGray;
            m_installIntoHALButton.Location = new Point(9, 45);
            m_installIntoHALButton.Name = "m_installIntoHALButton";
            m_installIntoHALButton.Size = new Size(75, 61);
            m_installIntoHALButton.TabIndex = 2;
            m_installIntoHALButton.Text = "Install into HAL";
            m_installIntoHALButton.UseVisualStyleBackColor = false;
            m_installIntoHALButton.Click += button1_Click;
            m_outputTextBox.Location = new Point(9, 121);
            m_outputTextBox.Multiline = true;
            m_outputTextBox.Name = "m_outputTextBox";
            m_outputTextBox.ScrollBars = ScrollBars.Both;
            m_outputTextBox.Size = new Size(311, 158);
            m_outputTextBox.TabIndex = 3;
            m_uninstallButton.BackColor = Color.LightGray;
            m_uninstallButton.Location = new Point(99, 45);
            m_uninstallButton.Name = "m_uninstallButton";
            m_uninstallButton.Size = new Size(75, 61);
            m_uninstallButton.TabIndex = 4;
            m_uninstallButton.Text = "Uninstall from HAL ";
            m_uninstallButton.UseVisualStyleBackColor = false;
            m_uninstallButton.Click += m_uninstallButton_Click;
            groupBox1.Controls.Add(button3);
            groupBox1.Controls.Add(m_outputTextBox);
            groupBox1.Controls.Add(m_uninstallButton);
            groupBox1.Controls.Add(m_comPortTextBox);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(m_installIntoHALButton);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(350, 285);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "Installation";
            button3.BackColor = Color.LightGray;
            button3.Location = new Point(180, 46);
            button3.Name = "button3";
            button3.Size = new Size(75, 59);
            button3.TabIndex = 6;
            button3.Text = "Connect to HAL";
            button3.UseVisualStyleBackColor = false;
            button3.Click += button3_Click_3;
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(listBox1);
            groupBox2.Controls.Add(groupBox6);
            groupBox2.Controls.Add(groupBox5);
            groupBox2.Controls.Add(groupBox3);
            groupBox2.Location = new Point(399, 18);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(517, 469);
            groupBox2.TabIndex = 7;
            groupBox2.TabStop = false;
            groupBox2.Text = "Device Control";
            label2.AutoSize = true;
            label2.Location = new Point(324, 21);
            label2.Name = "label2";
            label2.Size = new Size(89, 13);
            label2.TabIndex = 20;
            label2.Text = "Command Output";
            listBox1.FormattingEnabled = true;
            listBox1.HorizontalScrollbar = true;
            listBox1.Location = new Point(327, 38);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(174, 420);
            listBox1.TabIndex = 19;
            groupBox6.Controls.Add(m_redArrowOff);
            groupBox6.Controls.Add(m_greenArrowOff);
            groupBox6.Controls.Add(m_redArrowOn);
            groupBox6.Controls.Add(m_greenArrowOn);
            groupBox6.Controls.Add(m_redArrowBlink);
            groupBox6.Controls.Add(m_blinkGreenArrow);
            groupBox6.Location = new Point(16, 115);
            groupBox6.Name = "groupBox6";
            groupBox6.Size = new Size(289, 144);
            groupBox6.TabIndex = 18;
            groupBox6.TabStop = false;
            groupBox6.Text = "Arrow Commands";
            m_redArrowOff.BackColor = Color.LightGray;
            m_redArrowOff.Enabled = false;
            m_redArrowOff.Location = new Point(6, 78);
            m_redArrowOff.Name = "m_redArrowOff";
            m_redArrowOff.Size = new Size(75, 57);
            m_redArrowOff.TabIndex = 4;
            m_redArrowOff.Text = "Turn Off Red Arrow";
            m_redArrowOff.UseVisualStyleBackColor = false;
            m_redArrowOff.Click += m_redArrowOff_Click;
            m_greenArrowOff.BackColor = Color.LightGray;
            m_greenArrowOff.Enabled = false;
            m_greenArrowOff.Location = new Point(6, 15);
            m_greenArrowOff.Name = "m_greenArrowOff";
            m_greenArrowOff.Size = new Size(75, 57);
            m_greenArrowOff.TabIndex = 2;
            m_greenArrowOff.Text = "Turn Off Green Arrow";
            m_greenArrowOff.UseVisualStyleBackColor = false;
            m_greenArrowOff.Click += m_greenArrowOff_Click;
            m_redArrowOn.BackColor = Color.LightGray;
            m_redArrowOn.Enabled = false;
            m_redArrowOn.Location = new Point(91, 78);
            m_redArrowOn.Name = "m_redArrowOn";
            m_redArrowOn.Size = new Size(75, 57);
            m_redArrowOn.TabIndex = 5;
            m_redArrowOn.Text = "Turn On Red Arrow";
            m_redArrowOn.UseVisualStyleBackColor = false;
            m_redArrowOn.Click += m_redArrowOn_Click;
            m_greenArrowOn.BackColor = Color.LightGray;
            m_greenArrowOn.Enabled = false;
            m_greenArrowOn.Location = new Point(91, 15);
            m_greenArrowOn.Name = "m_greenArrowOn";
            m_greenArrowOn.Size = new Size(75, 57);
            m_greenArrowOn.TabIndex = 1;
            m_greenArrowOn.Text = "Turn On Green Arrow";
            m_greenArrowOn.UseVisualStyleBackColor = false;
            m_greenArrowOn.Click += m_greenArrowOn_Click;
            m_redArrowBlink.BackColor = Color.LightGray;
            m_redArrowBlink.Enabled = false;
            m_redArrowBlink.Location = new Point(186, 78);
            m_redArrowBlink.Name = "m_redArrowBlink";
            m_redArrowBlink.Size = new Size(75, 57);
            m_redArrowBlink.TabIndex = 6;
            m_redArrowBlink.Text = "Blink Red Arrow";
            m_redArrowBlink.UseVisualStyleBackColor = false;
            m_redArrowBlink.Click += m_redArrowBlink_Click;
            m_blinkGreenArrow.BackColor = Color.LightGray;
            m_blinkGreenArrow.Enabled = false;
            m_blinkGreenArrow.Location = new Point(186, 15);
            m_blinkGreenArrow.Name = "m_blinkGreenArrow";
            m_blinkGreenArrow.Size = new Size(75, 57);
            m_blinkGreenArrow.TabIndex = 3;
            m_blinkGreenArrow.Text = "Blink Green Arrow";
            m_blinkGreenArrow.UseVisualStyleBackColor = false;
            m_blinkGreenArrow.Click += m_blinkGreenArrow_Click;
            groupBox5.Controls.Add(m_turnOnBackLight);
            groupBox5.Controls.Add(m_turnOffBackLight);
            groupBox5.Location = new Point(16, 19);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(289, 81);
            groupBox5.TabIndex = 17;
            groupBox5.TabStop = false;
            groupBox5.Text = "Backlight Commands";
            m_turnOnBackLight.BackColor = Color.LightGray;
            m_turnOnBackLight.Enabled = false;
            m_turnOnBackLight.Location = new Point(6, 19);
            m_turnOnBackLight.Name = "m_turnOnBackLight";
            m_turnOnBackLight.Size = new Size(106, 52);
            m_turnOnBackLight.TabIndex = 14;
            m_turnOnBackLight.Text = "Turn On BackLight";
            m_turnOnBackLight.UseVisualStyleBackColor = false;
            m_turnOnBackLight.Click += m_turnOnBackLight_Click;
            m_turnOffBackLight.BackColor = Color.LightGray;
            m_turnOffBackLight.Enabled = false;
            m_turnOffBackLight.Location = new Point(149, 19);
            m_turnOffBackLight.Name = "m_turnOffBackLight";
            m_turnOffBackLight.Size = new Size(94, 52);
            m_turnOffBackLight.TabIndex = 15;
            m_turnOffBackLight.Text = "Turn Off BackLight";
            m_turnOffBackLight.UseVisualStyleBackColor = false;
            m_turnOffBackLight.Click += m_turnOffBackLight_Click;
            groupBox3.Controls.Add(m_blinkRedButton);
            groupBox3.Controls.Add(m_turnOnRedButton);
            groupBox3.Controls.Add(m_turnOffRedButton);
            groupBox3.Controls.Add(m_blinkButton);
            groupBox3.Controls.Add(m_turnOnButton);
            groupBox3.Controls.Add(m_turnOffButton);
            groupBox3.Controls.Add(m_clearStatus);
            groupBox3.Controls.Add(m_checkStatus);
            groupBox3.Location = new Point(16, 265);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(289, 190);
            groupBox3.TabIndex = 11;
            groupBox3.TabStop = false;
            groupBox3.Text = "Button Commands";
            m_blinkRedButton.BackColor = Color.LightGray;
            m_blinkRedButton.Enabled = false;
            m_blinkRedButton.Location = new Point(172, 129);
            m_blinkRedButton.Name = "m_blinkRedButton";
            m_blinkRedButton.Size = new Size(75, 43);
            m_blinkRedButton.TabIndex = 12;
            m_blinkRedButton.Text = "Blink Red";
            m_blinkRedButton.UseVisualStyleBackColor = false;
            m_blinkRedButton.Click += button5_Click;
            m_turnOnRedButton.BackColor = Color.LightGray;
            m_turnOnRedButton.Enabled = false;
            m_turnOnRedButton.Location = new Point(91, 129);
            m_turnOnRedButton.Name = "m_turnOnRedButton";
            m_turnOnRedButton.Size = new Size(75, 43);
            m_turnOnRedButton.TabIndex = 11;
            m_turnOnRedButton.Text = "Turn On Red";
            m_turnOnRedButton.UseVisualStyleBackColor = false;
            m_turnOnRedButton.Click += button4_Click;
            m_turnOffRedButton.BackColor = Color.LightGray;
            m_turnOffRedButton.Enabled = false;
            m_turnOffRedButton.Location = new Point(10, 129);
            m_turnOffRedButton.Name = "m_turnOffRedButton";
            m_turnOffRedButton.Size = new Size(75, 43);
            m_turnOffRedButton.TabIndex = 10;
            m_turnOffRedButton.Text = "Turn Off Red";
            m_turnOffRedButton.UseVisualStyleBackColor = false;
            m_turnOffRedButton.Click += button3_Click_1;
            m_blinkButton.BackColor = Color.LightGray;
            m_blinkButton.Enabled = false;
            m_blinkButton.Location = new Point(172, 80);
            m_blinkButton.Name = "m_blinkButton";
            m_blinkButton.Size = new Size(75, 43);
            m_blinkButton.TabIndex = 5;
            m_blinkButton.Text = "Blink Green";
            m_blinkButton.UseVisualStyleBackColor = false;
            m_blinkButton.Click += m_blinkButton_Click;
            m_turnOnButton.BackColor = Color.LightGray;
            m_turnOnButton.Enabled = false;
            m_turnOnButton.Location = new Point(91, 80);
            m_turnOnButton.Name = "m_turnOnButton";
            m_turnOnButton.Size = new Size(75, 43);
            m_turnOnButton.TabIndex = 4;
            m_turnOnButton.Text = "Turn On Green";
            m_turnOnButton.UseVisualStyleBackColor = false;
            m_turnOnButton.Click += m_turnOnButton_Click;
            m_turnOffButton.BackColor = Color.LightGray;
            m_turnOffButton.Enabled = false;
            m_turnOffButton.Location = new Point(10, 80);
            m_turnOffButton.Name = "m_turnOffButton";
            m_turnOffButton.Size = new Size(75, 43);
            m_turnOffButton.TabIndex = 3;
            m_turnOffButton.Text = "Turn Off Green";
            m_turnOffButton.UseVisualStyleBackColor = false;
            m_turnOffButton.Click += m_turnOffButton_Click;
            m_clearStatus.BackColor = Color.LightGray;
            m_clearStatus.Enabled = false;
            m_clearStatus.Location = new Point(131, 19);
            m_clearStatus.Name = "m_clearStatus";
            m_clearStatus.Size = new Size(95, 44);
            m_clearStatus.TabIndex = 2;
            m_clearStatus.Text = "Clear Status";
            m_clearStatus.UseVisualStyleBackColor = false;
            m_clearStatus.Click += m_clearStatus_Click;
            m_checkStatus.BackColor = Color.LightGray;
            m_checkStatus.Enabled = false;
            m_checkStatus.Location = new Point(10, 19);
            m_checkStatus.Name = "m_checkStatus";
            m_checkStatus.Size = new Size(100, 44);
            m_checkStatus.TabIndex = 0;
            m_checkStatus.Text = "Check Status";
            m_checkStatus.UseVisualStyleBackColor = false;
            m_checkStatus.Click += m_checkStatus_Click;
            m_suspendDaemonButton.BackColor = Color.LightGray;
            m_suspendDaemonButton.Enabled = false;
            m_suspendDaemonButton.Location = new Point(6, 19);
            m_suspendDaemonButton.Name = "m_suspendDaemonButton";
            m_suspendDaemonButton.Size = new Size(75, 61);
            m_suspendDaemonButton.TabIndex = 0;
            m_suspendDaemonButton.Text = "Stop QR Polling";
            m_suspendDaemonButton.UseVisualStyleBackColor = false;
            m_suspendDaemonButton.Click += button3_Click;
            m_suspendJobTB.Location = new Point(111, 100);
            m_suspendJobTB.Name = "m_suspendJobTB";
            m_suspendJobTB.ReadOnly = true;
            m_suspendJobTB.Size = new Size(100, 20);
            m_suspendJobTB.TabIndex = 12;
            groupBox4.Controls.Add(m_restartPollingButton);
            groupBox4.Controls.Add(label3);
            groupBox4.Controls.Add(m_scheduleJobButton);
            groupBox4.Controls.Add(m_resumeDaemonButton);
            groupBox4.Controls.Add(m_suspendDaemonButton);
            groupBox4.Controls.Add(m_suspendJobTB);
            groupBox4.Location = new Point(12, 312);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(350, 143);
            groupBox4.TabIndex = 8;
            groupBox4.TabStop = false;
            groupBox4.Text = "Poll Job Control";
            m_restartPollingButton.BackColor = Color.LightGray;
            m_restartPollingButton.Enabled = false;
            m_restartPollingButton.Location = new Point(87, 19);
            m_restartPollingButton.Name = "m_restartPollingButton";
            m_restartPollingButton.Size = new Size(75, 61);
            m_restartPollingButton.TabIndex = 16;
            m_restartPollingButton.Text = "Restart QR Polling";
            m_restartPollingButton.UseVisualStyleBackColor = false;
            m_restartPollingButton.Click += button1_Click_1;
            label3.AutoSize = true;
            label3.Location = new Point(108, 81);
            label3.Name = "label3";
            label3.Size = new Size(37, 13);
            label3.TabIndex = 15;
            label3.Text = "Status";
            m_scheduleJobButton.BackColor = Color.LightGray;
            m_scheduleJobButton.Enabled = false;
            m_scheduleJobButton.Location = new Point(249, 19);
            m_scheduleJobButton.Name = "m_scheduleJobButton";
            m_scheduleJobButton.Size = new Size(75, 61);
            m_scheduleJobButton.TabIndex = 14;
            m_scheduleJobButton.Text = "Schedule QR Job in Future";
            m_scheduleJobButton.UseVisualStyleBackColor = false;
            m_scheduleJobButton.Click += button4_Click_1;
            m_resumeDaemonButton.BackColor = Color.LightGray;
            m_resumeDaemonButton.Enabled = false;
            m_resumeDaemonButton.Location = new Point(168, 19);
            m_resumeDaemonButton.Name = "m_resumeDaemonButton";
            m_resumeDaemonButton.Size = new Size(75, 61);
            m_resumeDaemonButton.TabIndex = 13;
            m_resumeDaemonButton.Text = "QR Poll Status";
            m_resumeDaemonButton.UseVisualStyleBackColor = false;
            m_resumeDaemonButton.Click += button3_Click_2;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupBox4);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Name = nameof(QRInstallHelper);
            Size = new Size(937, 504);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox6.ResumeLayout(false);
            groupBox5.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ResumeLayout(false);
        }

        private enum InstallState
        {
            Installed,
            NotInstalled
        }
    }
}