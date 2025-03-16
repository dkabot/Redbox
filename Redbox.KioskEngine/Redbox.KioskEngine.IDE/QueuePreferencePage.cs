using Alsing.SourceCode;
using Alsing.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class QueuePreferencePage : UserControl, IPreferencePageHost
    {
        private IContainer components;
        private ListView m_queueListView;
        private ColumnHeader m_idColumnHeader;
        private ColumnHeader m_typeColumnHeader;
        private ColumnHeader m_priorityColumnHeader;
        private ColumnHeader m_createdOnColumnHeader;
        private ErrorProvider m_errorProvider;
        private ToolStrip m_toolStrip;
        private ToolStripButton toolStripButton1;
        private ToolStripLabel toolStripLabel1;
        private ToolStripTextBox m_refreshRateToolStripTextBox;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton m_startQueueWorkerToolStripButton;
        private ToolStripButton m_stopQueueWorkerToolStripButton;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton m_removeToolStripButton;
        private ToolStripButton m_exportToolStripButton;
        private ToolStripButton m_cleanToolStripButton;
        private SyntaxBoxControl m_syntaxBoxControl;
        private SyntaxDocument m_syntaxDocument;
        private ToolStripButton m_saveToolStripButton;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripLabel toolStripLabel3;
        private ToolStripLabel toolStripLabel4;
        private Label m_headerLabel;

        public QueuePreferencePage()
        {
            InitializeComponent();
            m_syntaxDocument.SetSyntaxFromEmbeddedResource(typeof(QueuePreferencePage).Assembly,
                "Redbox.KioskEngine.IDE.JavaScript.syn");
        }

        public bool SaveValues(IPreferencePage preferencePage)
        {
            return true;
        }

        public void LoadValues(IPreferencePage preferencePage)
        {
            m_refreshRateToolStripTextBox.Text =
                ServiceLocator.Instance.GetService<IQueueService>().QueueReadPeriod.ToString();
            SetButtonState();
            RefreshList();
        }

        private void OnSelectedMessageChanged(object sender, EventArgs e)
        {
            m_removeToolStripButton.Enabled = m_queueListView.SelectedItems.Count > 0;
            if (m_queueListView.SelectedItems.Count == 1)
            {
                var source = m_queueListView.SelectedItems[0].Tag as string;
                if (source.Length > 10000)
                    source = string.Empty;
                m_syntaxDocument.Text = source.ToPrettyJson();
            }
            else
            {
                m_syntaxDocument.Clear();
            }
        }

        private void OnClear(object sender, EventArgs e)
        {
            var service = ServiceLocator.Instance.GetService<IQueueService>();
            try
            {
                Cursor = Cursors.WaitCursor;
                service.Clear();
                RefreshList();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void OnExport(object sender, EventArgs e)
        {
            var service = ServiceLocator.Instance.GetService<IQueueService>();
            var flag = service.IsWorkerStarted();
            if (flag)
                service.StopQueueWorker();
            try
            {
                Cursor = Cursors.WaitCursor;
                var saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Title = "Export Message File";
                saveFileDialog1.DefaultExt = ".xml";
                saveFileDialog1.Filter = "XML Files (*.xml)|*.xml";
                var saveFileDialog2 = saveFileDialog1;
                if (saveFileDialog2.ShowDialog() != DialogResult.OK)
                    return;
                service.ExportToXml(saveFileDialog2.FileName);
            }
            finally
            {
                if (flag)
                    service.StartQueueWorker();
                Cursor = Cursors.Default;
                SetButtonState();
            }
        }

        private void OnRemove(object sender, EventArgs e)
        {
            var service = ServiceLocator.Instance.GetService<IQueueService>();
            if (m_queueListView.SelectedItems.Count == 0)
                return;
            try
            {
                Cursor = Cursors.WaitCursor;
                foreach (ListViewItem selectedItem in m_queueListView.SelectedItems)
                    service.DeleteMessage(int.Parse(selectedItem.Text));
                RefreshList();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void OnStopQueueWorker(object sender, EventArgs e)
        {
            var service = ServiceLocator.Instance.GetService<IQueueService>();
            try
            {
                Cursor = Cursors.WaitCursor;
                m_stopQueueWorkerToolStripButton.Enabled = false;
                service.StopQueueWorker();
                m_startQueueWorkerToolStripButton.Enabled = true;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void OnStartQueueWorker(object sender, EventArgs e)
        {
            var service = ServiceLocator.Instance.GetService<IQueueService>();
            try
            {
                Cursor = Cursors.WaitCursor;
                m_startQueueWorkerToolStripButton.Enabled = false;
                service.StartQueueWorker();
                m_stopQueueWorkerToolStripButton.Enabled = true;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            var service = ServiceLocator.Instance.GetService<IQueueService>();
            m_queueListView.SuspendLayout();
            m_queueListView.BeginUpdate();
            m_queueListView.Items.Clear();
            m_syntaxDocument.Clear();
            var action = (Action<IMessage>)(m => m_queueListView.Items.Add(new ListViewItem(m.ID.ToString())
            {
                Tag = !(m.Type != "Reconcile") ? (object)"Reconcile" : (object)Encoding.ASCII.GetString(m.Data),
                SubItems =
                {
                    m.Type,
                    m.Priority.ToString(),
                    m.CreatedOn.ToString()
                }
            }));
            service.ForEach(action);
            m_queueListView.EndUpdate();
            m_queueListView.ResumeLayout();
            m_removeToolStripButton.Enabled = m_queueListView.SelectedItems.Count > 0;
            m_cleanToolStripButton.Enabled = m_queueListView.Items.Count > 0;
            m_exportToolStripButton.Enabled = m_queueListView.Items.Count > 0;
        }

        private void SetButtonState()
        {
            var flag = ServiceLocator.Instance.GetService<IQueueService>().IsWorkerStarted();
            m_startQueueWorkerToolStripButton.Enabled = !flag;
            m_stopQueueWorkerToolStripButton.Enabled = flag;
        }

        private void OnDocumentChanged(object sender, EventArgs e)
        {
            m_saveToolStripButton.Enabled =
                m_syntaxDocument.UndoBuffer.Count > 0 && m_queueListView.SelectedItems.Count == 1;
        }

        private void OnSave(object sender, EventArgs e)
        {
            if (m_queueListView.SelectedItems.Count == 0)
                return;
            ServiceLocator.Instance.GetService<IQueueService>()
                .Update(int.Parse(m_queueListView.SelectedItems[0].SubItems[0].Text), m_syntaxDocument.Text);
            RefreshList();
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
            m_queueListView = new ListView();
            m_idColumnHeader = new ColumnHeader();
            m_typeColumnHeader = new ColumnHeader();
            m_priorityColumnHeader = new ColumnHeader();
            m_createdOnColumnHeader = new ColumnHeader();
            m_errorProvider = new ErrorProvider(components);
            m_toolStrip = new ToolStrip();
            toolStripLabel1 = new ToolStripLabel();
            m_refreshRateToolStripTextBox = new ToolStripTextBox();
            toolStripLabel3 = new ToolStripLabel();
            toolStripLabel4 = new ToolStripLabel();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripButton1 = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            m_startQueueWorkerToolStripButton = new ToolStripButton();
            m_stopQueueWorkerToolStripButton = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            m_saveToolStripButton = new ToolStripButton();
            m_removeToolStripButton = new ToolStripButton();
            toolStripSeparator5 = new ToolStripSeparator();
            m_exportToolStripButton = new ToolStripButton();
            m_cleanToolStripButton = new ToolStripButton();
            m_syntaxBoxControl = new SyntaxBoxControl();
            m_syntaxDocument = new SyntaxDocument(components);
            m_headerLabel = new Label();
            ((ISupportInitialize)m_errorProvider).BeginInit();
            m_toolStrip.SuspendLayout();
            SuspendLayout();
            m_queueListView.Columns.AddRange(new ColumnHeader[4]
            {
                m_idColumnHeader,
                m_typeColumnHeader,
                m_priorityColumnHeader,
                m_createdOnColumnHeader
            });
            m_queueListView.FullRowSelect = true;
            m_queueListView.HideSelection = false;
            m_queueListView.Location = new Point(0, 85);
            m_queueListView.Margin = new Padding(4);
            m_queueListView.Name = "m_queueListView";
            m_queueListView.Size = new Size(721, 377);
            m_queueListView.TabIndex = 12;
            m_queueListView.UseCompatibleStateImageBehavior = false;
            m_queueListView.View = View.Details;
            m_queueListView.SelectedIndexChanged += new EventHandler(OnSelectedMessageChanged);
            m_idColumnHeader.Text = "ID";
            m_idColumnHeader.Width = 100;
            m_typeColumnHeader.Text = "Type";
            m_typeColumnHeader.Width = 110;
            m_priorityColumnHeader.Text = "Priority";
            m_priorityColumnHeader.Width = 65;
            m_createdOnColumnHeader.Text = "Created On";
            m_createdOnColumnHeader.Width = 150;
            m_errorProvider.ContainerControl = (ContainerControl)this;
            m_toolStrip.CanOverflow = false;
            m_toolStrip.Dock = DockStyle.None;
            m_toolStrip.ImageScalingSize = new Size(20, 20);
            m_toolStrip.Items.AddRange(new ToolStripItem[15]
            {
                (ToolStripItem)toolStripLabel1,
                (ToolStripItem)m_refreshRateToolStripTextBox,
                (ToolStripItem)toolStripLabel3,
                (ToolStripItem)toolStripLabel4,
                (ToolStripItem)toolStripSeparator1,
                (ToolStripItem)toolStripButton1,
                (ToolStripItem)toolStripSeparator2,
                (ToolStripItem)m_startQueueWorkerToolStripButton,
                (ToolStripItem)m_stopQueueWorkerToolStripButton,
                (ToolStripItem)toolStripSeparator3,
                (ToolStripItem)m_saveToolStripButton,
                (ToolStripItem)m_removeToolStripButton,
                (ToolStripItem)toolStripSeparator5,
                (ToolStripItem)m_exportToolStripButton,
                (ToolStripItem)m_cleanToolStripButton
            });
            m_toolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
            m_toolStrip.Location = new Point(5, 53);
            m_toolStrip.Name = "m_toolStrip";
            m_toolStrip.Size = new Size(524, 27);
            m_toolStrip.TabIndex = 20;
            m_toolStrip.Text = "toolStrip1";
            toolStripLabel1.AutoSize = false;
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(120, 20);
            toolStripLabel1.Text = "Poll Interval (ms):";
            m_refreshRateToolStripTextBox.MaxLength = 6;
            m_refreshRateToolStripTextBox.Name = "m_refreshRateToolStripTextBox";
            m_refreshRateToolStripTextBox.Size = new Size(85, 27);
            toolStripLabel3.Name = "toolStripLabel3";
            toolStripLabel3.Size = new Size(25, 20);
            toolStripLabel3.Text = "    ";
            toolStripLabel4.Name = "toolStripLabel4";
            toolStripLabel4.Size = new Size(25, 20);
            toolStripLabel4.Text = "    ";
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 23);
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton1.Image = (Image)Resources.Refresh;
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(29, 24);
            toolStripButton1.Text = "Refresh";
            toolStripButton1.Click += new EventHandler(OnRefresh);
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 23);
            m_startQueueWorkerToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_startQueueWorkerToolStripButton.Image = (Image)Resources.DebugPlay;
            m_startQueueWorkerToolStripButton.ImageTransparentColor = Color.Magenta;
            m_startQueueWorkerToolStripButton.Name = "m_startQueueWorkerToolStripButton";
            m_startQueueWorkerToolStripButton.Size = new Size(29, 24);
            m_startQueueWorkerToolStripButton.Text = "Start Queue Worker";
            m_startQueueWorkerToolStripButton.Click += new EventHandler(OnStartQueueWorker);
            m_stopQueueWorkerToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_stopQueueWorkerToolStripButton.Image = (Image)Resources.DebugStop;
            m_stopQueueWorkerToolStripButton.ImageTransparentColor = Color.Magenta;
            m_stopQueueWorkerToolStripButton.Name = "m_stopQueueWorkerToolStripButton";
            m_stopQueueWorkerToolStripButton.Size = new Size(29, 24);
            m_stopQueueWorkerToolStripButton.Text = "Stop Queue Worker";
            m_stopQueueWorkerToolStripButton.Click += new EventHandler(OnStopQueueWorker);
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 23);
            m_saveToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_saveToolStripButton.Enabled = false;
            m_saveToolStripButton.Image = (Image)Resources.Save;
            m_saveToolStripButton.ImageTransparentColor = Color.Magenta;
            m_saveToolStripButton.Name = "m_saveToolStripButton";
            m_saveToolStripButton.Size = new Size(29, 24);
            m_saveToolStripButton.Text = "Save";
            m_saveToolStripButton.Click += new EventHandler(OnSave);
            m_removeToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_removeToolStripButton.Image = (Image)Resources.Delete;
            m_removeToolStripButton.ImageTransparentColor = Color.Magenta;
            m_removeToolStripButton.Name = "m_removeToolStripButton";
            m_removeToolStripButton.Size = new Size(29, 24);
            m_removeToolStripButton.Text = "Remove";
            m_removeToolStripButton.ToolTipText = "Remove";
            m_removeToolStripButton.Click += new EventHandler(OnRemove);
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(6, 23);
            m_exportToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_exportToolStripButton.Image = (Image)Resources.SaveAs;
            m_exportToolStripButton.ImageTransparentColor = Color.Magenta;
            m_exportToolStripButton.Name = "m_exportToolStripButton";
            m_exportToolStripButton.Size = new Size(29, 24);
            m_exportToolStripButton.Text = "Export";
            m_exportToolStripButton.Click += new EventHandler(OnExport);
            m_cleanToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_cleanToolStripButton.Image = (Image)Resources.Clean;
            m_cleanToolStripButton.ImageTransparentColor = Color.Magenta;
            m_cleanToolStripButton.Name = "m_cleanToolStripButton";
            m_cleanToolStripButton.Size = new Size(29, 24);
            m_cleanToolStripButton.Text = "Clear";
            m_cleanToolStripButton.Visible = false;
            m_cleanToolStripButton.Click += new EventHandler(OnClear);
            m_syntaxBoxControl.ActiveView = ActiveView.BottomRight;
            m_syntaxBoxControl.AllowBreakPoints = false;
            m_syntaxBoxControl.AutoListPosition = (TextPoint)null;
            m_syntaxBoxControl.AutoListSelectedText = "a123";
            m_syntaxBoxControl.AutoListVisible = false;
            m_syntaxBoxControl.BackColor = Color.White;
            m_syntaxBoxControl.BorderStyle = Alsing.Windows.Forms.BorderStyle.None;
            m_syntaxBoxControl.CopyAsRTF = false;
            m_syntaxBoxControl.Document = m_syntaxDocument;
            m_syntaxBoxControl.FontName = "Courier new";
            m_syntaxBoxControl.ImeMode = ImeMode.NoControl;
            m_syntaxBoxControl.InfoTipCount = 1;
            m_syntaxBoxControl.InfoTipPosition = (TextPoint)null;
            m_syntaxBoxControl.InfoTipSelectedIndex = 1;
            m_syntaxBoxControl.InfoTipVisible = false;
            m_syntaxBoxControl.Location = new Point(-1, 470);
            m_syntaxBoxControl.LockCursorUpdate = false;
            m_syntaxBoxControl.Margin = new Padding(4);
            m_syntaxBoxControl.Name = "m_syntaxBoxControl";
            m_syntaxBoxControl.ShowEOLMarker = true;
            m_syntaxBoxControl.ShowGutterMargin = false;
            m_syntaxBoxControl.ShowScopeIndicator = false;
            m_syntaxBoxControl.ShowTabGuides = true;
            m_syntaxBoxControl.ShowWhitespace = true;
            m_syntaxBoxControl.Size = new Size(723, 153);
            m_syntaxBoxControl.SmoothScroll = false;
            m_syntaxBoxControl.SplitView = false;
            m_syntaxBoxControl.SplitviewH = -4;
            m_syntaxBoxControl.SplitviewV = -4;
            m_syntaxBoxControl.TabGuideColor = Color.FromArgb(222, 219, 214);
            m_syntaxBoxControl.TabIndex = 22;
            m_syntaxBoxControl.TabsToSpaces = true;
            m_syntaxBoxControl.WhitespaceColor = SystemColors.ControlDark;
            m_syntaxDocument.Lines = new string[1] { "" };
            m_syntaxDocument.MaxUndoBufferSize = 1000;
            m_syntaxDocument.Modified = false;
            m_syntaxDocument.UndoStep = 0;
            m_syntaxDocument.Change += new EventHandler(OnDocumentChanged);
            m_headerLabel.Anchor = AnchorStyles.Top;
            m_headerLabel.BackColor = Color.MediumBlue;
            m_headerLabel.Font = new Font("Microsoft Sans Serif", 14.25f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            m_headerLabel.ForeColor = Color.White;
            m_headerLabel.Location = new Point(0, 0);
            m_headerLabel.Margin = new Padding(4, 0, 4, 0);
            m_headerLabel.Name = "m_headerLabel";
            m_headerLabel.Size = new Size(723, 47);
            m_headerLabel.TabIndex = 23;
            m_headerLabel.Text = "  Engine Core: Message Queue";
            m_headerLabel.TextAlign = ContentAlignment.MiddleLeft;
            AutoScaleDimensions = new SizeF(8f, 16f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add((Control)m_headerLabel);
            Controls.Add((Control)m_syntaxBoxControl);
            Controls.Add((Control)m_queueListView);
            Controls.Add((Control)m_toolStrip);
            Margin = new Padding(4);
            Name = nameof(QueuePreferencePage);
            Size = new Size(723, 623);
            ((ISupportInitialize)m_errorProvider).EndInit();
            m_toolStrip.ResumeLayout(false);
            m_toolStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}