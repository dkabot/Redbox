using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using System.Xml;
using WeifenLuo.WinFormsUI.Docking;

namespace Redbox.KioskEngine.IDE
{
    public class ResourceEditorWindow : DockContent
    {
        private IResource m_resource;
        private IContainer components;
        private ToolStrip m_toolStrip;
        private Panel m_panel;

        public ResourceEditorWindow()
        {
            InitializeComponent();
        }

        public IResource Resource
        {
            get => m_resource;
            set
            {
                m_resource = value;
                Text = m_resource.Name;
                SetAspects();
            }
        }

        public void SetDebugState(int lineNumber, string error)
        {
            var key = "text_editor_" + m_resource.Name;
            if (!m_panel.Controls.ContainsKey(key))
                return;
            ((TextEditor)m_panel.Controls[key]).SetCurrentDebugLine(lineNumber, error);
        }

        private void SetAspects()
        {
            foreach (var aspectType in m_resource.Type.AspectTypes)
            {
                var eachAspect = aspectType;
                var toolStripButton1 = new ToolStripButton(eachAspect["label"] ?? eachAspect.Name);
                toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                toolStripButton1.Tag = (object)eachAspect;
                var toolStripButton = toolStripButton1;
                toolStripButton.Click += (EventHandler)((s, e) =>
                {
                    ResetToolStripButtons();
                    toolStripButton.Checked = true;
                    m_panel.SuspendLayout();
                    m_panel.Controls.Clear();
                    if (eachAspect["editor_type"] == "text")
                    {
                        var textEditor = new TextEditor();
                        textEditor.Name = "text_editor_" + m_resource.Name;
                        if (eachAspect["syntax_schema"] != null)
                            textEditor.SetSyntaxResource(eachAspect["syntax_schema"]);
                        textEditor.SetDocumentText(Resource.GetAspect(eachAspect.Name).GetContent() as string);
                        textEditor.Dock = DockStyle.Fill;
                        m_panel.Controls.Add((Control)textEditor);
                    }
                    else if (eachAspect["editor_type"] == "table")
                    {
                        var splitContainer = new SplitContainer()
                        {
                            Dock = DockStyle.Fill,
                            Location = new Point(0, 0),
                            Name = "splitContainerView",
                            BorderStyle = BorderStyle.Fixed3D,
                            Orientation = Orientation.Horizontal
                        };
                        var textEditor = new TextEditor();
                        textEditor.SetSyntaxResource("XML");
                        textEditor.Name = string.Format("text_editor_{0}", (object)m_resource.Name);
                        if (Resource.GetAspect(eachAspect.Name).GetContent() is XmlLinkedNode content2)
                        {
                            var w1 = new StringWriter(new StringBuilder());
                            var w2 = new XmlTextWriter((TextWriter)w1)
                            {
                                Formatting = Formatting.Indented
                            };
                            content2.OwnerDocument.Save((XmlWriter)w2);
                            w2.Close();
                            textEditor.SetDocumentText(w1.ToString());
                        }

                        textEditor.Dock = DockStyle.Fill;
                        splitContainer.Panel2.Controls.Add((Control)textEditor);
                        m_panel.Controls.Add((Control)splitContainer);
                    }
                    else if (eachAspect["editor_type"] == "view")
                    {
                        var xmlNode = (Resource.GetAspect(eachAspect.Name).GetContent() as XmlLinkedNode)
                            .SelectSingleNode("scene");
                        var str = xmlNode.Attributes["name"].Value;
                        ServiceLocator.Instance.GetService<IRenderingService>();
                        var service = ServiceLocator.Instance.GetService<IViewService>();
                        service.Push(m_resource.Name);
                        var x = 0;
                        var y = 0;
                        int height;
                        int width;
                        IViewFrame viewFrame2 = null;

                        if (service.GetViewFrame(m_resource.Name) is IViewFrame vf)
                        {
                            viewFrame2 = vf;
                            var viewWindow = viewFrame2.ViewWindow;
                            if (viewWindow.HasValue)
                            {
                                var rectangle = viewWindow.Value;
                                height = rectangle.Height;
                                width = rectangle.Width;
                                x = rectangle.X;
                                y = rectangle.Y;
                                goto label_13;
                            }
                        }

                        height = int.Parse(xmlNode.Attributes["height"].Value);
                        width = int.Parse(xmlNode.Attributes["width"].Value);

                        label_13:
                        var targetRectangle = new Rectangle(0, 0, width + x, height + y);

                        if (viewFrame2 != null)
                        {
                            var scene = viewFrame2.Scene;
                            service.ShowFrameInDebug();

                            var pictureBox = new System.Windows.Forms.PictureBox();
                            var bitmap = new Bitmap(width + x, height + y);
                            using (var bufferedGraphics =
                                   BufferedGraphicsManager.Current.Allocate(Graphics.FromImage((Image)bitmap),
                                       targetRectangle))
                            {
                                scene.MakeDirty((Rectangle[])null);
                                scene.Render(bufferedGraphics.Graphics, out var _);
                                bufferedGraphics.Render();
                            }

                            if (x + y > 0)
                            {
                                var rect = new Rectangle(x, y, width, height);
                                pictureBox.Image = (Image)bitmap.Clone(rect, bitmap.PixelFormat);
                            }
                            else
                            {
                                pictureBox.Image = (Image)bitmap;
                            }

                            pictureBox.Dock = DockStyle.Fill;
                            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                            m_panel.Controls.Add((Control)pictureBox);
                            service.PopDiscard();
                            service.Show();
                        }
                        else
                        {
                            var errorLabel = new Label();
                            errorLabel.Text = "Could not create view frame";
                            errorLabel.Dock = DockStyle.Fill;
                            m_panel.Controls.Add(errorLabel);

                            service.PopDiscard();
                            service.Show();
                        }
                    }
                    else if (!(eachAspect["editor_type"] == "schema"))
                    {
                        if (m_resource.Name == "manifest")
                        {
                            var textEditor = new TextEditor();
                            textEditor.SetSyntaxResource("XML");
                            var service = ServiceLocator.Instance.GetService<IResourceBundleService>();
                            var obj = m_resource["product_name"];
                            textEditor.Name = "text_editor_" + obj?.ToString() + m_resource.Name;
                            foreach (var bundle1 in service.ActiveBundleSet.GetBundles())
                                if (bundle1.Name == (string)obj)
                                {
                                    var bundle2 = service.GetBundle(bundle1);
                                    var entry = bundle2.Storage.GetEntry("manifest.resource");
                                    var text = Encoding.UTF8.GetString(bundle2.Storage.GetEntryContent(entry));
                                    textEditor.SetDocumentText(text);
                                    break;
                                }

                            textEditor.Dock = DockStyle.Fill;
                            m_panel.Controls.Add((Control)textEditor);
                        }
                        else if (eachAspect["editor_type"] == "bitmap")
                        {
                            var pictureBox = new System.Windows.Forms.PictureBox();
                            var memoryStream = new MemoryStream((byte[])Resource.GetAspect("content").GetContent());
                            pictureBox.Image = Image.FromStream((Stream)memoryStream);
                            pictureBox.Dock = DockStyle.Fill;
                            m_panel.Controls.Add((Control)pictureBox);
                        }
                    }

                    m_panel.ResumeLayout();
                });
                m_toolStrip.Items.Add((ToolStripItem)toolStripButton);
            }

            if (m_toolStrip.Items.Count <= 0)
                return;
            m_toolStrip.Items[0].PerformClick();
        }

        private void ResetToolStripButtons()
        {
            foreach (ToolStripButton toolStripButton in (ArrangedElementCollection)m_toolStrip.Items)
                toolStripButton.Checked = false;
        }

        public static string RemoveTroublesomeCharacters(string inString)
        {
            if (inString == null)
                return (string)null;
            var stringBuilder = new StringBuilder();
            for (var index = 0; index < inString.Length; ++index)
            {
                var ch = inString[index];
                if ((ch < 'ý' && ch > '\u001F') || ch == '\t' || ch == '\n' || ch == '\r')
                    stringBuilder.Append(ch);
            }

            return stringBuilder.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_toolStrip = new ToolStrip();
            m_panel = new Panel();
            SuspendLayout();
            m_toolStrip.Dock = DockStyle.Bottom;
            m_toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            m_toolStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            m_toolStrip.Location = new Point(0, 485);
            m_toolStrip.Name = "m_toolStrip";
            m_toolStrip.ShowItemToolTips = false;
            m_toolStrip.Size = new Size(723, 25);
            m_toolStrip.Stretch = true;
            m_toolStrip.TabIndex = 0;
            m_panel.AutoSize = true;
            m_panel.Dock = DockStyle.Fill;
            m_panel.Location = new Point(0, 0);
            m_panel.Name = "m_panel";
            m_panel.Size = new Size(1523, 1285);
            m_panel.TabIndex = 1;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(923, 810);
            Controls.Add((Control)m_panel);
            Controls.Add((Control)m_toolStrip);
            Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            Name = nameof(ResourceEditorWindow);
            Text = "Resource Editor";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}