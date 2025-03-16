using Alsing.SourceCode;
using Alsing.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE.Properties;
using Redbox.Lua;
using Redbox.REDS.Framework;
using Skybound.VisualTips;
using Skybound.VisualTips.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using VisualHint.SmartPropertyGrid;

namespace Redbox.KioskEngine.IDE
{
    public class DebuggerForm : Form
    {
        private bool m_isActivated;
        private bool m_isApplicationRunning;
        private bool m_refreshingBreakpoints;
        private static readonly DebuggerForm m_instance = new DebuggerForm();
        private IContainer components;
        private StatusStrip m_statusStrip;
        private MenuStrip m_menuStrip;
        private ToolStripMenuItem m_debugToolStripMenuItem;
        private ToolStripMenuItem m_fileToolStripMenuItem;
        private Splitter m_mainSplitter;
        private SyntaxBoxControl m_syntaxBoxControl;
        private SyntaxDocument m_syntaxDocument;
        private Splitter m_sourceSplitter;
        private ToolStrip m_toolStrip;
        private TabControl m_tabControl;
        private TabPage m_localsTabPage;
        private TabPage m_callStackTabPage;
        private ListView m_callStackListView;
        private ColumnHeader m_funcNameColumnHeader;
        private ColumnHeader m_scriptNameColumnHeader;
        private ColumnHeader m_scriptLineNoColumnHeader;
        private TabPage m_globalsTabPage;
        private TabPage m_immediateTabPage;
        private ShellControl m_shellControl;
        private TreeView m_treeView;
        private ImageList m_imageList;
        private ToolStripButton m_resumeExecutionToolStripButton;
        private ToolStripButton m_breakAllToolStripButton;
        private ToolStripButton m_stopDebuggingToolStripButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton m_stepIntoToolStripButton;
        private ToolStripButton m_stepOverToolStripButton;
        private ToolStripMenuItem m_resumeExecutionToolStripMenuItem;
        private ToolStripMenuItem m_breakAllToolStripMenuItem;
        private ToolStripMenuItem m_stopDebugginToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem m_stepIntoToolStripMenuItem;
        private ToolStripMenuItem m_stepOverToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripMenuItem m_toggleBreakpointToolStripMenuItem;
        private ToolStripMenuItem m_deleteAllBreakpointToolStripMenuItem;
        private ToolStripStatusLabel m_statusLabel;
        private ToolStripMenuItem m_enableDebuggingToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton m_enableDebuggingToolStripButton;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton m_stepOutToolStripButton;
        private ToolStripMenuItem m_stepOutToolStripMenuItem;
        private ToolStripMenuItem m_editToolStripMenuItem;
        private DynamicPropertyGrid m_globalsPropertyGrid;
        private DynamicPropertyGrid m_localsPropertyGrid;
        private ToolStripMenuItem m_showWhitespaceToolStripMenuItem;
        private ToolStripMenuItem m_showEOLMarkerToolStripMenuItem;
        private ToolStripMenuItem m_switchBundleToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem m_closeToolStripMenuItem;
        private ToolStripMenuItem m_goToToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem m_findAndReplaceToolStripMenuItem;
        private ToolStripMenuItem m_quickFindToolStripMenuItem;
        private ToolStripMenuItem m_quickReplaceToolStripMenuItem;
        private VisualTipProvider m_visualTipProvider;
        private ToolStripMenuItem m_breakpointsToolStripMenuItem;
        private ImageList m_gutterImageList;

        public DebuggerForm()
        {
            InitializeComponent();
            m_syntaxDocument.SetSyntaxFromEmbeddedResource(typeof(DebuggerForm).Assembly,
                "Redbox.KioskEngine.IDE.Lua.syn");
            m_syntaxBoxControl.GutterIcons.Images.Add(m_gutterImageList.Images[0]);
        }

        public void ActivateDebugger()
        {
            ActivateDebugger((string)null);
        }

        public void ActivateDebugger(string resourceName)
        {
            ActivateDebugger(resourceName, 1, (string)null);
        }

        public void ActivateDebugger(string resourceName, int lineNumber, string error)
        {
            BringDebuggerToFront();
            if (resourceName != CurrentResourceName && m_treeView.Nodes.Count > 0)
            {
                SelectResourceNode(resourceName, m_treeView.Nodes[0]);
                SetDocumentText(resourceName);
            }

            var str = string.Format("Runtime Error: {0}", (object)error);
            SetCurrentDebugLine(lineNumber, !string.IsNullOrEmpty(error) ? str : (string)null);
            if (!string.IsNullOrEmpty(error) && string.Compare(error, "breakpoint hit", true) != 0)
            {
                var tip = new VisualTip(error, "REDS Engine Runtime Error")
                {
                    FooterText = string.Format("The error occurred on or near line: {0}.", (object)lineNumber)
                };
                var screen = m_syntaxBoxControl.PointToScreen(m_syntaxBoxControl.Cursor.HotSpot);
                m_visualTipProvider.ShowTip(tip, new Rectangle(screen.X + 250, screen.Y, 100, 100));
            }

            RefreshTabs();
        }

        public void Resume()
        {
            OnResume((object)this, EventArgs.Empty);
        }

        public void StepInto()
        {
            OnStepInto((object)this, EventArgs.Empty);
        }

        public void StepOut()
        {
            OnStepOut((object)this, EventArgs.Empty);
        }

        public void StepOver()
        {
            OnStepOver((object)this, EventArgs.Empty);
        }

        public static void BringDebuggerToFront()
        {
            m_instance.Show();
            m_instance.BringToFront();
        }

        public void SetDocumentText(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                m_syntaxDocument.Text = (string)null;
                Text = "REDS Engine Debugger";
            }
            else
            {
                var script = GetScript(resourceName);
                if (string.IsNullOrEmpty(script))
                    return;
                m_syntaxDocument.Text = script;
                m_syntaxBoxControl.GotoLine(1);
                Text = string.Format("{0} - REDS Engine Debugger", (object)resourceName);
                CurrentResourceName = resourceName;
                RefreshBreakpoints();
            }
        }

        public void SelectResourceNode(string resourceName)
        {
            SelectResourceNode(resourceName, m_treeView.Nodes[0]);
        }

        public void SetCurrentDebugLine(int lineNumber, string message)
        {
            if (lineNumber < m_syntaxDocument.Lines.Length)
            {
                ClearCurrentLineSymbol();
                var RowIndex1 = lineNumber - 1;
                if (RowIndex1 < 0)
                    RowIndex1 = 0;
                m_syntaxBoxControl.GotoLine(RowIndex1);
                var RowIndex2 = RowIndex1 - 5;
                if (RowIndex2 < 0)
                    RowIndex2 = 0;
                m_syntaxBoxControl.ScrollIntoView(RowIndex2);
                ShowCurrentLineSymbol();
            }

            if (!string.IsNullOrEmpty(message))
                m_statusLabel.Text = message;
            else
                SetReady();
        }

        public void SetActionButtonsState(bool enabled)
        {
            m_resumeExecutionToolStripButton.Enabled = enabled;
            m_resumeExecutionToolStripMenuItem.Enabled = enabled;
            m_stepIntoToolStripButton.Enabled = enabled;
            m_stepIntoToolStripMenuItem.Enabled = enabled;
            m_stepOverToolStripButton.Enabled = enabled;
            m_stepOverToolStripMenuItem.Enabled = enabled;
            m_stepOutToolStripButton.Enabled = enabled;
            m_stepOutToolStripMenuItem.Enabled = enabled;
        }

        public void RefreshTabs()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                switch (m_tabControl.SelectedTab.Text)
                {
                    case "Globals":
                        RefreshGlobalList();
                        break;
                    case "Call Stack":
                        RefreshCallStack();
                        break;
                    default:
                        RefreshLocalList();
                        break;
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        public void SetDebuggerInstance(object instance)
        {
            Debugger = instance as LuaDebugger;
        }

        public static DebuggerForm Instance => m_instance;

        public IResourceBundleFilter Filter { get; set; }

        public string CurrentResourceName { get; private set; }

        public bool IsApplicationRunning
        {
            get => m_isApplicationRunning;
            set
            {
                m_isApplicationRunning = value;
                m_resumeExecutionToolStripButton.Enabled = !m_isApplicationRunning;
                m_resumeExecutionToolStripMenuItem.Enabled = !m_isApplicationRunning;
            }
        }

        public IResourceBundle ActiveBundle { get; set; }

        public IResourceBundleSet ActiveBundleSet { get; set; }

        public LuaDebugger Debugger { get; set; }

        public event EventHandler StartApplication;

        private void OnActivated(object sender, EventArgs e)
        {
            Cursor.Show();
            if (m_isActivated)
                return;
            RefreshResourceTree();
            RefreshTabs();
            SelectNodePath(ServiceLocator.Instance.GetService<IUserSettingsStore>()
                .GetValue<string>("Debugger", "SelectedNodePath"));
            SetDebuggerState();
            SetBreakpointState();
            m_isActivated = true;
        }

        private void OnAfterSelect(object sender, TreeViewEventArgs e)
        {
            ServiceLocator.Instance.GetService<IUserSettingsStore>()
                .SetValue<string>("Debugger", "SelectedNodePath", e.Node.FullPath);
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            m_visualTipProvider.HideTip();
            if (Debugger != null)
                Debugger.Run();
            Cursor.Hide();
            Hide();
            m_isActivated = false;
            e.Cancel = true;
        }

        private void OnCommandEntered(object sender, CommandEnteredEventArgs e)
        {
            try
            {
                var readOnlyCollection = Debugger.Lua.DoString(e.Command);
                if (readOnlyCollection == null || readOnlyCollection.Count == 0)
                    m_shellControl.WriteText("nil");
                else
                    foreach (var obj in readOnlyCollection)
                        m_shellControl.WriteText(LuaHelper.FormatLuaValue(obj));
            }
            catch (Exception ex)
            {
                m_shellControl.WriteText(ex.Message);
            }
        }

        private void OnToggleDebugging(object sender, EventArgs e)
        {
            var service = ServiceLocator.Instance.GetService<IUserSettingsStore>();
            var flag = service.GetValue<bool>("Debugger", "Enabled");
            if (flag)
                OnDeleteAllBreakpoints(sender, e);
            service.SetValue<bool>("Debugger", "Enabled", !flag);
            SetDebuggerState();
            SetBreakpointState();
        }

        private void OnTreeDoubleClick(object sender, EventArgs e)
        {
            if (m_treeView.SelectedNode == null || !(m_treeView.SelectedNode.Tag is IResource tag) ||
                string.Compare(tag.Type.Name, "script", true) != 0)
                return;
            SetDocumentText(tag.Name);
        }

        private void OnToggleBreakpoint(object sender, EventArgs e)
        {
            ToggleBreakpoint();
        }

        private void OnDeleteAllBreakpoints(object sender, EventArgs e)
        {
            m_syntaxDocument.ClearBreakpoints();
            if (Debugger == null)
                return;
            Debugger.ClearAllBreakpoints();
            SetBreakpointState();
        }

        private void OnResume(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                ClearCurrentLineSymbol();
                SetActionButtonsState(false);
                if (!m_isApplicationRunning && StartApplication != null)
                    StartApplication((object)this, EventArgs.Empty);
                else
                    Debugger.Run();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void OnStepOut(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                ClearCurrentLineSymbol();
                SetActionButtonsState(false);
                Debugger.StepOut();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void OnStepInto(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                ClearCurrentLineSymbol();
                SetActionButtonsState(false);
                Debugger.StepInto();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void OnStepOver(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                ClearCurrentLineSymbol();
                SetActionButtonsState(false);
                Debugger.StepOver();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void OnBreakpointAdded(object sender, RowEventArgs e)
        {
            if (m_refreshingBreakpoints)
                return;
            Debugger.AddBreakpoint(CurrentResourceName, e.Row.Index + 1);
            SetBreakpointState();
        }

        private void OnBreakpointRemoved(object sender, RowEventArgs e)
        {
            if (m_refreshingBreakpoints)
                return;
            Debugger.RemoveBreakpoint(CurrentResourceName, e.Row.Index + 1);
            SetBreakpointState();
        }

        private void SetReady()
        {
            m_statusLabel.Text = "Ready";
        }

        private void OnCaretChange(object sender, EventArgs e)
        {
            SetBreakpointState();
        }

        private void ToggleBreakpoint()
        {
            var y = m_syntaxBoxControl.Caret.Position.Y;
            m_syntaxDocument[y].Breakpoint = !m_syntaxDocument[y].Breakpoint;
        }

        private void RefreshBreakpoints()
        {
            if (m_refreshingBreakpoints)
                return;
            try
            {
                m_refreshingBreakpoints = true;
                m_syntaxDocument.ClearBreakpoints();
                if (Debugger == null)
                    return;
                var breakpoints = Debugger.GetBreakpoints(CurrentResourceName);
                m_syntaxBoxControl.AllowBreakPoints = breakpoints.Count > 0;
                foreach (var luaDebugBreakpoint in breakpoints)
                {
                    m_syntaxDocument[luaDebugBreakpoint.Line - 1].Breakpoint = luaDebugBreakpoint.Enabled;
                    if (!luaDebugBreakpoint.Enabled)
                        m_syntaxDocument[luaDebugBreakpoint.Line - 1].Images.Add(13);
                    else
                        m_syntaxDocument[luaDebugBreakpoint.Line - 1].Images.Remove(13);
                }
            }
            finally
            {
                m_refreshingBreakpoints = false;
                SetBreakpointState();
            }
        }

        private void ShowCurrentLineSymbol()
        {
            m_syntaxBoxControl.Document[m_syntaxBoxControl.Caret.Position.Y].Images.Add(2);
        }

        private void ClearCurrentLineSymbol()
        {
            m_syntaxBoxControl.Document[m_syntaxBoxControl.Caret.Position.Y].Images.Remove(2);
        }

        private void SelectNodePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;
            var strArray1 = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var num1 = 0;
            var nodes = m_treeView.Nodes;
            var strArray2 = strArray1;
            var index = num1;
            var num2 = index + 1;
            var key = strArray2[index];
            var treeNodeArray1 = nodes.Find(key, false);
            if (treeNodeArray1.Length == 0)
                return;
            TreeNode treeNode;
            TreeNode[] treeNodeArray2;
            for (treeNode = treeNodeArray1[0]; treeNode != null; treeNode = treeNodeArray2[0])
            {
                treeNode.Expand();
                if (num2 < strArray1.Length)
                {
                    treeNodeArray2 = treeNode.Nodes.Find(strArray1[num2++], false);
                    if (treeNodeArray2.Length == 0)
                        break;
                }
                else
                {
                    break;
                }
            }

            m_treeView.SelectedNode = treeNode;
        }

        private void SetDebuggerState()
        {
            var service = ServiceLocator.Instance.GetService<IUserSettingsStore>();
            var flag = Debugger != null && service.GetValue<bool>("Debugger", "Enabled");
            m_enableDebuggingToolStripMenuItem.Checked = flag;
            m_enableDebuggingToolStripMenuItem.Text = flag ? "Disable Debugging" : "Enable Debugging";
            m_enableDebuggingToolStripButton.Checked = flag;
            m_enableDebuggingToolStripButton.Text = flag ? "Disable Debugging" : "Enable Debugging";
            if (Debugger == null)
                return;
            if (!flag)
            {
                Debugger.State = LuaDebuggerState.Disabled;
            }
            else
            {
                if (Debugger.State != LuaDebuggerState.Disabled)
                    return;
                Debugger.State = LuaDebuggerState.Running;
            }
        }

        private void SetBreakpointState()
        {
            var service = ServiceLocator.Instance.GetService<IUserSettingsStore>();
            m_syntaxBoxControl.AllowBreakPoints = false;
            m_breakpointsToolStripMenuItem.Visible = false;
            m_toggleBreakpointToolStripMenuItem.Enabled = false;
            m_deleteAllBreakpointToolStripMenuItem.Enabled = false;
            if ((Debugger == null ? 0 : service.GetValue<bool>("Debugger", "Enabled") ? 1 : 0) == 0)
                return;
            m_syntaxBoxControl.AllowBreakPoints = true;
            m_toggleBreakpointToolStripMenuItem.Enabled = !string.IsNullOrEmpty(CurrentResourceName);
            var files = Debugger.GetFiles();
            if (files.Count == 0)
                return;
            var flag = false;
            m_breakpointsToolStripMenuItem.DropDownItems.Clear();
            foreach (var luaDebugFile in files)
            {
                var workingFile = luaDebugFile;
                foreach (var breakpoint in luaDebugFile.Breakpoints)
                {
                    var workingBreakpoint = breakpoint;
                    var toolStripMenuItem = (ToolStripMenuItem)m_breakpointsToolStripMenuItem.DropDownItems.Add(
                        string.Format("Script: {0}, Line: {1}", (object)luaDebugFile.FileName,
                            (object)breakpoint.Line));
                    toolStripMenuItem.Click += (EventHandler)((o, e) =>
                    {
                        try
                        {
                            m_refreshingBreakpoints = true;
                            workingBreakpoint.Enabled = !workingBreakpoint.Enabled;
                            ((ToolStripMenuItem)o).Checked = workingBreakpoint.Enabled;
                            if (!(CurrentResourceName == workingFile.FileName))
                                return;
                            m_syntaxDocument[workingBreakpoint.Line - 1].Breakpoint = workingBreakpoint.Enabled;
                            if (!workingBreakpoint.Enabled)
                                m_syntaxDocument[workingBreakpoint.Line - 1].Images.Add(13);
                            else
                                m_syntaxDocument[workingBreakpoint.Line - 1].Images.Remove(13);
                        }
                        finally
                        {
                            m_refreshingBreakpoints = false;
                        }
                    });
                    toolStripMenuItem.Checked = breakpoint.Enabled;
                    flag = true;
                }
            }

            m_breakpointsToolStripMenuItem.Visible = flag;
            m_deleteAllBreakpointToolStripMenuItem.Enabled = flag;
        }

        private void RefreshGlobalList()
        {
            if (Debugger == null)
                return;
            RefreshVariableTable((VisualHint.SmartPropertyGrid.PropertyGrid)m_globalsPropertyGrid, "_G",
                (IEnumerable<LuaVar>)Debugger.GetGlobalVars());
        }

        private void RefreshLocalList()
        {
            if (Debugger == null)
                return;
            RefreshVariableTable((VisualHint.SmartPropertyGrid.PropertyGrid)m_localsPropertyGrid, "Locals",
                (IEnumerable<LuaVar>)Debugger.GetLocalVars());
        }

        private void OnTabSelected(object sender, TabControlEventArgs e)
        {
            RefreshTabs();
        }

        private void OnFind(object sender, EventArgs e)
        {
            m_syntaxBoxControl.ShowFind();
        }

        private void OnReplace(object sender, EventArgs e)
        {
            m_syntaxBoxControl.ShowReplace();
        }

        private void OnGoTo(object sender, EventArgs e)
        {
            m_syntaxBoxControl.ShowGotoLine();
        }

        private void OnShowWhitespace(object sender, EventArgs e)
        {
            m_syntaxBoxControl.ShowWhitespace = !m_syntaxBoxControl.ShowWhitespace;
            m_showWhitespaceToolStripMenuItem.Checked = !m_showWhitespaceToolStripMenuItem.Checked;
        }

        private void OnShowEOL(object sender, EventArgs e)
        {
            m_syntaxBoxControl.ShowEOLMarker = !m_syntaxBoxControl.ShowEOLMarker;
            m_showEOLMarkerToolStripMenuItem.Checked = !m_showEOLMarkerToolStripMenuItem.Checked;
        }

        private void SelectResourceNode(string resourceName, TreeNode parentNode)
        {
            foreach (TreeNode node in parentNode.Nodes)
            {
                SelectResourceNode(resourceName, node);
                if (node.Tag is IResource tag && string.Compare(tag.Name, resourceName, true) == 0)
                {
                    m_treeView.SelectedNode = node;
                    break;
                }
            }
        }

        private static object GetAdjustedValue(object var, out Type type, out bool isReadOnly)
        {
            isReadOnly = false;
            var adjustedValue = (object)"nil";
            if (var != null)
                adjustedValue = var;
            type = adjustedValue.GetType();
            if (type == typeof(LuaTable) || type == typeof(LuaFunction) || type.Name == "LuaCSFunction")
            {
                adjustedValue = (object)LuaHelper.FormatLuaValue(adjustedValue);
                type = adjustedValue.GetType();
                isReadOnly = true;
            }

            return adjustedValue;
        }

        private static void RefreshVariableTable(
            VisualHint.SmartPropertyGrid.PropertyGrid grid,
            string category,
            IEnumerable<LuaVar> vars)
        {
            grid.SuspendLayout();
            grid.BeginUpdate();
            grid.Clear();
            grid.DisplayMode = VisualHint.SmartPropertyGrid.PropertyGrid.DisplayModes.Categorized;
            var underCategory = grid.AppendRootCategory(1, category);
            var id = 2;
            foreach (var var in vars)
            {
                var visited = new List<LuaTable>();
                Type type;
                var adjustedValue = GetAdjustedValue(var.Value, out type, out var _);
                var parentEnumerator = grid.AppendManagedProperty(underCategory, id++, var.Name, type, adjustedValue,
                    string.Empty, (Attribute)new DefaultValueAttribute(adjustedValue));
                if (var.Value is LuaTable table)
                {
                    parentEnumerator.Property.Tag = adjustedValue;
                    parentEnumerator.Property.Feel = grid.GetRegisteredFeel("button");
                    BuildNestedTableProperties(grid, parentEnumerator, id, table, (ICollection<LuaTable>)visited);
                }
            }

            grid.EndUpdate();
            grid.ResumeLayout();
        }

        private static void BuildNestedTableProperties(
            VisualHint.SmartPropertyGrid.PropertyGrid grid,
            PropertyEnumerator parentEnumerator,
            int id,
            LuaTable table,
            ICollection<LuaTable> visited)
        {
            if (visited.Contains(table))
                return;
            visited.Add(table);
            foreach (var key in (IEnumerable)table.Keys)
            {
                Type type;
                var adjustedValue = GetAdjustedValue(table[key], out type, out var _);
                var parentEnumerator1 = grid.AppendManagedProperty(parentEnumerator, id++, key.ToString(), type,
                    adjustedValue, key.ToString(), (Attribute)new DefaultValueAttribute(adjustedValue));
                if (table[key] is LuaTable table1)
                {
                    parentEnumerator1.Property.Feel = grid.GetRegisteredFeel("button");
                    BuildNestedTableProperties(grid, parentEnumerator1, id, table1, visited);
                }
            }
        }

        private void RefreshCallStack()
        {
            var callStack = Debugger.GetCallStack();
            m_callStackListView.BeginUpdate();
            m_callStackListView.Items.Clear();
            foreach (var callStackEntry in callStack)
                m_callStackListView.Items.Add(new ListViewItem(callStackEntry.FunctionName)
                {
                    SubItems =
                    {
                        callStackEntry.FileName,
                        callStackEntry.Line.ToString()
                    }
                });
            m_callStackListView.EndUpdate();
        }

        private void RefreshResourceTree()
        {
            m_treeView.BeginUpdate();
            m_treeView.Nodes.Clear();
            if (ActiveBundleSet == null)
                return;
            var node1 = new TreeNode("Loaded Resources", 5, 5);
            node1.Name = node1.Text;
            var node2 = new TreeNode("Views", 6, 6);
            node2.Name = node2.Text;
            var node3 = new TreeNode("Scripts", 6, 6);
            node3.Name = node3.Text;
            node1.Nodes.Add(node2);
            var dictionary = new Dictionary<string, bool>();
            foreach (var resource in ActiveBundleSet.GetResources("view", Filter))
            {
                var node4 = new TreeNode(resource.Name, 4, 4)
                {
                    Name = resource.Name,
                    Tag = (object)resource
                };
                var node5 = new TreeNode("Actors", 6, 6);
                node5.Name = node5.Text;
                var childNode = ((XmlNode)resource.GetAspect("content").GetContent()).ChildNodes[0];
                var attributeValue1 = childNode.GetAttributeValue<string>("onEnter");
                if (!string.IsNullOrEmpty(attributeValue1))
                {
                    dictionary[attributeValue1] = true;
                    var node6 = new TreeNode(string.Format("onEnter: {0}", (object)attributeValue1), 7, 7)
                    {
                        Tag = (object)ActiveBundleSet.GetResource(attributeValue1, Filter)
                    };
                    node6.Name = node6.Text;
                    node4.Nodes.Add(node6);
                }

                var attributeValue2 = childNode.GetAttributeValue<string>("onLeave");
                if (!string.IsNullOrEmpty(attributeValue2))
                {
                    dictionary[attributeValue2] = true;
                    var node7 = new TreeNode(string.Format("onLeave: {0}", (object)attributeValue2), 7, 7)
                    {
                        Tag = (object)ActiveBundleSet.GetResource(attributeValue2, Filter)
                    };
                    node7.Name = node7.Text;
                    node4.Nodes.Add(node7);
                }

                var xmlNodeList = childNode.SelectNodes("actor");
                if (xmlNodeList != null)
                    foreach (XmlNode node8 in xmlNodeList)
                    {
                        var attributeValue3 = node8.GetAttributeValue<string>("onHit");
                        if (!string.IsNullOrEmpty(attributeValue3))
                        {
                            dictionary[attributeValue3] = true;
                            var node9 = new TreeNode(
                                string.Format("{0}: {1}", (object)node8.GetAttributeValue<string>("name"),
                                    (object)attributeValue3), 7, 7)
                            {
                                Tag = (object)ActiveBundleSet.GetResource(attributeValue3, Filter)
                            };
                            node9.Name = node9.Text;
                            node5.Nodes.Add(node9);
                        }
                    }

                if (node5.Nodes.Count > 0)
                    node4.Nodes.Add(node5);
                node2.Nodes.Add(node4);
            }

            node1.Nodes.Add(node3);
            foreach (var resource in ActiveBundleSet.GetResources("script", Filter))
                if (!dictionary.ContainsKey(resource.Name))
                {
                    var node10 = new TreeNode(resource.Name, 7, 7)
                    {
                        Name = resource.Name,
                        Tag = (object)resource
                    };
                    node3.Nodes.Add(node10);
                }

            m_treeView.Nodes.Add(node1);
            m_treeView.EndUpdate();
        }

        private string GetScript(string resourceName)
        {
            if (ActiveBundleSet == null)
                throw new ArgumentException("No active resource bundle set available.");
            var resource = ActiveBundleSet.GetResource(resourceName, Filter);
            return resource == null ? (string)null : resource.GetAspect("content").GetContent() as string;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = (IContainer)new Container();
            var componentResourceManager = new ComponentResourceManager(typeof(DebuggerForm));
            var tipOfficeRenderer = new VisualTipOfficeRenderer();
            m_statusStrip = new StatusStrip();
            m_statusLabel = new ToolStripStatusLabel();
            m_menuStrip = new MenuStrip();
            m_fileToolStripMenuItem = new ToolStripMenuItem();
            m_switchBundleToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            m_closeToolStripMenuItem = new ToolStripMenuItem();
            m_editToolStripMenuItem = new ToolStripMenuItem();
            m_findAndReplaceToolStripMenuItem = new ToolStripMenuItem();
            m_quickFindToolStripMenuItem = new ToolStripMenuItem();
            m_quickReplaceToolStripMenuItem = new ToolStripMenuItem();
            m_goToToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            m_showWhitespaceToolStripMenuItem = new ToolStripMenuItem();
            m_showEOLMarkerToolStripMenuItem = new ToolStripMenuItem();
            m_debugToolStripMenuItem = new ToolStripMenuItem();
            m_enableDebuggingToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            m_resumeExecutionToolStripMenuItem = new ToolStripMenuItem();
            m_breakAllToolStripMenuItem = new ToolStripMenuItem();
            m_stopDebugginToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            m_stepIntoToolStripMenuItem = new ToolStripMenuItem();
            m_stepOverToolStripMenuItem = new ToolStripMenuItem();
            m_stepOutToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripSeparator();
            m_toggleBreakpointToolStripMenuItem = new ToolStripMenuItem();
            m_deleteAllBreakpointToolStripMenuItem = new ToolStripMenuItem();
            m_breakpointsToolStripMenuItem = new ToolStripMenuItem();
            m_mainSplitter = new Splitter();
            m_syntaxBoxControl = new SyntaxBoxControl();
            m_syntaxDocument = new SyntaxDocument(components);
            m_sourceSplitter = new Splitter();
            m_toolStrip = new ToolStrip();
            m_enableDebuggingToolStripButton = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            m_resumeExecutionToolStripButton = new ToolStripButton();
            m_breakAllToolStripButton = new ToolStripButton();
            m_stopDebuggingToolStripButton = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            m_stepIntoToolStripButton = new ToolStripButton();
            m_stepOverToolStripButton = new ToolStripButton();
            m_stepOutToolStripButton = new ToolStripButton();
            m_tabControl = new TabControl();
            m_localsTabPage = new TabPage();
            m_localsPropertyGrid = new DynamicPropertyGrid();
            m_globalsTabPage = new TabPage();
            m_globalsPropertyGrid = new DynamicPropertyGrid();
            m_callStackTabPage = new TabPage();
            m_callStackListView = new ListView();
            m_funcNameColumnHeader = new ColumnHeader();
            m_scriptNameColumnHeader = new ColumnHeader();
            m_scriptLineNoColumnHeader = new ColumnHeader();
            m_immediateTabPage = new TabPage();
            m_shellControl = new ShellControl();
            m_imageList = new ImageList(components);
            m_treeView = new TreeView();
            m_visualTipProvider = new VisualTipProvider(components);
            m_gutterImageList = new ImageList(components);
            m_statusStrip.SuspendLayout();
            m_menuStrip.SuspendLayout();
            m_toolStrip.SuspendLayout();
            m_tabControl.SuspendLayout();
            m_localsTabPage.SuspendLayout();
            m_localsPropertyGrid.BeginInit();
            m_globalsTabPage.SuspendLayout();
            m_globalsPropertyGrid.BeginInit();
            m_callStackTabPage.SuspendLayout();
            m_immediateTabPage.SuspendLayout();
            SuspendLayout();
            m_statusStrip.Items.AddRange(new ToolStripItem[1]
            {
                (ToolStripItem)m_statusLabel
            });
            m_statusStrip.Location = new Point(0, 551);
            m_statusStrip.Name = "m_statusStrip";
            m_statusStrip.Size = new Size(792, 22);
            m_statusStrip.TabIndex = 0;
            m_statusLabel.Name = "m_statusLabel";
            m_statusLabel.Size = new Size(38, 17);
            m_statusLabel.Text = "Ready";
            m_menuStrip.Items.AddRange(new ToolStripItem[3]
            {
                (ToolStripItem)m_fileToolStripMenuItem,
                (ToolStripItem)m_editToolStripMenuItem,
                (ToolStripItem)m_debugToolStripMenuItem
            });
            m_menuStrip.Location = new Point(0, 0);
            m_menuStrip.Name = "m_menuStrip";
            m_menuStrip.Size = new Size(792, 24);
            m_menuStrip.TabIndex = 1;
            m_fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[3]
            {
                (ToolStripItem)m_switchBundleToolStripMenuItem,
                (ToolStripItem)toolStripSeparator4,
                (ToolStripItem)m_closeToolStripMenuItem
            });
            m_fileToolStripMenuItem.Name = "m_fileToolStripMenuItem";
            m_fileToolStripMenuItem.Size = new Size(35, 20);
            m_fileToolStripMenuItem.Text = "&File";
            m_switchBundleToolStripMenuItem.Name = "m_switchBundleToolStripMenuItem";
            m_switchBundleToolStripMenuItem.Size = new Size(152, 22);
            m_switchBundleToolStripMenuItem.Text = "Switch Bundle...";
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(149, 6);
            m_closeToolStripMenuItem.Name = "m_closeToolStripMenuItem";
            m_closeToolStripMenuItem.Size = new Size(152, 22);
            m_closeToolStripMenuItem.Text = "Close";
            m_editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[5]
            {
                (ToolStripItem)m_findAndReplaceToolStripMenuItem,
                (ToolStripItem)m_goToToolStripMenuItem,
                (ToolStripItem)toolStripSeparator5,
                (ToolStripItem)m_showWhitespaceToolStripMenuItem,
                (ToolStripItem)m_showEOLMarkerToolStripMenuItem
            });
            m_editToolStripMenuItem.Name = "m_editToolStripMenuItem";
            m_editToolStripMenuItem.Size = new Size(37, 20);
            m_editToolStripMenuItem.Text = "&Edit";
            m_findAndReplaceToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[2]
            {
                (ToolStripItem)m_quickFindToolStripMenuItem,
                (ToolStripItem)m_quickReplaceToolStripMenuItem
            });
            m_findAndReplaceToolStripMenuItem.Name = "m_findAndReplaceToolStripMenuItem";
            m_findAndReplaceToolStripMenuItem.Size = new Size(159, 22);
            m_findAndReplaceToolStripMenuItem.Text = "&Find and Replace";
            m_quickFindToolStripMenuItem.Name = "m_quickFindToolStripMenuItem";
            m_quickFindToolStripMenuItem.ShortcutKeys = Keys.F | Keys.Control;
            m_quickFindToolStripMenuItem.Size = new Size(192, 22);
            m_quickFindToolStripMenuItem.Text = "Quick Find...";
            m_quickFindToolStripMenuItem.Click += new EventHandler(OnFind);
            m_quickReplaceToolStripMenuItem.Name = "m_quickReplaceToolStripMenuItem";
            m_quickReplaceToolStripMenuItem.ShortcutKeys = Keys.H | Keys.Control;
            m_quickReplaceToolStripMenuItem.Size = new Size(192, 22);
            m_quickReplaceToolStripMenuItem.Text = "Quick Replace...";
            m_quickReplaceToolStripMenuItem.Click += new EventHandler(OnReplace);
            m_goToToolStripMenuItem.Name = "m_goToToolStripMenuItem";
            m_goToToolStripMenuItem.ShortcutKeys = Keys.G | Keys.Control;
            m_goToToolStripMenuItem.Size = new Size(159, 22);
            m_goToToolStripMenuItem.Text = "Go To...";
            m_goToToolStripMenuItem.Click += new EventHandler(OnGoTo);
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(156, 6);
            m_showWhitespaceToolStripMenuItem.Name = "m_showWhitespaceToolStripMenuItem";
            m_showWhitespaceToolStripMenuItem.Size = new Size(159, 22);
            m_showWhitespaceToolStripMenuItem.Text = "Show Whitespace";
            m_showWhitespaceToolStripMenuItem.Click += new EventHandler(OnShowWhitespace);
            m_showEOLMarkerToolStripMenuItem.Name = "m_showEOLMarkerToolStripMenuItem";
            m_showEOLMarkerToolStripMenuItem.Size = new Size(159, 22);
            m_showEOLMarkerToolStripMenuItem.Text = "Show EOL Marker";
            m_showEOLMarkerToolStripMenuItem.Click += new EventHandler(OnShowEOL);
            m_debugToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[13]
            {
                (ToolStripItem)m_enableDebuggingToolStripMenuItem,
                (ToolStripItem)toolStripSeparator2,
                (ToolStripItem)m_resumeExecutionToolStripMenuItem,
                (ToolStripItem)m_breakAllToolStripMenuItem,
                (ToolStripItem)m_stopDebugginToolStripMenuItem,
                (ToolStripItem)toolStripMenuItem1,
                (ToolStripItem)m_stepIntoToolStripMenuItem,
                (ToolStripItem)m_stepOverToolStripMenuItem,
                (ToolStripItem)m_stepOutToolStripMenuItem,
                (ToolStripItem)toolStripMenuItem2,
                (ToolStripItem)m_toggleBreakpointToolStripMenuItem,
                (ToolStripItem)m_deleteAllBreakpointToolStripMenuItem,
                (ToolStripItem)m_breakpointsToolStripMenuItem
            });
            m_debugToolStripMenuItem.Name = "m_debugToolStripMenuItem";
            m_debugToolStripMenuItem.Size = new Size(50, 20);
            m_debugToolStripMenuItem.Text = "&Debug";
            m_enableDebuggingToolStripMenuItem.Image = (Image)Resources.Debugger;
            m_enableDebuggingToolStripMenuItem.Name = "m_enableDebuggingToolStripMenuItem";
            m_enableDebuggingToolStripMenuItem.Size = new Size(179, 22);
            m_enableDebuggingToolStripMenuItem.Text = "Enable Debugging";
            m_enableDebuggingToolStripMenuItem.Click += new EventHandler(OnToggleDebugging);
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(176, 6);
            m_resumeExecutionToolStripMenuItem.Enabled = false;
            m_resumeExecutionToolStripMenuItem.Image = (Image)Resources.DebugPlay;
            m_resumeExecutionToolStripMenuItem.Name = "m_resumeExecutionToolStripMenuItem";
            m_resumeExecutionToolStripMenuItem.Size = new Size(179, 22);
            m_resumeExecutionToolStripMenuItem.Text = "Resume Execution";
            m_resumeExecutionToolStripMenuItem.Click += new EventHandler(OnResume);
            m_breakAllToolStripMenuItem.Enabled = false;
            m_breakAllToolStripMenuItem.Image = (Image)Resources.DebugPause;
            m_breakAllToolStripMenuItem.Name = "m_breakAllToolStripMenuItem";
            m_breakAllToolStripMenuItem.Size = new Size(179, 22);
            m_breakAllToolStripMenuItem.Text = "Break All";
            m_stopDebugginToolStripMenuItem.Enabled = false;
            m_stopDebugginToolStripMenuItem.Image = (Image)Resources.DebugStop;
            m_stopDebugginToolStripMenuItem.Name = "m_stopDebugginToolStripMenuItem";
            m_stopDebugginToolStripMenuItem.Size = new Size(179, 22);
            m_stopDebugginToolStripMenuItem.Text = "Stop Debugging";
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(176, 6);
            m_stepIntoToolStripMenuItem.Enabled = false;
            m_stepIntoToolStripMenuItem.Image = (Image)Resources.DebugStepInto;
            m_stepIntoToolStripMenuItem.Name = "m_stepIntoToolStripMenuItem";
            m_stepIntoToolStripMenuItem.ShortcutKeys = Keys.F11;
            m_stepIntoToolStripMenuItem.Size = new Size(179, 22);
            m_stepIntoToolStripMenuItem.Text = "Step Into";
            m_stepIntoToolStripMenuItem.Click += new EventHandler(OnStepInto);
            m_stepOverToolStripMenuItem.Enabled = false;
            m_stepOverToolStripMenuItem.Image = (Image)Resources.DebugStepOver;
            m_stepOverToolStripMenuItem.Name = "m_stepOverToolStripMenuItem";
            m_stepOverToolStripMenuItem.ShortcutKeys = Keys.F10;
            m_stepOverToolStripMenuItem.Size = new Size(179, 22);
            m_stepOverToolStripMenuItem.Text = "Step Over";
            m_stepOverToolStripMenuItem.Click += new EventHandler(OnStepOver);
            m_stepOutToolStripMenuItem.Enabled = false;
            m_stepOutToolStripMenuItem.Image = (Image)Resources.DebugStepOut;
            m_stepOutToolStripMenuItem.Name = "m_stepOutToolStripMenuItem";
            m_stepOutToolStripMenuItem.ShortcutKeys = Keys.F11 | Keys.Shift;
            m_stepOutToolStripMenuItem.Size = new Size(179, 22);
            m_stepOutToolStripMenuItem.Text = "Step Out";
            m_stepOutToolStripMenuItem.Click += new EventHandler(OnStepOut);
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(176, 6);
            m_toggleBreakpointToolStripMenuItem.Enabled = false;
            m_toggleBreakpointToolStripMenuItem.Image = (Image)Resources.DebugBreakpoint;
            m_toggleBreakpointToolStripMenuItem.Name = "m_toggleBreakpointToolStripMenuItem";
            m_toggleBreakpointToolStripMenuItem.ShortcutKeys = Keys.F9;
            m_toggleBreakpointToolStripMenuItem.Size = new Size(179, 22);
            m_toggleBreakpointToolStripMenuItem.Text = "Toggle Breakpoint";
            m_toggleBreakpointToolStripMenuItem.Click += new EventHandler(OnToggleBreakpoint);
            m_deleteAllBreakpointToolStripMenuItem.Enabled = false;
            m_deleteAllBreakpointToolStripMenuItem.Image = (Image)Resources.DebugBreakpointClear;
            m_deleteAllBreakpointToolStripMenuItem.Name = "m_deleteAllBreakpointToolStripMenuItem";
            m_deleteAllBreakpointToolStripMenuItem.Size = new Size(179, 22);
            m_deleteAllBreakpointToolStripMenuItem.Text = "Delete All Breakpoints";
            m_deleteAllBreakpointToolStripMenuItem.Click += new EventHandler(OnDeleteAllBreakpoints);
            m_breakpointsToolStripMenuItem.Name = "m_breakpointsToolStripMenuItem";
            m_breakpointsToolStripMenuItem.Size = new Size(179, 22);
            m_breakpointsToolStripMenuItem.Text = "Breakpoints";
            m_breakpointsToolStripMenuItem.Visible = false;
            m_mainSplitter.Location = new Point(259, 49);
            m_mainSplitter.Name = "m_mainSplitter";
            m_mainSplitter.Size = new Size(3, 502);
            m_mainSplitter.TabIndex = 3;
            m_mainSplitter.TabStop = false;
            m_syntaxBoxControl.ActiveView = ActiveView.BottomRight;
            m_syntaxBoxControl.AutoListPosition = (TextPoint)null;
            m_syntaxBoxControl.AutoListSelectedText = "a123";
            m_syntaxBoxControl.AutoListVisible = false;
            m_syntaxBoxControl.BackColor = Color.White;
            m_syntaxBoxControl.BorderStyle = Alsing.Windows.Forms.BorderStyle.None;
            m_syntaxBoxControl.CopyAsRTF = false;
            m_syntaxBoxControl.Dock = DockStyle.Top;
            m_syntaxBoxControl.Document = m_syntaxDocument;
            m_syntaxBoxControl.FontName = "Courier new";
            m_syntaxBoxControl.HighLightActiveLine = true;
            m_syntaxBoxControl.ImeMode = ImeMode.NoControl;
            m_syntaxBoxControl.InfoTipCount = 1;
            m_syntaxBoxControl.InfoTipPosition = (TextPoint)null;
            m_syntaxBoxControl.InfoTipSelectedIndex = 1;
            m_syntaxBoxControl.InfoTipVisible = false;
            m_syntaxBoxControl.Location = new Point(262, 49);
            m_syntaxBoxControl.LockCursorUpdate = false;
            m_syntaxBoxControl.Name = "m_syntaxBoxControl";
            m_syntaxBoxControl.ReadOnly = true;
            m_syntaxBoxControl.ShowScopeIndicator = false;
            m_syntaxBoxControl.Size = new Size(530, 328);
            m_syntaxBoxControl.SmoothScroll = false;
            m_syntaxBoxControl.SplitviewH = -4;
            m_syntaxBoxControl.SplitviewV = -4;
            m_syntaxBoxControl.TabGuideColor = Color.FromArgb(222, 219, 214);
            m_syntaxBoxControl.TabIndex = 4;
            m_syntaxBoxControl.WhitespaceColor = SystemColors.ControlDark;
            m_syntaxBoxControl.CaretChange += new EventHandler(OnCaretChange);
            m_syntaxDocument.Lines = new string[1] { "" };
            m_syntaxDocument.MaxUndoBufferSize = 1000;
            m_syntaxDocument.Modified = false;
            m_syntaxDocument.UndoStep = 0;
            m_syntaxDocument.BreakPointAdded += new RowEventHandler(OnBreakpointAdded);
            m_syntaxDocument.BreakPointRemoved += new RowEventHandler(OnBreakpointRemoved);
            m_sourceSplitter.Dock = DockStyle.Top;
            m_sourceSplitter.Location = new Point(262, 377);
            m_sourceSplitter.Name = "m_sourceSplitter";
            m_sourceSplitter.Size = new Size(530, 3);
            m_sourceSplitter.TabIndex = 5;
            m_sourceSplitter.TabStop = false;
            m_toolStrip.Items.AddRange(new ToolStripItem[9]
            {
                (ToolStripItem)m_enableDebuggingToolStripButton,
                (ToolStripItem)toolStripSeparator3,
                (ToolStripItem)m_resumeExecutionToolStripButton,
                (ToolStripItem)m_breakAllToolStripButton,
                (ToolStripItem)m_stopDebuggingToolStripButton,
                (ToolStripItem)toolStripSeparator1,
                (ToolStripItem)m_stepIntoToolStripButton,
                (ToolStripItem)m_stepOverToolStripButton,
                (ToolStripItem)m_stepOutToolStripButton
            });
            m_toolStrip.Location = new Point(0, 24);
            m_toolStrip.Name = "m_toolStrip";
            m_toolStrip.Size = new Size(792, 25);
            m_toolStrip.TabIndex = 6;
            m_enableDebuggingToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_enableDebuggingToolStripButton.Image = (Image)Resources.Debugger;
            m_enableDebuggingToolStripButton.ImageTransparentColor = Color.Magenta;
            m_enableDebuggingToolStripButton.Name = "m_enableDebuggingToolStripButton";
            m_enableDebuggingToolStripButton.Size = new Size(23, 22);
            m_enableDebuggingToolStripButton.Text = "Enable Debugging";
            m_enableDebuggingToolStripButton.Click += new EventHandler(OnToggleDebugging);
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 25);
            m_resumeExecutionToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_resumeExecutionToolStripButton.Enabled = false;
            m_resumeExecutionToolStripButton.Image = (Image)Resources.DebugPlay;
            m_resumeExecutionToolStripButton.ImageTransparentColor = Color.Magenta;
            m_resumeExecutionToolStripButton.Name = "m_resumeExecutionToolStripButton";
            m_resumeExecutionToolStripButton.Size = new Size(23, 22);
            m_resumeExecutionToolStripButton.Text = "ResumeExecution";
            m_resumeExecutionToolStripButton.ToolTipText = "Resume Execution";
            m_resumeExecutionToolStripButton.Click += new EventHandler(OnResume);
            m_breakAllToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_breakAllToolStripButton.Enabled = false;
            m_breakAllToolStripButton.Image = (Image)Resources.DebugPause;
            m_breakAllToolStripButton.ImageTransparentColor = Color.Magenta;
            m_breakAllToolStripButton.Name = "m_breakAllToolStripButton";
            m_breakAllToolStripButton.Size = new Size(23, 22);
            m_breakAllToolStripButton.Text = "toolStripButton2";
            m_breakAllToolStripButton.ToolTipText = "Break All";
            m_stopDebuggingToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_stopDebuggingToolStripButton.Enabled = false;
            m_stopDebuggingToolStripButton.Image = (Image)Resources.DebugStop;
            m_stopDebuggingToolStripButton.ImageTransparentColor = Color.Magenta;
            m_stopDebuggingToolStripButton.Name = "m_stopDebuggingToolStripButton";
            m_stopDebuggingToolStripButton.Size = new Size(23, 22);
            m_stopDebuggingToolStripButton.Text = "toolStripButton3";
            m_stopDebuggingToolStripButton.ToolTipText = "Stop Debugging";
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            m_stepIntoToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_stepIntoToolStripButton.Enabled = false;
            m_stepIntoToolStripButton.Image = (Image)Resources.DebugStepInto;
            m_stepIntoToolStripButton.ImageTransparentColor = Color.Magenta;
            m_stepIntoToolStripButton.Name = "m_stepIntoToolStripButton";
            m_stepIntoToolStripButton.Size = new Size(23, 22);
            m_stepIntoToolStripButton.Text = "toolStripButton4";
            m_stepIntoToolStripButton.ToolTipText = "Step Into";
            m_stepIntoToolStripButton.Click += new EventHandler(OnStepInto);
            m_stepOverToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_stepOverToolStripButton.Enabled = false;
            m_stepOverToolStripButton.Image = (Image)Resources.DebugStepOver;
            m_stepOverToolStripButton.ImageTransparentColor = Color.Magenta;
            m_stepOverToolStripButton.Name = "m_stepOverToolStripButton";
            m_stepOverToolStripButton.Size = new Size(23, 22);
            m_stepOverToolStripButton.Text = "toolStripButton5";
            m_stepOverToolStripButton.ToolTipText = "Step Over";
            m_stepOverToolStripButton.Click += new EventHandler(OnStepOver);
            m_stepOutToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_stepOutToolStripButton.Enabled = false;
            m_stepOutToolStripButton.Image = (Image)Resources.DebugStepOut;
            m_stepOutToolStripButton.ImageTransparentColor = Color.Magenta;
            m_stepOutToolStripButton.Name = "m_stepOutToolStripButton";
            m_stepOutToolStripButton.Size = new Size(23, 22);
            m_stepOutToolStripButton.Text = "Step Out";
            m_stepOutToolStripButton.Click += new EventHandler(OnStepOut);
            m_tabControl.Alignment = TabAlignment.Bottom;
            m_tabControl.Controls.Add((Control)m_localsTabPage);
            m_tabControl.Controls.Add((Control)m_globalsTabPage);
            m_tabControl.Controls.Add((Control)m_callStackTabPage);
            m_tabControl.Controls.Add((Control)m_immediateTabPage);
            m_tabControl.Dock = DockStyle.Fill;
            m_tabControl.ImageList = m_imageList;
            m_tabControl.Location = new Point(262, 380);
            m_tabControl.Name = "m_tabControl";
            m_tabControl.SelectedIndex = 0;
            m_tabControl.Size = new Size(530, 171);
            m_tabControl.TabIndex = 7;
            m_tabControl.Selected += new TabControlEventHandler(OnTabSelected);
            m_localsTabPage.Controls.Add((Control)m_localsPropertyGrid);
            m_localsTabPage.ImageIndex = 1;
            m_localsTabPage.Location = new Point(4, 4);
            m_localsTabPage.Name = "m_localsTabPage";
            m_localsTabPage.Padding = new Padding(3);
            m_localsTabPage.Size = new Size(522, 144);
            m_localsTabPage.TabIndex = 0;
            m_localsTabPage.Text = "Locals";
            m_localsTabPage.UseVisualStyleBackColor = true;
            m_localsPropertyGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            m_localsPropertyGrid.Dock = DockStyle.Fill;
            m_localsPropertyGrid.Location = new Point(3, 3);
            m_localsPropertyGrid.Name = "m_localsPropertyGrid";
            m_localsPropertyGrid.PropertyLabelBackColor = SystemColors.Window;
            m_localsPropertyGrid.PropertyValueBackColor = SystemColors.Window;
            m_localsPropertyGrid.ShowAdditionalIndentation = true;
            m_localsPropertyGrid.ShowDefaultValues = true;
            m_localsPropertyGrid.Size = new Size(516, 138);
            m_localsPropertyGrid.TabIndex = 0;
            m_localsPropertyGrid.ToolbarVisibility = true;
            m_localsPropertyGrid.ToolTipMode =
                VisualHint.SmartPropertyGrid.PropertyGrid.ToolTipModes.ToolTipsOnValuesAndLabels;
            m_globalsTabPage.Controls.Add((Control)m_globalsPropertyGrid);
            m_globalsTabPage.ImageIndex = 2;
            m_globalsTabPage.Location = new Point(4, 4);
            m_globalsTabPage.Name = "m_globalsTabPage";
            m_globalsTabPage.Padding = new Padding(3);
            m_globalsTabPage.Size = new Size(522, 144);
            m_globalsTabPage.TabIndex = 2;
            m_globalsTabPage.Text = "Globals";
            m_globalsTabPage.UseVisualStyleBackColor = true;
            m_globalsPropertyGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            m_globalsPropertyGrid.Dock = DockStyle.Fill;
            m_globalsPropertyGrid.Location = new Point(3, 3);
            m_globalsPropertyGrid.Name = "m_globalsPropertyGrid";
            m_globalsPropertyGrid.PropertyLabelBackColor = SystemColors.Window;
            m_globalsPropertyGrid.PropertyValueBackColor = SystemColors.Window;
            m_globalsPropertyGrid.ShowAdditionalIndentation = true;
            m_globalsPropertyGrid.ShowDefaultValues = true;
            m_globalsPropertyGrid.Size = new Size(516, 138);
            m_globalsPropertyGrid.TabIndex = 1;
            m_globalsPropertyGrid.ToolbarVisibility = true;
            m_globalsPropertyGrid.ToolTipMode =
                VisualHint.SmartPropertyGrid.PropertyGrid.ToolTipModes.ToolTipsOnValuesAndLabels;
            m_callStackTabPage.Controls.Add((Control)m_callStackListView);
            m_callStackTabPage.ImageIndex = 3;
            m_callStackTabPage.Location = new Point(4, 4);
            m_callStackTabPage.Name = "m_callStackTabPage";
            m_callStackTabPage.Padding = new Padding(3);
            m_callStackTabPage.Size = new Size(522, 144);
            m_callStackTabPage.TabIndex = 1;
            m_callStackTabPage.Text = "Call Stack";
            m_callStackTabPage.UseVisualStyleBackColor = true;
            m_callStackListView.Columns.AddRange(new ColumnHeader[3]
            {
                m_funcNameColumnHeader,
                m_scriptNameColumnHeader,
                m_scriptLineNoColumnHeader
            });
            m_callStackListView.Dock = DockStyle.Fill;
            m_callStackListView.FullRowSelect = true;
            m_callStackListView.GridLines = true;
            m_callStackListView.Location = new Point(3, 3);
            m_callStackListView.Name = "m_callStackListView";
            m_callStackListView.Size = new Size(516, 138);
            m_callStackListView.TabIndex = 0;
            m_callStackListView.UseCompatibleStateImageBehavior = false;
            m_callStackListView.View = View.Details;
            m_funcNameColumnHeader.Text = "Function Name";
            m_funcNameColumnHeader.Width = 150;
            m_scriptNameColumnHeader.Text = "Script";
            m_scriptNameColumnHeader.Width = 150;
            m_scriptLineNoColumnHeader.Text = "Line #";
            m_scriptLineNoColumnHeader.TextAlign = HorizontalAlignment.Right;
            m_immediateTabPage.Controls.Add((Control)m_shellControl);
            m_immediateTabPage.ImageIndex = 0;
            m_immediateTabPage.Location = new Point(4, 4);
            m_immediateTabPage.Name = "m_immediateTabPage";
            m_immediateTabPage.Padding = new Padding(3);
            m_immediateTabPage.Size = new Size(522, 144);
            m_immediateTabPage.TabIndex = 3;
            m_immediateTabPage.Text = "Immediate";
            m_immediateTabPage.UseVisualStyleBackColor = true;
            m_shellControl.Dock = DockStyle.Fill;
            m_shellControl.Font = new Font("Courier New", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            m_shellControl.Location = new Point(3, 3);
            m_shellControl.Name = "m_shellControl";
            m_shellControl.Prompt = "> ";
            m_shellControl.ShellTextBackColor = SystemColors.Window;
            m_shellControl.ShellTextFont =
                new Font("Courier New", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            m_shellControl.ShellTextForeColor = SystemColors.WindowText;
            m_shellControl.Size = new Size(516, 138);
            m_shellControl.TabIndex = 0;
            m_shellControl.CommandEntered += new EventCommandEntered(OnCommandEntered);
            m_imageList.ImageStream = (ImageListStreamer)componentResourceManager.GetObject("m_imageList.ImageStream");
            m_imageList.TransparentColor = Color.Transparent;
            m_imageList.Images.SetKeyName(0, "debug-immediate");
            m_imageList.Images.SetKeyName(1, "debug-watch");
            m_imageList.Images.SetKeyName(2, "symbols");
            m_imageList.Images.SetKeyName(3, "debug-callstack");
            m_imageList.Images.SetKeyName(4, "view-resource");
            m_imageList.Images.SetKeyName(5, "bundle");
            m_imageList.Images.SetKeyName(6, "folder");
            m_imageList.Images.SetKeyName(7, "script");
            m_treeView.Dock = DockStyle.Left;
            m_treeView.FullRowSelect = true;
            m_treeView.HideSelection = false;
            m_treeView.ImageIndex = 0;
            m_treeView.ImageList = m_imageList;
            m_treeView.ItemHeight = 19;
            m_treeView.Location = new Point(0, 49);
            m_treeView.Name = "m_treeView";
            m_treeView.PathSeparator = "/";
            m_treeView.SelectedImageIndex = 0;
            m_treeView.Size = new Size(259, 502);
            m_treeView.TabIndex = 8;
            m_treeView.DoubleClick += new EventHandler(OnTreeDoubleClick);
            m_treeView.AfterSelect += new TreeViewEventHandler(OnAfterSelect);
            m_visualTipProvider.Animation = VisualTipAnimation.Enabled;
            m_visualTipProvider.DisplayAtMousePosition = false;
            m_visualTipProvider.DisplayMode = VisualTipDisplayMode.Manual;
            m_visualTipProvider.Renderer = (VisualTipRenderer)tipOfficeRenderer;
            m_gutterImageList.ImageStream =
                (ImageListStreamer)componentResourceManager.GetObject("m_gutterImageList.ImageStream");
            m_gutterImageList.TransparentColor = Color.Transparent;
            m_gutterImageList.Images.SetKeyName(0, "breakpoint-disabled.png");
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(792, 573);
            Controls.Add((Control)m_tabControl);
            Controls.Add((Control)m_sourceSplitter);
            Controls.Add((Control)m_syntaxBoxControl);
            Controls.Add((Control)m_mainSplitter);
            Controls.Add((Control)m_treeView);
            Controls.Add((Control)m_statusStrip);
            Controls.Add((Control)m_toolStrip);
            Controls.Add((Control)m_menuStrip);
            Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            MainMenuStrip = m_menuStrip;
            MinimumSize = new Size(640, 480);
            Name = nameof(DebuggerForm);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Kiosk Engine Debugger";
            Activated += new EventHandler(OnActivated);
            FormClosing += new FormClosingEventHandler(OnFormClosing);
            m_statusStrip.ResumeLayout(false);
            m_statusStrip.PerformLayout();
            m_menuStrip.ResumeLayout(false);
            m_menuStrip.PerformLayout();
            m_toolStrip.ResumeLayout(false);
            m_toolStrip.PerformLayout();
            m_tabControl.ResumeLayout(false);
            m_localsTabPage.ResumeLayout(false);
            m_localsPropertyGrid.EndInit();
            m_globalsTabPage.ResumeLayout(false);
            m_globalsPropertyGrid.EndInit();
            m_callStackTabPage.ResumeLayout(false);
            m_immediateTabPage.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}