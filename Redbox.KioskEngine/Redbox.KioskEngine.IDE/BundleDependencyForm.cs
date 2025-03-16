using Redbox.REDS.Framework;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class BundleDependencyForm : Form
    {
        private IContainer components;
        private ListView m_bundlesListView;
        private ColumnHeader m_productNameColumn;
        private ColumnHeader m_productVersionColumn;
        private ColumnHeader m_pathColumn;

        public BundleDependencyForm()
        {
            InitializeComponent();
        }

        public IBundleSpecifier BundleSpecifier { get; set; }

        private void OnLoad(object sender, EventArgs e)
        {
            IManifestInfo manifestInfo;
            if (BundleSpecifier == null ||
                BundleSpecifier.Instance.GetManifest(BundleSpecifier.Instance.CreateFilter(), out manifestInfo) ==
                null || manifestInfo == null)
                return;
            foreach (var require in manifestInfo.Requires)
            {
                var listViewItem = new ListViewItem(require.Name);
                var text = require.Version.ToString();
                if (text.StartsWith("9"))
                    listViewItem.SubItems.Add("*");
                else
                    listViewItem.SubItems.Add(text);
                if (require.Instance != null)
                    listViewItem.SubItems.Add(require.Instance.Storage.BundlePath);
                else
                    listViewItem.SubItems.Add("N/A");
                listViewItem.Tag = (object)require.Instance;
                m_bundlesListView.Items.Add(listViewItem);
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
            m_bundlesListView = new ListView();
            m_productNameColumn = new ColumnHeader();
            m_productVersionColumn = new ColumnHeader();
            m_pathColumn = new ColumnHeader();
            SuspendLayout();
            m_bundlesListView.Columns.AddRange(new ColumnHeader[3]
            {
                m_productNameColumn,
                m_productVersionColumn,
                m_pathColumn
            });
            m_bundlesListView.Dock = DockStyle.Fill;
            m_bundlesListView.FullRowSelect = true;
            m_bundlesListView.HideSelection = false;
            m_bundlesListView.Location = new Point(0, 0);
            m_bundlesListView.MultiSelect = false;
            m_bundlesListView.Name = "m_bundlesListView";
            m_bundlesListView.Size = new Size(748, 287);
            m_bundlesListView.TabIndex = 5;
            m_bundlesListView.UseCompatibleStateImageBehavior = false;
            m_bundlesListView.View = View.Details;
            m_productNameColumn.Text = "Product Name";
            m_productNameColumn.Width = 175;
            m_productVersionColumn.Text = "Version";
            m_pathColumn.Text = "Path";
            m_pathColumn.Width = 410;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(748, 287);
            Controls.Add((Control)m_bundlesListView);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = nameof(BundleDependencyForm);
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Bundle Dependencies";
            Load += new EventHandler(OnLoad);
            ResumeLayout(false);
        }
    }
}