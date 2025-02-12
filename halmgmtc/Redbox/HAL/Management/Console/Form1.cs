using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Management.Console.Properties;

namespace Redbox.HAL.Management.Console
{
    public class Form1 : Form
    {
        private ToolStripMenuItem aboutHalManagementConsoleToolStripMenuItem;
        private ToolStripMenuItem autoResumeJobOnScheduleToolStripMenuItem;
        private ToolStripMenuItem backupHalConfigurationToolStripMenuItem;
        private IContainer components;
        private ToolStripMenuItem connectToolStripMenuItem;
        private ToolStripMenuItem dataInfoAndConfigurationToolStripMenuItem;
        private ToolStripMenuItem disconnectToolStripMenuItem;
        private ToolStripMenuItem emptySearchPatternToolToolStripMenuItem;
        private ToolStripMenuItem errorListToolStripMenuItem;
        private ToolStripMenuItem executeScriptToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem getInventoryStateToolStripMenuItem;
        private ToolStripMenuItem getMachineListToolStripMenuItem;
        private ToolStripMenuItem getMaintenanceListToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem immediateWindowToolStripMenuItem;
        private ToolStripMenuItem initializeToolStripMenuItem;
        private ToolStripMenuItem jobControlToolStripMenuItem;
        private ToolStripMenuItem jobListToolStripMenuItem;
        private ToolStripMenuItem lockSettingsToolStripMenuItem;
        private ToolStripButton m_connectButton;
        private ToolStripButton m_disconnectButton;
        internal SplitContainer m_eastWestSplitContainer;
        private ToolStripButton m_executeScriptButton;
        private ToolStripButton m_getMachineInfoButton;
        private ToolStripButton m_getMaintListButton;
        private ToolStripButton m_initButton;
        private ToolStripButton m_initialNightSyncButton;
        internal TabControl m_listViewTabControl;
        private ToolStripButton m_lockButton;
        private MenuStrip m_menuStrip;
        internal SplitContainer m_northSouthSplitContainer;
        private ToolStripLabel m_profileLabel;
        private ToolStripButton m_returnButton;
        private ToolStripButton m_saveButton;
        private ToolStripButton m_softSyncButton;
        private ToolStrip m_standardToolStrip;
        private ToolStripButton m_syncButton;
        private ToolStripButton m_thinButton;
        private Timer m_timer;
        private ToolStripButton m_unloadButton;
        private ToolStripButton m_unloadThinButton;
        private ToolStripButton m_unlockButton;
        private ToolStripButton m_vendButton;
        private ToolStripMenuItem newInstallSyncToolStripMenuItem;
        private ToolStripMenuItem outputToolStripMenuItem;
        private ToolStripMenuItem pickerSensorsToolStripMenuItem;
        private ToolStripMenuItem programEventsToolStripMenuItem;
        private ToolStripMenuItem programResultsToolStripMenuItem;
        private ToolStripMenuItem programsToolStripMenuItem;
        private ToolStripMenuItem propertiesWindowToolStripMenuItem;
        private ToolStripMenuItem qlmUnloadToolStripMenuItem;
        private ToolStripMenuItem reloadHalConfigurationToolStripMenuItem;
        private ToolStripMenuItem resetInventoryStateToolStripMenuItem;
        private ToolStripMenuItem returnToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem softSyncToolStripMenuItem;
        private ToolStripMenuItem stackToolStripMenuItem;
        private ToolStripMenuItem standardToolStripMenuItem;
        private ToolStripMenuItem symbolsToolStripMenuItem;
        private ToolStripMenuItem syncToolStripMenuItem;
        private ToolStripMenuItem thinToolStripMenuItem;
        private ToolStripMenuItem toolbarsToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator11;
        private ToolStripSeparator toolStripSeparator12;
        private ToolStripSeparator toolStripSeparator13;
        private ToolStripSeparator toolStripSeparator14;
        private ToolStripSeparator toolStripSeparator15;
        private ToolStripSeparator toolStripSeparator16;
        private ToolStripSeparator toolStripSeparator17;
        private ToolStripSeparator toolStripSeparator26;
        private ToolStripSeparator toolStripSeparator27;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripSeparator toolStripSeparator35;
        private ToolStripSeparator toolStripSeparator36;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripSeparator toolStripSeparator40;
        private ToolStripSeparator toolStripSeparator41;
        private ToolStripSeparator toolStripSeparator42;
        private ToolStripSeparator toolStripSeparator43;
        private ToolStripSeparator toolStripSeparator44;
        private ToolStripSeparator toolStripSeparator45;
        private ToolStripSeparator toolStripSeparator46;
        private ToolStripSeparator toolStripSeparator47;
        private ToolStripSeparator toolStripSeparator49;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripSeparator toolStripSeparator50;
        private ToolStripMenuItem unloadThinToolStripMenuItem;
        private ToolStripMenuItem unlockSettingsToolStripMenuItem;
        private ToolStripMenuItem vendToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;

        public Form1()
        {
            EnvironmentHelper.IsLocked = !Settings.Default.DeveloperMode;
            InitializeToolBars();
            LoadConfiguration();
            InitializeComponent();
            InitializeListViews();
            FormClosing += OnFormClose;
            var jobDropDownMenu =
                new JobDropDownMenu(JobComboBox.Instance.GetSelectedJobList, UpdateSource.JobComboBox, true);
            var toolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem.DropDown = jobDropDownMenu;
            toolStripMenuItem.Name = "JOB";
            toolStripMenuItem.Text = "&Job";
            m_menuStrip.Items.Insert(3, toolStripMenuItem);
            JobComboBox.Instance.SelectedIndexChanged += jobDropDownMenu.UpdateButtons;
            JobComboBox.Instance.EnabledChanged += jobDropDownMenu.UpdateButtons;
            JobComboBox.Instance.TextChanged += jobDropDownMenu.UpdateButtons;
            JobHelper.JobList.ListChanged += jobDropDownMenu.UpdateButtons;
            m_eastWestSplitContainer.Panel1.Controls.Add(LeftPanel.Instance);
            m_northSouthSplitContainer.Panel1.Controls.Add(TouchScreenAccess.Instance);
            m_northSouthSplitContainer.Panel2.Controls.Add(ListViewTabControl.Instance);
            ProfileManager.Instance.Connected += (_param1, _param2) =>
            {
                m_timer.Enabled = true;
                TouchScreenAccess.Instance.Enabled = true;
                ImmediateWindow.Instance.Enabled = true;
                LeftPanel.Instance.Enabled = true;
                m_disconnectButton.Enabled = true;
                disconnectToolStripMenuItem.Enabled = true;
                connectToolStripMenuItem.Enabled = false;
                m_connectButton.Enabled = false;
                ToggleLocks(EnvironmentHelper.IsLocked);
                UpdateProgramButtons();
                Refresh();
            };
            ProfileManager.Instance.Disconnected += (_param1, _param2) =>
            {
                m_timer.Enabled = false;
                TouchScreenAccess.Instance.Enabled = false;
                ImmediateWindow.Instance.Enabled = false;
                LeftPanel.Instance.Enabled = false;
                UpdateProgramButtons();
                m_connectButton.Enabled = true;
                connectToolStripMenuItem.Enabled = true;
                disconnectToolStripMenuItem.Enabled = false;
                m_disconnectButton.Enabled = false;
                m_lockButton.Enabled = false;
                lockSettingsToolStripMenuItem.Enabled = false;
                m_unlockButton.Enabled = false;
                unlockSettingsToolStripMenuItem.Enabled = false;
                thinToolStripMenuItem.Enabled = false;
                m_thinButton.Enabled = false;
                qlmUnloadToolStripMenuItem.Enabled = false;
                m_unloadButton.Enabled = false;
                unloadThinToolStripMenuItem.Enabled = false;
                m_unloadThinButton.Enabled = false;
                m_initialNightSyncButton.Enabled = false;
                newInstallSyncToolStripMenuItem.Enabled = false;
                immediateWindowToolStripMenuItem.Enabled = false;
                propertiesWindowToolStripMenuItem.Enabled = false;
                outputToolStripMenuItem.Enabled = false;
                getInventoryStateToolStripMenuItem.Enabled = false;
                resetInventoryStateToolStripMenuItem.Enabled = false;
                EnvironmentHelper.IsDirty = false;
                Refresh();
            };
            EnvironmentHelper.SaveableStatusChanged += OnSaveableStateChanged;
            EnvironmentHelper.LockStatusChanged += OnLockStateChanged;
            LockControls();
            InitializeExteriorHandlers();
            if (EnvironmentHelper.IsLocked)
                return;
            DevMode();
        }

        private bool FormDisposed { get; set; }

        public void CloseForm()
        {
            if (FormDisposed)
                return;
            FormDisposed = true;
            var service = ProfileManager.Instance.Service;
            if (service != null)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(" VENDDOOR CLOSE");
                stringBuilder.AppendLine(" GRIPPER RENT");
                stringBuilder.AppendLine(" GRIPPER RETRACT");
                stringBuilder.AppendLine(" SENSOR PICKER-OFF");
                stringBuilder.AppendLine(" ROLLER STOP");
                stringBuilder.AppendLine(" RINGLIGHT OFF");
                stringBuilder.AppendLine(" CLEAR");
                service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
            }

            Application.Exit();
        }

        private void OnLockStateChanged(object sender, BoolEventArgs e)
        {
            ToggleLocks(e.State);
        }

        private void OnSaveableStateChanged(object sender, BoolEventArgs e)
        {
            m_saveButton.Enabled = e.State;
            saveToolStripMenuItem.Enabled = e.State;
        }

        private void InitializeExteriorHandlers()
        {
            connectToolStripMenuItem.Click += ProfileManager.Instance.OnConnect;
            m_connectButton.Click += ProfileManager.Instance.OnConnect;
            disconnectToolStripMenuItem.Click += ProfileManager.Instance.OnDisconnect;
            m_disconnectButton.Click += ProfileManager.Instance.OnDisconnect;
            lockSettingsToolStripMenuItem.Click += EnvironmentHelper.OnLock;
            m_lockButton.Click += EnvironmentHelper.OnLock;
            unlockSettingsToolStripMenuItem.Click += EnvironmentHelper.OnUnlock;
            m_unlockButton.Click += EnvironmentHelper.OnUnlock;
            saveToolStripMenuItem.Click += EnvironmentHelper.OnSave;
            m_saveButton.Click += EnvironmentHelper.OnSave;
        }

        private void LoadConfiguration()
        {
        }

        private void InitializeListViews()
        {
            ListViewFactory.Instance.MakeTab(ListViewNames.Job, false);
            ListViewFactory.Instance.MakeTab(ListViewNames.Stack, false);
            ListViewFactory.Instance.MakeTab(ListViewNames.Symbols, false);
            ListViewFactory.Instance.MakeTab(ListViewNames.ProgramEvents, false);
            ListViewTabControl.Instance.SetFocus(ListViewNames.Job);
        }

        private void InitializeToolBars()
        {
            var toolStrip = (ToolStrip)new JobToolStrip(JobComboBox.Instance.GetSelectedJobList,
                UpdateSource.JobComboBox, true);
            toolStrip.SuspendLayout();
            toolStrip.Name = "JobToolStrip";
            toolStrip.RenderMode = ToolStripRenderMode.Professional;
            toolStrip.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            toolStrip.Dock = DockStyle.Top;
            toolStrip.Location = new Point(0, 48);
            toolStrip.Size = new Size(1024, 25);
            toolStrip.TabIndex = 2;
            toolStrip.Text = "Job Tool Strip";
            toolStrip.ResumeLayout(false);
            Controls.Add(toolStrip);
            var jobToolStrip =
                new JobToolStrip(JobComboBox.Instance.GetSelectedJobList, UpdateSource.JobComboBox, true);
            JobComboBox.Instance.SelectedIndexChanged += (toolStrip as JobToolStrip).UpdateButtons;
            JobComboBox.Instance.EnabledChanged += (toolStrip as JobToolStrip).UpdateButtons;
            JobComboBox.Instance.TextChanged += (toolStrip as JobToolStrip).UpdateButtons;
            JobHelper.JobList.ListChanged += (toolStrip as JobToolStrip).UpdateButtons;
            toolStrip.Items.Insert(0, JobComboBox.Instance);
            var items = toolStrip.Items;
            var toolStripLabel = new ToolStripLabel();
            toolStripLabel.Text = "Job: ";
            items.Insert(0, toolStripLabel);
            toolStrip.ResumeLayout();
        }

        private void UpdateProgramButtons()
        {
            m_executeScriptButton.Enabled = ProfileManager.Instance.IsConnected;
            m_syncButton.Enabled = ProfileManager.Instance.IsConnected;
            m_returnButton.Enabled = ProfileManager.Instance.IsConnected;
            m_vendButton.Enabled = ProfileManager.Instance.IsConnected;
            m_softSyncButton.Enabled = ProfileManager.Instance.IsConnected;
            m_initButton.Enabled = ProfileManager.Instance.IsConnected;
            m_getMachineInfoButton.Enabled = ProfileManager.Instance.IsConnected;
            m_getMaintListButton.Enabled = ProfileManager.Instance.IsConnected;
            m_thinButton.Enabled = ProfileManager.Instance.IsConnected && !EnvironmentHelper.IsLocked;
            m_unloadButton.Enabled = ProfileManager.Instance.IsConnected && !EnvironmentHelper.IsLocked;
            m_unloadThinButton.Enabled = ProfileManager.Instance.IsConnected && !EnvironmentHelper.IsLocked;
            m_initialNightSyncButton.Enabled = ProfileManager.Instance.IsConnected && !EnvironmentHelper.IsLocked;
            returnToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected;
            vendToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected;
            initializeToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected;
            softSyncToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected;
            syncToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected;
            executeScriptToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected;
            getMachineListToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected;
            getMaintenanceListToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected;
            thinToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected && !EnvironmentHelper.IsLocked;
            qlmUnloadToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected && !EnvironmentHelper.IsLocked;
            unloadThinToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected && !EnvironmentHelper.IsLocked;
            newInstallSyncToolStripMenuItem.Enabled =
                ProfileManager.Instance.IsConnected && !EnvironmentHelper.IsLocked;
            dataInfoAndConfigurationToolStripMenuItem.Enabled = ProfileManager.Instance.IsConnected;
        }

        private void ToggleLocks(bool locked)
        {
            m_unlockButton.Enabled = locked;
            unlockSettingsToolStripMenuItem.Enabled = locked;
            m_lockButton.Enabled = !locked;
            lockSettingsToolStripMenuItem.Enabled = !locked;
        }

        private void OnGlobalTimer(object sender, EventArgs e)
        {
            ListViewTabControl.Instance.RefreshTab();
        }

        private void OnScheduleScriptByTag(object sender, EventArgs e)
        {
            if (!(sender is ToolStripItem toolStripItem))
                return;
            var lower = toolStripItem.Tag.ToString().ToLower();
            var label = toolStripItem.Tag.ToString();
            var schedule = new HardwareJobSchedule();
            schedule.Priority = HardwareJobPriority.Normal;
            var num = autoResumeJobOnScheduleToolStripMenuItem.Checked ? 1 : 0;
            CommonFunctions.ScheduleJob(lower, label, schedule, num != 0);
        }

        private void OnMenuListViewToggle(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem toolStripMenuItem) || toolStripMenuItem.Tag == null)
                return;
            var tag = (ListViewNames)toolStripMenuItem.Tag;
            var tab = ListViewTabControl.Instance.Find(tag);
            if (toolStripMenuItem.Checked)
            {
                if (tab == null)
                    ListViewFactory.Instance.MakeTab(tag, ProfileManager.Instance.IsConnected);
                if (tag != ListViewNames.Errors)
                    return;
                ErrorListView.Instance.KeepOpenOnSuccessfulInstruction = true;
            }
            else
            {
                ListViewTabControl.Instance.Remove(tab);
            }
        }

        private void OnMenuLeftSideToggle(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem toolStripMenuItem) || toolStripMenuItem.Tag == null)
                return;
            var tag = (LeftPanelTab)toolStripMenuItem.Tag;
            if (toolStripMenuItem.Checked)
                LeftPanel.Instance.AddTab(tag);
            else
                LeftPanel.Instance.RemoveTab(tag);
        }

        private void OnMenuHelpAbout(object sender, EventArgs e)
        {
            var label1 = new Label();
            label1.Dock = DockStyle.Fill;
            label1.Image = Resources.hal_splash;
            var label2 = new Label();
            label2.Text = string.Format("Version: {0}", Program.Version);
            label2.BackColor = Color.Maroon;
            label2.ForeColor = Color.White;
            label2.AutoSize = true;
            label2.Font = new Font("Arial", 13f, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(40, label1.Image.Size.Height / 2 - 20);
            var f = new Form();
            f.AutoSize = false;
            f.Size = label1.Image.Size;
            f.FormBorderStyle = FormBorderStyle.None;
            label1.Click += (_param1, _param2) => f.Close();
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Controls.Add(label2);
            f.Controls.Add(label1);
            f.Show();
        }

        private void OnMenuToolsDeckConfiguration(object sender, EventArgs e)
        {
            try
            {
                var num = (int)new DeckConfigurationForm(ProfileManager.Instance.Service, true).ShowDialog();
            }
            catch (Exception ex)
            {
                OutputWindow.Instance.Append("Unable to run Decks form.");
                OutputWindow.Instance.Append(ex.Message);
            }

            LeftPanel.Instance.ReloadPropertiesConfiguration();
        }

        private void OnMenuToolsExportInventory(object sender, EventArgs e)
        {
            var inventoryState = ProfileManager.Instance.Service.GetInventoryState();
            if (!inventoryState.Success)
            {
                CommonFunctions.ProcessCommandResult(inventoryState);
            }
            else
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "XML|*.xml";
                saveFileDialog.Title = "Export Inventory";
                if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName))
                    return;
                using (var streamWriter = new StreamWriter(saveFileDialog.FileName, false))
                {
                    streamWriter.Write(inventoryState.CommandMessages[0]);
                }
            }
        }

        private void OnMenuToolsImportInventory(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML|*.xml";
            openFileDialog.Title = "Import Inventory";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            string end;
            using (var streamReader = new StreamReader(openFileDialog.FileName))
            {
                end = streamReader.ReadToEnd();
            }

            var result = ProfileManager.Instance.Service.SetInventoryState(end);
            CommonFunctions.ProcessCommandResult(result);
            if (result.Success)
                return;
            var num = (int)MessageBox.Show("Import Inventory Failed.", "Failed to import inventory.",
                MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        private void OnMenuToolsExportXml(object sender, EventArgs e)
        {
            var inventoryState = ProfileManager.Instance.Service.GetInventoryState();
            if (!inventoryState.Success)
            {
                CommonFunctions.ProcessCommandResult(inventoryState);
            }
            else
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "XML|*.xml";
                saveFileDialog.Title = "Export Inventory xml";
                if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName) ||
                    !File.Exists(saveFileDialog.FileName))
                    return;
                using (var streamWriter = new StreamWriter(saveFileDialog.FileName, false))
                {
                    streamWriter.Write(inventoryState.CommandMessages[0]);
                }
            }
        }

        private void OnMenuToolsImportXml(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML|*.xml";
            openFileDialog.Title = "Import Inventory xml";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            string end;
            using (var streamReader = new StreamReader(openFileDialog.FileName))
            {
                end = streamReader.ReadToEnd();
            }

            CommonFunctions.ProcessCommandResult(ProfileManager.Instance.Service.SetInventoryState(end));
        }

        private void OnMenuToolsESPTool(object sender, EventArgs e)
        {
        }

        private void OnSync(object sender, EventArgs e)
        {
            var slotRangeForm = new SlotRangeForm(true);
            if (slotRangeForm.ShowDialog() != DialogResult.OK)
                return;
            var service = ProfileManager.Instance.Service;
            var range = slotRangeForm.Range;
            var schedule = new HardwareJobSchedule();
            schedule.Priority = HardwareJobPriority.Normal;
            HardwareJob hardwareJob;
            var result = service.HardSync(range, schedule, out hardwareJob);
            if (result.Success && autoResumeJobOnScheduleToolStripMenuItem.Checked)
                hardwareJob.Resume();
            CommonFunctions.ProcessCommandResult(result);
        }

        private void OnVend(object sender, EventArgs e)
        {
            var barcodeInputForm1 = new BarcodeInputForm();
            barcodeInputForm1.Text = "Vend Items";
            var barcodeInputForm2 = barcodeInputForm1;
            if (barcodeInputForm2.ShowDialog() != DialogResult.OK || barcodeInputForm2.Barcodes.Length == 0)
                return;
            var service = ProfileManager.Instance.Service;
            var barcodes = barcodeInputForm2.Barcodes;
            var schedule = new HardwareJobSchedule();
            schedule.Priority = HardwareJobPriority.Normal;
            HardwareJob hardwareJob;
            var result = service.Vend(barcodes, schedule, out hardwareJob);
            if (result.Success && autoResumeJobOnScheduleToolStripMenuItem.Checked)
                hardwareJob.Resume();
            CommonFunctions.ProcessCommandResult(result);
        }

        private void OnNewInstallSync(object sender, EventArgs e)
        {
            var slotRangeForm = new SlotRangeForm(true);
            if (slotRangeForm.ShowDialog() != DialogResult.OK)
                return;
            var service = ProfileManager.Instance.Service;
            var range = slotRangeForm.Range;
            var schedule = new HardwareJobSchedule();
            schedule.Priority = HardwareJobPriority.Normal;
            HardwareJob hardwareJob;
            var result = service.HardSync(range, schedule, out hardwareJob);
            if (result.Success && autoResumeJobOnScheduleToolStripMenuItem.Checked)
                hardwareJob.Resume();
            CommonFunctions.ProcessCommandResult(result);
        }

        private void m_getMaintListButton_Click(object sender, EventArgs e)
        {
        }

        private void m_getMachineInfoButton_Click(object sender, EventArgs e)
        {
        }

        private void OnExecuteScript(object sender, EventArgs e)
        {
            var executeScriptForm = new ExecuteScriptForm();
            if (executeScriptForm.ShowDialog() == DialogResult.Cancel)
                return;
            var service = ProfileManager.Instance.Service;
            var selectedScript = executeScriptForm.SelectedScript;
            var schedule = new HardwareJobSchedule();
            schedule.Priority = HardwareJobPriority.Normal;
            HardwareJob hardwareJob;
            CommonFunctions.ProcessCommandResult(service.ScheduleJob(selectedScript, null, false, schedule, out hardwareJob));
        }

        private void LockControls()
        {
            EnvironmentHelper.LockControl(thinToolStripMenuItem);
            EnvironmentHelper.LockControl(m_thinButton);
            EnvironmentHelper.LockControl(qlmUnloadToolStripMenuItem);
            EnvironmentHelper.LockControl(m_unloadButton);
            EnvironmentHelper.LockControl(unloadThinToolStripMenuItem);
            EnvironmentHelper.LockControl(m_unloadThinButton);
            EnvironmentHelper.LockControl(m_initialNightSyncButton);
            EnvironmentHelper.LockControl(newInstallSyncToolStripMenuItem);
            EnvironmentHelper.LockControl(immediateWindowToolStripMenuItem);
            EnvironmentHelper.LockControl(propertiesWindowToolStripMenuItem);
            EnvironmentHelper.LockControl(outputToolStripMenuItem);
            EnvironmentHelper.LockControl(getInventoryStateToolStripMenuItem);
            EnvironmentHelper.LockControl(resetInventoryStateToolStripMenuItem);
        }

        private void OnFormClose(object sender, FormClosingEventArgs e)
        {
            CloseForm();
        }

        private void OnClose(object sender, EventArgs e)
        {
            CloseForm();
        }

        private void DevMode()
        {
            LeftPanel.Instance.AddTab(LeftPanelTab.Immediate);
            immediateWindowToolStripMenuItem.Checked = true;
            ListViewFactory.Instance.MakeTab(ListViewNames.OutputWindow, true);
            outputToolStripMenuItem.Checked = true;
            autoResumeJobOnScheduleToolStripMenuItem.Checked = true;
            ProfileManager.Instance.Connected += (_param1, _param2) =>
            {
                CommonFunctions.ProcessCommandResult(
                    ProfileManager.Instance.Service.ExecuteImmediate("NOP", out var _));
                EnvironmentHelper.IsLocked = EnvironmentHelper.IsLocked;
            };
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
            var componentResourceManager = new ComponentResourceManager(typeof(Form1));
            m_menuStrip = new MenuStrip();
            toolStripMenuItem1 = new ToolStripMenuItem();
            connectToolStripMenuItem = new ToolStripMenuItem();
            disconnectToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            saveToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            lockSettingsToolStripMenuItem = new ToolStripMenuItem();
            unlockSettingsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            toolbarsToolStripMenuItem = new ToolStripMenuItem();
            standardToolStripMenuItem = new ToolStripMenuItem();
            jobControlToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator49 = new ToolStripSeparator();
            pickerSensorsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator46 = new ToolStripSeparator();
            toolStripSeparator47 = new ToolStripSeparator();
            programsToolStripMenuItem = new ToolStripMenuItem();
            autoResumeJobOnScheduleToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator11 = new ToolStripSeparator();
            executeScriptToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator12 = new ToolStripSeparator();
            initializeToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator13 = new ToolStripSeparator();
            vendToolStripMenuItem = new ToolStripMenuItem();
            returnToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator14 = new ToolStripSeparator();
            syncToolStripMenuItem = new ToolStripMenuItem();
            softSyncToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator15 = new ToolStripSeparator();
            thinToolStripMenuItem = new ToolStripMenuItem();
            qlmUnloadToolStripMenuItem = new ToolStripMenuItem();
            unloadThinToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator16 = new ToolStripSeparator();
            getMaintenanceListToolStripMenuItem = new ToolStripMenuItem();
            getMachineListToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator17 = new ToolStripSeparator();
            newInstallSyncToolStripMenuItem = new ToolStripMenuItem();
            toolsToolStripMenuItem = new ToolStripMenuItem();
            getInventoryStateToolStripMenuItem = new ToolStripMenuItem();
            resetInventoryStateToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator26 = new ToolStripSeparator();
            reloadHalConfigurationToolStripMenuItem = new ToolStripMenuItem();
            backupHalConfigurationToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator27 = new ToolStripSeparator();
            dataInfoAndConfigurationToolStripMenuItem = new ToolStripMenuItem();
            emptySearchPatternToolToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            aboutHalManagementConsoleToolStripMenuItem = new ToolStripMenuItem();
            m_standardToolStrip = new ToolStrip();
            m_lockButton = new ToolStripButton();
            m_unlockButton = new ToolStripButton();
            toolStripSeparator35 = new ToolStripSeparator();
            m_profileLabel = new ToolStripLabel();
            m_connectButton = new ToolStripButton();
            m_disconnectButton = new ToolStripButton();
            toolStripSeparator36 = new ToolStripSeparator();
            m_saveButton = new ToolStripButton();
            toolStripSeparator40 = new ToolStripSeparator();
            m_executeScriptButton = new ToolStripButton();
            toolStripSeparator41 = new ToolStripSeparator();
            m_initButton = new ToolStripButton();
            toolStripSeparator42 = new ToolStripSeparator();
            m_vendButton = new ToolStripButton();
            m_returnButton = new ToolStripButton();
            toolStripSeparator43 = new ToolStripSeparator();
            m_syncButton = new ToolStripButton();
            m_softSyncButton = new ToolStripButton();
            toolStripSeparator44 = new ToolStripSeparator();
            m_thinButton = new ToolStripButton();
            m_unloadButton = new ToolStripButton();
            m_unloadThinButton = new ToolStripButton();
            toolStripSeparator45 = new ToolStripSeparator();
            m_getMaintListButton = new ToolStripButton();
            m_getMachineInfoButton = new ToolStripButton();
            toolStripSeparator50 = new ToolStripSeparator();
            m_initialNightSyncButton = new ToolStripButton();
            m_timer = new Timer(components);
            m_eastWestSplitContainer = new SplitContainer();
            m_northSouthSplitContainer = new SplitContainer();
            m_listViewTabControl = new TabControl();
            immediateWindowToolStripMenuItem = new ToolStripMenuItem();
            propertiesWindowToolStripMenuItem = new ToolStripMenuItem();
            errorListToolStripMenuItem = new ToolStripMenuItem();
            outputToolStripMenuItem = new ToolStripMenuItem();
            jobListToolStripMenuItem = new ToolStripMenuItem();
            stackToolStripMenuItem = new ToolStripMenuItem();
            symbolsToolStripMenuItem = new ToolStripMenuItem();
            programEventsToolStripMenuItem = new ToolStripMenuItem();
            programResultsToolStripMenuItem = new ToolStripMenuItem();
            m_menuStrip.SuspendLayout();
            m_standardToolStrip.SuspendLayout();
            m_eastWestSplitContainer.Panel2.SuspendLayout();
            m_eastWestSplitContainer.SuspendLayout();
            m_northSouthSplitContainer.SuspendLayout();
            SuspendLayout();
            m_menuStrip.Items.AddRange(new ToolStripItem[5]
            {
                toolStripMenuItem1,
                viewToolStripMenuItem,
                programsToolStripMenuItem,
                toolsToolStripMenuItem,
                helpToolStripMenuItem
            });
            m_menuStrip.Location = new Point(0, 0);
            m_menuStrip.Name = "m_menuStrip";
            m_menuStrip.Size = new Size(1024, 24);
            m_menuStrip.TabIndex = 0;
            m_menuStrip.Text = "Menu ";
            toolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[9]
            {
                connectToolStripMenuItem,
                disconnectToolStripMenuItem,
                toolStripSeparator3,
                saveToolStripMenuItem,
                toolStripSeparator4,
                lockSettingsToolStripMenuItem,
                unlockSettingsToolStripMenuItem,
                toolStripSeparator5,
                exitToolStripMenuItem
            });
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(37, 20);
            toolStripMenuItem1.Text = "&File";
            connectToolStripMenuItem.Image = Resources.connect;
            connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            connectToolStripMenuItem.Size = new Size(156, 22);
            connectToolStripMenuItem.Text = "&Connect";
            disconnectToolStripMenuItem.Enabled = false;
            disconnectToolStripMenuItem.Image = Resources.disconnect;
            disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
            disconnectToolStripMenuItem.Size = new Size(156, 22);
            disconnectToolStripMenuItem.Text = "&Disconnect";
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(153, 6);
            saveToolStripMenuItem.Enabled = false;
            saveToolStripMenuItem.Image = Resources.save;
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new Size(156, 22);
            saveToolStripMenuItem.Text = "&Save";
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(153, 6);
            lockSettingsToolStripMenuItem.Enabled = false;
            lockSettingsToolStripMenuItem.Image = Resources.locked;
            lockSettingsToolStripMenuItem.Name = "lockSettingsToolStripMenuItem";
            lockSettingsToolStripMenuItem.Size = new Size(156, 22);
            lockSettingsToolStripMenuItem.Text = "&Lock Settings";
            unlockSettingsToolStripMenuItem.Enabled = false;
            unlockSettingsToolStripMenuItem.Image = Resources.unlock;
            unlockSettingsToolStripMenuItem.Name = "unlockSettingsToolStripMenuItem";
            unlockSettingsToolStripMenuItem.Size = new Size(156, 22);
            unlockSettingsToolStripMenuItem.Text = "&Unlock Settings";
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(153, 6);
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(156, 22);
            exitToolStripMenuItem.Text = "&Exit";
            exitToolStripMenuItem.Click += OnClose;
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[14]
            {
                toolbarsToolStripMenuItem,
                toolStripSeparator49,
                immediateWindowToolStripMenuItem,
                propertiesWindowToolStripMenuItem,
                pickerSensorsToolStripMenuItem,
                toolStripSeparator46,
                errorListToolStripMenuItem,
                outputToolStripMenuItem,
                toolStripSeparator47,
                jobListToolStripMenuItem,
                stackToolStripMenuItem,
                symbolsToolStripMenuItem,
                programEventsToolStripMenuItem,
                programResultsToolStripMenuItem
            });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(44, 20);
            viewToolStripMenuItem.Text = "&View";
            toolbarsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[2]
            {
                standardToolStripMenuItem,
                jobControlToolStripMenuItem
            });
            toolbarsToolStripMenuItem.Enabled = false;
            toolbarsToolStripMenuItem.Name = "toolbarsToolStripMenuItem";
            toolbarsToolStripMenuItem.Size = new Size(193, 22);
            toolbarsToolStripMenuItem.Text = "Toolbars";
            standardToolStripMenuItem.Checked = true;
            standardToolStripMenuItem.CheckState = CheckState.Checked;
            standardToolStripMenuItem.Name = "standardToolStripMenuItem";
            standardToolStripMenuItem.Size = new Size(135, 22);
            standardToolStripMenuItem.Text = "Standard";
            jobControlToolStripMenuItem.Checked = true;
            jobControlToolStripMenuItem.CheckState = CheckState.Checked;
            jobControlToolStripMenuItem.Name = "jobControlToolStripMenuItem";
            jobControlToolStripMenuItem.Size = new Size(135, 22);
            jobControlToolStripMenuItem.Text = "Job Control";
            toolStripSeparator49.Name = "toolStripSeparator49";
            toolStripSeparator49.Size = new Size(190, 6);
            pickerSensorsToolStripMenuItem.Checked = true;
            pickerSensorsToolStripMenuItem.CheckState = CheckState.Checked;
            pickerSensorsToolStripMenuItem.Enabled = false;
            pickerSensorsToolStripMenuItem.Name = "pickerSensorsToolStripMenuItem";
            pickerSensorsToolStripMenuItem.Size = new Size(193, 22);
            pickerSensorsToolStripMenuItem.Text = "Picker Sensors";
            toolStripSeparator46.Name = "toolStripSeparator46";
            toolStripSeparator46.Size = new Size(190, 6);
            toolStripSeparator47.Name = "toolStripSeparator47";
            toolStripSeparator47.Size = new Size(190, 6);
            programsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[20]
            {
                autoResumeJobOnScheduleToolStripMenuItem,
                toolStripSeparator11,
                executeScriptToolStripMenuItem,
                toolStripSeparator12,
                initializeToolStripMenuItem,
                toolStripSeparator13,
                vendToolStripMenuItem,
                returnToolStripMenuItem,
                toolStripSeparator14,
                syncToolStripMenuItem,
                softSyncToolStripMenuItem,
                toolStripSeparator15,
                thinToolStripMenuItem,
                qlmUnloadToolStripMenuItem,
                unloadThinToolStripMenuItem,
                toolStripSeparator16,
                getMaintenanceListToolStripMenuItem,
                getMachineListToolStripMenuItem,
                toolStripSeparator17,
                newInstallSyncToolStripMenuItem
            });
            programsToolStripMenuItem.Name = "programsToolStripMenuItem";
            programsToolStripMenuItem.Size = new Size(70, 20);
            programsToolStripMenuItem.Text = "&Programs";
            autoResumeJobOnScheduleToolStripMenuItem.CheckOnClick = true;
            autoResumeJobOnScheduleToolStripMenuItem.Name = "autoResumeJobOnScheduleToolStripMenuItem";
            autoResumeJobOnScheduleToolStripMenuItem.Size = new Size(224, 22);
            autoResumeJobOnScheduleToolStripMenuItem.Text = "&Auto Resume Job on Submit";
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new Size(221, 6);
            executeScriptToolStripMenuItem.Enabled = false;
            executeScriptToolStripMenuItem.Image = Resources.execute;
            executeScriptToolStripMenuItem.Name = "executeScriptToolStripMenuItem";
            executeScriptToolStripMenuItem.Size = new Size(224, 22);
            executeScriptToolStripMenuItem.Text = "&Execute Script";
            executeScriptToolStripMenuItem.Click += OnExecuteScript;
            toolStripSeparator12.Name = "toolStripSeparator12";
            toolStripSeparator12.Size = new Size(221, 6);
            initializeToolStripMenuItem.Enabled = false;
            initializeToolStripMenuItem.Image = Resources.init;
            initializeToolStripMenuItem.Name = "initializeToolStripMenuItem";
            initializeToolStripMenuItem.Size = new Size(224, 22);
            initializeToolStripMenuItem.Tag = "init";
            initializeToolStripMenuItem.Text = "&Initialize";
            initializeToolStripMenuItem.Click += OnScheduleScriptByTag;
            toolStripSeparator13.Name = "toolStripSeparator13";
            toolStripSeparator13.Size = new Size(221, 6);
            vendToolStripMenuItem.Enabled = false;
            vendToolStripMenuItem.Image = Resources.cd_in_case;
            vendToolStripMenuItem.Name = "vendToolStripMenuItem";
            vendToolStripMenuItem.Size = new Size(224, 22);
            vendToolStripMenuItem.Tag = "vend";
            vendToolStripMenuItem.Text = "&Vend";
            vendToolStripMenuItem.Click += OnVend;
            returnToolStripMenuItem.Enabled = false;
            returnToolStripMenuItem.Image = Resources.cd_in_case_return;
            returnToolStripMenuItem.Name = "returnToolStripMenuItem";
            returnToolStripMenuItem.Size = new Size(224, 22);
            returnToolStripMenuItem.Tag = "return";
            returnToolStripMenuItem.Text = "&Return";
            returnToolStripMenuItem.Click += OnScheduleScriptByTag;
            toolStripSeparator14.Name = "toolStripSeparator14";
            toolStripSeparator14.Size = new Size(221, 6);
            syncToolStripMenuItem.Enabled = false;
            syncToolStripMenuItem.Image = Resources.sync;
            syncToolStripMenuItem.Name = "syncToolStripMenuItem";
            syncToolStripMenuItem.Size = new Size(224, 22);
            syncToolStripMenuItem.Text = "&Sync";
            syncToolStripMenuItem.Click += OnSync;
            softSyncToolStripMenuItem.Enabled = false;
            softSyncToolStripMenuItem.Image = Resources.soft_sync;
            softSyncToolStripMenuItem.Name = "softSyncToolStripMenuItem";
            softSyncToolStripMenuItem.Size = new Size(224, 22);
            softSyncToolStripMenuItem.Tag = "soft-sync";
            softSyncToolStripMenuItem.Text = "S&oft Sync";
            softSyncToolStripMenuItem.Click += OnScheduleScriptByTag;
            toolStripSeparator15.Name = "toolStripSeparator15";
            toolStripSeparator15.Size = new Size(221, 6);
            thinToolStripMenuItem.Enabled = false;
            thinToolStripMenuItem.Image = Resources.thin;
            thinToolStripMenuItem.Name = "thinToolStripMenuItem";
            thinToolStripMenuItem.Size = new Size(224, 22);
            thinToolStripMenuItem.Tag = "thin";
            thinToolStripMenuItem.Text = "&Thin";
            thinToolStripMenuItem.Click += OnScheduleScriptByTag;
            qlmUnloadToolStripMenuItem.Enabled = false;
            qlmUnloadToolStripMenuItem.Image = Resources.qlm_unload;
            qlmUnloadToolStripMenuItem.Name = "qlmUnloadToolStripMenuItem";
            qlmUnloadToolStripMenuItem.Size = new Size(224, 22);
            qlmUnloadToolStripMenuItem.Tag = "qlm-unload";
            qlmUnloadToolStripMenuItem.Text = "Qlm &Unload";
            qlmUnloadToolStripMenuItem.Click += OnScheduleScriptByTag;
            unloadThinToolStripMenuItem.Enabled = false;
            unloadThinToolStripMenuItem.Image = Resources.UnloadThin;
            unloadThinToolStripMenuItem.Name = "unloadThinToolStripMenuItem";
            unloadThinToolStripMenuItem.Size = new Size(224, 22);
            unloadThinToolStripMenuItem.Tag = "unload-thin";
            unloadThinToolStripMenuItem.Text = "Un&load Thin";
            unloadThinToolStripMenuItem.Click += OnScheduleScriptByTag;
            toolStripSeparator16.Name = "toolStripSeparator16";
            toolStripSeparator16.Size = new Size(221, 6);
            getMaintenanceListToolStripMenuItem.Enabled = false;
            getMaintenanceListToolStripMenuItem.Image = Resources.todo_list;
            getMaintenanceListToolStripMenuItem.Name = "getMaintenanceListToolStripMenuItem";
            getMaintenanceListToolStripMenuItem.Size = new Size(224, 22);
            getMaintenanceListToolStripMenuItem.Tag = "get-maintenance-list";
            getMaintenanceListToolStripMenuItem.Text = "Get Maintenance List";
            getMaintenanceListToolStripMenuItem.Click += OnScheduleScriptByTag;
            getMachineListToolStripMenuItem.Enabled = false;
            getMachineListToolStripMenuItem.Image = Resources.computer;
            getMachineListToolStripMenuItem.Name = "getMachineListToolStripMenuItem";
            getMachineListToolStripMenuItem.Size = new Size(224, 22);
            getMachineListToolStripMenuItem.Tag = "get-machine-info";
            getMachineListToolStripMenuItem.Text = "Get Machine List";
            getMachineListToolStripMenuItem.Click += OnScheduleScriptByTag;
            toolStripSeparator17.Name = "toolStripSeparator17";
            toolStripSeparator17.Size = new Size(221, 6);
            newInstallSyncToolStripMenuItem.Enabled = false;
            newInstallSyncToolStripMenuItem.Image = Resources.newinstallsync;
            newInstallSyncToolStripMenuItem.Name = "newInstallSyncToolStripMenuItem";
            newInstallSyncToolStripMenuItem.Size = new Size(224, 22);
            newInstallSyncToolStripMenuItem.Text = "&New Install Sync";
            newInstallSyncToolStripMenuItem.Click += OnNewInstallSync;
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[8]
            {
                getInventoryStateToolStripMenuItem,
                resetInventoryStateToolStripMenuItem,
                toolStripSeparator26,
                reloadHalConfigurationToolStripMenuItem,
                backupHalConfigurationToolStripMenuItem,
                toolStripSeparator27,
                dataInfoAndConfigurationToolStripMenuItem,
                emptySearchPatternToolToolStripMenuItem
            });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new Size(48, 20);
            toolsToolStripMenuItem.Text = "&Tools";
            getInventoryStateToolStripMenuItem.Enabled = false;
            getInventoryStateToolStripMenuItem.Name = "getInventoryStateToolStripMenuItem";
            getInventoryStateToolStripMenuItem.Size = new Size(224, 22);
            getInventoryStateToolStripMenuItem.Text = "&Get Inventory State";
            getInventoryStateToolStripMenuItem.Click += OnMenuToolsExportInventory;
            resetInventoryStateToolStripMenuItem.Enabled = false;
            resetInventoryStateToolStripMenuItem.Name = "resetInventoryStateToolStripMenuItem";
            resetInventoryStateToolStripMenuItem.Size = new Size(224, 22);
            resetInventoryStateToolStripMenuItem.Text = "&Reset Inventory State";
            resetInventoryStateToolStripMenuItem.Click += OnMenuToolsImportInventory;
            toolStripSeparator26.Name = "toolStripSeparator26";
            toolStripSeparator26.Size = new Size(221, 6);
            reloadHalConfigurationToolStripMenuItem.Enabled = false;
            reloadHalConfigurationToolStripMenuItem.Name = "reloadHalConfigurationToolStripMenuItem";
            reloadHalConfigurationToolStripMenuItem.Size = new Size(224, 22);
            reloadHalConfigurationToolStripMenuItem.Text = "Reload Hal Configuration";
            reloadHalConfigurationToolStripMenuItem.Click += OnMenuToolsImportXml;
            backupHalConfigurationToolStripMenuItem.Enabled = false;
            backupHalConfigurationToolStripMenuItem.Name = "backupHalConfigurationToolStripMenuItem";
            backupHalConfigurationToolStripMenuItem.Size = new Size(224, 22);
            backupHalConfigurationToolStripMenuItem.Text = "Backup Hal Configuration";
            backupHalConfigurationToolStripMenuItem.Click += OnMenuToolsExportInventory;
            toolStripSeparator27.Name = "toolStripSeparator27";
            toolStripSeparator27.Size = new Size(221, 6);
            dataInfoAndConfigurationToolStripMenuItem.Enabled = false;
            dataInfoAndConfigurationToolStripMenuItem.Name = "dataInfoAndConfigurationToolStripMenuItem";
            dataInfoAndConfigurationToolStripMenuItem.Size = new Size(224, 22);
            dataInfoAndConfigurationToolStripMenuItem.Text = "&Deck Info and Configuration";
            dataInfoAndConfigurationToolStripMenuItem.Click += OnMenuToolsDeckConfiguration;
            emptySearchPatternToolToolStripMenuItem.Enabled = false;
            emptySearchPatternToolToolStripMenuItem.Name = "emptySearchPatternToolToolStripMenuItem";
            emptySearchPatternToolToolStripMenuItem.Size = new Size(224, 22);
            emptySearchPatternToolToolStripMenuItem.Text = "&Empty Search Pattern Tool";
            emptySearchPatternToolToolStripMenuItem.Click += OnMenuToolsESPTool;
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[1]
            {
                aboutHalManagementConsoleToolStripMenuItem
            });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "&Help";
            aboutHalManagementConsoleToolStripMenuItem.Image = Resources.hal;
            aboutHalManagementConsoleToolStripMenuItem.Name = "aboutHalManagementConsoleToolStripMenuItem";
            aboutHalManagementConsoleToolStripMenuItem.Size = new Size(248, 22);
            aboutHalManagementConsoleToolStripMenuItem.Text = "&About Hal Management Console";
            aboutHalManagementConsoleToolStripMenuItem.Click += OnMenuHelpAbout;
            m_standardToolStrip.Items.AddRange(new ToolStripItem[27]
            {
                m_lockButton,
                m_unlockButton,
                toolStripSeparator35,
                m_profileLabel,
                m_connectButton,
                m_disconnectButton,
                toolStripSeparator36,
                m_saveButton,
                toolStripSeparator40,
                m_executeScriptButton,
                toolStripSeparator41,
                m_initButton,
                toolStripSeparator42,
                m_vendButton,
                m_returnButton,
                toolStripSeparator43,
                m_syncButton,
                m_softSyncButton,
                toolStripSeparator44,
                m_thinButton,
                m_unloadButton,
                m_unloadThinButton,
                toolStripSeparator45,
                m_getMaintListButton,
                m_getMachineInfoButton,
                toolStripSeparator50,
                m_initialNightSyncButton
            });
            m_standardToolStrip.Location = new Point(0, 24);
            m_standardToolStrip.Name = "m_standardToolStrip";
            m_standardToolStrip.RenderMode = ToolStripRenderMode.Professional;
            m_standardToolStrip.Size = new Size(1024, 25);
            m_standardToolStrip.TabIndex = 1;
            m_standardToolStrip.Text = "Standard ToolStrip";
            m_lockButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_lockButton.Enabled = false;
            m_lockButton.Image = (Image)componentResourceManager.GetObject("m_lockButton.Image");
            m_lockButton.ImageTransparentColor = Color.Magenta;
            m_lockButton.Name = "m_lockButton";
            m_lockButton.Size = new Size(23, 22);
            m_lockButton.Text = "Lock";
            m_unlockButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_unlockButton.Enabled = false;
            m_unlockButton.Image = (Image)componentResourceManager.GetObject("m_unlockButton.Image");
            m_unlockButton.ImageTransparentColor = Color.Magenta;
            m_unlockButton.Name = "m_unlockButton";
            m_unlockButton.Size = new Size(23, 22);
            m_unlockButton.Text = "Unlock";
            toolStripSeparator35.Name = "toolStripSeparator35";
            toolStripSeparator35.Size = new Size(6, 25);
            m_profileLabel.Name = "m_profileLabel";
            m_profileLabel.Size = new Size(44, 22);
            m_profileLabel.Text = "Profile:";
            m_connectButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_connectButton.Image = (Image)componentResourceManager.GetObject("m_connectButton.Image");
            m_connectButton.ImageTransparentColor = Color.Magenta;
            m_connectButton.Name = "m_connectButton";
            m_connectButton.Size = new Size(23, 22);
            m_connectButton.Text = "Connect";
            m_disconnectButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_disconnectButton.Enabled = false;
            m_disconnectButton.Image = (Image)componentResourceManager.GetObject("m_disconnectButton.Image");
            m_disconnectButton.ImageTransparentColor = Color.Magenta;
            m_disconnectButton.Name = "m_disconnectButton";
            m_disconnectButton.Size = new Size(23, 22);
            m_disconnectButton.Text = "Disconnect";
            toolStripSeparator36.Name = "toolStripSeparator36";
            toolStripSeparator36.Size = new Size(6, 25);
            m_saveButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_saveButton.Enabled = false;
            m_saveButton.Image = (Image)componentResourceManager.GetObject("m_saveButton.Image");
            m_saveButton.ImageTransparentColor = Color.Magenta;
            m_saveButton.Name = "m_saveButton";
            m_saveButton.Size = new Size(23, 22);
            m_saveButton.Text = "Save";
            toolStripSeparator40.Name = "toolStripSeparator40";
            toolStripSeparator40.Size = new Size(6, 25);
            m_executeScriptButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_executeScriptButton.Enabled = false;
            m_executeScriptButton.Image = (Image)componentResourceManager.GetObject("m_executeScriptButton.Image");
            m_executeScriptButton.ImageTransparentColor = Color.Magenta;
            m_executeScriptButton.Name = "m_executeScriptButton";
            m_executeScriptButton.Size = new Size(23, 22);
            m_executeScriptButton.Text = "Execute Script";
            m_executeScriptButton.Click += OnExecuteScript;
            toolStripSeparator41.Name = "toolStripSeparator41";
            toolStripSeparator41.Size = new Size(6, 25);
            m_initButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_initButton.Enabled = false;
            m_initButton.Image = (Image)componentResourceManager.GetObject("m_initButton.Image");
            m_initButton.ImageTransparentColor = Color.Magenta;
            m_initButton.Name = "m_initButton";
            m_initButton.Size = new Size(23, 22);
            m_initButton.Tag = "init";
            m_initButton.Text = "Init";
            m_initButton.Click += OnScheduleScriptByTag;
            toolStripSeparator42.Name = "toolStripSeparator42";
            toolStripSeparator42.Size = new Size(6, 25);
            m_vendButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_vendButton.Enabled = false;
            m_vendButton.Image = (Image)componentResourceManager.GetObject("m_vendButton.Image");
            m_vendButton.ImageTransparentColor = Color.Magenta;
            m_vendButton.Name = "m_vendButton";
            m_vendButton.Size = new Size(23, 22);
            m_vendButton.Tag = "vend";
            m_vendButton.Text = "Vend";
            m_vendButton.Click += OnVend;
            m_returnButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_returnButton.Enabled = false;
            m_returnButton.Image = (Image)componentResourceManager.GetObject("m_returnButton.Image");
            m_returnButton.ImageTransparentColor = Color.Magenta;
            m_returnButton.Name = "m_returnButton";
            m_returnButton.Size = new Size(23, 22);
            m_returnButton.Tag = "return";
            m_returnButton.Text = "Return";
            m_returnButton.Click += OnScheduleScriptByTag;
            toolStripSeparator43.Name = "toolStripSeparator43";
            toolStripSeparator43.Size = new Size(6, 25);
            m_syncButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_syncButton.Enabled = false;
            m_syncButton.Image = (Image)componentResourceManager.GetObject("m_syncButton.Image");
            m_syncButton.ImageTransparentColor = Color.Magenta;
            m_syncButton.Name = "m_syncButton";
            m_syncButton.Size = new Size(23, 22);
            m_syncButton.Tag = "sync";
            m_syncButton.Text = "Sync";
            m_syncButton.Click += OnSync;
            m_softSyncButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_softSyncButton.Enabled = false;
            m_softSyncButton.Image = (Image)componentResourceManager.GetObject("m_softSyncButton.Image");
            m_softSyncButton.ImageTransparentColor = Color.Magenta;
            m_softSyncButton.Name = "m_softSyncButton";
            m_softSyncButton.Size = new Size(23, 22);
            m_softSyncButton.Tag = "soft-sync";
            m_softSyncButton.Text = "Soft Sync";
            m_softSyncButton.Click += OnScheduleScriptByTag;
            toolStripSeparator44.Name = "toolStripSeparator44";
            toolStripSeparator44.Size = new Size(6, 25);
            m_thinButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_thinButton.Enabled = false;
            m_thinButton.Image = (Image)componentResourceManager.GetObject("m_thinButton.Image");
            m_thinButton.ImageTransparentColor = Color.Magenta;
            m_thinButton.Name = "m_thinButton";
            m_thinButton.Size = new Size(23, 22);
            m_thinButton.Tag = "thin";
            m_thinButton.Text = "Thin";
            m_thinButton.Click += OnScheduleScriptByTag;
            m_unloadButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_unloadButton.Enabled = false;
            m_unloadButton.Image = (Image)componentResourceManager.GetObject("m_unloadButton.Image");
            m_unloadButton.ImageTransparentColor = Color.Magenta;
            m_unloadButton.Name = "m_unloadButton";
            m_unloadButton.Size = new Size(23, 22);
            m_unloadButton.Tag = "qlm-unload";
            m_unloadButton.Text = "QLM Unload";
            m_unloadButton.Click += OnScheduleScriptByTag;
            m_unloadThinButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_unloadThinButton.Enabled = false;
            m_unloadThinButton.Image = (Image)componentResourceManager.GetObject("m_unloadThinButton.Image");
            m_unloadThinButton.ImageTransparentColor = Color.Magenta;
            m_unloadThinButton.Name = "m_unloadThinButton";
            m_unloadThinButton.Size = new Size(23, 22);
            m_unloadThinButton.Tag = "unload-thin";
            m_unloadThinButton.Text = "QLM Unload Thin Super Job";
            m_unloadThinButton.Click += OnScheduleScriptByTag;
            toolStripSeparator45.Name = "toolStripSeparator45";
            toolStripSeparator45.Size = new Size(6, 25);
            m_getMaintListButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_getMaintListButton.Enabled = false;
            m_getMaintListButton.Image = (Image)componentResourceManager.GetObject("m_getMaintListButton.Image");
            m_getMaintListButton.ImageTransparentColor = Color.Magenta;
            m_getMaintListButton.Name = "m_getMaintListButton";
            m_getMaintListButton.Size = new Size(23, 22);
            m_getMaintListButton.Tag = "get-maintenance-list";
            m_getMaintListButton.Text = "Get Maintenance List";
            m_getMaintListButton.Click += OnScheduleScriptByTag;
            m_getMachineInfoButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_getMachineInfoButton.Enabled = false;
            m_getMachineInfoButton.Image = (Image)componentResourceManager.GetObject("m_getMachineInfoButton.Image");
            m_getMachineInfoButton.ImageTransparentColor = Color.Magenta;
            m_getMachineInfoButton.Name = "m_getMachineInfoButton";
            m_getMachineInfoButton.Size = new Size(23, 22);
            m_getMachineInfoButton.Tag = "get-machine-info";
            m_getMachineInfoButton.Text = "Get Machine Info";
            m_getMachineInfoButton.Click += OnScheduleScriptByTag;
            toolStripSeparator50.Name = "toolStripSeparator50";
            toolStripSeparator50.Size = new Size(6, 25);
            m_initialNightSyncButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_initialNightSyncButton.Enabled = false;
            m_initialNightSyncButton.Image =
                (Image)componentResourceManager.GetObject("m_initialNightSyncButton.Image");
            m_initialNightSyncButton.ImageTransparentColor = Color.Magenta;
            m_initialNightSyncButton.Name = "m_initialNightSyncButton";
            m_initialNightSyncButton.Size = new Size(23, 22);
            m_initialNightSyncButton.Text = "Initial Night Sync";
            m_initialNightSyncButton.Click += OnNewInstallSync;
            m_timer.Interval = 1500;
            m_timer.Tick += OnGlobalTimer;
            m_eastWestSplitContainer.Anchor =
                AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            m_eastWestSplitContainer.BackColor = Color.WhiteSmoke;
            m_eastWestSplitContainer.BorderStyle = BorderStyle.Fixed3D;
            m_eastWestSplitContainer.IsSplitterFixed = true;
            m_eastWestSplitContainer.Location = new Point(0, 74);
            m_eastWestSplitContainer.Margin = new Padding(0);
            m_eastWestSplitContainer.Name = "m_eastWestSplitContainer";
            m_eastWestSplitContainer.Panel1.BackColor = Color.White;
            m_eastWestSplitContainer.Panel2.Controls.Add(m_northSouthSplitContainer);
            m_eastWestSplitContainer.Panel2MinSize = 20;
            m_eastWestSplitContainer.Size = new Size(1024, 669);
            m_eastWestSplitContainer.SplitterDistance = 252;
            m_eastWestSplitContainer.SplitterWidth = 6;
            m_eastWestSplitContainer.TabIndex = 100;
            m_eastWestSplitContainer.TabStop = false;
            m_northSouthSplitContainer.BackColor = Color.WhiteSmoke;
            m_northSouthSplitContainer.BorderStyle = BorderStyle.Fixed3D;
            m_northSouthSplitContainer.Dock = DockStyle.Fill;
            m_northSouthSplitContainer.IsSplitterFixed = true;
            m_northSouthSplitContainer.Location = new Point(0, 0);
            m_northSouthSplitContainer.Name = "m_northSouthSplitContainer";
            m_northSouthSplitContainer.Orientation = Orientation.Horizontal;
            m_northSouthSplitContainer.Panel1.BackColor = Color.White;
            m_northSouthSplitContainer.Panel2.BackColor = Color.White;
            m_northSouthSplitContainer.Size = new Size(766, 669);
            m_northSouthSplitContainer.SplitterDistance = 343;
            m_northSouthSplitContainer.SplitterWidth = 6;
            m_northSouthSplitContainer.TabIndex = 100;
            m_northSouthSplitContainer.TabStop = false;
            m_listViewTabControl.Dock = DockStyle.Fill;
            m_listViewTabControl.Location = new Point(0, 0);
            m_listViewTabControl.Name = "m_listViewTabControl";
            m_listViewTabControl.SelectedIndex = 0;
            m_listViewTabControl.Size = new Size(762, 215);
            m_listViewTabControl.TabIndex = 0;
            immediateWindowToolStripMenuItem.CheckOnClick = true;
            immediateWindowToolStripMenuItem.Enabled = false;
            immediateWindowToolStripMenuItem.Name = "immediateWindowToolStripMenuItem";
            immediateWindowToolStripMenuItem.Size = new Size(193, 22);
            immediateWindowToolStripMenuItem.Tag = LeftPanelTab.Immediate;
            immediateWindowToolStripMenuItem.Text = "&Immediate Window";
            immediateWindowToolStripMenuItem.Click += OnMenuLeftSideToggle;
            propertiesWindowToolStripMenuItem.Checked = true;
            propertiesWindowToolStripMenuItem.CheckOnClick = true;
            propertiesWindowToolStripMenuItem.CheckState = CheckState.Checked;
            propertiesWindowToolStripMenuItem.Enabled = false;
            propertiesWindowToolStripMenuItem.Name = "propertiesWindowToolStripMenuItem";
            propertiesWindowToolStripMenuItem.ShortcutKeys = Keys.F4;
            propertiesWindowToolStripMenuItem.Size = new Size(193, 22);
            propertiesWindowToolStripMenuItem.Tag = LeftPanelTab.Properties;
            propertiesWindowToolStripMenuItem.Text = "&Properties Window";
            propertiesWindowToolStripMenuItem.Click += OnMenuLeftSideToggle;
            errorListToolStripMenuItem.CheckOnClick = true;
            errorListToolStripMenuItem.Name = "errorListToolStripMenuItem";
            errorListToolStripMenuItem.ShortcutKeys = Keys.E | Keys.Control;
            errorListToolStripMenuItem.Size = new Size(193, 22);
            errorListToolStripMenuItem.Tag = ListViewNames.Errors;
            errorListToolStripMenuItem.Text = "Error &List";
            errorListToolStripMenuItem.Click += OnMenuListViewToggle;
            outputToolStripMenuItem.CheckOnClick = true;
            outputToolStripMenuItem.Enabled = false;
            outputToolStripMenuItem.Name = "outputToolStripMenuItem";
            outputToolStripMenuItem.Size = new Size(193, 22);
            outputToolStripMenuItem.Tag = ListViewNames.OutputWindow;
            outputToolStripMenuItem.Text = "&Output";
            outputToolStripMenuItem.Click += OnMenuListViewToggle;
            jobListToolStripMenuItem.Checked = true;
            jobListToolStripMenuItem.CheckOnClick = true;
            jobListToolStripMenuItem.CheckState = CheckState.Checked;
            jobListToolStripMenuItem.Enabled = false;
            jobListToolStripMenuItem.Name = "jobListToolStripMenuItem";
            jobListToolStripMenuItem.Size = new Size(193, 22);
            jobListToolStripMenuItem.Tag = ListViewNames.Job;
            jobListToolStripMenuItem.Text = "&Jobs";
            jobListToolStripMenuItem.Click += OnMenuListViewToggle;
            stackToolStripMenuItem.Checked = true;
            stackToolStripMenuItem.CheckOnClick = true;
            stackToolStripMenuItem.CheckState = CheckState.Checked;
            stackToolStripMenuItem.Name = "stackToolStripMenuItem";
            stackToolStripMenuItem.Size = new Size(193, 22);
            stackToolStripMenuItem.Tag = ListViewNames.Stack;
            stackToolStripMenuItem.Text = "&Stack";
            stackToolStripMenuItem.Click += OnMenuListViewToggle;
            symbolsToolStripMenuItem.Checked = true;
            symbolsToolStripMenuItem.CheckOnClick = true;
            symbolsToolStripMenuItem.CheckState = CheckState.Checked;
            symbolsToolStripMenuItem.Name = "symbolsToolStripMenuItem";
            symbolsToolStripMenuItem.Size = new Size(193, 22);
            symbolsToolStripMenuItem.Tag = ListViewNames.Symbols;
            symbolsToolStripMenuItem.Text = "S&ymbols";
            symbolsToolStripMenuItem.Click += OnMenuListViewToggle;
            programEventsToolStripMenuItem.Checked = true;
            programEventsToolStripMenuItem.CheckOnClick = true;
            programEventsToolStripMenuItem.CheckState = CheckState.Checked;
            programEventsToolStripMenuItem.Name = "programEventsToolStripMenuItem";
            programEventsToolStripMenuItem.Size = new Size(193, 22);
            programEventsToolStripMenuItem.Tag = ListViewNames.ProgramEvents;
            programEventsToolStripMenuItem.Text = "Program Events";
            programEventsToolStripMenuItem.Click += OnMenuListViewToggle;
            programResultsToolStripMenuItem.CheckOnClick = true;
            programResultsToolStripMenuItem.Name = "programResultsToolStripMenuItem";
            programResultsToolStripMenuItem.Size = new Size(193, 22);
            programResultsToolStripMenuItem.Tag = ListViewNames.Results;
            programResultsToolStripMenuItem.Text = "Program Results";
            programResultsToolStripMenuItem.Click += OnMenuListViewToggle;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1024, 742);
            Controls.Add(m_standardToolStrip);
            Controls.Add(m_menuStrip);
            Controls.Add(m_eastWestSplitContainer);
            Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            MainMenuStrip = m_menuStrip;
            Name = nameof(Form1);
            Text = "HAL Management Console";
            WindowState = FormWindowState.Maximized;
            Click += OnClose;
            m_menuStrip.ResumeLayout(false);
            m_menuStrip.PerformLayout();
            m_standardToolStrip.ResumeLayout(false);
            m_standardToolStrip.PerformLayout();
            m_eastWestSplitContainer.Panel2.ResumeLayout(false);
            m_eastWestSplitContainer.ResumeLayout(false);
            m_northSouthSplitContainer.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}