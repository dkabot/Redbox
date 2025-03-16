using Alsing.SourceCode;
using Alsing.Windows.Forms;
using Skybound.VisualTips;
using Skybound.VisualTips.Rendering;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class TextEditor : UserControl
    {
        private IContainer components;
        private SyntaxBoxControl m_syntaxBoxControl;
        private VisualTipProvider m_visualTipProvider;
        private SyntaxDocument m_syntaxDocument;
        private ImageList m_gutterImageList;

        public TextEditor()
        {
            InitializeComponent();
            m_syntaxBoxControl.GutterIcons.Images.Add(m_gutterImageList.Images[0]);
        }

        public void SetDocumentText(string text)
        {
            m_syntaxDocument.Text = text;
        }

        public void SetSyntaxResource(string name)
        {
            m_syntaxDocument.SetSyntaxFromEmbeddedResource(typeof(DebuggerForm).Assembly,
                string.Format("Redbox.KioskEngine.IDE.{0}.syn", (object)name));
        }

        private void ShowCurrentLineSymbol()
        {
            m_syntaxBoxControl.Document[m_syntaxBoxControl.Caret.Position.Y].Images.Add(2);
        }

        private void ClearCurrentLineSymbol()
        {
            m_syntaxBoxControl.Document[m_syntaxBoxControl.Caret.Position.Y].Images.Remove(2);
        }

        public void SetCurrentDebugLine(int lineNumber, string error)
        {
            if (lineNumber < m_syntaxDocument.Lines.Length)
            {
                ClearCurrentLineSymbol();
                var RowIndex1 = lineNumber - 1;
                if (RowIndex1 < 0)
                    RowIndex1 = 0;
                m_syntaxBoxControl.GotoLine(RowIndex1);
                var RowIndex2 = RowIndex1 - 5;
                if (RowIndex2 < 0)
                    RowIndex2 = 0;
                m_syntaxBoxControl.ScrollIntoView(RowIndex2);
                ShowCurrentLineSymbol();
            }

            if (string.IsNullOrEmpty(error) || string.Compare(error, "breakpoint hit", true) == 0)
                return;
            var tip = new VisualTip(error, "REDS Engine Runtime Error")
            {
                FooterText = string.Format("The error occurred on or near line: {0}.", (object)lineNumber)
            };
            var screen = m_syntaxBoxControl.PointToScreen(m_syntaxBoxControl.Cursor.HotSpot);
            m_visualTipProvider.ShowTip(tip, new Rectangle(screen.X + 250, screen.Y, 100, 100));
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
            var componentResourceManager = new ComponentResourceManager(typeof(TextEditor));
            var tipOfficeRenderer = new VisualTipOfficeRenderer();
            m_syntaxBoxControl = new SyntaxBoxControl();
            m_visualTipProvider = new VisualTipProvider(components);
            m_syntaxDocument = new SyntaxDocument(components);
            m_gutterImageList = new ImageList(components);
            SuspendLayout();
            m_syntaxBoxControl.ActiveView = ActiveView.BottomRight;
            m_syntaxBoxControl.AutoListPosition = (TextPoint)null;
            m_syntaxBoxControl.AutoListSelectedText = "a123";
            m_syntaxBoxControl.AutoListVisible = false;
            m_syntaxBoxControl.BackColor = Color.White;
            m_syntaxBoxControl.BorderStyle = Alsing.Windows.Forms.BorderStyle.None;
            m_syntaxBoxControl.CopyAsRTF = false;
            m_syntaxBoxControl.Dock = DockStyle.Fill;
            m_syntaxBoxControl.Document = m_syntaxDocument;
            m_syntaxBoxControl.FontName = "Courier new";
            m_syntaxBoxControl.ImeMode = ImeMode.NoControl;
            m_syntaxBoxControl.InfoTipCount = 1;
            m_syntaxBoxControl.InfoTipPosition = (TextPoint)null;
            m_syntaxBoxControl.InfoTipSelectedIndex = 1;
            m_syntaxBoxControl.InfoTipVisible = false;
            m_syntaxBoxControl.Location = new Point(0, 0);
            m_syntaxBoxControl.LockCursorUpdate = false;
            m_syntaxBoxControl.Name = "m_syntaxBoxControl";
            m_syntaxBoxControl.ShowScopeIndicator = false;
            m_syntaxBoxControl.Size = new Size(590, 501);
            m_syntaxBoxControl.SmoothScroll = false;
            m_syntaxBoxControl.SplitviewH = -4;
            m_syntaxBoxControl.SplitviewV = -4;
            m_syntaxBoxControl.TabGuideColor = Color.FromArgb(222, 219, 214);
            m_syntaxBoxControl.TabIndex = 0;
            m_syntaxBoxControl.Text = "syntaxBoxControl1";
            m_syntaxBoxControl.WhitespaceColor = SystemColors.ControlDark;
            m_syntaxDocument.Lines = new string[1] { "" };
            m_syntaxDocument.MaxUndoBufferSize = 1000;
            m_syntaxDocument.Modified = false;
            m_syntaxDocument.UndoStep = 0;
            m_visualTipProvider.Animation = VisualTipAnimation.Enabled;
            m_visualTipProvider.DisplayAtMousePosition = false;
            m_visualTipProvider.DisplayMode = VisualTipDisplayMode.Manual;
            m_visualTipProvider.Renderer = (VisualTipRenderer)tipOfficeRenderer;
            m_gutterImageList.ImageStream =
                (ImageListStreamer)componentResourceManager.GetObject("m_gutterImageList.ImageStream");
            m_gutterImageList.TransparentColor = Color.Transparent;
            m_gutterImageList.Images.SetKeyName(0, "breakpoint-disabled.png");
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add((Control)m_syntaxBoxControl);
            Name = nameof(TextEditor);
            Size = new Size(590, 501);
            ResumeLayout(false);
        }
    }
}