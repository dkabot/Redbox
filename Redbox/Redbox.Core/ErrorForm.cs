using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.Core
{
    public class ErrorForm : Form
    {
        private IContainer components;
        private Panel m_bottomPanel;
        private Button m_closeButton;
        private ColumnHeader m_codeColumnHeader;
        private ColumnHeader m_descriptionColumnHeader;
        private TextBox m_detailTextBox;
        private ListView m_errorListView;
        private IEnumerable m_errors;
        private ImageList m_imageList;
        private Label m_messageLabel;
        private Panel m_messagePanel;
        private Splitter m_splitter;
        private Panel m_topPanel;

        public ErrorForm()
        {
            InitializeComponent();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Message
        {
            get => m_messageLabel.Text;
            set => m_messageLabel.Text = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEnumerable Errors
        {
            get => m_errors;
            set
            {
                m_errors = value;
                RefreshList();
            }
        }

        private void OnClose(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void OnSelectedErrorChanged(object sender, EventArgs e)
        {
            m_detailTextBox.Clear();
            if (m_errorListView.SelectedItems.Count <= 0)
                return;
            var tag = m_errorListView.SelectedItems[0].Tag;
            if (tag == null)
                return;
            var property = tag.GetType().GetProperty("Details");
            var empty = (object)string.Empty;
            if (property != null)
                empty = property.GetValue(tag, null);
            m_detailTextBox.Text = empty != null ? empty.ToString() : string.Empty;
        }

        private void RefreshList()
        {
            m_errorListView.Items.Clear();
            if (Errors == null)
                return;
            foreach (var error in Errors)
            {
                var property1 = error.GetType().GetProperty("Code");
                var property2 = error.GetType().GetProperty("IsWarning");
                var property3 = error.GetType().GetProperty("Description");
                var empty1 = string.Empty;
                if (property1 != null)
                {
                    var obj = property1.GetValue(error, null);
                    if (obj != null)
                        empty1 = obj.ToString();
                }

                var flag = false;
                if (property2 != null)
                    flag = (bool)property2.GetValue(error, null);
                var empty2 = string.Empty;
                if (property3 != null)
                {
                    var obj = property3.GetValue(error, null);
                    if (obj != null)
                        empty2 = obj.ToString();
                }

                m_errorListView.Items.Add(new ListViewItem(empty1, flag ? 1 : 0)
                {
                    SubItems =
                    {
                        empty2
                    },
                    Tag = error
                });
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
            components = new Container();
            var componentResourceManager = new ComponentResourceManager(typeof(ErrorForm));
            m_messagePanel = new Panel();
            m_messageLabel = new Label();
            m_errorListView = new ListView();
            m_codeColumnHeader = new ColumnHeader();
            m_descriptionColumnHeader = new ColumnHeader();
            m_imageList = new ImageList(components);
            m_detailTextBox = new TextBox();
            m_topPanel = new Panel();
            m_splitter = new Splitter();
            m_bottomPanel = new Panel();
            m_closeButton = new Button();
            m_messagePanel.SuspendLayout();
            m_topPanel.SuspendLayout();
            m_bottomPanel.SuspendLayout();
            SuspendLayout();
            m_messagePanel.AutoSize = true;
            m_messagePanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            m_messagePanel.Controls.Add(m_messageLabel);
            m_messagePanel.Dock = DockStyle.Top;
            m_messagePanel.Location = new Point(0, 0);
            m_messagePanel.Name = "m_messagePanel";
            m_messagePanel.Padding = new Padding(3);
            m_messagePanel.Size = new Size(542, 19);
            m_messagePanel.TabIndex = 1;
            m_messageLabel.AutoSize = true;
            m_messageLabel.Dock = DockStyle.Fill;
            m_messageLabel.Location = new Point(3, 3);
            m_messageLabel.Name = "m_messageLabel";
            m_messageLabel.Size = new Size(102, 13);
            m_messageLabel.TabIndex = 0;
            m_messageLabel.Text = "The following errors:";
            m_errorListView.Columns.AddRange(new ColumnHeader[2]
            {
                m_codeColumnHeader,
                m_descriptionColumnHeader
            });
            m_errorListView.Dock = DockStyle.Fill;
            m_errorListView.FullRowSelect = true;
            m_errorListView.Location = new Point(3, 3);
            m_errorListView.Name = "m_errorListView";
            m_errorListView.Size = new Size(536, 160);
            m_errorListView.SmallImageList = m_imageList;
            m_errorListView.TabIndex = 2;
            m_errorListView.UseCompatibleStateImageBehavior = false;
            m_errorListView.View = View.Details;
            m_errorListView.SelectedIndexChanged += OnSelectedErrorChanged;
            m_codeColumnHeader.Text = "Code";
            m_codeColumnHeader.Width = 75;
            m_descriptionColumnHeader.Text = "Description";
            m_descriptionColumnHeader.Width = 455;
            m_imageList.ImageStream = (ImageListStreamer)componentResourceManager.GetObject("m_imageList.ImageStream");
            m_imageList.TransparentColor = Color.Transparent;
            m_imageList.Images.SetKeyName(0, "cross.png");
            m_imageList.Images.SetKeyName(1, "warning.png");
            m_detailTextBox.Dock = DockStyle.Fill;
            m_detailTextBox.Location = new Point(3, 1);
            m_detailTextBox.Multiline = true;
            m_detailTextBox.Name = "m_detailTextBox";
            m_detailTextBox.ReadOnly = true;
            m_detailTextBox.Size = new Size(536, 132);
            m_detailTextBox.TabIndex = 4;
            m_topPanel.Controls.Add(m_errorListView);
            m_topPanel.Dock = DockStyle.Top;
            m_topPanel.Location = new Point(0, 19);
            m_topPanel.Name = "m_topPanel";
            m_topPanel.Padding = new Padding(3, 3, 3, 1);
            m_topPanel.Size = new Size(542, 164);
            m_topPanel.TabIndex = 5;
            m_splitter.Dock = DockStyle.Top;
            m_splitter.Location = new Point(0, 183);
            m_splitter.MinSize = 100;
            m_splitter.Name = "m_splitter";
            m_splitter.Size = new Size(542, 3);
            m_splitter.TabIndex = 6;
            m_splitter.TabStop = false;
            m_bottomPanel.Controls.Add(m_detailTextBox);
            m_bottomPanel.Dock = DockStyle.Fill;
            m_bottomPanel.Location = new Point(0, 186);
            m_bottomPanel.Name = "m_bottomPanel";
            m_bottomPanel.Padding = new Padding(3, 1, 3, 3);
            m_bottomPanel.Size = new Size(542, 136);
            m_bottomPanel.TabIndex = 7;
            m_closeButton.Dock = DockStyle.Bottom;
            m_closeButton.Location = new Point(0, 322);
            m_closeButton.Name = "m_closeButton";
            m_closeButton.Size = new Size(542, 51);
            m_closeButton.TabIndex = 8;
            m_closeButton.Text = "Close";
            m_closeButton.UseVisualStyleBackColor = true;
            m_closeButton.Click += OnClose;
            AcceptButton = m_closeButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(542, 373);
            Controls.Add(m_bottomPanel);
            Controls.Add(m_closeButton);
            Controls.Add(m_splitter);
            Controls.Add(m_topPanel);
            Controls.Add(m_messagePanel);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(550, 400);
            Name = nameof(ErrorForm);
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Errors";
            m_messagePanel.ResumeLayout(false);
            m_messagePanel.PerformLayout();
            m_topPanel.ResumeLayout(false);
            m_bottomPanel.ResumeLayout(false);
            m_bottomPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}