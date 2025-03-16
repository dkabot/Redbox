using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE
{
    public class ProjectExplorerToolWindow : ToolWindow
    {
        private IContainer components;
        private ToolStrip m_toolStrip;
        private ToolStripButton m_applyFilterToolStripButton;
        private BaseTreeView m_treeView;
        private ImageList m_imageList;
        private ToolStripButton m_refreshToolStripButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton m_exploreBundle;
        private ToolStripButton m_exploreKernel;
        private ContextMenuStrip m_contextMenu;
        private ToolStripMenuItem m_contextGenerateDoc;
        private ToolStripMenuItem asHTMLToFileToolStripMenuItem;
        private ToolStripMenuItem asHTMLToWindowToolStripMenuItem;
        private ToolStripMenuItem asXMLToFileToolStripMenuItem;
        private ToolStripMenuItem asXMLToWindowToolStripMenuItem;

        public ProjectExplorerToolWindow()
        {
            InitializeComponent();
        }

        public void RefreshTree()
        {
            if (m_exploreBundle.Checked)
            {
                InnerRefreshTree2();
            }
            else
            {
                if (!m_exploreKernel.Checked)
                    return;
                InnerRefreshTree3();
            }
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            RefreshTree();
        }

        private void OnAfterSelect(object sender, TreeViewEventArgs e)
        {
            ((IdeMainForm)ServiceLocator.Instance.GetService<IIdeService>()?.MainForm).PropertiesWindow
                .SetObject(e.Node.Tag);
        }

        private void OnDoubleClick(object sender, EventArgs e)
        {
            if (m_treeView.SelectedNode == null)
                return;
            var service = ServiceLocator.Instance.GetService<IIdeService>();
            if (service == null)
                return;
            var mainForm = (IdeMainForm)service.MainForm;
            if (!m_exploreBundle.Checked || !(m_treeView.SelectedNode.Tag is IResource tag))
                return;
            mainForm.OpenResourceEditor(tag);
        }

        private void OnApplyFilter(object sender, EventArgs e)
        {
            m_applyFilterToolStripButton.Checked = !m_applyFilterToolStripButton.Checked;
        }

        private void InnerRefreshTree2()
        {
            m_treeView.BeginUpdate();
            m_treeView.Nodes.Clear();
            if (!BundleService.HasActiveBundle())
                return;
            var resourceTypes = BundleService.GetResourceTypes();
            foreach (var bundle1 in BundleService.ActiveBundleSet.GetBundles())
            {
                var node1 = new TreeNode(string.Format("{0} ( {1} )", (object)bundle1.Name, (object)bundle1.Version),
                    (int)bundle1.Type, (int)bundle1.Type);
                var bundle2 = BundleService.GetBundle(bundle1);
                foreach (var keyValuePair in (IEnumerable<KeyValuePair<string, IResourceType>>)resourceTypes)
                {
                    var node2 = new TreeNode(keyValuePair.Key, 3, 3);
                    foreach (var allResource in bundle2.GetAllResources(keyValuePair.Key))
                    {
                        var str = allResource["program_name"] as string;
                        var text = string.IsNullOrEmpty(str)
                            ? allResource.Name
                            : string.Format("{0} '{1}'", (object)allResource.Name, (object)str);
                        var ignoringCase =
                            Enum<ResourceTypeIcons>.ParseIgnoringCase(keyValuePair.Key, ResourceTypeIcons.invalid);
                        var imageIndex = (int)ignoringCase;
                        var selectedImageIndex = (int)ignoringCase;
                        var node3 = new TreeNode(text, imageIndex, selectedImageIndex)
                        {
                            Tag = (object)allResource
                        };
                        node2.Nodes.Add(node3);
                    }

                    if (node2.Nodes.Count > 0)
                        node1.Nodes.Add(node2);
                }

                if (bundle1.Type == BundleType.Application)
                    m_treeView.Nodes.Insert(0, node1);
                else
                    m_treeView.Nodes.Add(node1);
            }

            m_treeView.EndUpdate();
            m_treeView.ExpandAll();
        }

        private void InnerRefreshTree3()
        {
            m_treeView.BeginUpdate();
            m_treeView.Nodes.Clear();
            foreach (var api in GetAPIDictionary())
            {
                var node1 = new TreeNode(api.Key, 17, 17);
                if (api.Value is SortedDictionary<string, object> sortedDictionary1)
                    foreach (var keyValuePair1 in sortedDictionary1)
                    {
                        var node2 = new TreeNode(keyValuePair1.Key, 18, 18);
                        if (keyValuePair1.Value is SortedDictionary<string, object> sortedDictionary)
                            foreach (var keyValuePair2 in sortedDictionary)
                                node2.Nodes.Add(new TreeNode(keyValuePair2.Key, 16, 16)
                                {
                                    Tag = keyValuePair2.Value
                                });
                        node1.Nodes.Add(node2);
                    }

                m_treeView.Nodes.Add(node1);
            }

            m_treeView.EndUpdate();
        }

        private SortedDictionary<string, object> GetAPIDictionary()
        {
            var apiDictionary = new SortedDictionary<string, object>();
            var service = ServiceLocator.Instance.GetService<IKernelFunctionRegistryService>();
            if (service != null)
                foreach (var kernelFunction in service.GetKernelFunctions())
                {
                    if (!apiDictionary.ContainsKey(kernelFunction.Extension))
                        apiDictionary.Add(kernelFunction.Extension, (object)new SortedDictionary<string, object>());
                    if (apiDictionary[kernelFunction.Extension] is SortedDictionary<string, object> sortedDictionary1)
                    {
                        if (!sortedDictionary1.ContainsKey(kernelFunction.Method.DeclaringType.FullName))
                            sortedDictionary1.Add(kernelFunction.Method.DeclaringType.FullName,
                                (object)new SortedDictionary<string, object>());
                        if (sortedDictionary1[kernelFunction.Method.DeclaringType.FullName] is
                                SortedDictionary<string, object> sortedDictionary &&
                            !sortedDictionary.ContainsKey(kernelFunction.Attributes[0].Name))
                            sortedDictionary.Add(kernelFunction.Attributes[0].Name, (object)kernelFunction);
                    }
                }

            return apiDictionary;
        }

        private void m_exploreBundle_Click(object sender, EventArgs e)
        {
            m_exploreKernel.Checked = false;
            m_contextGenerateDoc.Visible = false;
            InnerRefreshTree2();
        }

        private void m_exploreKernel_Click(object sender, EventArgs e)
        {
            m_exploreBundle.Checked = false;
            m_contextGenerateDoc.Visible = true;
            InnerRefreshTree3();
        }

        private void asHTMLToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var directoryName = Path.GetDirectoryName(Application.ExecutablePath);
            var apiDictionary = GetAPIDictionary();
            using (var streamWriter = new StreamWriter(directoryName + "\\APIOutput.html"))
            {
                streamWriter.WriteLine("<HTML>");
                streamWriter.WriteLine("<BODY>");
                foreach (var keyValuePair1 in apiDictionary)
                {
                    streamWriter.WriteLine(string.Format("<h1 style=\"font-family:Segoe UI\">{0}</h1>",
                        (object)keyValuePair1.Key));
                    if (keyValuePair1.Value is SortedDictionary<string, object> sortedDictionary1)
                        foreach (var keyValuePair2 in sortedDictionary1)
                        {
                            streamWriter.WriteLine(string.Format(
                                "<p style=\"font-style:bold;font-family:Segoe UI;color:blue;font-size:14;\">{0}</p>",
                                (object)keyValuePair2.Key));
                            if (keyValuePair2.Value is SortedDictionary<string, object> sortedDictionary)
                            {
                                streamWriter.WriteLine(
                                    "<table border=\"0\" width=\"100%\" style=\"font-family:Courier;font-size:10\">");
                                streamWriter.WriteLine("<col valign=\"top\">");
                                streamWriter.WriteLine("<col valign=\"top\">");
                                foreach (var keyValuePair3 in sortedDictionary)
                                    if (keyValuePair3.Value is KernelFunctionInfo kernelFunctionInfo)
                                    {
                                        streamWriter.WriteLine("<tr>");
                                        streamWriter.WriteLine("<td width=\"15%\">" +
                                                               kernelFunctionInfo.Attributes[0].Name + "</td>");
                                        var stringBuilder = new StringBuilder();
                                        stringBuilder.Append("<font color=\"blue\">internal " +
                                                             kernelFunctionInfo.Method.ReturnType.Name + " ");
                                        stringBuilder.Append("<font color=\"black\">" + kernelFunctionInfo.Method.Name +
                                                             "(");
                                        var parameters = kernelFunctionInfo.Method.GetParameters();
                                        var num = 0;
                                        if (parameters != null)
                                            foreach (var parameterInfo in parameters)
                                            {
                                                ++num;
                                                stringBuilder.Append("<font color=\"blue\">" +
                                                                     parameterInfo.ParameterType.Name +
                                                                     " <font color=\"black\">" + parameterInfo.Name);
                                                if (num < ((IEnumerable<ParameterInfo>)parameters)
                                                    .Count<ParameterInfo>())
                                                    stringBuilder.Append(", ");
                                            }

                                        stringBuilder.Append(")");
                                        streamWriter.WriteLine(
                                            "<td width=\"85%\">" + stringBuilder.ToString() + "</td>");
                                        streamWriter.WriteLine("</tr>");
                                    }

                                streamWriter.WriteLine("</table>");
                            }
                        }
                }

                streamWriter.WriteLine("</BODY>");
                streamWriter.WriteLine("</HTML>");
                streamWriter.Close();
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
            components = (IContainer)new Container();
            var componentResourceManager = new ComponentResourceManager(typeof(ProjectExplorerToolWindow));
            m_toolStrip = new ToolStrip();
            m_exploreBundle = new ToolStripButton();
            m_exploreKernel = new ToolStripButton();
            m_refreshToolStripButton = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            m_applyFilterToolStripButton = new ToolStripButton();
            m_treeView = new BaseTreeView();
            m_contextMenu = new ContextMenuStrip(components);
            m_contextGenerateDoc = new ToolStripMenuItem();
            m_imageList = new ImageList(components);
            asHTMLToFileToolStripMenuItem = new ToolStripMenuItem();
            asHTMLToWindowToolStripMenuItem = new ToolStripMenuItem();
            asXMLToFileToolStripMenuItem = new ToolStripMenuItem();
            asXMLToWindowToolStripMenuItem = new ToolStripMenuItem();
            m_toolStrip.SuspendLayout();
            m_contextMenu.SuspendLayout();
            SuspendLayout();
            m_toolStrip.Items.AddRange(new ToolStripItem[5]
            {
                (ToolStripItem)m_exploreBundle,
                (ToolStripItem)m_exploreKernel,
                (ToolStripItem)m_refreshToolStripButton,
                (ToolStripItem)toolStripSeparator1,
                (ToolStripItem)m_applyFilterToolStripButton
            });
            m_toolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
            m_toolStrip.Location = new Point(0, 0);
            m_toolStrip.Name = "m_toolStrip";
            m_toolStrip.Size = new Size(349, 23);
            m_toolStrip.TabIndex = 0;
            m_toolStrip.Text = "toolStrip1";
            m_exploreBundle.Checked = true;
            m_exploreBundle.CheckOnClick = true;
            m_exploreBundle.CheckState = CheckState.Checked;
            m_exploreBundle.Image = (Image)Properties.Resources.Cube;
            m_exploreBundle.ImageTransparentColor = Color.Magenta;
            m_exploreBundle.Name = "m_exploreBundle";
            m_exploreBundle.Size = new Size(64, 20);
            m_exploreBundle.Text = "Bundle";
            m_exploreBundle.Click += new EventHandler(m_exploreBundle_Click);
            m_exploreKernel.CheckOnClick = true;
            m_exploreKernel.Image = (Image)Properties.Resources.Sitemap;
            m_exploreKernel.ImageTransparentColor = Color.Magenta;
            m_exploreKernel.Name = "m_exploreKernel";
            m_exploreKernel.Size = new Size(60, 20);
            m_exploreKernel.Text = "Kernel";
            m_exploreKernel.Click += new EventHandler(m_exploreKernel_Click);
            m_refreshToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_refreshToolStripButton.Image = (Image)Properties.Resources.Refresh;
            m_refreshToolStripButton.ImageTransparentColor = Color.Magenta;
            m_refreshToolStripButton.Name = "m_refreshToolStripButton";
            m_refreshToolStripButton.Size = new Size(23, 20);
            m_refreshToolStripButton.ToolTipText = "Refresh";
            m_refreshToolStripButton.Click += new EventHandler(OnRefresh);
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 23);
            m_applyFilterToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            m_applyFilterToolStripButton.Image = (Image)Properties.Resources.Filter;
            m_applyFilterToolStripButton.ImageTransparentColor = Color.Magenta;
            m_applyFilterToolStripButton.Name = "m_applyFilterToolStripButton";
            m_applyFilterToolStripButton.Size = new Size(23, 20);
            m_applyFilterToolStripButton.ToolTipText = "Apply Bundle Filter";
            m_applyFilterToolStripButton.Click += new EventHandler(OnApplyFilter);
            m_treeView.ContextMenuStrip = m_contextMenu;
            m_treeView.Dock = DockStyle.Fill;
            m_treeView.FullRowSelect = true;
            m_treeView.HideSelection = false;
            m_treeView.ImageIndex = 0;
            m_treeView.ImageList = m_imageList;
            m_treeView.Location = new Point(0, 23);
            m_treeView.Name = "m_treeView";
            m_treeView.PathSeparator = "/";
            m_treeView.SelectedImageIndex = 0;
            m_treeView.Size = new Size(349, 597);
            m_treeView.TabIndex = 1;
            m_treeView.DoubleClick += new EventHandler(OnDoubleClick);
            m_treeView.AfterSelect += new TreeViewEventHandler(OnAfterSelect);
            m_contextMenu.Items.AddRange(new ToolStripItem[1]
            {
                (ToolStripItem)m_contextGenerateDoc
            });
            m_contextMenu.Name = "m_contextMenu";
            m_contextMenu.Size = new Size(208, 48);
            m_contextGenerateDoc.DropDownItems.AddRange(new ToolStripItem[4]
            {
                (ToolStripItem)asHTMLToFileToolStripMenuItem,
                (ToolStripItem)asHTMLToWindowToolStripMenuItem,
                (ToolStripItem)asXMLToFileToolStripMenuItem,
                (ToolStripItem)asXMLToWindowToolStripMenuItem
            });
            m_contextGenerateDoc.Name = "m_contextGenerateDoc";
            m_contextGenerateDoc.Size = new Size(207, 22);
            m_contextGenerateDoc.Text = "Generate Documentation";
            m_contextGenerateDoc.Visible = false;
            m_imageList.ImageStream = (ImageListStreamer)componentResourceManager.GetObject("m_imageList.ImageStream");
            m_imageList.TransparentColor = Color.Transparent;
            m_imageList.Images.SetKeyName(0, "Application-red.png");
            m_imageList.Images.SetKeyName(1, "Gear-red.png");
            m_imageList.Images.SetKeyName(2, "Dots-red.png");
            m_imageList.Images.SetKeyName(3, "Arrow3 Right.png");
            m_imageList.Images.SetKeyName(4, "Cancel.png");
            m_imageList.Images.SetKeyName(5, "Picture.png");
            m_imageList.Images.SetKeyName(6, "Movie.png");
            m_imageList.Images.SetKeyName(7, "Fbook.png");
            m_imageList.Images.SetKeyName(8, "Music.png");
            m_imageList.Images.SetKeyName(9, "Clipboard Copy.png");
            m_imageList.Images.SetKeyName(10, "Document.png");
            m_imageList.Images.SetKeyName(11, "Table.png");
            m_imageList.Images.SetKeyName(12, "Screen.png");
            m_imageList.Images.SetKeyName(13, "Key.png");
            m_imageList.Images.SetKeyName(14, "Cube.png");
            m_imageList.Images.SetKeyName(15, "Write.png");
            m_imageList.Images.SetKeyName(16, "Document2.png");
            m_imageList.Images.SetKeyName(17, "Dots Down.png");
            m_imageList.Images.SetKeyName(18, "Folder3.png");
            asHTMLToFileToolStripMenuItem.Name = "asHTMLToFileToolStripMenuItem";
            asHTMLToFileToolStripMenuItem.Size = new Size(182, 22);
            asHTMLToFileToolStripMenuItem.Text = "As HTML to file";
            asHTMLToFileToolStripMenuItem.Click += new EventHandler(asHTMLToFileToolStripMenuItem_Click);
            asHTMLToWindowToolStripMenuItem.Name = "asHTMLToWindowToolStripMenuItem";
            asHTMLToWindowToolStripMenuItem.Size = new Size(182, 22);
            asHTMLToWindowToolStripMenuItem.Text = "As HTML to window";
            asXMLToFileToolStripMenuItem.Name = "asXMLToFileToolStripMenuItem";
            asXMLToFileToolStripMenuItem.Size = new Size(182, 22);
            asXMLToFileToolStripMenuItem.Text = "As XML to file";
            asXMLToWindowToolStripMenuItem.Name = "asXMLToWindowToolStripMenuItem";
            asXMLToWindowToolStripMenuItem.Size = new Size(182, 22);
            asXMLToWindowToolStripMenuItem.Text = "As XML to window";
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(349, 620);
            ControlBox = false;
            Controls.Add((Control)m_treeView);
            Controls.Add((Control)m_toolStrip);
            Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            HideOnClose = true;
            Name = nameof(ProjectExplorerToolWindow);
            ShowHint = DockState.DockRight;
            Text = "Project Explorer";
            m_toolStrip.ResumeLayout(false);
            m_toolStrip.PerformLayout();
            m_contextMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private enum ResourceTypeIcons
        {
            invalid = 4,
            bitmap = 5,
            movie = 6,
            font = 7,
            sound = 8,
            template = 9,
            script = 10, // 0x0000000A
            table = 11, // 0x0000000B
            view = 12, // 0x0000000C
            publickey = 13, // 0x0000000D
            manifest = 14, // 0x0000000E
            stylesheet = 15 // 0x0000000F
        }

        private enum KernelExtensionIcons
        {
            function = 16, // 0x00000010
            extension = 17, // 0x00000011
            namespaces = 18 // 0x00000012
        }
    }
}