using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using VisualHint.SmartPropertyGrid;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE
{
    public class PropertiesToolWindow : ToolWindow
    {
        private IContainer components;
        private DynamicPropertyGrid m_dynamicPropertyGrid;

        public PropertiesToolWindow()
        {
            InitializeComponent();
            m_dynamicPropertyGrid.PropertyChanged +=
                (VisualHint.SmartPropertyGrid.PropertyGrid.PropertyChangedEventHandler)((sender, e) =>
                {
                    if (!(e.PropertyEnum.Property.Tag is PropertyHandler tag2))
                        return;
                    tag2.UpdatePropertyHost(e.PropertyEnum.Property);
                });
        }

        public void SetObject(object instance)
        {
            m_dynamicPropertyGrid.BeginUpdate();
            m_dynamicPropertyGrid.Clear();
            var num1 = 2;
            var resourceBundle = instance as IResourceBundle;
            if (instance is IResource resource)
            {
                var dictionary = new Dictionary<string, PropertyEnumerator>();
                var resourceType = resource.Type;
                var propertyEnumerator1 = (PropertyEnumerator)null;
                for (; resourceType != null; resourceType = resourceType.SuperType)
                    foreach (var property in resourceType.Properties)
                        try
                        {
                            var propertyEnumerator2 = (PropertyEnumerator)null;
                            if (!string.IsNullOrEmpty(property.Category))
                            {
                                if (!dictionary.ContainsKey(property.Category))
                                    dictionary[property.Category] =
                                        m_dynamicPropertyGrid.AppendRootCategory(num1++, property.Category);
                                propertyEnumerator2 = dictionary[property.Category];
                            }
                            else if (propertyEnumerator1 == (PropertyEnumerator)null)
                            {
                                propertyEnumerator1 = m_dynamicPropertyGrid.AppendRootCategory(num1++, "Misc");
                            }

                            var str = property.Name;
                            if (!string.IsNullOrEmpty(property.Label))
                                str = string.Format("{0} ({1})", (object)property.Label, (object)property.Name);
                            var obj = resource[property.Name];
                            var typedValue1 = property.GetTypedValue(obj);
                            var typedValue2 = property.GetTypedValue(property.DefaultValue);
                            if (typedValue1 is List<object>)
                            {
                                var stringList = convertValueArray(typedValue1 as List<object>);
                                var dynamicPropertyGrid = m_dynamicPropertyGrid;
                                var underCategory = propertyEnumerator2;
                                if ((object)underCategory == null)
                                    underCategory = propertyEnumerator1;
                                var id = num1++;
                                var propName = str;
                                var valueType = typeof(List<string>);
                                var initialValue = stringList;
                                var comment = property.Description ?? string.Empty;
                                var attributeArray = new Attribute[1]
                                {
                                    (Attribute)new DefaultValueAttribute(typedValue2)
                                };
                                dynamicPropertyGrid.AppendManagedProperty(underCategory, id, propName, valueType,
                                    (object)initialValue, comment, attributeArray);
                            }
                            else
                            {
                                var dynamicPropertyGrid = m_dynamicPropertyGrid;
                                var underCategory = propertyEnumerator2;
                                if ((object)underCategory == null)
                                    underCategory = propertyEnumerator1;
                                var id = num1++;
                                var propName = str;
                                var type = property.Type;
                                var initialValue = typedValue1;
                                var comment = property.Description ?? string.Empty;
                                var attributeArray = new Attribute[1]
                                {
                                    (Attribute)new DefaultValueAttribute(typedValue2)
                                };
                                dynamicPropertyGrid.AppendManagedProperty(underCategory, id, propName, type,
                                    initialValue, comment, attributeArray).Property.Tag = (object)new PropertyHandler()
                                {
                                    Properties =
                                    {
                                        {
                                            "resource",
                                            (object)resource
                                        },
                                        {
                                            "propertyType",
                                            (object)property
                                        }
                                    }
                                };
                            }
                        }
                        catch (Exception ex)
                        {
                        }
            }

            var resourceType1 = instance as IResourceType;
            if (instance is KernelFunctionInfo kernelFunctionInfo)
            {
                var dictionary1 = new Dictionary<string, PropertyEnumerator>();
                var dictionary2 = dictionary1;
                var dynamicPropertyGrid1 = m_dynamicPropertyGrid;
                var id1 = num1;
                var num2 = id1 + 1;
                var propertyEnumerator3 = dynamicPropertyGrid1.AppendRootCategory(id1, "Library");
                dictionary2["Library"] = propertyEnumerator3;
                var dynamicPropertyGrid2 = m_dynamicPropertyGrid;
                var underCategory1 = dictionary1["Library"];
                var id2 = num2;
                var num3 = id2 + 1;
                var valueType1 = typeof(string);
                var extension = kernelFunctionInfo.Extension;
                dynamicPropertyGrid2.AppendManagedProperty(underCategory1, id2, "Extension", valueType1,
                    (object)extension, "The Kiosk Extension name or Core name into which the function is grouped.");
                var dynamicPropertyGrid3 = m_dynamicPropertyGrid;
                var underCategory2 = dictionary1["Library"];
                var id3 = num3;
                var num4 = id3 + 1;
                var valueType2 = typeof(string);
                var fullName = kernelFunctionInfo.Method.DeclaringType.FullName;
                dynamicPropertyGrid3.AppendManagedProperty(underCategory2, id3, "Namespace", valueType2,
                    (object)fullName, "The namespace in which this function is defined");
                var dynamicPropertyGrid4 = m_dynamicPropertyGrid;
                var underCategory3 = dictionary1["Library"];
                var id4 = num4;
                var num5 = id4 + 1;
                var valueType3 = typeof(string);
                var name1 = kernelFunctionInfo.Method.Module.Name;
                dynamicPropertyGrid4.AppendManagedProperty(underCategory3, id4, "Module", valueType3, (object)name1,
                    "The DLL into which this method is compiled and distributed.");
                var dynamicPropertyGrid5 = m_dynamicPropertyGrid;
                var underCategory4 = dictionary1["Library"];
                var id5 = num5;
                var num6 = id5 + 1;
                var valueType4 = typeof(string);
                var name2 = kernelFunctionInfo.Method.Name;
                dynamicPropertyGrid5.AppendManagedProperty(underCategory4, id5, "C# Function", valueType4,
                    (object)name2, "The C# function which implements the API when it is called from Lua.");
                var dictionary3 = dictionary1;
                var dynamicPropertyGrid6 = m_dynamicPropertyGrid;
                var id6 = num6;
                var num7 = id6 + 1;
                var propertyEnumerator4 = dynamicPropertyGrid6.AppendRootCategory(id6, "Function");
                dictionary3["Function"] = propertyEnumerator4;
                var dynamicPropertyGrid7 = m_dynamicPropertyGrid;
                var underCategory5 = dictionary1["Function"];
                var id7 = num7;
                var num8 = id7 + 1;
                var valueType5 = typeof(string);
                var name3 = kernelFunctionInfo.Attributes[0].Name;
                dynamicPropertyGrid7.AppendManagedProperty(underCategory5, id7, "API Name", valueType5, (object)name3,
                    "The API name which is accessible from the Lua runtime.");
                var dynamicPropertyGrid8 = m_dynamicPropertyGrid;
                var underCategory6 = dictionary1["Function"];
                var id8 = num8;
                var num9 = id8 + 1;
                var valueType6 = typeof(string);
                var name4 = kernelFunctionInfo.Method.ReturnType.Name;
                dynamicPropertyGrid8.AppendManagedProperty(underCategory6, id8, "Return Type", valueType6,
                    (object)name4, "The object type of the return value.");
                var dictionary4 = dictionary1;
                var dynamicPropertyGrid9 = m_dynamicPropertyGrid;
                var id9 = num9;
                var num10 = id9 + 1;
                var propertyEnumerator5 = dynamicPropertyGrid9.AppendRootCategory(id9, "Parameters");
                dictionary4["Parameters"] = propertyEnumerator5;
                var parameters = kernelFunctionInfo.Method.GetParameters();
                if (parameters != null)
                    foreach (var parameterInfo in parameters)
                    {
                        var stringBuilder = new StringBuilder();
                        stringBuilder.Append("(" + (parameterInfo.IsOut ? "out" : "in") + ") ");
                        stringBuilder.Append(parameterInfo.ParameterType.Name + " ");
                        m_dynamicPropertyGrid.AppendManagedProperty(dictionary1["Parameters"], num10++,
                            parameterInfo.Name, typeof(string), (object)stringBuilder.ToString(), string.Empty);
                    }
            }

            m_dynamicPropertyGrid.EndUpdate();
        }

        internal DynamicPropertyGrid Properties => m_dynamicPropertyGrid;

        private List<string> convertValueArray(List<object> input)
        {
            var stringList = new List<string>();
            foreach (var obj in input)
                if (obj is string)
                    stringList.Add(obj as string);
            return stringList;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            var componentResourceManager = new ComponentResourceManager(typeof(PropertiesToolWindow));
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
            m_dynamicPropertyGrid.Size = new Size(342, 303);
            m_dynamicPropertyGrid.TabIndex = 0;
            m_dynamicPropertyGrid.Text = "dynamicPropertyGrid1";
            m_dynamicPropertyGrid.ToolbarVisibility = true;
            m_dynamicPropertyGrid.ToolTipMode =
                VisualHint.SmartPropertyGrid.PropertyGrid.ToolTipModes.ToolTipsOnValuesAndLabels;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(342, 303);
            Controls.Add((Control)m_dynamicPropertyGrid);
            Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            HideOnClose = true;
            DockAreas = DockAreas.Float | DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockTop |
                        DockAreas.DockBottom;
            Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            Name = nameof(PropertiesToolWindow);
            ShowHint = DockState.DockRightAutoHide;
            Text = "Properties";
            m_dynamicPropertyGrid.EndInit();
            ResumeLayout(false);
        }

        internal class PropertyHandler
        {
            public IDictionary<string, object> Properties =
                (IDictionary<string, object>)new Dictionary<string, object>();

            public virtual void UpdatePropertyHost(Property property)
            {
                if (!Properties.ContainsKey("resource") || !Properties.ContainsKey("propertyType"))
                    return;
                var property1 = Properties["resource"] as IResource;
                var property2 = Properties["propertyType"] as IPropertyType;
                if (property1 == null || property2 == null)
                    return;
                property1[property2.Name] = property.Value.GetValue();
            }
        }
    }
}