using Redbox.Lua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using VisualHint.SmartPropertyGrid;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE
{
    public class SymbolsToolWindow : ToolWindow
    {
        private IContainer components;
        private DynamicPropertyGrid m_dynamicPropertyGrid;

        public SymbolsToolWindow()
        {
            InitializeComponent();
            m_dynamicPropertyGrid.PropertyButtonClicked +=
                (VisualHint.SmartPropertyGrid.PropertyGrid.PropertyButtonClickedEventHandler)((sender, e) =>
                {
                    if (e.PropertyEnum.Property.Tag is LuaTable tag3)
                    {
                        var num = (int)new DebuggerVariableForm()
                        {
                            VariableName = e.PropertyEnum.Property.Name,
                            VariableValue = tag3.ToString()
                        }.ShowDialog();
                    }
                    else
                    {
                        var tag2 = e.PropertyEnum.Property.Tag as LuaFunction;
                    }
                });
            m_dynamicPropertyGrid.PropertyExpanded +=
                (VisualHint.SmartPropertyGrid.PropertyGrid.PropertyExpandedEventHandler)((sender, e) =>
                {
                    if (!e.Expanded)
                        return;
                    if (m_dynamicPropertyGrid.DisplayMode !=
                        VisualHint.SmartPropertyGrid.PropertyGrid.DisplayModes.Categorized)
                    {
                        m_dynamicPropertyGrid.DisplayMode =
                            VisualHint.SmartPropertyGrid.PropertyGrid.DisplayModes.Categorized;
                    }
                    else
                    {
                        if (!(e.PropertyEnum.Property.Tag is LuaTable tag5))
                            return;
                        try
                        {
                            Cursor = Cursors.WaitCursor;
                            m_dynamicPropertyGrid.SuspendLayout();
                            m_dynamicPropertyGrid.BeginUpdate();
                            m_dynamicPropertyGrid.ClearUnderProperty(e.PropertyEnum);
                            BuildNestedTableProperties((VisualHint.SmartPropertyGrid.PropertyGrid)m_dynamicPropertyGrid,
                                e.PropertyEnum, e.PropertyEnum.Property.Id + 1000, tag5,
                                (ICollection<LuaTable>)new List<LuaTable>());
                        }
                        finally
                        {
                            Cursor = Cursors.Default;
                            m_dynamicPropertyGrid.EndUpdate();
                            m_dynamicPropertyGrid.ResumeLayout();
                        }
                    }
                });
        }

        public void RefreshSymbols()
        {
            OnRefresh((object)this, EventArgs.Empty);
        }

        public void ShowSymbols(string categoryName, IEnumerable<LuaVar> vars)
        {
            RefreshVariableGrid((VisualHint.SmartPropertyGrid.PropertyGrid)m_dynamicPropertyGrid, categoryName, vars);
        }

        public event EventHandler RefreshRequested;

        protected override string GetPersistString()
        {
            return GetType()?.ToString() + "," + Text;
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            if (RefreshRequested == null)
                return;
            RefreshRequested(sender, e);
        }

        private static object GetAdjustedValue(object var, out Type type, out bool isReadOnly)
        {
            isReadOnly = false;
            var adjustedValue = (object)"nil";
            if (var != null)
                adjustedValue = var;
            type = adjustedValue.GetType();
            if (type == typeof(LuaTable))
            {
                adjustedValue = (object)"(table)";
                type = typeof(string);
                isReadOnly = true;
            }
            else if (type == typeof(LuaFunction))
            {
                adjustedValue = (object)"(function)";
                type = typeof(string);
                isReadOnly = true;
            }

            return adjustedValue;
        }

        private void RefreshVariableGrid(VisualHint.SmartPropertyGrid.PropertyGrid grid, string category,
            IEnumerable<LuaVar> vars)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                grid.SuspendLayout();
                grid.BeginUpdate();
                grid.Clear();
                grid.DisplayMode = VisualHint.SmartPropertyGrid.PropertyGrid.DisplayModes.Categorized;
                var underCategory1 = grid.AppendRootCategory(1, category);
                var num = 2;
                foreach (var var in vars)
                    if (!(var.Name == "package"))
                    {
                        Type type;
                        var adjustedValue = GetAdjustedValue(var.Value, out type, out var _);
                        var attributeList = new List<Attribute>()
                        {
                            (Attribute)new DefaultValueAttribute(adjustedValue)
                        };
                        var luaTable = var.Value as LuaTable;
                        var underCategory2 = grid.AppendManagedProperty(underCategory1, num++, var.Name, type,
                            adjustedValue, string.Empty, attributeList.ToArray());
                        if (luaTable != null)
                        {
                            underCategory2.Property.Tag = var.Value;
                            underCategory2.Property.Feel = grid.GetRegisteredFeel("button");
                            grid.AppendManagedProperty(underCategory2, num++, "hidden", typeof(string), (object)null,
                                string.Empty);
                        }

                        if (var.Value is LuaFunction)
                        {
                            underCategory2.Property.Tag = var.Value;
                            underCategory2.Property.Feel = grid.GetRegisteredFeel("button");
                        }
                    }

                grid.EndUpdate();
                grid.ResumeLayout();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
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
                var attributeList = new List<Attribute>()
                {
                    (Attribute)new DefaultValueAttribute(adjustedValue)
                };
                var underCategory = grid.AppendManagedProperty(parentEnumerator, id++, key.ToString(), type,
                    adjustedValue, key.ToString(), attributeList.ToArray());
                if (table[key] is LuaTable luaTable)
                {
                    underCategory.Property.Tag = (object)luaTable;
                    underCategory.Property.Feel = grid.GetRegisteredFeel("button");
                    grid.AppendManagedProperty(underCategory, id++, "hidden", typeof(string), (object)null,
                        string.Empty);
                }

                if (table[key] is LuaFunction luaFunction)
                {
                    underCategory.Property.Tag = (object)luaFunction;
                    underCategory.Property.Feel = grid.GetRegisteredFeel("button");
                }
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
            var componentResourceManager = new ComponentResourceManager(typeof(SymbolsToolWindow));
            m_dynamicPropertyGrid = new DynamicPropertyGrid();
            m_dynamicPropertyGrid.BeginInit();
            SuspendLayout();
            m_dynamicPropertyGrid.BorderStyle = BorderStyle.None;
            m_dynamicPropertyGrid.Dock = DockStyle.Fill;
            m_dynamicPropertyGrid.Location = new Point(0, 0);
            m_dynamicPropertyGrid.Name = "m_dynamicPropertyGrid";
            m_dynamicPropertyGrid.PropertyLabelBackColor = SystemColors.Window;
            m_dynamicPropertyGrid.PropertyValueBackColor = SystemColors.Window;
            m_dynamicPropertyGrid.ShowAdditionalIndentation = true;
            m_dynamicPropertyGrid.ShowDefaultValues = true;
            m_dynamicPropertyGrid.Size = new Size(398, 318);
            m_dynamicPropertyGrid.TabIndex = 0;
            m_dynamicPropertyGrid.Text = "m_dynamicPropertyGrid";
            m_dynamicPropertyGrid.ToolbarVisibility = true;
            m_dynamicPropertyGrid.ToolTipMode =
                VisualHint.SmartPropertyGrid.PropertyGrid.ToolTipModes.ToolTipsOnValuesAndLabels;
            m_dynamicPropertyGrid.RefreshGrid += new EventHandler(OnRefresh);
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(398, 318);
            Controls.Add((Control)m_dynamicPropertyGrid);
            Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            HideOnClose = true;
            Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            Name = nameof(SymbolsToolWindow);
            ShowHint = DockState.DockBottom;
            Text = "Symbols";
            m_dynamicPropertyGrid.EndInit();
            ResumeLayout(false);
        }
    }
}