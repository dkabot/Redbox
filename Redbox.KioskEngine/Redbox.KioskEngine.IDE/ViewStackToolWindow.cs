using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE
{
    public class ViewStackToolWindow : ToolWindow
    {
        private IContainer components;
        private ListView m_stackListView;
        private ColumnHeader m_stackViewNameColumnHeader;
        private ColumnHeader m_clearColumnHeader;
        private ColumnHeader m_sceneNameColumnHeader;
        private ColumnHeader m_widthColumnHeader;
        private ColumnHeader m_heightColumnHeader;
        private ToolStrip m_toolStrip;
        private ToolStripButton m_refreshToolStripButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripComboBox m_viewResourceToolStripComboBox;
        private ToolStripButton m_pushToolStripButton;
        private ToolStripButton m_popToolStripButton;
        private ToolStripButton m_popDiscardToolStripButton;
        private ToolStripButton m_showToolStripButton;
        private ToolStripLabel m_viewToolStripLabel;

        public ViewStackToolWindow()
        {
            InitializeComponent();
            var service = ServiceLocator.Instance.GetService<IViewService>();
            if (service == null)
                return;
            service.ViewStateChanged += (EventHandler<ViewStateChangedArgs>)((o, e) => RefreshStackList());
        }

        private void OnLoad(object sender, EventArgs e)
        {
            RefreshViewResourceList();
            RefreshStackList();
        }

        private void OnPush(object sender, EventArgs e)
        {
            var control = (ComboBox)m_viewResourceToolStripComboBox.Control;
            if (control.SelectedItem == null)
                return;
            try
            {
                Cursor = Cursors.WaitCursor;
                ServiceLocator.Instance.GetService<IViewService>().Push(((IResource)control.SelectedItem).Name);
                RefreshStackList();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void OnPop(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                ServiceLocator.Instance.GetService<IViewService>().Pop();
                RefreshStackList();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void OnPopDiscard(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                ServiceLocator.Instance.GetService<IViewService>().PopDiscard();
                RefreshStackList();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void OnShow(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                ServiceLocator.Instance.GetService<IViewService>().Show();
                RefreshStackList();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            RefreshViewResourceList();
            RefreshStackList();
        }

        private void RefreshViewResourceList()
        {
            var control = (ComboBox)m_viewResourceToolStripComboBox.Control;
            control.BeginUpdate();
            control.Items.Clear();
            foreach (var resource in ServiceLocator.Instance.GetService<IResourceBundleService>().GetResources("view"))
                control.Items.Add((object)resource);
            control.EndUpdate();
        }

        private void RefreshStackList()
        {
            m_stackListView.BeginUpdate();
            m_stackListView.Items.Clear();
            foreach (var viewFrameInstance in ServiceLocator.Instance.GetService<IViewService>().Stack)
            {
                var listViewItem = new ListViewItem(viewFrameInstance?.ViewFrame.ViewName)
                {
                    Tag = (object)viewFrameInstance
                };
                var viewFrame = viewFrameInstance.ViewFrame as IViewFrame;
                listViewItem.SubItems.Add(viewFrame != null
                    ? viewFrame.SceneName
                    : viewFrameInstance?.ViewFrame?.Scene?.Name);
                listViewItem.SubItems.Add(viewFrameInstance?.ViewFrame?.Clear.ToString());
                var subItems1 = listViewItem.SubItems;
                int num;
                string text1;
                if (viewFrame == null)
                {
                    text1 = (string)null;
                }
                else
                {
                    num = viewFrame.Width;
                    text1 = num.ToString();
                }

                subItems1.Add(text1);
                var subItems2 = listViewItem.SubItems;
                string text2;
                if (viewFrame == null)
                {
                    text2 = (string)null;
                }
                else
                {
                    num = viewFrame.Height;
                    text2 = num.ToString();
                }

                subItems2.Add(text2);
                m_stackListView.Items.Add(listViewItem);
            }

            m_stackListView.EndUpdate();
            m_popToolStripButton.Enabled = m_stackListView.Items.Count > 0;
            m_popDiscardToolStripButton.Enabled = m_stackListView.Items.Count > 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            var componentResourceManager = new ComponentResourceManager(typeof(ViewStackToolWindow));
            m_stackListView = new ListView();
            m_stackViewNameColumnHeader = new ColumnHeader();
            m_sceneNameColumnHeader = new ColumnHeader();
            m_clearColumnHeader = new ColumnHeader();
            m_widthColumnHeader = new ColumnHeader();
            m_heightColumnHeader = new ColumnHeader();
            m_toolStrip = new ToolStrip();
            toolStripSeparator1 = new ToolStripSeparator();
            m_viewToolStripLabel = new ToolStripLabel();
            m_viewResourceToolStripComboBox = new ToolStripComboBox();
            m_refreshToolStripButton = new ToolStripButton();
            m_pushToolStripButton = new ToolStripButton();
            m_popToolStripButton = new ToolStripButton();
            m_popDiscardToolStripButton = new ToolStripButton();
            m_showToolStripButton = new ToolStripButton();
            m_toolStrip.SuspendLayout();
            SuspendLayout();
            m_stackListView.Columns.AddRange(new ColumnHeader[5]
            {
                m_stackViewNameColumnHeader,
                m_sceneNameColumnHeader,
                m_clearColumnHeader,
                m_widthColumnHeader,
                m_heightColumnHeader
            });
            m_stackListView.Dock = DockStyle.Fill;
            m_stackListView.FullRowSelect = true;
            m_stackListView.Location = new Point(0, 25);
            m_stackListView.MultiSelect = false;
            m_stackListView.Name = "m_stackListView";
            m_stackListView.Size = new Size(787, 294);
            m_stackListView.TabIndex = 4;
            m_stackListView.UseCompatibleStateImageBehavior = false;
            m_stackListView.View = View.Details;
            m_stackViewNameColumnHeader.Text = "View Name";
            m_stackViewNameColumnHeader.Width = 125;
            m_sceneNameColumnHeader.Text = "Scene Name";
            m_sceneNameColumnHeader.Width = 100;
            m_clearColumnHeader.Text = "Clear?";
            m_clearColumnHeader.Width = 45;
            m_widthColumnHeader.Text = "Width";
            m_widthColumnHeader.TextAlign = HorizontalAlignment.Right;
            m_heightColumnHeader.Text = "Height";
            m_heightColumnHeader.TextAlign = HorizontalAlignment.Right;
            m_toolStrip.CanOverflow = false;
            m_toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            m_toolStrip.Items.AddRange(new ToolStripItem[8]
            {
                (ToolStripItem)m_refreshToolStripButton,
                (ToolStripItem)toolStripSeparator1,
                (ToolStripItem)m_viewToolStripLabel,
                (ToolStripItem)m_viewResourceToolStripComboBox,
                (ToolStripItem)m_pushToolStripButton,
                (ToolStripItem)m_popToolStripButton,
                (ToolStripItem)m_popDiscardToolStripButton,
                (ToolStripItem)m_showToolStripButton
            });
            m_toolStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            m_toolStrip.Location = new Point(0, 0);
            m_toolStrip.Name = "m_toolStrip";
            m_toolStrip.Size = new Size(787, 25);
            m_toolStrip.TabIndex = 5;
            m_toolStrip.Text = "toolStrip1";
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            m_viewToolStripLabel.AutoSize = false;
            m_viewToolStripLabel.Name = "m_viewToolStripLabel";
            m_viewToolStripLabel.Size = new Size(85, 20);
            m_viewToolStripLabel.Text = "View Resource:";
            m_viewResourceToolStripComboBox.AutoSize = false;
            m_viewResourceToolStripComboBox.DropDownHeight = 250;
            m_viewResourceToolStripComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            m_viewResourceToolStripComboBox.DropDownWidth = 160;
            m_viewResourceToolStripComboBox.IntegralHeight = false;
            m_viewResourceToolStripComboBox.MaxDropDownItems = 25;
            m_viewResourceToolStripComboBox.Name = "m_viewResourceToolStripComboBox";
            m_viewResourceToolStripComboBox.Size = new Size(200, 21);
            m_refreshToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_refreshToolStripButton.Image = (Image)Properties.Resources.Refresh;
            m_refreshToolStripButton.ImageTransparentColor = Color.Magenta;
            m_refreshToolStripButton.Name = "m_refreshToolStripButton";
            m_refreshToolStripButton.Size = new Size(23, 22);
            m_refreshToolStripButton.Click += new EventHandler(OnRefresh);
            m_pushToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_pushToolStripButton.Image = (Image)Properties.Resources.Plus;
            m_pushToolStripButton.ImageTransparentColor = Color.Magenta;
            m_pushToolStripButton.Name = "m_pushToolStripButton";
            m_pushToolStripButton.Size = new Size(23, 22);
            m_pushToolStripButton.Text = "toolStripButton1";
            m_pushToolStripButton.ToolTipText = "Push View Resource";
            m_pushToolStripButton.Click += new EventHandler(OnPush);
            m_popToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_popToolStripButton.Image = (Image)Properties.Resources.Minus;
            m_popToolStripButton.ImageTransparentColor = Color.Magenta;
            m_popToolStripButton.Name = "m_popToolStripButton";
            m_popToolStripButton.Size = new Size(23, 22);
            m_popToolStripButton.Text = "toolStripButton1";
            m_popToolStripButton.ToolTipText = "Pop TOS";
            m_popToolStripButton.Click += new EventHandler(OnPop);
            m_popDiscardToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_popDiscardToolStripButton.Image = (Image)Properties.Resources.Cross;
            m_popDiscardToolStripButton.ImageTransparentColor = Color.Magenta;
            m_popDiscardToolStripButton.Name = "m_popDiscardToolStripButton";
            m_popDiscardToolStripButton.Size = new Size(23, 22);
            m_popDiscardToolStripButton.Text = "toolStripButton1";
            m_popDiscardToolStripButton.ToolTipText = "Pop and Discard TOS";
            m_popDiscardToolStripButton.Click += new EventHandler(OnPopDiscard);
            m_showToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_showToolStripButton.Image = (Image)Properties.Resources.Views;
            m_showToolStripButton.ImageTransparentColor = Color.Magenta;
            m_showToolStripButton.Name = "m_showToolStripButton";
            m_showToolStripButton.Size = new Size(23, 22);
            m_showToolStripButton.Text = "toolStripButton1";
            m_showToolStripButton.ToolTipText = "Show TOS";
            m_showToolStripButton.Click += new EventHandler(OnShow);
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(787, 319);
            Controls.Add((Control)m_stackListView);
            Controls.Add((Control)m_toolStrip);
            DockAreas = DockAreas.Float | DockAreas.DockBottom;
            Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            HideOnClose = true;
            Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            Name = nameof(ViewStackToolWindow);
            ShowHint = DockState.DockBottom;
            StartPosition = FormStartPosition.CenterParent;
            Text = "View Stack";
            Load += new EventHandler(OnLoad);
            m_toolStrip.ResumeLayout(false);
            m_toolStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}