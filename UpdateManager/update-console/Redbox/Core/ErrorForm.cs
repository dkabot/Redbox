using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Redbox.Core
{
    internal class ErrorForm : Form
    {
        private IEnumerable m_errors;
        private IContainer components;
        private Panel m_messagePanel;
        private Label m_messageLabel;
        private ListView m_errorListView;
        private TextBox m_detailTextBox;
        private ColumnHeader m_codeColumnHeader;
        private ColumnHeader m_descriptionColumnHeader;
        private ImageList m_imageList;
        private Panel m_topPanel;
        private Splitter m_splitter;
        private Panel m_bottomPanel;
        private Button m_closeButton;

        public ErrorForm() => this.InitializeComponent();

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Message
        {
            get => this.m_messageLabel.Text;
            set => this.m_messageLabel.Text = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEnumerable Errors
        {
            get => this.m_errors;
            set
            {
                this.m_errors = value;
                this.RefreshList();
            }
        }

        private void OnClose(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OnSelectedErrorChanged(object sender, EventArgs e)
        {
            this.m_detailTextBox.Clear();
            if (this.m_errorListView.SelectedItems.Count <= 0)
                return;
            object tag = this.m_errorListView.SelectedItems[0].Tag;
            if (tag == null)
                return;
            PropertyInfo property = tag.GetType().GetProperty("Details");
            object empty = (object)string.Empty;
            if (property != (PropertyInfo)null)
                empty = property.GetValue(tag, (object[])null);
            this.m_detailTextBox.Text = empty != null ? empty.ToString() : string.Empty;
        }

        private void RefreshList()
        {
            this.m_errorListView.Items.Clear();
            if (this.Errors == null)
                return;
            foreach (object error in this.Errors)
            {
                PropertyInfo property1 = error.GetType().GetProperty("Code");
                PropertyInfo property2 = error.GetType().GetProperty("IsWarning");
                PropertyInfo property3 = error.GetType().GetProperty("Description");
                string empty1 = string.Empty;
                if (property1 != (PropertyInfo)null)
                {
                    object obj = property1.GetValue(error, (object[])null);
                    if (obj != null)
                        empty1 = obj.ToString();
                }
                bool flag = false;
                if (property2 != (PropertyInfo)null)
                    flag = (bool)property2.GetValue(error, (object[])null);
                string empty2 = string.Empty;
                if (property3 != (PropertyInfo)null)
                {
                    object obj = property3.GetValue(error, (object[])null);
                    if (obj != null)
                        empty2 = obj.ToString();
                }
                this.m_errorListView.Items.Add(new ListViewItem(empty1, flag ? 1 : 0)
                {
                    SubItems = {
            empty2
          },
                    Tag = error
                });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = (IContainer)new System.ComponentModel.Container();
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(ErrorForm));
            this.m_messagePanel = new Panel();
            this.m_messageLabel = new Label();
            this.m_errorListView = new ListView();
            this.m_codeColumnHeader = new ColumnHeader();
            this.m_descriptionColumnHeader = new ColumnHeader();
            this.m_imageList = new ImageList(this.components);
            this.m_detailTextBox = new TextBox();
            this.m_topPanel = new Panel();
            this.m_splitter = new Splitter();
            this.m_bottomPanel = new Panel();
            this.m_closeButton = new Button();
            this.m_messagePanel.SuspendLayout();
            this.m_topPanel.SuspendLayout();
            this.m_bottomPanel.SuspendLayout();
            this.SuspendLayout();
            this.m_messagePanel.AutoSize = true;
            this.m_messagePanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.m_messagePanel.Controls.Add((Control)this.m_messageLabel);
            this.m_messagePanel.Dock = DockStyle.Top;
            this.m_messagePanel.Location = new Point(0, 0);
            this.m_messagePanel.Name = "m_messagePanel";
            this.m_messagePanel.Padding = new Padding(3);
            this.m_messagePanel.Size = new Size(542, 19);
            this.m_messagePanel.TabIndex = 1;
            this.m_messageLabel.AutoSize = true;
            this.m_messageLabel.Dock = DockStyle.Fill;
            this.m_messageLabel.Location = new Point(3, 3);
            this.m_messageLabel.Name = "m_messageLabel";
            this.m_messageLabel.Size = new Size(102, 13);
            this.m_messageLabel.TabIndex = 0;
            this.m_messageLabel.Text = "The following errors:";
            this.m_errorListView.Columns.AddRange(new ColumnHeader[2]
            {
        this.m_codeColumnHeader,
        this.m_descriptionColumnHeader
            });
            this.m_errorListView.Dock = DockStyle.Fill;
            this.m_errorListView.FullRowSelect = true;
            this.m_errorListView.Location = new Point(3, 3);
            this.m_errorListView.Name = "m_errorListView";
            this.m_errorListView.Size = new Size(536, 160);
            this.m_errorListView.SmallImageList = this.m_imageList;
            this.m_errorListView.TabIndex = 2;
            this.m_errorListView.UseCompatibleStateImageBehavior = false;
            this.m_errorListView.View = View.Details;
            this.m_errorListView.SelectedIndexChanged += new EventHandler(this.OnSelectedErrorChanged);
            this.m_codeColumnHeader.Text = "Code";
            this.m_codeColumnHeader.Width = 75;
            this.m_descriptionColumnHeader.Text = "Description";
            this.m_descriptionColumnHeader.Width = 455;
            this.m_imageList.ImageStream = (ImageListStreamer)componentResourceManager.GetObject("m_imageList.ImageStream");
            this.m_imageList.TransparentColor = Color.Transparent;
            this.m_imageList.Images.SetKeyName(0, "cross.png");
            this.m_imageList.Images.SetKeyName(1, "warning.png");
            this.m_detailTextBox.Dock = DockStyle.Fill;
            this.m_detailTextBox.Location = new Point(3, 1);
            this.m_detailTextBox.Multiline = true;
            this.m_detailTextBox.Name = "m_detailTextBox";
            this.m_detailTextBox.ReadOnly = true;
            this.m_detailTextBox.Size = new Size(536, 132);
            this.m_detailTextBox.TabIndex = 4;
            this.m_topPanel.Controls.Add((Control)this.m_errorListView);
            this.m_topPanel.Dock = DockStyle.Top;
            this.m_topPanel.Location = new Point(0, 19);
            this.m_topPanel.Name = "m_topPanel";
            this.m_topPanel.Padding = new Padding(3, 3, 3, 1);
            this.m_topPanel.Size = new Size(542, 164);
            this.m_topPanel.TabIndex = 5;
            this.m_splitter.Dock = DockStyle.Top;
            this.m_splitter.Location = new Point(0, 183);
            this.m_splitter.MinSize = 100;
            this.m_splitter.Name = "m_splitter";
            this.m_splitter.Size = new Size(542, 3);
            this.m_splitter.TabIndex = 6;
            this.m_splitter.TabStop = false;
            this.m_bottomPanel.Controls.Add((Control)this.m_detailTextBox);
            this.m_bottomPanel.Dock = DockStyle.Fill;
            this.m_bottomPanel.Location = new Point(0, 186);
            this.m_bottomPanel.Name = "m_bottomPanel";
            this.m_bottomPanel.Padding = new Padding(3, 1, 3, 3);
            this.m_bottomPanel.Size = new Size(542, 136);
            this.m_bottomPanel.TabIndex = 7;
            this.m_closeButton.Dock = DockStyle.Bottom;
            this.m_closeButton.Location = new Point(0, 322);
            this.m_closeButton.Name = "m_closeButton";
            this.m_closeButton.Size = new Size(542, 51);
            this.m_closeButton.TabIndex = 8;
            this.m_closeButton.Text = "Close";
            this.m_closeButton.UseVisualStyleBackColor = true;
            this.m_closeButton.Click += new EventHandler(this.OnClose);
            this.AcceptButton = (IButtonControl)this.m_closeButton;
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(542, 373);
            this.Controls.Add((Control)this.m_bottomPanel);
            this.Controls.Add((Control)this.m_closeButton);
            this.Controls.Add((Control)this.m_splitter);
            this.Controls.Add((Control)this.m_topPanel);
            this.Controls.Add((Control)this.m_messagePanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new Size(550, 400);
            this.Name = nameof(ErrorForm);
            this.ShowIcon = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Errors";
            this.m_messagePanel.ResumeLayout(false);
            this.m_messagePanel.PerformLayout();
            this.m_topPanel.ResumeLayout(false);
            this.m_bottomPanel.ResumeLayout(false);
            this.m_bottomPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
