using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE.Properties;
using Redbox.REDS.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class SelectBundleForm : Form
    {
        private IContainer components;
        private ListView m_appBundlesListView;
        private ColumnHeader m_productNameColumn;
        private ColumnHeader m_productVersionColumn;
        private ColumnHeader m_pathColumn;
        private ColumnHeader m_typeColumn;
        private ToolStrip m_toolStrip;
        private ToolStripButton m_refreshToolStripButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripLabel toolStripLabel1;
        private ToolStripTextBox m_searchPathToolStripTextBox;
        private ToolStripButton m_editSearchPathToolStripButton;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton m_breakOnStartToolStripButton;
        private ToolStripButton m_activateToolStripButton;
        private ToolStripButton m_unloadActiveBundleToolStripButton;
        private Splitter m_splitter;
        private Panel m_panel;
        private ListView m_sharedBundlesListView;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private Label label1;
        private ToolStripButton m_dependenciesToolStripButton;

        public SelectBundleForm()
        {
            InitializeComponent();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowDebugger => m_breakOnStartToolStripButton.Checked;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IResourceBundle SelectedBundle { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SearchPath
        {
            get => m_searchPathToolStripTextBox.Text;
            set
            {
                if (m_searchPathToolStripTextBox.Text == value)
                    return;
                m_searchPathToolStripTextBox.Text = value;
                RefreshList();
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Show();
            var service = ServiceLocator.Instance.GetService<IResourceBundleService>();
            if (service != null)
                m_unloadActiveBundleToolStripButton.Enabled = service.HasActiveBundle();
            if (m_appBundlesListView.Items.Count != 0)
                return;
            RefreshList();
        }

        private void OnItemActivate(object sender, EventArgs e)
        {
            if (m_appBundlesListView.SelectedItems.Count == 0)
                return;
            if (string.Compare(m_appBundlesListView.SelectedItems[0].SubItems[0].Text,
                    BundleType.Application.ToString(), true) != 0)
            {
                var num = (int)MessageBox.Show("Only bundles of type Application may be activated.",
                    ServiceLocator.Instance.GetService<IMacroService>()
                        .ExpandProperties("${ProductName} ${ProductVersion}"), MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_appBundlesListView.SelectedItems.Count > 0)
            {
                m_activateToolStripButton.Enabled = true;
                if (m_appBundlesListView.SelectedItems[0].Tag is IBundleSpecifier tag)
                    SelectedBundle = tag.Instance;
            }
            else
            {
                m_activateToolStripButton.Enabled = false;
                SelectedBundle = (IResourceBundle)null;
            }

            m_dependenciesToolStripButton.Enabled = m_appBundlesListView.SelectedItems.Count == 1;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_appBundlesListView.SelectedItems.Count == 0)
                SelectedBundle = (IResourceBundle)null;
            Cursor.Hide();
        }

        private void OnUnloadActiveBundle(object sender, EventArgs e)
        {
            var service = ServiceLocator.Instance.GetService<IResourceBundleService>();
            if (service == null)
                return;
            try
            {
                Cursor = Cursors.WaitCursor;
                service.Deactivate();
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            m_unloadActiveBundleToolStripButton.Enabled = service.HasActiveBundle();
        }

        private void OnEditSearchPath(object sender, EventArgs e)
        {
            var stringList =
                new List<string>((IEnumerable<string>)m_searchPathToolStripTextBox.Text.Split(";".ToCharArray(),
                    StringSplitOptions.RemoveEmptyEntries));
            var editSearchPathForm = new EditSearchPathForm()
            {
                SearchPath = stringList
            };
            if (editSearchPathForm.ShowDialog() != DialogResult.OK)
                return;
            m_searchPathToolStripTextBox.Text = editSearchPathForm.SearchPath.Join<string>(";");
            RefreshList();
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void OnBreakOnStartup(object sender, EventArgs e)
        {
            m_breakOnStartToolStripButton.Checked = !m_breakOnStartToolStripButton.Checked;
        }

        private void OnShowDependencies(object sender, EventArgs e)
        {
            var num = (int)new BundleDependencyForm()
            {
                BundleSpecifier = m_appBundlesListView.SelectedItems[0].Tag as IBundleSpecifier
            }.ShowDialog();
        }

        private void RefreshList()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                var service1 = ServiceLocator.Instance.GetService<IResourceBundleService>();
                service1.SearchPath =
                    m_searchPathToolStripTextBox.Text.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                service1.LoadBundles();
                m_appBundlesListView.Items.Clear();
                m_sharedBundlesListView.Items.Clear();
                foreach (var bundle in service1.Bundles)
                {
                    var listViewItem = new ListViewItem(bundle.Type.ToString());
                    listViewItem.SubItems.Add(bundle.Name);
                    listViewItem.SubItems.Add(bundle.Version.ToString());
                    listViewItem.SubItems.Add(bundle.Instance.Storage.BundlePath);
                    listViewItem.Tag = (object)bundle;
                    if (bundle.Type == BundleType.Application)
                        m_appBundlesListView.Items.Add(listViewItem);
                    else
                        m_sharedBundlesListView.Items.Add(listViewItem);
                }

                var service2 = ServiceLocator.Instance.GetService<IDebugService>();
                if (service2 == null)
                    return;
                m_breakOnStartToolStripButton.Enabled = service2.IsEnabled;
            }
            finally
            {
                Cursor = Cursors.Default;
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
            m_appBundlesListView = new ListView();
            m_typeColumn = new ColumnHeader();
            m_productNameColumn = new ColumnHeader();
            m_productVersionColumn = new ColumnHeader();
            m_pathColumn = new ColumnHeader();
            m_toolStrip = new ToolStrip();
            m_refreshToolStripButton = new ToolStripButton();
            m_activateToolStripButton = new ToolStripButton();
            m_unloadActiveBundleToolStripButton = new ToolStripButton();
            m_dependenciesToolStripButton = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripLabel1 = new ToolStripLabel();
            m_searchPathToolStripTextBox = new ToolStripTextBox();
            m_editSearchPathToolStripButton = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            m_breakOnStartToolStripButton = new ToolStripButton();
            m_splitter = new Splitter();
            m_panel = new Panel();
            m_sharedBundlesListView = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            label1 = new Label();
            m_toolStrip.SuspendLayout();
            m_panel.SuspendLayout();
            SuspendLayout();
            m_appBundlesListView.Columns.AddRange(new ColumnHeader[4]
            {
                m_typeColumn,
                m_productNameColumn,
                m_productVersionColumn,
                m_pathColumn
            });
            m_appBundlesListView.Dock = DockStyle.Top;
            m_appBundlesListView.FullRowSelect = true;
            m_appBundlesListView.HideSelection = false;
            m_appBundlesListView.Location = new Point(3, 26);
            m_appBundlesListView.MultiSelect = false;
            m_appBundlesListView.Name = "m_appBundlesListView";
            m_appBundlesListView.Size = new Size(777, 208);
            m_appBundlesListView.TabIndex = 4;
            m_appBundlesListView.UseCompatibleStateImageBehavior = false;
            m_appBundlesListView.View = View.Details;
            m_appBundlesListView.ItemActivate += new EventHandler(OnItemActivate);
            m_appBundlesListView.SelectedIndexChanged += new EventHandler(OnSelectedIndexChanged);
            m_typeColumn.Text = "Bundle Type";
            m_typeColumn.Width = 95;
            m_productNameColumn.Text = "Product Name";
            m_productNameColumn.Width = 175;
            m_productVersionColumn.Text = "Version";
            m_pathColumn.Text = "Path";
            m_pathColumn.Width = 410;
            m_toolStrip.AllowMerge = false;
            m_toolStrip.CanOverflow = false;
            m_toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            m_toolStrip.Items.AddRange(new ToolStripItem[10]
            {
                (ToolStripItem)m_refreshToolStripButton,
                (ToolStripItem)m_activateToolStripButton,
                (ToolStripItem)m_unloadActiveBundleToolStripButton,
                (ToolStripItem)m_dependenciesToolStripButton,
                (ToolStripItem)toolStripSeparator1,
                (ToolStripItem)toolStripLabel1,
                (ToolStripItem)m_searchPathToolStripTextBox,
                (ToolStripItem)m_editSearchPathToolStripButton,
                (ToolStripItem)toolStripSeparator2,
                (ToolStripItem)m_breakOnStartToolStripButton
            });
            m_toolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
            m_toolStrip.Location = new Point(3, 3);
            m_toolStrip.Name = "m_toolStrip";
            m_toolStrip.Size = new Size(777, 23);
            m_toolStrip.TabIndex = 5;
            m_refreshToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_refreshToolStripButton.Image = (Image)Resources.Refresh;
            m_refreshToolStripButton.ImageTransparentColor = Color.Magenta;
            m_refreshToolStripButton.Name = "m_refreshToolStripButton";
            m_refreshToolStripButton.Size = new Size(23, 20);
            m_refreshToolStripButton.Text = "Refresh";
            m_refreshToolStripButton.Click += new EventHandler(OnRefresh);
            m_activateToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_activateToolStripButton.Enabled = false;
            m_activateToolStripButton.Image = (Image)Resources.Execute;
            m_activateToolStripButton.ImageTransparentColor = Color.Magenta;
            m_activateToolStripButton.Name = "m_activateToolStripButton";
            m_activateToolStripButton.Size = new Size(23, 20);
            m_activateToolStripButton.Text = "Activate";
            m_activateToolStripButton.Click += new EventHandler(OnItemActivate);
            m_unloadActiveBundleToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_unloadActiveBundleToolStripButton.Enabled = false;
            m_unloadActiveBundleToolStripButton.Image = (Image)Resources.Clean;
            m_unloadActiveBundleToolStripButton.ImageTransparentColor = Color.Magenta;
            m_unloadActiveBundleToolStripButton.Name = "m_unloadActiveBundleToolStripButton";
            m_unloadActiveBundleToolStripButton.Size = new Size(23, 20);
            m_unloadActiveBundleToolStripButton.Text = "Unload Active Bundle";
            m_unloadActiveBundleToolStripButton.Click += new EventHandler(OnUnloadActiveBundle);
            m_dependenciesToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_dependenciesToolStripButton.Enabled = false;
            m_dependenciesToolStripButton.Image = (Image)Resources.Dependencies;
            m_dependenciesToolStripButton.ImageTransparentColor = Color.Magenta;
            m_dependenciesToolStripButton.Name = "m_dependenciesToolStripButton";
            m_dependenciesToolStripButton.Size = new Size(23, 20);
            m_dependenciesToolStripButton.Text = "Dependencies";
            m_dependenciesToolStripButton.Click += new EventHandler(OnShowDependencies);
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 23);
            toolStripLabel1.AutoSize = false;
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(69, 20);
            toolStripLabel1.Text = "Search Path:";
            m_searchPathToolStripTextBox.MaxLength = 1024;
            m_searchPathToolStripTextBox.Name = "m_searchPathToolStripTextBox";
            m_searchPathToolStripTextBox.Size = new Size(400, 21);
            m_editSearchPathToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_editSearchPathToolStripButton.Image = (Image)Resources.Properties;
            m_editSearchPathToolStripButton.ImageTransparentColor = Color.Magenta;
            m_editSearchPathToolStripButton.Name = "m_editSearchPathToolStripButton";
            m_editSearchPathToolStripButton.Size = new Size(23, 20);
            m_editSearchPathToolStripButton.Text = "Edit Search Path";
            m_editSearchPathToolStripButton.Click += new EventHandler(OnEditSearchPath);
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 23);
            m_breakOnStartToolStripButton.Image = (Image)Resources.Debugger;
            m_breakOnStartToolStripButton.ImageTransparentColor = Color.Magenta;
            m_breakOnStartToolStripButton.Name = "m_breakOnStartToolStripButton";
            m_breakOnStartToolStripButton.Size = new Size(114, 20);
            m_breakOnStartToolStripButton.Text = "Break On Activate";
            m_breakOnStartToolStripButton.Click += new EventHandler(OnBreakOnStartup);
            m_splitter.Dock = DockStyle.Top;
            m_splitter.Location = new Point(3, 234);
            m_splitter.Name = "m_splitter";
            m_splitter.Size = new Size(777, 3);
            m_splitter.TabIndex = 6;
            m_splitter.TabStop = false;
            m_panel.Controls.Add((Control)m_sharedBundlesListView);
            m_panel.Controls.Add((Control)label1);
            m_panel.Dock = DockStyle.Fill;
            m_panel.Location = new Point(3, 237);
            m_panel.Name = "m_panel";
            m_panel.Size = new Size(777, 234);
            m_panel.TabIndex = 7;
            m_sharedBundlesListView.Columns.AddRange(new ColumnHeader[4]
            {
                columnHeader1,
                columnHeader2,
                columnHeader3,
                columnHeader4
            });
            m_sharedBundlesListView.Dock = DockStyle.Fill;
            m_sharedBundlesListView.FullRowSelect = true;
            m_sharedBundlesListView.HideSelection = false;
            m_sharedBundlesListView.Location = new Point(0, 13);
            m_sharedBundlesListView.MultiSelect = false;
            m_sharedBundlesListView.Name = "m_sharedBundlesListView";
            m_sharedBundlesListView.Size = new Size(777, 221);
            m_sharedBundlesListView.TabIndex = 5;
            m_sharedBundlesListView.UseCompatibleStateImageBehavior = false;
            m_sharedBundlesListView.View = View.Details;
            columnHeader1.Text = "Bundle Type";
            columnHeader1.Width = 95;
            columnHeader2.Text = "Product Name";
            columnHeader2.Width = 175;
            columnHeader3.Text = "Version";
            columnHeader4.Text = "Path";
            columnHeader4.Width = 410;
            label1.AutoSize = true;
            label1.Dock = DockStyle.Top;
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(85, 13);
            label1.TabIndex = 0;
            label1.Text = "Shared Bundles:";
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(783, 474);
            Controls.Add((Control)m_panel);
            Controls.Add((Control)m_splitter);
            Controls.Add((Control)m_appBundlesListView);
            Controls.Add((Control)m_toolStrip);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(640, 480);
            Name = nameof(SelectBundleForm);
            Padding = new Padding(3);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Select Resource Bundle";
            Load += new EventHandler(OnLoad);
            FormClosing += new FormClosingEventHandler(OnFormClosing);
            m_toolStrip.ResumeLayout(false);
            m_toolStrip.PerformLayout();
            m_panel.ResumeLayout(false);
            m_panel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}