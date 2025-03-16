using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.REDS.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE
{
    public class IdeMainForm : Form
    {
        private bool m_isActivated;
        public static IdeMainForm m_instance;
        private readonly OutputToolWindow m_outputToolWindow = new OutputToolWindow();
        private readonly TimersToolWindow m_timersToolWindow = new TimersToolWindow();
        private readonly SymbolsToolWindow m_localsToolWindow = new SymbolsToolWindow();
        private readonly SymbolsToolWindow m_globalsToolWindow = new SymbolsToolWindow();
        private readonly ErrorListToolWindow m_errorListToolWindow = new ErrorListToolWindow();
        private readonly ImmediateToolWindow m_immediateToolWindow = new ImmediateToolWindow();
        private readonly CallStackToolWindow m_callStackToolWindow = new CallStackToolWindow();
        private readonly ViewStackToolWindow m_viewStackToolWindow = new ViewStackToolWindow();
        private readonly PropertiesToolWindow m_propertiesToolWindow = new PropertiesToolWindow();
        private readonly BundleFilterToolWindow m_bundleFilterToolWindow = new BundleFilterToolWindow();
        private readonly ProjectExplorerToolWindow m_projectExplorerToolWindow = new ProjectExplorerToolWindow();
        private IContainer components;
        private MenuStrip m_menuStrip;
        private StatusStrip m_statusStrip;
        private DockPanel m_dockPanel;
        private ToolStrip m_standardToolStrip;
        private ToolStripButton m_openProjectToolStripButton;
        private ToolStripMenuItem m_viewToolStripMenuItem;
        private ToolStripMenuItem m_viewProjectExplorerToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem m_timersToolStripMenuItem;
        private ToolStripMenuItem m_viewStackToolStripMenuItem;
        private ToolStripMenuItem m_outputToolStripMenuItem;
        private ToolStripMenuItem m_propertiesToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripMenuItem m_legacyDebuggerToolStripMenuItem;
        private ToolStripMenuItem m_fileToolStripMenuItem;
        private ToolStripMenuItem m_editToolStripMenuItem;
        private ToolStripMenuItem m_toolsToolStripMenuItem;
        private ToolStripMenuItem m_helpToolStripMenuItem;
        private ToolStripMenuItem m_globalsToolStripMenuItem;
        private ToolStripMenuItem m_localsToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem3;
        private ToolStripMenuItem m_callstackToolStripMenuItem;
        private ToolStripMenuItem m_immediateWindowToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem4;
        private ToolStripMenuItem m_bundleFiltersToolStripMenuItem;
        private ToolStripMenuItem m_windowToolStripMenuItem;
        private ToolStripMenuItem m_buildToolStripMenuItem;
        private ToolStripStatusLabel m_toolStripStatusLabel;
        private ToolStripProgressBar m_toolStripProgressBar;
        private ToolStripStatusLabel m_editorToolStripStatusLabel;
        private ToolStripMenuItem m_errorListToolStripMenuItem;
        private ToolStripMenuItem m_newToolStripMenuItem;
        private ToolStripMenuItem m_newProjectToolStripMenuItem;
        private ToolStripMenuItem m_newResourceToolStripMenuItem;
        private ToolStripMenuItem m_openToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem5;
        private ToolStripMenuItem m_closeToolStripMenuItem;
        private ToolStripMenuItem m_closeProjectToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem6;
        private ToolStripMenuItem m_saveToolStripMenuItem;
        private ToolStripMenuItem m_saveAsToolStripMenuItem;
        private ToolStripMenuItem m_saveAllToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem7;
        private ToolStripMenuItem m_pageSetupToolStripMenuItem;
        private ToolStripMenuItem m_printToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem8;
        private ToolStripMenuItem m_recentResourcesToolStripMenuItem;
        private ToolStripMenuItem m_recentProjectsToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem9;
        private ToolStripMenuItem m_exitToolStripMenuItem;
        private ToolStripMenuItem m_undoToolStripMenuItem;
        private ToolStripMenuItem m_redoToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem10;
        private ToolStripMenuItem m_cutToolStripMenuItem;
        private ToolStripMenuItem m_copyToolStripMenuItem;
        private ToolStripMenuItem m_pasteToolStripMenuItem;
        private ToolStripMenuItem m_deleteToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem11;
        private ToolStripMenuItem m_selectAllToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem12;
        private ToolStripMenuItem m_findAndReplaceToolStripMenuItem;
        private ToolStripMenuItem m_findToolStripMenuItem;
        private ToolStripMenuItem m_replaceToolStripMenuItem;
        private ToolStripMenuItem goToToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem13;
        private ToolStripMenuItem m_bookmarksToolStripMenuItem;
        private ToolStripMenuItem m_buildBundleToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem14;
        private ToolStripMenuItem m_enableDebuggingToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem m_resumeExecutionToolStripMenuItem;
        private ToolStripMenuItem m_breakAllToolStripMenuItem;
        private ToolStripMenuItem m_stopDebugginToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem m_stepIntoToolStripMenuItem;
        private ToolStripMenuItem m_stepOverToolStripMenuItem;
        private ToolStripMenuItem m_stepOutToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem m_toggleBreakpointToolStripMenuItem;
        private ToolStripMenuItem m_deleteAllBreakpointToolStripMenuItem;
        private ToolStripMenuItem m_breakpointsToolStripMenuItem;
        private ToolStripMenuItem m_optionsToolStripMenuItem;
        private ToolStrip m_debuggerToolStrip;
        private ToolStripButton m_enableDebuggingToolStripButton;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripButton m_resumeExecutionToolStripButton;
        private ToolStripButton m_breakAllToolStripButton;
        private ToolStripButton m_stopDebuggingToolStripButton;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripButton m_stepIntoToolStripButton;
        private ToolStripButton m_stepOverToolStripButton;
        private ToolStripButton m_stepOutToolStripButton;
        private ToolStripContainer m_toolStripContainer;
        private ToolStripSeparator toolStripMenuItem15;
        private ToolStripMenuItem m_showWhitespaceToolStripMenuItem;
        private ToolStripMenuItem m_showEOLMarkerToolStripMenuItem;
        private ToolStripButton m_saveToolStripButton;
        private ToolStripButton m_saveAllToolStripButton;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripButton m_cutToolStripButton;
        private ToolStripButton m_copyToolStripButton;
        private ToolStripButton m_pasteToolStripButton;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripButton m_undoToolStripButton;
        private ToolStripButton m_redoToolStripButton;
        private ToolStripDropDownButton m_newToolStripDropDownButton;
        private ToolStripMenuItem m_newProject2ToolStripMenuItem;
        private ToolStripMenuItem m_newResource2ToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripTextBox m_findToolStripTextBox;
        private ToolStripMenuItem m_addToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem16;
        private ToolStripMenuItem m_addNewBundleToolStripMenuItem;
        private ToolStripMenuItem m_addExistingBundleToolStripMenuItem;

        public IdeMainForm()
        {
            InitializeComponent();
            m_localsToolWindow.Text = "Locals";
            m_localsToolWindow.RefreshRequested += (EventHandler)((o, e) =>
            {
                var service = ServiceLocator.Instance.GetService<IDebugService>();
                if (service.DebuggerInstance == null)
                    return;
                m_localsToolWindow.ShowSymbols("Locals",
                    (IEnumerable<LuaVar>)((LuaDebugger)service.DebuggerInstance).GetLocalVars());
            });
            m_globalsToolWindow.Text = "Globals";
            m_globalsToolWindow.RefreshRequested += (EventHandler)((o, e) =>
            {
                var service = ServiceLocator.Instance.GetService<IDebugService>();
                if (service.DebuggerInstance == null)
                    return;
                m_globalsToolWindow.ShowSymbols("_G",
                    (IEnumerable<LuaVar>)((LuaDebugger)service.DebuggerInstance).GetGlobalVars());
            });
            LogHelper.Instance.Logged += new LoggedEventHandler(LogToOutputWindow);
            FormAspects.ActsAsPersistent((Form)this, ServiceLocator.Instance.GetService<IUserSettingsStore>());
            Load += (EventHandler)((_param1, _param2) =>
            {
                ToolStripManager.LoadSettings((Form)this);
                var buffer = ServiceLocator.Instance.GetService<IUserSettingsStore>()
                    .GetValue<byte[]>("Shell", "DockLayout");
                if (buffer == null)
                    return;
                using (var memoryStream = new MemoryStream(buffer))
                {
                    m_dockPanel.LoadFromXml((Stream)memoryStream, (DeserializeDockContent)(s =>
                    {
                        if (s == typeof(TimersToolWindow).ToString())
                            return (IDockContent)m_timersToolWindow;
                        if (s.StartsWith(typeof(SymbolsToolWindow).ToString()))
                        {
                            var strArray = s.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            if (strArray.Length == 2)
                            {
                                var symbolsToolWindow =
                                    strArray[1] == "Locals" ? m_localsToolWindow : m_globalsToolWindow;
                                symbolsToolWindow.RefreshSymbols();
                                return (IDockContent)symbolsToolWindow;
                            }
                        }

                        if (s == typeof(ImmediateToolWindow).ToString())
                            return (IDockContent)m_immediateToolWindow;
                        if (s == typeof(CallStackToolWindow).ToString())
                            return (IDockContent)m_callStackToolWindow;
                        if (s == typeof(ViewStackToolWindow).ToString())
                            return (IDockContent)m_viewStackToolWindow;
                        if (s == typeof(PropertiesToolWindow).ToString())
                            return (IDockContent)m_propertiesToolWindow;
                        if (s == typeof(BundleFilterToolWindow).ToString())
                        {
                            m_bundleFilterToolWindow.RefreshFilters();
                            return (IDockContent)m_bundleFilterToolWindow;
                        }

                        if (s == typeof(ProjectExplorerToolWindow).ToString())
                        {
                            m_projectExplorerToolWindow.RefreshTree();
                            return (IDockContent)m_projectExplorerToolWindow;
                        }

                        if (s == typeof(ErrorListToolWindow).ToString())
                            return (IDockContent)m_errorListToolWindow;
                        return s == typeof(OutputToolWindow).ToString()
                            ? (IDockContent)m_outputToolWindow
                            : (IDockContent)null;
                    }));
                }
            });
            FormClosing += (FormClosingEventHandler)((_param1, _param2) =>
            {
                m_isActivated = false;
                ToolStripManager.SaveSettings((Form)this);
                var service = ServiceLocator.Instance.GetService<IUserSettingsStore>();
                var tempFileName = Path.GetTempFileName();
                m_dockPanel.SaveAsXml(tempFileName, Encoding.ASCII);
                var numArray = File.ReadAllBytes(tempFileName);
                service.SetValue<byte[]>("Shell", "DockLayout", numArray);
                File.Delete(tempFileName);
                LogHelper.Instance.Logged -= new LoggedEventHandler(LogToOutputWindow);
                m_instance = (IdeMainForm)null;
            });
        }

        public void OpenResourceEditor(IResource resource)
        {
            new ResourceEditorWindow() { Resource = resource }.Show(m_dockPanel);
        }

        public void OpenResourceEditor(IResource resource, int lineNumber)
        {
            var resourceEditorWindow = new ResourceEditorWindow();
            resourceEditorWindow.Resource = resource;
            resourceEditorWindow.SetDebugState(lineNumber, (string)null);
            resourceEditorWindow.Show(m_dockPanel);
        }

        public void OpenResourceEditor(IResource resource, int lineNumber, string error)
        {
            var resourceEditorWindow = new ResourceEditorWindow();
            resourceEditorWindow.Resource = resource;
            resourceEditorWindow.SetDebugState(lineNumber, error);
            resourceEditorWindow.Show(m_dockPanel);
        }

        public static void LogToOutputWindow(string message)
        {
            if (m_instance == null || m_instance.m_outputToolWindow == null || !m_instance.m_outputToolWindow.Visible)
                return;
            m_instance.m_outputToolWindow.UpdateText(message);
        }

        internal PropertiesToolWindow PropertiesWindow => m_propertiesToolWindow;

        private void OnActivated(object sender, EventArgs e)
        {
            if (m_isActivated)
                return;
            var service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
            if (service != null)
                ShowInTaskbar = service.GetValue<bool>("Ide", "ShowIDEInTaskBar", true);
            m_isActivated = true;
        }

        private void OnNewProject(object sender, EventArgs e)
        {
            var num = (int)new NewProjectForm().ShowDialog();
        }

        private void OnNewResource(object sender, EventArgs e)
        {
            var num = (int)new NewResourceForm().ShowDialog();
        }

        private void OnAddNewBundle(object sender, EventArgs e)
        {
        }

        private void OnAddExistingBundle(object sender, EventArgs e)
        {
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OnShowTimers(object sender, EventArgs e)
        {
            m_timersToolWindow.Show(m_dockPanel);
        }

        private void OnShowLocals(object sender, EventArgs e)
        {
            m_localsToolWindow.RefreshSymbols();
            m_localsToolWindow.Show(m_dockPanel);
        }

        private void OnShowGlobals(object sender, EventArgs e)
        {
            m_globalsToolWindow.RefreshSymbols();
            m_globalsToolWindow.Show(m_dockPanel);
        }

        private void OnShowViewStack(object sender, EventArgs e)
        {
            m_viewStackToolWindow.Show(m_dockPanel);
        }

        private void OnShowCallStack(object sender, EventArgs e)
        {
            m_callStackToolWindow.RefreshCallStack();
            m_callStackToolWindow.Show(m_dockPanel);
        }

        private void OnShowProperties(object sender, EventArgs e)
        {
            m_propertiesToolWindow.Show(m_dockPanel);
        }

        private void OnShowLegacyDebugger(object sender, EventArgs e)
        {
            var service = ServiceLocator.Instance.GetService<IResourceBundleService>();
            DebuggerForm.Instance.ActiveBundleSet = service.ActiveBundleSet;
            DebuggerForm.Instance.Filter = service.Filter;
            DebuggerForm.Instance.ActivateDebugger();
        }

        private void OnShowImmediateWindow(object sender, EventArgs e)
        {
            m_immediateToolWindow.Show(m_dockPanel);
        }

        private void OnShowBundleFilters(object sender, EventArgs e)
        {
            m_bundleFilterToolWindow.RefreshFilters();
            m_bundleFilterToolWindow.Show(m_dockPanel);
        }

        private void OnShowBundleExplorer(object sender, EventArgs e)
        {
            m_projectExplorerToolWindow.RefreshTree();
            m_projectExplorerToolWindow.Show(m_dockPanel);
        }

        private void OnShowErrorList(object sender, EventArgs e)
        {
            m_errorListToolWindow.Show(m_dockPanel);
        }

        private void OnShowOutput(object sender, EventArgs e)
        {
            m_outputToolWindow.Show(m_dockPanel);
        }

        private void OnShowOptions(object sender, EventArgs e)
        {
            var num = (int)new PreferencesManager().ShowDialog();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            var dockPanelSkin = new DockPanelSkin();
            var autoHideStripSkin = new AutoHideStripSkin();
            var dockPanelGradient1 = new DockPanelGradient();
            var tabGradient1 = new TabGradient();
            var dockPaneStripSkin = new DockPaneStripSkin();
            var paneStripGradient = new DockPaneStripGradient();
            var tabGradient2 = new TabGradient();
            var dockPanelGradient2 = new DockPanelGradient();
            var tabGradient3 = new TabGradient();
            var toolWindowGradient = new DockPaneStripToolWindowGradient();
            var tabGradient4 = new TabGradient();
            var tabGradient5 = new TabGradient();
            var dockPanelGradient3 = new DockPanelGradient();
            var tabGradient6 = new TabGradient();
            var tabGradient7 = new TabGradient();
            var componentResourceManager = new ComponentResourceManager(typeof(IdeMainForm));
            m_menuStrip = new MenuStrip();
            m_fileToolStripMenuItem = new ToolStripMenuItem();
            m_newToolStripMenuItem = new ToolStripMenuItem();
            m_newProjectToolStripMenuItem = new ToolStripMenuItem();
            m_newResourceToolStripMenuItem = new ToolStripMenuItem();
            m_openToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem5 = new ToolStripSeparator();
            m_addToolStripMenuItem = new ToolStripMenuItem();
            m_addNewBundleToolStripMenuItem = new ToolStripMenuItem();
            m_addExistingBundleToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem16 = new ToolStripSeparator();
            m_closeToolStripMenuItem = new ToolStripMenuItem();
            m_closeProjectToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem6 = new ToolStripSeparator();
            m_saveToolStripMenuItem = new ToolStripMenuItem();
            m_saveAsToolStripMenuItem = new ToolStripMenuItem();
            m_saveAllToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem7 = new ToolStripSeparator();
            m_pageSetupToolStripMenuItem = new ToolStripMenuItem();
            m_printToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem8 = new ToolStripSeparator();
            m_recentResourcesToolStripMenuItem = new ToolStripMenuItem();
            m_recentProjectsToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem9 = new ToolStripSeparator();
            m_exitToolStripMenuItem = new ToolStripMenuItem();
            m_editToolStripMenuItem = new ToolStripMenuItem();
            m_undoToolStripMenuItem = new ToolStripMenuItem();
            m_redoToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem10 = new ToolStripSeparator();
            m_cutToolStripMenuItem = new ToolStripMenuItem();
            m_copyToolStripMenuItem = new ToolStripMenuItem();
            m_pasteToolStripMenuItem = new ToolStripMenuItem();
            m_deleteToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem11 = new ToolStripSeparator();
            m_selectAllToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem12 = new ToolStripSeparator();
            m_findAndReplaceToolStripMenuItem = new ToolStripMenuItem();
            m_findToolStripMenuItem = new ToolStripMenuItem();
            m_replaceToolStripMenuItem = new ToolStripMenuItem();
            goToToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem13 = new ToolStripSeparator();
            m_bookmarksToolStripMenuItem = new ToolStripMenuItem();
            m_viewToolStripMenuItem = new ToolStripMenuItem();
            m_viewProjectExplorerToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            m_timersToolStripMenuItem = new ToolStripMenuItem();
            m_viewStackToolStripMenuItem = new ToolStripMenuItem();
            m_bundleFiltersToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem3 = new ToolStripSeparator();
            m_globalsToolStripMenuItem = new ToolStripMenuItem();
            m_localsToolStripMenuItem = new ToolStripMenuItem();
            m_callstackToolStripMenuItem = new ToolStripMenuItem();
            m_immediateWindowToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem4 = new ToolStripSeparator();
            m_errorListToolStripMenuItem = new ToolStripMenuItem();
            m_outputToolStripMenuItem = new ToolStripMenuItem();
            m_propertiesToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripSeparator();
            m_legacyDebuggerToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem15 = new ToolStripSeparator();
            m_showWhitespaceToolStripMenuItem = new ToolStripMenuItem();
            m_showEOLMarkerToolStripMenuItem = new ToolStripMenuItem();
            m_buildToolStripMenuItem = new ToolStripMenuItem();
            m_buildBundleToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem14 = new ToolStripMenuItem();
            m_enableDebuggingToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            m_resumeExecutionToolStripMenuItem = new ToolStripMenuItem();
            m_breakAllToolStripMenuItem = new ToolStripMenuItem();
            m_stopDebugginToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            m_stepIntoToolStripMenuItem = new ToolStripMenuItem();
            m_stepOverToolStripMenuItem = new ToolStripMenuItem();
            m_stepOutToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            m_toggleBreakpointToolStripMenuItem = new ToolStripMenuItem();
            m_deleteAllBreakpointToolStripMenuItem = new ToolStripMenuItem();
            m_breakpointsToolStripMenuItem = new ToolStripMenuItem();
            m_toolsToolStripMenuItem = new ToolStripMenuItem();
            m_optionsToolStripMenuItem = new ToolStripMenuItem();
            m_windowToolStripMenuItem = new ToolStripMenuItem();
            m_helpToolStripMenuItem = new ToolStripMenuItem();
            m_statusStrip = new StatusStrip();
            m_toolStripStatusLabel = new ToolStripStatusLabel();
            m_toolStripProgressBar = new ToolStripProgressBar();
            m_editorToolStripStatusLabel = new ToolStripStatusLabel();
            m_dockPanel = new DockPanel();
            m_standardToolStrip = new ToolStrip();
            m_newToolStripDropDownButton = new ToolStripDropDownButton();
            m_newProject2ToolStripMenuItem = new ToolStripMenuItem();
            m_newResource2ToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator8 = new ToolStripSeparator();
            m_openProjectToolStripButton = new ToolStripButton();
            m_saveToolStripButton = new ToolStripButton();
            m_saveAllToolStripButton = new ToolStripButton();
            toolStripSeparator6 = new ToolStripSeparator();
            m_cutToolStripButton = new ToolStripButton();
            m_copyToolStripButton = new ToolStripButton();
            m_pasteToolStripButton = new ToolStripButton();
            toolStripSeparator7 = new ToolStripSeparator();
            m_undoToolStripButton = new ToolStripButton();
            m_redoToolStripButton = new ToolStripButton();
            toolStripSeparator9 = new ToolStripSeparator();
            m_findToolStripTextBox = new ToolStripTextBox();
            m_debuggerToolStrip = new ToolStrip();
            m_enableDebuggingToolStripButton = new ToolStripButton();
            toolStripSeparator4 = new ToolStripSeparator();
            m_resumeExecutionToolStripButton = new ToolStripButton();
            m_breakAllToolStripButton = new ToolStripButton();
            m_stopDebuggingToolStripButton = new ToolStripButton();
            toolStripSeparator5 = new ToolStripSeparator();
            m_stepIntoToolStripButton = new ToolStripButton();
            m_stepOverToolStripButton = new ToolStripButton();
            m_stepOutToolStripButton = new ToolStripButton();
            m_toolStripContainer = new ToolStripContainer();
            m_menuStrip.SuspendLayout();
            m_statusStrip.SuspendLayout();
            m_standardToolStrip.SuspendLayout();
            m_debuggerToolStrip.SuspendLayout();
            m_toolStripContainer.TopToolStripPanel.SuspendLayout();
            m_toolStripContainer.SuspendLayout();
            SuspendLayout();
            m_menuStrip.Items.AddRange(new ToolStripItem[8]
            {
                (ToolStripItem)m_fileToolStripMenuItem,
                (ToolStripItem)m_editToolStripMenuItem,
                (ToolStripItem)m_viewToolStripMenuItem,
                (ToolStripItem)m_buildToolStripMenuItem,
                (ToolStripItem)toolStripMenuItem14,
                (ToolStripItem)m_toolsToolStripMenuItem,
                (ToolStripItem)m_windowToolStripMenuItem,
                (ToolStripItem)m_helpToolStripMenuItem
            });
            m_menuStrip.Location = new Point(0, 0);
            m_menuStrip.Name = "m_menuStrip";
            m_menuStrip.Size = new Size(800, 24);
            m_menuStrip.TabIndex = 1;
            m_menuStrip.Text = "menuStrip1";
            m_fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[19]
            {
                (ToolStripItem)m_newToolStripMenuItem,
                (ToolStripItem)m_openToolStripMenuItem,
                (ToolStripItem)toolStripMenuItem5,
                (ToolStripItem)m_addToolStripMenuItem,
                (ToolStripItem)toolStripMenuItem16,
                (ToolStripItem)m_closeToolStripMenuItem,
                (ToolStripItem)m_closeProjectToolStripMenuItem,
                (ToolStripItem)toolStripMenuItem6,
                (ToolStripItem)m_saveToolStripMenuItem,
                (ToolStripItem)m_saveAsToolStripMenuItem,
                (ToolStripItem)m_saveAllToolStripMenuItem,
                (ToolStripItem)toolStripMenuItem7,
                (ToolStripItem)m_pageSetupToolStripMenuItem,
                (ToolStripItem)m_printToolStripMenuItem,
                (ToolStripItem)toolStripMenuItem8,
                (ToolStripItem)m_recentResourcesToolStripMenuItem,
                (ToolStripItem)m_recentProjectsToolStripMenuItem,
                (ToolStripItem)toolStripMenuItem9,
                (ToolStripItem)m_exitToolStripMenuItem
            });
            m_fileToolStripMenuItem.Name = "m_fileToolStripMenuItem";
            m_fileToolStripMenuItem.Size = new Size(37, 20);
            m_fileToolStripMenuItem.Text = "&File";
            m_newToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[2]
            {
                (ToolStripItem)m_newProjectToolStripMenuItem,
                (ToolStripItem)m_newResourceToolStripMenuItem
            });
            m_newToolStripMenuItem.Name = "m_newToolStripMenuItem";
            m_newToolStripMenuItem.Size = new Size(166, 22);
            m_newToolStripMenuItem.Text = "New";
            m_newProjectToolStripMenuItem.Name = "m_newProjectToolStripMenuItem";
            m_newProjectToolStripMenuItem.Size = new Size(131, 22);
            m_newProjectToolStripMenuItem.Text = "Project...";
            m_newProjectToolStripMenuItem.Click += new EventHandler(OnNewProject);
            m_newResourceToolStripMenuItem.Name = "m_newResourceToolStripMenuItem";
            m_newResourceToolStripMenuItem.Size = new Size(131, 22);
            m_newResourceToolStripMenuItem.Text = "Resource...";
            m_newResourceToolStripMenuItem.Click += new EventHandler(OnNewResource);
            m_openToolStripMenuItem.Image = (Image)Properties.Resources.Open;
            m_openToolStripMenuItem.Name = "m_openToolStripMenuItem";
            m_openToolStripMenuItem.Size = new Size(166, 22);
            m_openToolStripMenuItem.Text = "Open...";
            toolStripMenuItem5.Name = "toolStripMenuItem5";
            toolStripMenuItem5.Size = new Size(163, 6);
            m_addToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[2]
            {
                (ToolStripItem)m_addNewBundleToolStripMenuItem,
                (ToolStripItem)m_addExistingBundleToolStripMenuItem
            });
            m_addToolStripMenuItem.Enabled = false;
            m_addToolStripMenuItem.Name = "m_addToolStripMenuItem";
            m_addToolStripMenuItem.Size = new Size(166, 22);
            m_addToolStripMenuItem.Text = "Add";
            m_addNewBundleToolStripMenuItem.Enabled = false;
            m_addNewBundleToolStripMenuItem.Name = "m_addNewBundleToolStripMenuItem";
            m_addNewBundleToolStripMenuItem.Size = new Size(163, 22);
            m_addNewBundleToolStripMenuItem.Text = "New Bundle...";
            m_addNewBundleToolStripMenuItem.Click += new EventHandler(OnAddNewBundle);
            m_addExistingBundleToolStripMenuItem.Enabled = false;
            m_addExistingBundleToolStripMenuItem.Name = "m_addExistingBundleToolStripMenuItem";
            m_addExistingBundleToolStripMenuItem.Size = new Size(163, 22);
            m_addExistingBundleToolStripMenuItem.Text = "Existing Bundle...";
            m_addExistingBundleToolStripMenuItem.Click += new EventHandler(OnAddExistingBundle);
            toolStripMenuItem16.Name = "toolStripMenuItem16";
            toolStripMenuItem16.Size = new Size(163, 6);
            m_closeToolStripMenuItem.Enabled = false;
            m_closeToolStripMenuItem.Name = "m_closeToolStripMenuItem";
            m_closeToolStripMenuItem.Size = new Size(166, 22);
            m_closeToolStripMenuItem.Text = "Close";
            m_closeProjectToolStripMenuItem.Enabled = false;
            m_closeProjectToolStripMenuItem.Name = "m_closeProjectToolStripMenuItem";
            m_closeProjectToolStripMenuItem.Size = new Size(166, 22);
            m_closeProjectToolStripMenuItem.Text = "Close Project";
            toolStripMenuItem6.Name = "toolStripMenuItem6";
            toolStripMenuItem6.Size = new Size(163, 6);
            m_saveToolStripMenuItem.Enabled = false;
            m_saveToolStripMenuItem.Image = (Image)Properties.Resources.Save;
            m_saveToolStripMenuItem.Name = "m_saveToolStripMenuItem";
            m_saveToolStripMenuItem.ShortcutKeys = Keys.S | Keys.Control;
            m_saveToolStripMenuItem.Size = new Size(166, 22);
            m_saveToolStripMenuItem.Text = "Save";
            m_saveAsToolStripMenuItem.Enabled = false;
            m_saveAsToolStripMenuItem.Image = (Image)Properties.Resources.SaveAs;
            m_saveAsToolStripMenuItem.Name = "m_saveAsToolStripMenuItem";
            m_saveAsToolStripMenuItem.Size = new Size(166, 22);
            m_saveAsToolStripMenuItem.Text = "Save As...";
            m_saveAllToolStripMenuItem.Enabled = false;
            m_saveAllToolStripMenuItem.Image = (Image)Properties.Resources.SaveAll;
            m_saveAllToolStripMenuItem.Name = "m_saveAllToolStripMenuItem";
            m_saveAllToolStripMenuItem.Size = new Size(166, 22);
            m_saveAllToolStripMenuItem.Text = "Save All";
            toolStripMenuItem7.Name = "toolStripMenuItem7";
            toolStripMenuItem7.Size = new Size(163, 6);
            m_pageSetupToolStripMenuItem.Enabled = false;
            m_pageSetupToolStripMenuItem.Image = (Image)Properties.Resources.PageSetup;
            m_pageSetupToolStripMenuItem.Name = "m_pageSetupToolStripMenuItem";
            m_pageSetupToolStripMenuItem.Size = new Size(166, 22);
            m_pageSetupToolStripMenuItem.Text = "Page Setup...";
            m_printToolStripMenuItem.Enabled = false;
            m_printToolStripMenuItem.Image = (Image)Properties.Resources.Print;
            m_printToolStripMenuItem.Name = "m_printToolStripMenuItem";
            m_printToolStripMenuItem.ShortcutKeys = Keys.P | Keys.Control;
            m_printToolStripMenuItem.Size = new Size(166, 22);
            m_printToolStripMenuItem.Text = "Print...";
            toolStripMenuItem8.Name = "toolStripMenuItem8";
            toolStripMenuItem8.Size = new Size(163, 6);
            m_recentResourcesToolStripMenuItem.Enabled = false;
            m_recentResourcesToolStripMenuItem.Name = "m_recentResourcesToolStripMenuItem";
            m_recentResourcesToolStripMenuItem.Size = new Size(166, 22);
            m_recentResourcesToolStripMenuItem.Text = "Recent Resources";
            m_recentProjectsToolStripMenuItem.Enabled = false;
            m_recentProjectsToolStripMenuItem.Name = "m_recentProjectsToolStripMenuItem";
            m_recentProjectsToolStripMenuItem.Size = new Size(166, 22);
            m_recentProjectsToolStripMenuItem.Text = "Recent Projects";
            toolStripMenuItem9.Name = "toolStripMenuItem9";
            toolStripMenuItem9.Size = new Size(163, 6);
            m_exitToolStripMenuItem.Name = "m_exitToolStripMenuItem";
            m_exitToolStripMenuItem.ShortcutKeys = Keys.F4 | Keys.Alt;
            m_exitToolStripMenuItem.Size = new Size(166, 22);
            m_exitToolStripMenuItem.Text = "E&xit";
            m_exitToolStripMenuItem.Click += new EventHandler(OnExit);
            m_editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[14]
            {
                (ToolStripItem)m_undoToolStripMenuItem,
                (ToolStripItem)m_redoToolStripMenuItem,
                (ToolStripItem)toolStripMenuItem10,
                (ToolStripItem)m_cutToolStripMenuItem,
                (ToolStripItem)m_copyToolStripMenuItem,
                (ToolStripItem)m_pasteToolStripMenuItem,
                (ToolStripItem)m_deleteToolStripMenuItem,
                (ToolStripItem)toolStripMenuItem11,
                (ToolStripItem)m_selectAllToolStripMenuItem,
                (ToolStripItem)toolStripMenuItem12,
                (ToolStripItem)m_findAndReplaceToolStripMenuItem,
                (ToolStripItem)goToToolStripMenuItem,
                (ToolStripItem)toolStripMenuItem13,
                (ToolStripItem)m_bookmarksToolStripMenuItem
            });
            m_editToolStripMenuItem.Name = "m_editToolStripMenuItem";
            m_editToolStripMenuItem.Size = new Size(39, 20);
            m_editToolStripMenuItem.Text = "&Edit";
            m_undoToolStripMenuItem.Enabled = false;
            m_undoToolStripMenuItem.Image = (Image)Properties.Resources.Undo;
            m_undoToolStripMenuItem.Name = "m_undoToolStripMenuItem";
            m_undoToolStripMenuItem.Size = new Size(164, 22);
            m_undoToolStripMenuItem.Text = "Undo";
            m_redoToolStripMenuItem.Enabled = false;
            m_redoToolStripMenuItem.Image = (Image)Properties.Resources.Redo;
            m_redoToolStripMenuItem.Name = "m_redoToolStripMenuItem";
            m_redoToolStripMenuItem.Size = new Size(164, 22);
            m_redoToolStripMenuItem.Text = "Redo";
            toolStripMenuItem10.Name = "toolStripMenuItem10";
            toolStripMenuItem10.Size = new Size(161, 6);
            m_cutToolStripMenuItem.Enabled = false;
            m_cutToolStripMenuItem.Image = (Image)Properties.Resources.Cut;
            m_cutToolStripMenuItem.Name = "m_cutToolStripMenuItem";
            m_cutToolStripMenuItem.ShortcutKeys = Keys.X | Keys.Control;
            m_cutToolStripMenuItem.Size = new Size(164, 22);
            m_cutToolStripMenuItem.Text = "Cut";
            m_copyToolStripMenuItem.Enabled = false;
            m_copyToolStripMenuItem.Image = (Image)Properties.Resources.Copy;
            m_copyToolStripMenuItem.Name = "m_copyToolStripMenuItem";
            m_copyToolStripMenuItem.ShortcutKeys = Keys.C | Keys.Control;
            m_copyToolStripMenuItem.Size = new Size(164, 22);
            m_copyToolStripMenuItem.Text = "Copy";
            m_pasteToolStripMenuItem.Enabled = false;
            m_pasteToolStripMenuItem.Image = (Image)Properties.Resources.Paste;
            m_pasteToolStripMenuItem.Name = "m_pasteToolStripMenuItem";
            m_pasteToolStripMenuItem.ShortcutKeys = Keys.V | Keys.Control;
            m_pasteToolStripMenuItem.Size = new Size(164, 22);
            m_pasteToolStripMenuItem.Text = "Paste";
            m_deleteToolStripMenuItem.Enabled = false;
            m_deleteToolStripMenuItem.Image = (Image)Properties.Resources.Delete;
            m_deleteToolStripMenuItem.Name = "m_deleteToolStripMenuItem";
            m_deleteToolStripMenuItem.Size = new Size(164, 22);
            m_deleteToolStripMenuItem.Text = "Delete";
            toolStripMenuItem11.Name = "toolStripMenuItem11";
            toolStripMenuItem11.Size = new Size(161, 6);
            m_selectAllToolStripMenuItem.Enabled = false;
            m_selectAllToolStripMenuItem.Name = "m_selectAllToolStripMenuItem";
            m_selectAllToolStripMenuItem.ShortcutKeys = Keys.A | Keys.Control;
            m_selectAllToolStripMenuItem.Size = new Size(164, 22);
            m_selectAllToolStripMenuItem.Text = "Select All";
            toolStripMenuItem12.Name = "toolStripMenuItem12";
            toolStripMenuItem12.Size = new Size(161, 6);
            m_findAndReplaceToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[2]
            {
                (ToolStripItem)m_findToolStripMenuItem,
                (ToolStripItem)m_replaceToolStripMenuItem
            });
            m_findAndReplaceToolStripMenuItem.Name = "m_findAndReplaceToolStripMenuItem";
            m_findAndReplaceToolStripMenuItem.Size = new Size(164, 22);
            m_findAndReplaceToolStripMenuItem.Text = "Find and Replace";
            m_findToolStripMenuItem.Name = "m_findToolStripMenuItem";
            m_findToolStripMenuItem.Size = new Size(124, 22);
            m_findToolStripMenuItem.Text = "Find...";
            m_replaceToolStripMenuItem.Name = "m_replaceToolStripMenuItem";
            m_replaceToolStripMenuItem.Size = new Size(124, 22);
            m_replaceToolStripMenuItem.Text = "Replace...";
            goToToolStripMenuItem.Enabled = false;
            goToToolStripMenuItem.Name = "goToToolStripMenuItem";
            goToToolStripMenuItem.Size = new Size(164, 22);
            goToToolStripMenuItem.Text = "Go To...";
            toolStripMenuItem13.Name = "toolStripMenuItem13";
            toolStripMenuItem13.Size = new Size(161, 6);
            m_bookmarksToolStripMenuItem.Name = "m_bookmarksToolStripMenuItem";
            m_bookmarksToolStripMenuItem.Size = new Size(164, 22);
            m_bookmarksToolStripMenuItem.Text = "Bookmarks";
            m_viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[19]
            {
                (ToolStripItem)m_viewProjectExplorerToolStripMenuItem,
                (ToolStripItem)toolStripMenuItem1,
                (ToolStripItem)m_timersToolStripMenuItem,
                (ToolStripItem)m_viewStackToolStripMenuItem,
                (ToolStripItem)m_bundleFiltersToolStripMenuItem,
                (ToolStripItem)toolStripMenuItem3,
                (ToolStripItem)m_globalsToolStripMenuItem,
                (ToolStripItem)m_localsToolStripMenuItem,
                (ToolStripItem)m_callstackToolStripMenuItem,
                (ToolStripItem)m_immediateWindowToolStripMenuItem,
                (ToolStripItem)toolStripMenuItem4,
                (ToolStripItem)m_errorListToolStripMenuItem,
                (ToolStripItem)m_outputToolStripMenuItem,
                (ToolStripItem)m_propertiesToolStripMenuItem,
                (ToolStripItem)toolStripMenuItem2,
                (ToolStripItem)m_legacyDebuggerToolStripMenuItem,
                (ToolStripItem)toolStripMenuItem15,
                (ToolStripItem)m_showWhitespaceToolStripMenuItem,
                (ToolStripItem)m_showEOLMarkerToolStripMenuItem
            });
            m_viewToolStripMenuItem.Name = "m_viewToolStripMenuItem";
            m_viewToolStripMenuItem.Size = new Size(44, 20);
            m_viewToolStripMenuItem.Text = "&View";
            m_viewProjectExplorerToolStripMenuItem.Image = (Image)Properties.Resources.Bundle;
            m_viewProjectExplorerToolStripMenuItem.Name = "m_viewProjectExplorerToolStripMenuItem";
            m_viewProjectExplorerToolStripMenuItem.ShortcutKeys = Keys.L | Keys.Control | Keys.Alt;
            m_viewProjectExplorerToolStripMenuItem.Size = new Size(238, 22);
            m_viewProjectExplorerToolStripMenuItem.Text = "Project Explorer";
            m_viewProjectExplorerToolStripMenuItem.Click += new EventHandler(OnShowBundleExplorer);
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(235, 6);
            m_timersToolStripMenuItem.Image = (Image)Properties.Resources.Timers;
            m_timersToolStripMenuItem.Name = "m_timersToolStripMenuItem";
            m_timersToolStripMenuItem.Size = new Size(238, 22);
            m_timersToolStripMenuItem.Text = "Timers";
            m_timersToolStripMenuItem.Click += new EventHandler(OnShowTimers);
            m_viewStackToolStripMenuItem.Image = (Image)Properties.Resources.Views;
            m_viewStackToolStripMenuItem.Name = "m_viewStackToolStripMenuItem";
            m_viewStackToolStripMenuItem.Size = new Size(238, 22);
            m_viewStackToolStripMenuItem.Text = "View Stack";
            m_viewStackToolStripMenuItem.Click += new EventHandler(OnShowViewStack);
            m_bundleFiltersToolStripMenuItem.Image = (Image)Properties.Resources.Filter;
            m_bundleFiltersToolStripMenuItem.Name = "m_bundleFiltersToolStripMenuItem";
            m_bundleFiltersToolStripMenuItem.Size = new Size(238, 22);
            m_bundleFiltersToolStripMenuItem.Text = "Bundle Filters";
            m_bundleFiltersToolStripMenuItem.Click += new EventHandler(OnShowBundleFilters);
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new Size(235, 6);
            m_globalsToolStripMenuItem.Name = "m_globalsToolStripMenuItem";
            m_globalsToolStripMenuItem.Size = new Size(238, 22);
            m_globalsToolStripMenuItem.Text = "Globals";
            m_globalsToolStripMenuItem.Click += new EventHandler(OnShowGlobals);
            m_localsToolStripMenuItem.Name = "m_localsToolStripMenuItem";
            m_localsToolStripMenuItem.Size = new Size(238, 22);
            m_localsToolStripMenuItem.Text = "Locals";
            m_localsToolStripMenuItem.Click += new EventHandler(OnShowLocals);
            m_callstackToolStripMenuItem.Image = (Image)Properties.Resources.CallStack;
            m_callstackToolStripMenuItem.Name = "m_callstackToolStripMenuItem";
            m_callstackToolStripMenuItem.Size = new Size(238, 22);
            m_callstackToolStripMenuItem.Text = "Call Stack";
            m_callstackToolStripMenuItem.Click += new EventHandler(OnShowCallStack);
            m_immediateWindowToolStripMenuItem.Image = (Image)Properties.Resources.ImmediateWindow;
            m_immediateWindowToolStripMenuItem.Name = "m_immediateWindowToolStripMenuItem";
            m_immediateWindowToolStripMenuItem.ShortcutKeys = Keys.I | Keys.Control | Keys.Alt;
            m_immediateWindowToolStripMenuItem.Size = new Size(238, 22);
            m_immediateWindowToolStripMenuItem.Text = "Immediate Window";
            m_immediateWindowToolStripMenuItem.Click += new EventHandler(OnShowImmediateWindow);
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new Size(235, 6);
            m_errorListToolStripMenuItem.Image = (Image)Properties.Resources.ErrorList;
            m_errorListToolStripMenuItem.Name = "m_errorListToolStripMenuItem";
            m_errorListToolStripMenuItem.Size = new Size(238, 22);
            m_errorListToolStripMenuItem.Text = "Error List";
            m_errorListToolStripMenuItem.Click += new EventHandler(OnShowErrorList);
            m_outputToolStripMenuItem.Image = (Image)Properties.Resources.Output;
            m_outputToolStripMenuItem.Name = "m_outputToolStripMenuItem";
            m_outputToolStripMenuItem.Size = new Size(238, 22);
            m_outputToolStripMenuItem.Text = "Output";
            m_outputToolStripMenuItem.Click += new EventHandler(OnShowOutput);
            m_propertiesToolStripMenuItem.Image = (Image)Properties.Resources.Properties;
            m_propertiesToolStripMenuItem.Name = "m_propertiesToolStripMenuItem";
            m_propertiesToolStripMenuItem.ShortcutKeys = Keys.F4;
            m_propertiesToolStripMenuItem.Size = new Size(238, 22);
            m_propertiesToolStripMenuItem.Text = "Properties Window";
            m_propertiesToolStripMenuItem.Click += new EventHandler(OnShowProperties);
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(235, 6);
            m_legacyDebuggerToolStripMenuItem.Image = (Image)Properties.Resources.Debugger;
            m_legacyDebuggerToolStripMenuItem.Name = "m_legacyDebuggerToolStripMenuItem";
            m_legacyDebuggerToolStripMenuItem.Size = new Size(238, 22);
            m_legacyDebuggerToolStripMenuItem.Text = "Legacy Debugger...";
            m_legacyDebuggerToolStripMenuItem.Click += new EventHandler(OnShowLegacyDebugger);
            toolStripMenuItem15.Name = "toolStripMenuItem15";
            toolStripMenuItem15.Size = new Size(235, 6);
            m_showWhitespaceToolStripMenuItem.Enabled = false;
            m_showWhitespaceToolStripMenuItem.Name = "m_showWhitespaceToolStripMenuItem";
            m_showWhitespaceToolStripMenuItem.Size = new Size(238, 22);
            m_showWhitespaceToolStripMenuItem.Text = "Show Whitespace";
            m_showEOLMarkerToolStripMenuItem.Enabled = false;
            m_showEOLMarkerToolStripMenuItem.Name = "m_showEOLMarkerToolStripMenuItem";
            m_showEOLMarkerToolStripMenuItem.Size = new Size(238, 22);
            m_showEOLMarkerToolStripMenuItem.Text = "Show EOL Marker";
            m_buildToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[1]
            {
                (ToolStripItem)m_buildBundleToolStripMenuItem
            });
            m_buildToolStripMenuItem.Name = "m_buildToolStripMenuItem";
            m_buildToolStripMenuItem.Size = new Size(46, 20);
            m_buildToolStripMenuItem.Text = "&Build";
            m_buildBundleToolStripMenuItem.Name = "m_buildBundleToolStripMenuItem";
            m_buildBundleToolStripMenuItem.Size = new Size(141, 22);
            m_buildBundleToolStripMenuItem.Text = "Build Bundle";
            toolStripMenuItem14.DropDownItems.AddRange(new ToolStripItem[13]
            {
                (ToolStripItem)m_enableDebuggingToolStripMenuItem,
                (ToolStripItem)toolStripSeparator2,
                (ToolStripItem)m_resumeExecutionToolStripMenuItem,
                (ToolStripItem)m_breakAllToolStripMenuItem,
                (ToolStripItem)m_stopDebugginToolStripMenuItem,
                (ToolStripItem)toolStripSeparator1,
                (ToolStripItem)m_stepIntoToolStripMenuItem,
                (ToolStripItem)m_stepOverToolStripMenuItem,
                (ToolStripItem)m_stepOutToolStripMenuItem,
                (ToolStripItem)toolStripSeparator3,
                (ToolStripItem)m_toggleBreakpointToolStripMenuItem,
                (ToolStripItem)m_deleteAllBreakpointToolStripMenuItem,
                (ToolStripItem)m_breakpointsToolStripMenuItem
            });
            toolStripMenuItem14.Name = "toolStripMenuItem14";
            toolStripMenuItem14.Size = new Size(54, 20);
            toolStripMenuItem14.Text = "&Debug";
            m_enableDebuggingToolStripMenuItem.Image = (Image)Properties.Resources.Debugger;
            m_enableDebuggingToolStripMenuItem.Name = "m_enableDebuggingToolStripMenuItem";
            m_enableDebuggingToolStripMenuItem.Size = new Size(190, 22);
            m_enableDebuggingToolStripMenuItem.Text = "Enable Debugging";
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(187, 6);
            m_resumeExecutionToolStripMenuItem.Enabled = false;
            m_resumeExecutionToolStripMenuItem.Image = (Image)Properties.Resources.DebugPlay;
            m_resumeExecutionToolStripMenuItem.Name = "m_resumeExecutionToolStripMenuItem";
            m_resumeExecutionToolStripMenuItem.Size = new Size(190, 22);
            m_resumeExecutionToolStripMenuItem.Text = "Resume Execution";
            m_breakAllToolStripMenuItem.Enabled = false;
            m_breakAllToolStripMenuItem.Image = (Image)Properties.Resources.DebugPause;
            m_breakAllToolStripMenuItem.Name = "m_breakAllToolStripMenuItem";
            m_breakAllToolStripMenuItem.Size = new Size(190, 22);
            m_breakAllToolStripMenuItem.Text = "Break All";
            m_stopDebugginToolStripMenuItem.Enabled = false;
            m_stopDebugginToolStripMenuItem.Image = (Image)Properties.Resources.DebugStop;
            m_stopDebugginToolStripMenuItem.Name = "m_stopDebugginToolStripMenuItem";
            m_stopDebugginToolStripMenuItem.Size = new Size(190, 22);
            m_stopDebugginToolStripMenuItem.Text = "Stop Debugging";
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(187, 6);
            m_stepIntoToolStripMenuItem.Enabled = false;
            m_stepIntoToolStripMenuItem.Image = (Image)Properties.Resources.DebugStepInto;
            m_stepIntoToolStripMenuItem.Name = "m_stepIntoToolStripMenuItem";
            m_stepIntoToolStripMenuItem.ShortcutKeys = Keys.F11;
            m_stepIntoToolStripMenuItem.Size = new Size(190, 22);
            m_stepIntoToolStripMenuItem.Text = "Step Into";
            m_stepOverToolStripMenuItem.Enabled = false;
            m_stepOverToolStripMenuItem.Image = (Image)Properties.Resources.DebugStepOver;
            m_stepOverToolStripMenuItem.Name = "m_stepOverToolStripMenuItem";
            m_stepOverToolStripMenuItem.ShortcutKeys = Keys.F10;
            m_stepOverToolStripMenuItem.Size = new Size(190, 22);
            m_stepOverToolStripMenuItem.Text = "Step Over";
            m_stepOutToolStripMenuItem.Enabled = false;
            m_stepOutToolStripMenuItem.Image = (Image)Properties.Resources.DebugStepOut;
            m_stepOutToolStripMenuItem.Name = "m_stepOutToolStripMenuItem";
            m_stepOutToolStripMenuItem.ShortcutKeys = Keys.F11 | Keys.Shift;
            m_stepOutToolStripMenuItem.Size = new Size(190, 22);
            m_stepOutToolStripMenuItem.Text = "Step Out";
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(187, 6);
            m_toggleBreakpointToolStripMenuItem.Enabled = false;
            m_toggleBreakpointToolStripMenuItem.Image = (Image)Properties.Resources.DebugBreakpoint;
            m_toggleBreakpointToolStripMenuItem.Name = "m_toggleBreakpointToolStripMenuItem";
            m_toggleBreakpointToolStripMenuItem.ShortcutKeys = Keys.F9;
            m_toggleBreakpointToolStripMenuItem.Size = new Size(190, 22);
            m_toggleBreakpointToolStripMenuItem.Text = "Toggle Breakpoint";
            m_deleteAllBreakpointToolStripMenuItem.Enabled = false;
            m_deleteAllBreakpointToolStripMenuItem.Image = (Image)Properties.Resources.DebugBreakpointClear;
            m_deleteAllBreakpointToolStripMenuItem.Name = "m_deleteAllBreakpointToolStripMenuItem";
            m_deleteAllBreakpointToolStripMenuItem.Size = new Size(190, 22);
            m_deleteAllBreakpointToolStripMenuItem.Text = "Delete All Breakpoints";
            m_breakpointsToolStripMenuItem.Name = "m_breakpointsToolStripMenuItem";
            m_breakpointsToolStripMenuItem.Size = new Size(190, 22);
            m_breakpointsToolStripMenuItem.Text = "Breakpoints";
            m_breakpointsToolStripMenuItem.Visible = false;
            m_toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[1]
            {
                (ToolStripItem)m_optionsToolStripMenuItem
            });
            m_toolsToolStripMenuItem.Name = "m_toolsToolStripMenuItem";
            m_toolsToolStripMenuItem.Size = new Size(48, 20);
            m_toolsToolStripMenuItem.Text = "&Tools";
            m_optionsToolStripMenuItem.Image = (Image)Properties.Resources.Tools;
            m_optionsToolStripMenuItem.Name = "m_optionsToolStripMenuItem";
            m_optionsToolStripMenuItem.Size = new Size(125, 22);
            m_optionsToolStripMenuItem.Text = "Options...";
            m_optionsToolStripMenuItem.Click += new EventHandler(OnShowOptions);
            m_windowToolStripMenuItem.Name = "m_windowToolStripMenuItem";
            m_windowToolStripMenuItem.Size = new Size(63, 20);
            m_windowToolStripMenuItem.Text = "&Window";
            m_helpToolStripMenuItem.Name = "m_helpToolStripMenuItem";
            m_helpToolStripMenuItem.Size = new Size(44, 20);
            m_helpToolStripMenuItem.Text = "&Help";
            m_statusStrip.Items.AddRange(new ToolStripItem[3]
            {
                (ToolStripItem)m_toolStripStatusLabel,
                (ToolStripItem)m_toolStripProgressBar,
                (ToolStripItem)m_editorToolStripStatusLabel
            });
            m_statusStrip.Location = new Point(0, 551);
            m_statusStrip.Name = "m_statusStrip";
            m_statusStrip.Size = new Size(800, 22);
            m_statusStrip.TabIndex = 2;
            m_statusStrip.Text = "statusStrip1";
            m_toolStripStatusLabel.Name = "m_toolStripStatusLabel";
            m_toolStripStatusLabel.Size = new Size(785, 17);
            m_toolStripStatusLabel.Spring = true;
            m_toolStripStatusLabel.Text = "Ready";
            m_toolStripStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            m_toolStripProgressBar.Name = "m_toolStripProgressBar";
            m_toolStripProgressBar.Size = new Size(100, 16);
            m_toolStripProgressBar.Visible = false;
            m_editorToolStripStatusLabel.AutoSize = false;
            m_editorToolStripStatusLabel.Name = "m_editorToolStripStatusLabel";
            m_editorToolStripStatusLabel.Size = new Size(150, 17);
            m_editorToolStripStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            m_editorToolStripStatusLabel.Visible = false;
            m_dockPanel.ActiveAutoHideContent = (IDockContent)null;
            m_dockPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            m_dockPanel.DockBackColor = SystemColors.AppWorkspace;
            m_dockPanel.Location = new Point(0, 76);
            m_dockPanel.Name = "m_dockPanel";
            m_dockPanel.Size = new Size(800, 472);
            dockPanelGradient1.EndColor = SystemColors.ControlLight;
            dockPanelGradient1.StartColor = SystemColors.ControlLight;
            autoHideStripSkin.DockStripGradient = dockPanelGradient1;
            tabGradient1.EndColor = SystemColors.Control;
            tabGradient1.StartColor = SystemColors.Control;
            tabGradient1.TextColor = SystemColors.ControlDarkDark;
            autoHideStripSkin.TabGradient = tabGradient1;
            dockPanelSkin.AutoHideStripSkin = autoHideStripSkin;
            tabGradient2.EndColor = SystemColors.ControlLightLight;
            tabGradient2.StartColor = SystemColors.ControlLightLight;
            tabGradient2.TextColor = SystemColors.ControlText;
            paneStripGradient.ActiveTabGradient = tabGradient2;
            dockPanelGradient2.EndColor = SystemColors.Control;
            dockPanelGradient2.StartColor = SystemColors.Control;
            paneStripGradient.DockStripGradient = dockPanelGradient2;
            tabGradient3.EndColor = SystemColors.ControlLight;
            tabGradient3.StartColor = SystemColors.ControlLight;
            tabGradient3.TextColor = SystemColors.ControlText;
            paneStripGradient.InactiveTabGradient = tabGradient3;
            dockPaneStripSkin.DocumentGradient = paneStripGradient;
            tabGradient4.EndColor = SystemColors.ActiveCaption;
            tabGradient4.LinearGradientMode = LinearGradientMode.Vertical;
            tabGradient4.StartColor = SystemColors.GradientActiveCaption;
            tabGradient4.TextColor = SystemColors.ActiveCaptionText;
            toolWindowGradient.ActiveCaptionGradient = tabGradient4;
            tabGradient5.EndColor = SystemColors.Control;
            tabGradient5.StartColor = SystemColors.Control;
            tabGradient5.TextColor = SystemColors.ControlText;
            toolWindowGradient.ActiveTabGradient = tabGradient5;
            dockPanelGradient3.EndColor = SystemColors.ControlLight;
            dockPanelGradient3.StartColor = SystemColors.ControlLight;
            toolWindowGradient.DockStripGradient = dockPanelGradient3;
            tabGradient6.EndColor = SystemColors.GradientInactiveCaption;
            tabGradient6.LinearGradientMode = LinearGradientMode.Vertical;
            tabGradient6.StartColor = SystemColors.GradientInactiveCaption;
            tabGradient6.TextColor = SystemColors.ControlText;
            toolWindowGradient.InactiveCaptionGradient = tabGradient6;
            tabGradient7.EndColor = Color.Transparent;
            tabGradient7.StartColor = Color.Transparent;
            tabGradient7.TextColor = SystemColors.ControlDarkDark;
            toolWindowGradient.InactiveTabGradient = tabGradient7;
            dockPaneStripSkin.ToolWindowGradient = toolWindowGradient;
            dockPanelSkin.DockPaneStripSkin = dockPaneStripSkin;
            m_dockPanel.Skin = dockPanelSkin;
            m_dockPanel.TabIndex = 3;
            m_standardToolStrip.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            m_standardToolStrip.Dock = DockStyle.None;
            m_standardToolStrip.Items.AddRange(new ToolStripItem[14]
            {
                (ToolStripItem)m_newToolStripDropDownButton,
                (ToolStripItem)toolStripSeparator8,
                (ToolStripItem)m_openProjectToolStripButton,
                (ToolStripItem)m_saveToolStripButton,
                (ToolStripItem)m_saveAllToolStripButton,
                (ToolStripItem)toolStripSeparator6,
                (ToolStripItem)m_cutToolStripButton,
                (ToolStripItem)m_copyToolStripButton,
                (ToolStripItem)m_pasteToolStripButton,
                (ToolStripItem)toolStripSeparator7,
                (ToolStripItem)m_undoToolStripButton,
                (ToolStripItem)m_redoToolStripButton,
                (ToolStripItem)toolStripSeparator9,
                (ToolStripItem)m_findToolStripTextBox
            });
            m_standardToolStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            m_standardToolStrip.Location = new Point(3, 25);
            m_standardToolStrip.Name = "m_standardToolStrip";
            m_standardToolStrip.Size = new Size(401, 25);
            m_standardToolStrip.TabIndex = 5;
            m_standardToolStrip.Text = "Standard";
            m_newToolStripDropDownButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_newToolStripDropDownButton.DropDownItems.AddRange(new ToolStripItem[2]
            {
                (ToolStripItem)m_newProject2ToolStripMenuItem,
                (ToolStripItem)m_newResource2ToolStripMenuItem
            });
            m_newToolStripDropDownButton.Image = (Image)Properties.Resources.NewDocument;
            m_newToolStripDropDownButton.ImageTransparentColor = Color.Magenta;
            m_newToolStripDropDownButton.Name = "m_newToolStripDropDownButton";
            m_newToolStripDropDownButton.Size = new Size(29, 22);
            m_newToolStripDropDownButton.Text = "toolStripDropDownButton1";
            m_newToolStripDropDownButton.ToolTipText = "New";
            m_newProject2ToolStripMenuItem.Name = "m_newProject2ToolStripMenuItem";
            m_newProject2ToolStripMenuItem.Size = new Size(158, 22);
            m_newProject2ToolStripMenuItem.Text = "New Project...";
            m_newProject2ToolStripMenuItem.Click += new EventHandler(OnNewProject);
            m_newResource2ToolStripMenuItem.Name = "m_newResource2ToolStripMenuItem";
            m_newResource2ToolStripMenuItem.Size = new Size(158, 22);
            m_newResource2ToolStripMenuItem.Text = "New Resource...";
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new Size(6, 25);
            m_openProjectToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_openProjectToolStripButton.Image = (Image)Properties.Resources.Open;
            m_openProjectToolStripButton.ImageTransparentColor = Color.Magenta;
            m_openProjectToolStripButton.Name = "m_openProjectToolStripButton";
            m_openProjectToolStripButton.Size = new Size(23, 22);
            m_openProjectToolStripButton.Text = "Open Project";
            m_openProjectToolStripButton.ToolTipText = "Open Project...";
            m_saveToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_saveToolStripButton.Enabled = false;
            m_saveToolStripButton.Image = (Image)Properties.Resources.Save;
            m_saveToolStripButton.ImageTransparentColor = Color.Magenta;
            m_saveToolStripButton.Name = "m_saveToolStripButton";
            m_saveToolStripButton.Size = new Size(23, 22);
            m_saveToolStripButton.Text = "toolStripButton1";
            m_saveToolStripButton.ToolTipText = "Save (Ctrl+S)";
            m_saveAllToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_saveAllToolStripButton.Enabled = false;
            m_saveAllToolStripButton.Image = (Image)Properties.Resources.SaveAll;
            m_saveAllToolStripButton.ImageTransparentColor = Color.Magenta;
            m_saveAllToolStripButton.Name = "m_saveAllToolStripButton";
            m_saveAllToolStripButton.Size = new Size(23, 22);
            m_saveAllToolStripButton.Text = "toolStripButton2";
            m_saveAllToolStripButton.ToolTipText = "Save All (Ctrl+Shift+S)";
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(6, 25);
            m_cutToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_cutToolStripButton.Enabled = false;
            m_cutToolStripButton.Image = (Image)Properties.Resources.Cut;
            m_cutToolStripButton.ImageTransparentColor = Color.Magenta;
            m_cutToolStripButton.Name = "m_cutToolStripButton";
            m_cutToolStripButton.Size = new Size(23, 22);
            m_cutToolStripButton.Text = "toolStripButton3";
            m_cutToolStripButton.ToolTipText = "Cut (Ctrl+X)";
            m_copyToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_copyToolStripButton.Enabled = false;
            m_copyToolStripButton.Image = (Image)Properties.Resources.Copy;
            m_copyToolStripButton.ImageTransparentColor = Color.Magenta;
            m_copyToolStripButton.Name = "m_copyToolStripButton";
            m_copyToolStripButton.Size = new Size(23, 22);
            m_copyToolStripButton.Text = "toolStripButton4";
            m_copyToolStripButton.ToolTipText = "Copy (Ctrl+V)";
            m_pasteToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_pasteToolStripButton.Enabled = false;
            m_pasteToolStripButton.Image = (Image)Properties.Resources.Paste;
            m_pasteToolStripButton.ImageTransparentColor = Color.Magenta;
            m_pasteToolStripButton.Name = "m_pasteToolStripButton";
            m_pasteToolStripButton.Size = new Size(23, 22);
            m_pasteToolStripButton.Text = "toolStripButton5";
            m_pasteToolStripButton.ToolTipText = "Paste (Ctrl+V)";
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new Size(6, 25);
            m_undoToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_undoToolStripButton.Enabled = false;
            m_undoToolStripButton.Image = (Image)Properties.Resources.Undo;
            m_undoToolStripButton.ImageTransparentColor = Color.Magenta;
            m_undoToolStripButton.Name = "m_undoToolStripButton";
            m_undoToolStripButton.Size = new Size(23, 22);
            m_undoToolStripButton.Text = "toolStripButton6";
            m_undoToolStripButton.ToolTipText = "Undo (Ctrl+Z)";
            m_redoToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_redoToolStripButton.Enabled = false;
            m_redoToolStripButton.Image = (Image)Properties.Resources.Redo;
            m_redoToolStripButton.ImageTransparentColor = Color.Magenta;
            m_redoToolStripButton.Name = "m_redoToolStripButton";
            m_redoToolStripButton.Size = new Size(23, 22);
            m_redoToolStripButton.Text = "toolStripButton7";
            m_redoToolStripButton.ToolTipText = "Redo (Ctrl+Y)";
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new Size(6, 25);
            m_findToolStripTextBox.MaxLength = (int)byte.MaxValue;
            m_findToolStripTextBox.Name = "m_findToolStripTextBox";
            m_findToolStripTextBox.Size = new Size(150, 25);
            m_debuggerToolStrip.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            m_debuggerToolStrip.Dock = DockStyle.None;
            m_debuggerToolStrip.Items.AddRange(new ToolStripItem[9]
            {
                (ToolStripItem)m_enableDebuggingToolStripButton,
                (ToolStripItem)toolStripSeparator4,
                (ToolStripItem)m_resumeExecutionToolStripButton,
                (ToolStripItem)m_breakAllToolStripButton,
                (ToolStripItem)m_stopDebuggingToolStripButton,
                (ToolStripItem)toolStripSeparator5,
                (ToolStripItem)m_stepIntoToolStripButton,
                (ToolStripItem)m_stepOverToolStripButton,
                (ToolStripItem)m_stepOutToolStripButton
            });
            m_debuggerToolStrip.Location = new Point(3, 0);
            m_debuggerToolStrip.Name = "m_debuggerToolStrip";
            m_debuggerToolStrip.Size = new Size(185, 25);
            m_debuggerToolStrip.TabIndex = 7;
            m_enableDebuggingToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_enableDebuggingToolStripButton.Image = (Image)Properties.Resources.Debugger;
            m_enableDebuggingToolStripButton.ImageTransparentColor = Color.Magenta;
            m_enableDebuggingToolStripButton.Name = "m_enableDebuggingToolStripButton";
            m_enableDebuggingToolStripButton.Size = new Size(23, 22);
            m_enableDebuggingToolStripButton.Text = "Enable Debugging";
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(6, 25);
            m_resumeExecutionToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_resumeExecutionToolStripButton.Enabled = false;
            m_resumeExecutionToolStripButton.Image = (Image)Properties.Resources.DebugPlay;
            m_resumeExecutionToolStripButton.ImageTransparentColor = Color.Magenta;
            m_resumeExecutionToolStripButton.Name = "m_resumeExecutionToolStripButton";
            m_resumeExecutionToolStripButton.Size = new Size(23, 22);
            m_resumeExecutionToolStripButton.Text = "ResumeExecution";
            m_resumeExecutionToolStripButton.ToolTipText = "Resume Execution";
            m_breakAllToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_breakAllToolStripButton.Enabled = false;
            m_breakAllToolStripButton.Image = (Image)Properties.Resources.DebugPause;
            m_breakAllToolStripButton.ImageTransparentColor = Color.Magenta;
            m_breakAllToolStripButton.Name = "m_breakAllToolStripButton";
            m_breakAllToolStripButton.Size = new Size(23, 22);
            m_breakAllToolStripButton.Text = "toolStripButton2";
            m_breakAllToolStripButton.ToolTipText = "Break All";
            m_stopDebuggingToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_stopDebuggingToolStripButton.Enabled = false;
            m_stopDebuggingToolStripButton.Image = (Image)Properties.Resources.DebugStop;
            m_stopDebuggingToolStripButton.ImageTransparentColor = Color.Magenta;
            m_stopDebuggingToolStripButton.Name = "m_stopDebuggingToolStripButton";
            m_stopDebuggingToolStripButton.Size = new Size(23, 22);
            m_stopDebuggingToolStripButton.Text = "toolStripButton3";
            m_stopDebuggingToolStripButton.ToolTipText = "Stop Debugging";
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(6, 25);
            m_stepIntoToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_stepIntoToolStripButton.Enabled = false;
            m_stepIntoToolStripButton.Image = (Image)Properties.Resources.DebugStepInto;
            m_stepIntoToolStripButton.ImageTransparentColor = Color.Magenta;
            m_stepIntoToolStripButton.Name = "m_stepIntoToolStripButton";
            m_stepIntoToolStripButton.Size = new Size(23, 22);
            m_stepIntoToolStripButton.Text = "toolStripButton4";
            m_stepIntoToolStripButton.ToolTipText = "Step Into";
            m_stepOverToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_stepOverToolStripButton.Enabled = false;
            m_stepOverToolStripButton.Image = (Image)Properties.Resources.DebugStepOver;
            m_stepOverToolStripButton.ImageTransparentColor = Color.Magenta;
            m_stepOverToolStripButton.Name = "m_stepOverToolStripButton";
            m_stepOverToolStripButton.Size = new Size(23, 22);
            m_stepOverToolStripButton.Text = "toolStripButton5";
            m_stepOverToolStripButton.ToolTipText = "Step Over";
            m_stepOutToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_stepOutToolStripButton.Enabled = false;
            m_stepOutToolStripButton.Image = (Image)Properties.Resources.DebugStepOut;
            m_stepOutToolStripButton.ImageTransparentColor = Color.Magenta;
            m_stepOutToolStripButton.Name = "m_stepOutToolStripButton";
            m_stepOutToolStripButton.Size = new Size(23, 22);
            m_stepOutToolStripButton.Text = "Step Out";
            m_toolStripContainer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            m_toolStripContainer.ContentPanel.Size = new Size(800, 2);
            m_toolStripContainer.Location = new Point(0, 24);
            m_toolStripContainer.Name = "m_toolStripContainer";
            m_toolStripContainer.Size = new Size(800, 52);
            m_toolStripContainer.TabIndex = 8;
            m_toolStripContainer.Text = "toolStripContainer1";
            m_toolStripContainer.TopToolStripPanel.Controls.Add((Control)m_debuggerToolStrip);
            m_toolStripContainer.TopToolStripPanel.Controls.Add((Control)m_standardToolStrip);
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 573);
            Controls.Add((Control)m_toolStripContainer);
            Controls.Add((Control)m_dockPanel);
            Controls.Add((Control)m_statusStrip);
            Controls.Add((Control)m_menuStrip);
            Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            IsMdiContainer = true;
            MainMenuStrip = m_menuStrip;
            MinimumSize = new Size(640, 480);
            Name = nameof(IdeMainForm);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Redbox Engine for Distributed Stores";
            Activated += new EventHandler(OnActivated);
            m_menuStrip.ResumeLayout(false);
            m_menuStrip.PerformLayout();
            m_statusStrip.ResumeLayout(false);
            m_statusStrip.PerformLayout();
            m_standardToolStrip.ResumeLayout(false);
            m_standardToolStrip.PerformLayout();
            m_debuggerToolStrip.ResumeLayout(false);
            m_debuggerToolStrip.PerformLayout();
            m_toolStripContainer.TopToolStripPanel.ResumeLayout(false);
            m_toolStripContainer.TopToolStripPanel.PerformLayout();
            m_toolStripContainer.ResumeLayout(false);
            m_toolStripContainer.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}