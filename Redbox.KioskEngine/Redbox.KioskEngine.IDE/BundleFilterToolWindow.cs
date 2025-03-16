using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using VisualHint.SmartPropertyGrid;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE
{
    public class BundleFilterToolWindow : ToolWindow
    {
        private IContainer components;
        private DynamicPropertyGrid m_dynamicPropertyGrid;

        public BundleFilterToolWindow()
        {
            InitializeComponent();
            m_dynamicPropertyGrid.PropertyChanged +=
                (VisualHint.SmartPropertyGrid.PropertyGrid.PropertyChangedEventHandler)((sender, e) =>
                {
                    if (!(e.PropertyEnum.Property.Tag is IPropertyType tag2))
                        return;
                    BundleService.Filter[tag2.Name] = e.PropertyEnum.Property.Value.GetValue() as string;
                });
        }

        public void RefreshFilters()
        {
            InnerRefreshFilters();
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            InnerRefreshFilters();
        }

        private void InnerRefreshFilters()
        {
            m_dynamicPropertyGrid.SuspendLayout();
            m_dynamicPropertyGrid.BeginUpdate();
            m_dynamicPropertyGrid.Clear();
            m_dynamicPropertyGrid.DisplayMode = VisualHint.SmartPropertyGrid.PropertyGrid.DisplayModes.Categorized;
            var num = 2;
            var stringList = new List<string>();
            var service = ServiceLocator.Instance.GetService<IResourceBundleService>();
            foreach (var resourceType in (IEnumerable<KeyValuePair<string, IResourceType>>)service.GetResourceTypes())
            {
                var filterProperties = resourceType.Value.GetFilterProperties();
                if (filterProperties.Count != 0)
                {
                    var propertyTypeList = new List<IPropertyType>();
                    foreach (var propertyType in filterProperties)
                    {
                        var each = propertyType;
                        if (stringList.Find((Predicate<string>)(eachName => each.Name == eachName)) == null)
                        {
                            propertyTypeList.Add(each);
                            stringList.Add(each.Name);
                        }
                    }

                    if (propertyTypeList.Count != 0)
                    {
                        var underCategory = m_dynamicPropertyGrid.AppendRootCategory(num++, resourceType.Key);
                        foreach (var propertyType in propertyTypeList)
                            m_dynamicPropertyGrid.AppendManagedProperty(underCategory, num++,
                                    string.Format("{0} ({1})", (object)propertyType.Label, (object)propertyType.Name),
                                    propertyType.Type, (object)service.Filter[propertyType.Name],
                                    propertyType.Description ?? string.Empty,
                                    (Attribute)new DefaultValueAttribute(propertyType.DefaultValue)).Property.Tag =
                                (object)propertyType;
                    }
                }
            }

            m_dynamicPropertyGrid.EndUpdate();
            m_dynamicPropertyGrid.ResumeLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            var componentResourceManager = new ComponentResourceManager(typeof(BundleFilterToolWindow));
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
            m_dynamicPropertyGrid.Size = new Size(460, 260);
            m_dynamicPropertyGrid.TabIndex = 0;
            m_dynamicPropertyGrid.ToolbarVisibility = true;
            m_dynamicPropertyGrid.ToolTipMode =
                VisualHint.SmartPropertyGrid.PropertyGrid.ToolTipModes.ToolTipsOnValuesAndLabels;
            m_dynamicPropertyGrid.RefreshGrid += new EventHandler(OnRefresh);
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(460, 260);
            Controls.Add((Control)m_dynamicPropertyGrid);
            Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            HideOnClose = true;
            Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            Name = nameof(BundleFilterToolWindow);
            ShowHint = DockState.DockBottom;
            Text = "Bundle Filters";
            m_dynamicPropertyGrid.EndInit();
            ResumeLayout(false);
        }
    }
}