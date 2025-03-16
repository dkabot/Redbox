using Redbox.REDS.Framework;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class ResourceTypeTreeView : UserControl
    {
        private IContainer components;
        private BaseTreeView m_treeView;
        private ImageList m_imageList;

        public ResourceTypeTreeView()
        {
            InitializeComponent();
        }

        public void RefreshTree()
        {
        }

        public IResourceBundle Bundle { get; set; }

        public IResourceType SelectedType { get; set; }

        private void OnAfterSelect(object sender, TreeViewEventArgs e)
        {
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
            m_treeView = new BaseTreeView();
            m_imageList = new ImageList(components);
            SuspendLayout();
            m_treeView.Dock = DockStyle.Fill;
            m_treeView.Location = new Point(0, 0);
            m_treeView.Name = "m_treeView";
            m_treeView.Size = new Size(281, 357);
            m_treeView.TabIndex = 0;
            m_treeView.AfterSelect += new TreeViewEventHandler(OnAfterSelect);
            m_imageList.ColorDepth = ColorDepth.Depth32Bit;
            m_imageList.ImageSize = new Size(16, 16);
            m_imageList.TransparentColor = Color.Transparent;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add((Control)m_treeView);
            Name = nameof(ResourceTypeTreeView);
            Size = new Size(281, 357);
            ResumeLayout(false);
        }
    }
}