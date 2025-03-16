using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class EditSearchPathForm : Form
    {
        private IContainer components;
        private ListBox m_listBox;
        private Label label1;
        private TextBox m_pathTextBox;
        private Button m_browseButton;
        private Button m_addButton;
        private Button m_removeButton;
        private Button m_clearButton;
        private ErrorProvider m_errorProvider;
        private Button m_moveDownButton;
        private Button m_moveUpButton;
        private Button m_okButton;
        private Button m_cancelButton;

        public EditSearchPathForm()
        {
            InitializeComponent();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<string> SearchPath { get; set; }

        private void OnSelectedPathChanged(object sender, EventArgs e)
        {
            m_removeButton.Enabled = m_listBox.SelectedItems.Count > 0;
            m_moveUpButton.Enabled = m_listBox.SelectedItems.Count > 0;
            m_moveDownButton.Enabled = m_listBox.SelectedItems.Count > 0;
        }

        private void OnRemove(object sender, EventArgs e)
        {
            foreach (int selectedIndex in m_listBox.SelectedIndices)
                m_listBox.Items.RemoveAt(selectedIndex);
        }

        private void OnClear(object sender, EventArgs e)
        {
            m_listBox.Items.Clear();
        }

        private void OnBrowse(object sender, EventArgs e)
        {
            var folderBrowserDialog = new FolderBrowserDialog()
            {
                SelectedPath = m_pathTextBox.Text
            };
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                return;
            m_pathTextBox.Text = folderBrowserDialog.SelectedPath;
        }

        private void OnAdd(object sender, EventArgs e)
        {
            m_errorProvider.SetError((Control)m_browseButton, string.Empty);
            if (m_pathTextBox.Text.Length == 0)
            {
                m_errorProvider.SetError((Control)m_browseButton, "Specify a valid path.");
            }
            else
            {
                m_listBox.Items.Add((object)m_pathTextBox.Text);
                m_pathTextBox.Clear();
                m_pathTextBox.Focus();
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void OnMoveUp(object sender, EventArgs e)
        {
            var selectedItem = m_listBox.SelectedItem;
            if (selectedItem == null)
                return;
            var index = m_listBox.Items.IndexOf(selectedItem);
            if (index == 0)
                return;
            m_listBox.Items.RemoveAt(index);
            m_listBox.Items.Insert(index - 1, selectedItem);
            m_listBox.SelectedItem = selectedItem;
        }

        private void OnMoveDown(object sender, EventArgs e)
        {
            var selectedItem = m_listBox.SelectedItem;
            if (selectedItem == null)
                return;
            var index = m_listBox.Items.IndexOf(selectedItem);
            if (index == -1 || index >= m_listBox.Items.Count - 1)
                return;
            m_listBox.Items.RemoveAt(index);
            m_listBox.Items.Insert(index + 1, selectedItem);
            m_listBox.SelectedItem = selectedItem;
        }

        private void RefreshList()
        {
            m_listBox.SuspendLayout();
            m_listBox.BeginUpdate();
            m_listBox.Items.Clear();
            foreach (object obj in SearchPath)
                m_listBox.Items.Add(obj);
            m_listBox.EndUpdate();
            m_listBox.ResumeLayout();
            m_clearButton.Enabled = m_listBox.Items.Count > 0;
        }

        private void OnOK(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            SearchPath.Clear();
            foreach (string str in m_listBox.Items)
                SearchPath.Add(str);
            Close();
        }

        private void OnCancel(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
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
            m_listBox = new ListBox();
            label1 = new Label();
            m_pathTextBox = new TextBox();
            m_browseButton = new Button();
            m_addButton = new Button();
            m_removeButton = new Button();
            m_clearButton = new Button();
            m_errorProvider = new ErrorProvider(components);
            m_moveUpButton = new Button();
            m_moveDownButton = new Button();
            m_okButton = new Button();
            m_cancelButton = new Button();
            ((ISupportInitialize)m_errorProvider).BeginInit();
            SuspendLayout();
            m_listBox.FormattingEnabled = true;
            m_listBox.Location = new Point(93, 41);
            m_listBox.Name = "m_listBox";
            m_listBox.Size = new Size(438, 290);
            m_listBox.TabIndex = 6;
            m_listBox.SelectedIndexChanged += new EventHandler(OnSelectedPathChanged);
            label1.AutoSize = true;
            label1.Location = new Point(55, 9);
            label1.Name = "label1";
            label1.Size = new Size(32, 13);
            label1.TabIndex = 0;
            label1.Text = "Path:";
            m_pathTextBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            m_pathTextBox.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
            m_pathTextBox.Location = new Point(93, 6);
            m_pathTextBox.Name = "m_pathTextBox";
            m_pathTextBox.Size = new Size(308, 20);
            m_pathTextBox.TabIndex = 1;
            m_browseButton.Location = new Point(407, 4);
            m_browseButton.Name = "m_browseButton";
            m_browseButton.Size = new Size(24, 23);
            m_browseButton.TabIndex = 2;
            m_browseButton.Text = "...";
            m_browseButton.UseVisualStyleBackColor = true;
            m_browseButton.Click += new EventHandler(OnBrowse);
            m_addButton.Location = new Point(465, 4);
            m_addButton.Name = "m_addButton";
            m_addButton.Size = new Size(66, 23);
            m_addButton.TabIndex = 3;
            m_addButton.Text = "Add";
            m_addButton.UseVisualStyleBackColor = true;
            m_addButton.Click += new EventHandler(OnAdd);
            m_removeButton.Enabled = false;
            m_removeButton.Location = new Point(93, 337);
            m_removeButton.Name = "m_removeButton";
            m_removeButton.Size = new Size(75, 23);
            m_removeButton.TabIndex = 7;
            m_removeButton.Text = "Remove";
            m_removeButton.UseVisualStyleBackColor = true;
            m_removeButton.Click += new EventHandler(OnRemove);
            m_clearButton.Enabled = false;
            m_clearButton.Location = new Point(174, 337);
            m_clearButton.Name = "m_clearButton";
            m_clearButton.Size = new Size(75, 23);
            m_clearButton.TabIndex = 8;
            m_clearButton.Text = "Clear";
            m_clearButton.UseVisualStyleBackColor = true;
            m_clearButton.Click += new EventHandler(OnClear);
            m_errorProvider.ContainerControl = (ContainerControl)this;
            m_moveUpButton.Enabled = false;
            m_moveUpButton.Location = new Point(12, 41);
            m_moveUpButton.Name = "m_moveUpButton";
            m_moveUpButton.Size = new Size(75, 23);
            m_moveUpButton.TabIndex = 4;
            m_moveUpButton.Text = "Move Up";
            m_moveUpButton.UseVisualStyleBackColor = true;
            m_moveUpButton.Click += new EventHandler(OnMoveUp);
            m_moveDownButton.Enabled = false;
            m_moveDownButton.Location = new Point(12, 70);
            m_moveDownButton.Name = "m_moveDownButton";
            m_moveDownButton.Size = new Size(75, 23);
            m_moveDownButton.TabIndex = 5;
            m_moveDownButton.Text = "Move Down";
            m_moveDownButton.UseVisualStyleBackColor = true;
            m_moveDownButton.Click += new EventHandler(OnMoveDown);
            m_okButton.Location = new Point(375, 337);
            m_okButton.Name = "m_okButton";
            m_okButton.Size = new Size(75, 23);
            m_okButton.TabIndex = 9;
            m_okButton.Text = "OK";
            m_okButton.UseVisualStyleBackColor = true;
            m_okButton.Click += new EventHandler(OnOK);
            m_cancelButton.DialogResult = DialogResult.Cancel;
            m_cancelButton.Location = new Point(456, 337);
            m_cancelButton.Name = "m_cancelButton";
            m_cancelButton.Size = new Size(75, 23);
            m_cancelButton.TabIndex = 10;
            m_cancelButton.Text = "Cancel";
            m_cancelButton.UseVisualStyleBackColor = true;
            m_cancelButton.Click += new EventHandler(OnCancel);
            AcceptButton = (IButtonControl)m_okButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = (IButtonControl)m_cancelButton;
            ClientSize = new Size(543, 368);
            Controls.Add((Control)m_cancelButton);
            Controls.Add((Control)m_okButton);
            Controls.Add((Control)m_moveDownButton);
            Controls.Add((Control)m_moveUpButton);
            Controls.Add((Control)m_clearButton);
            Controls.Add((Control)m_removeButton);
            Controls.Add((Control)m_addButton);
            Controls.Add((Control)m_browseButton);
            Controls.Add((Control)m_pathTextBox);
            Controls.Add((Control)label1);
            Controls.Add((Control)m_listBox);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = nameof(EditSearchPathForm);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Edit Search Path";
            Load += new EventHandler(OnLoad);
            ((ISupportInitialize)m_errorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}