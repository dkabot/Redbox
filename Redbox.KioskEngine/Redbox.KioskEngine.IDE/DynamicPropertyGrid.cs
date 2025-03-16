using Redbox.KioskEngine.IDE.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;
using VisualHint.SmartPropertyGrid;

namespace Redbox.KioskEngine.IDE
{
    public class DynamicPropertyGrid : VisualHint.SmartPropertyGrid.PropertyGrid
    {
        public DynamicPropertyGrid()
        {
            ToolbarVisibility = true;
            ShowDefaultValues = true;
            CommentsVisibility = false;
            ShowAdditionalIndentation = true;
            ToolTipMode = ToolTipModes.ToolTipsOnValuesAndLabels;
            var toolStripButton1 = new ToolStripButton((Image)Resources.Refresh);
            toolStripButton1.ToolTipText = "Refresh";
            var toolStripButton2 = toolStripButton1;
            toolStripButton2.Click += (EventHandler)((sender, e) =>
            {
                if (RefreshGrid == null)
                    return;
                RefreshGrid(sender, e);
            });
            var toolStripLabel = new ToolStripLabel("Filter:");
            var toolStripTextBox1 = new ToolStripTextBox("filter");
            toolStripTextBox1.BorderStyle = BorderStyle.None;
            toolStripTextBox1.Width = 350;
            var toolStripTextBox2 = toolStripTextBox1;
            Toolbar.Items.Add((ToolStripItem)toolStripButton2);
            Toolbar.Items.Add((ToolStripItem)new ToolStripSeparator());
            Toolbar.Items.Add((ToolStripItem)toolStripLabel);
            Toolbar.Items.Add((ToolStripItem)toolStripTextBox2);
            toolStripTextBox2.TextChanged +=
                (EventHandler)((sender, e) => FilterProperties(((ToolStripItem)sender).Text));
        }

        public event EventHandler RefreshGrid;

        protected override void OnDisplayModeChanged(EventArgs e)
        {
            FilterProperties(Toolbar.Items[4].Text);
        }

        private void FilterProperties(string filter)
        {
            BeginUpdate();
            var firstProperty = FirstProperty;
            while (firstProperty != firstProperty.RightBound)
            {
                if (firstProperty.Property.DisplayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var deepEnumerator = (PropertyEnumerator)firstProperty.GetDeepEnumerator();
                    while (deepEnumerator != deepEnumerator.RightBound)
                    {
                        if (!deepEnumerator.Property.Visible)
                            ShowProperty(deepEnumerator, true);
                        deepEnumerator.MoveParent();
                    }
                }
                else
                {
                    ShowProperty(firstProperty, false);
                }

                firstProperty.MoveNext();
            }

            EndUpdate();
        }
    }
}