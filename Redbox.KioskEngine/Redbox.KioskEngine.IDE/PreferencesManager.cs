using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Redbox.KioskEngine.IDE
{
    public class PreferencesManager : Form
    {
        private bool m_isActivated;
        private ElementHost _elementHost;
        private IContainer components;
        private TreeView m_treeView;
        private Button m_closeButton;
        private GroupBox m_groupBox;
        private Panel m_pagePanel;
        private ImageList m_imageList;
        private Button buttonOpenNewPreferenceManager;

        public PreferencesManager()
        {
            InitializeComponent();
        }

        private void OnClose(object sender, EventArgs e)
        {
            if (!DeactivateCurrentPage() && System.Windows.Forms.MessageBox.Show(
                    "There are unsaved changes because of errors.\n\nWould you like to correct them or close the window and lose your changes?",
                    ServiceLocator.Instance.GetService<IMacroService>()
                        .ExpandProperties("${ProductName} ${ProductVersion}"), MessageBoxButtons.YesNo,
                    MessageBoxIcon.Asterisk) == DialogResult.Yes)
                return;
            DialogResult = DialogResult.OK;
            if (_elementHost != null)
                _elementHost.Child = (UIElement)null;
            Close();
        }

        private void OnAfterSelect(object sender, TreeViewEventArgs e)
        {
            e.Node.SelectedImageIndex = e.Node.ImageIndex;
            ServiceLocator.Instance.GetService<IUserSettingsStore>().SetValue<string>("Shell\\Preferences",
                "CurrentSelection", m_treeView.SelectedNode.FullPath);
            m_pagePanel.Controls.Clear();
            if (!(e.Node.Tag is IPreferencePage tag) || tag.Host == null)
                return;
            tag.RaiseActivate();
            if (!(tag.Host is Control control))
            {
                if (!(tag.Host is System.Windows.Controls.UserControl host))
                    return;
                if (_elementHost == null)
                    _elementHost = new ElementHost();
                else
                    _elementHost.Child = (UIElement)null;
                _elementHost.Child = (UIElement)host;
                control = (Control)_elementHost;
            }

            tag.Host.LoadValues(tag);
            control.Dock = DockStyle.Fill;
            m_pagePanel.Controls.Add(control);
        }

        private void OnBeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (DeactivateCurrentPage())
                return;
            e.Cancel = true;
        }

        private void OnAfterCollapse(object sender, TreeViewEventArgs e)
        {
            e.Node.ImageIndex = 0;
            e.Node.SelectedImageIndex = e.Node.ImageIndex;
        }

        private void OnAfterExpand(object sender, TreeViewEventArgs e)
        {
            e.Node.ImageIndex = 1;
            e.Node.SelectedImageIndex = e.Node.ImageIndex;
        }

        private void OnActivated(object sender, EventArgs e)
        {
            if (m_isActivated)
                return;
            m_isActivated = true;
            RefreshTree();
            var service = ServiceLocator.Instance.GetService<IUserSettingsStore>();
            if (m_treeView.SelectedNode != null)
                service.SetValue<string>("Shell\\Preferences", "CurrentSelection", m_treeView.SelectedNode.FullPath);
            var str = service.GetValue<string>("Shell\\Preferences", "CurrentSelection");
            if (str == null)
                return;
            var strArray = str.Split(m_treeView.PathSeparator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var treeNode = (TreeNode)null;
            foreach (var key in strArray)
            {
                var treeNodeArray =
                    treeNode == null ? m_treeView.Nodes.Find(key, false) : treeNode.Nodes.Find(key, false);
                if (treeNodeArray.Length != 0)
                {
                    treeNode = treeNodeArray[0];
                    treeNode.Expand();
                }
                else
                {
                    break;
                }
            }

            m_treeView.SelectedNode = treeNode;
        }

        private void RefreshTree()
        {
            var service = ServiceLocator.Instance.GetService<IPreferenceService>();
            if (service == null)
                return;
            m_treeView.SuspendLayout();
            m_treeView.BeginUpdate();
            foreach (var preferencePage in service.PreferencePages)
            {
                var stringList = new List<string>();
                if (string.IsNullOrEmpty(preferencePage.DisplayPath))
                    stringList.Add(preferencePage.Name);
                else
                    stringList.AddRange((IEnumerable<string>)preferencePage.DisplayPath.Split("/".ToCharArray(),
                        StringSplitOptions.RemoveEmptyEntries));
                var treeNode = (TreeNode)null;
                foreach (var str in stringList)
                {
                    var treeNodeArray = treeNode == null
                        ? m_treeView.Nodes.Find(str, false)
                        : treeNode.Nodes.Find(str, false);
                    if (treeNodeArray.Length != 0)
                    {
                        treeNode = treeNodeArray[0];
                    }
                    else
                    {
                        treeNode = treeNode == null ? m_treeView.Nodes.Add(str, str) : treeNode.Nodes.Add(str, str);
                        if (str == stringList[stringList.Count - 1])
                            treeNode.Tag = (object)preferencePage;
                        treeNode.ImageIndex = 2;
                        if (treeNode.Parent != null && treeNode.Parent.Nodes.Count > 0)
                            treeNode.Parent.ImageIndex = 0;
                    }
                }
            }

            m_treeView.EndUpdate();
            m_treeView.ResumeLayout();
        }

        private bool DeactivateCurrentPage()
        {
            if (m_treeView.SelectedNode == null || !(m_treeView.SelectedNode.Tag is IPreferencePage tag) ||
                tag.Host == null)
                return true;
            if (!tag.Host.SaveValues(tag))
                return false;
            tag.RaiseDeactivate();
            return true;
        }

        private void PreferencesManager_FormClosed(object sender, FormClosedEventArgs e)
        {
            OnClose((object)this, (EventArgs)null);
        }

        private void buttonOpenNewPreferenceManager_Click(object sender, EventArgs e)
        {
            var path2 = "Redbox.KioskEngine.PreferenceManager.exe";
            var str = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), path2);
            if (!File.Exists(str))
                return;
            Process.Start(str);
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
            var componentResourceManager = new ComponentResourceManager(typeof(PreferencesManager));
            m_treeView = new TreeView();
            m_imageList = new ImageList(components);
            m_closeButton = new Button();
            m_groupBox = new GroupBox();
            m_pagePanel = new Panel();
            buttonOpenNewPreferenceManager = new Button();
            SuspendLayout();
            m_treeView.FullRowSelect = true;
            m_treeView.HideSelection = false;
            m_treeView.ImageIndex = 0;
            m_treeView.ImageList = m_imageList;
            m_treeView.ItemHeight = 19;
            m_treeView.Location = new Point(16, 15);
            m_treeView.Margin = new Padding(4);
            m_treeView.Name = "m_treeView";
            m_treeView.PathSeparator = "/";
            m_treeView.SelectedImageIndex = 0;
            m_treeView.Size = new Size(295, 632);
            m_treeView.TabIndex = 0;
            m_treeView.AfterCollapse += new TreeViewEventHandler(OnAfterCollapse);
            m_treeView.AfterExpand += new TreeViewEventHandler(OnAfterExpand);
            m_treeView.BeforeSelect += new TreeViewCancelEventHandler(OnBeforeSelect);
            m_treeView.AfterSelect += new TreeViewEventHandler(OnAfterSelect);
            m_imageList.ImageStream = (ImageListStreamer)componentResourceManager.GetObject("m_imageList.ImageStream");
            m_imageList.TransparentColor = Color.Transparent;
            m_imageList.Images.SetKeyName(0, "folder-closed.png");
            m_imageList.Images.SetKeyName(1, "folder.png");
            m_imageList.Images.SetKeyName(2, "settings.png");
            m_closeButton.Location = new Point(943, 665);
            m_closeButton.Margin = new Padding(4);
            m_closeButton.Name = "m_closeButton";
            m_closeButton.Size = new Size(100, 28);
            m_closeButton.TabIndex = 1;
            m_closeButton.Text = "Close";
            m_closeButton.UseVisualStyleBackColor = true;
            m_closeButton.Click += new EventHandler(OnClose);
            m_groupBox.Location = new Point(320, 645);
            m_groupBox.Margin = new Padding(4);
            m_groupBox.Name = "m_groupBox";
            m_groupBox.Padding = new Padding(4);
            m_groupBox.Size = new Size(724, 2);
            m_groupBox.TabIndex = 3;
            m_groupBox.TabStop = false;
            m_pagePanel.Location = new Point(320, 15);
            m_pagePanel.Margin = new Padding(4);
            m_pagePanel.Name = "m_pagePanel";
            m_pagePanel.Size = new Size(723, 623);
            m_pagePanel.TabIndex = 4;
            buttonOpenNewPreferenceManager.Location = new Point(27, 665);
            buttonOpenNewPreferenceManager.Margin = new Padding(4);
            buttonOpenNewPreferenceManager.Name = "buttonOpenNewPreferenceManager";
            buttonOpenNewPreferenceManager.Size = new Size(235, 28);
            buttonOpenNewPreferenceManager.TabIndex = 5;
            buttonOpenNewPreferenceManager.Text = "Open new Preference Manager";
            buttonOpenNewPreferenceManager.UseVisualStyleBackColor = true;
            buttonOpenNewPreferenceManager.Click += new EventHandler(buttonOpenNewPreferenceManager_Click);
            AcceptButton = (IButtonControl)m_closeButton;
            AutoScaleDimensions = new SizeF(8f, 16f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1059, 708);
            Controls.Add((Control)buttonOpenNewPreferenceManager);
            Controls.Add((Control)m_pagePanel);
            Controls.Add((Control)m_groupBox);
            Controls.Add((Control)m_closeButton);
            Controls.Add((Control)m_treeView);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            HelpButton = true;
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = nameof(PreferencesManager);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Engine Preferences & Configuration";
            Activated += new EventHandler(OnActivated);
            FormClosed += new FormClosedEventHandler(PreferencesManager_FormClosed);
            ResumeLayout(false);
        }
    }
}