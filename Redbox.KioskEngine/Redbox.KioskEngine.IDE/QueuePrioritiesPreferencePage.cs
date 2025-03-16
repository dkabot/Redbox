using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class QueuePrioritiesPreferencePage : UserControl, IPreferencePageHost
    {
        private IContainer components;
        private Label m_headerLabel;
        private ListView listViewQueuePriorities;
        private ColumnHeader columnHeaderPriority;
        private ColumnHeader columnHeaderValueRange;
        private ColumnHeader columnHeaderIncludeTimeRange;
        private ColumnHeader columnHeaderExcludeTimeRange;
        private ColumnHeader columnHeaderDescription;

        public QueuePrioritiesPreferencePage()
        {
            InitializeComponent();
        }

        public bool SaveValues(IPreferencePage preferencePage)
        {
            return true;
        }

        public void LoadValues(IPreferencePage preferencePage)
        {
            var service = ServiceLocator.Instance.GetService<IQueueService>();
            listViewQueuePriorities.Items.Clear();
            service?.ForEachPriority((Action<IQueueServicePriority>)(eachQueueServicePriority =>
            {
                var listViewItem = new ListViewItem()
                {
                    Text = eachQueueServicePriority.PriorityType.ToString()
                };
                var text1 = string.Format("{0} - {1}", (object)eachQueueServicePriority.MinimumPriorityValue,
                    (object)eachQueueServicePriority.MaximumPriorityValue);
                listViewItem.SubItems.Add(text1);
                var text2 = string.Format("{0} - {1}", (object)eachQueueServicePriority.StartTime,
                    (object)eachQueueServicePriority.EndTime);
                listViewItem.SubItems.Add(text2);
                var text3 = eachQueueServicePriority.ExcludeTimeRanges
                    .Select<IQueueServicePriority, string>(
                        (Func<IQueueServicePriority, string>)(y => y.PriorityType.ToString())).Join<string>(", ");
                listViewItem.SubItems.Add(text3);
                listViewItem.SubItems.Add(eachQueueServicePriority.Description);
                listViewQueuePriorities.Items.Add(listViewItem);
            }));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_headerLabel = new Label();
            listViewQueuePriorities = new ListView();
            columnHeaderPriority = new ColumnHeader();
            columnHeaderValueRange = new ColumnHeader();
            columnHeaderIncludeTimeRange = new ColumnHeader();
            columnHeaderExcludeTimeRange = new ColumnHeader();
            columnHeaderDescription = new ColumnHeader();
            SuspendLayout();
            m_headerLabel.BackColor = Color.MediumBlue;
            m_headerLabel.Dock = DockStyle.Top;
            m_headerLabel.Font = new Font("Microsoft Sans Serif", 14.25f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            m_headerLabel.ForeColor = Color.White;
            m_headerLabel.Location = new Point(0, 0);
            m_headerLabel.Name = "m_headerLabel";
            m_headerLabel.Size = new Size(524, 38);
            m_headerLabel.TabIndex = 24;
            m_headerLabel.Text = "Message Queue Priorities";
            m_headerLabel.TextAlign = ContentAlignment.MiddleLeft;
            listViewQueuePriorities.Columns.AddRange(new ColumnHeader[5]
            {
                columnHeaderPriority,
                columnHeaderValueRange,
                columnHeaderIncludeTimeRange,
                columnHeaderExcludeTimeRange,
                columnHeaderDescription
            });
            listViewQueuePriorities.Dock = DockStyle.Fill;
            listViewQueuePriorities.Location = new Point(0, 38);
            listViewQueuePriorities.Name = "listViewQueuePriorities";
            listViewQueuePriorities.Size = new Size(524, 468);
            listViewQueuePriorities.TabIndex = 25;
            listViewQueuePriorities.UseCompatibleStateImageBehavior = false;
            listViewQueuePriorities.View = View.Details;
            columnHeaderPriority.Text = "Priority";
            columnHeaderPriority.Width = 70;
            columnHeaderValueRange.Text = "Value Range";
            columnHeaderValueRange.TextAlign = HorizontalAlignment.Center;
            columnHeaderValueRange.Width = 100;
            columnHeaderIncludeTimeRange.Text = "Time Range";
            columnHeaderIncludeTimeRange.Width = 140;
            columnHeaderExcludeTimeRange.Text = "Exclude Time Ranges";
            columnHeaderExcludeTimeRange.Width = 120;
            columnHeaderDescription.Text = "Description";
            columnHeaderDescription.Width = 700;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add((Control)listViewQueuePriorities);
            Controls.Add((Control)m_headerLabel);
            Name = nameof(QueuePrioritiesPreferencePage);
            Size = new Size(524, 506);
            ResumeLayout(false);
        }
    }
}