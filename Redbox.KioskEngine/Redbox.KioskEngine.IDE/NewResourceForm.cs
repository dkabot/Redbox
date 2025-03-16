using Redbox.REDS.Framework;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public class NewResourceForm : Form
    {
        private IContainer components;
        private Label label1;
        private TextBox m_nameTextBox;
        private ErrorProvider m_errorProvider;
        private Button m_okButton;
        private Button m_cancelButton;
        private Label label2;
        private ResourceTypeTreeView m_resourceTypeTreeView;

        public NewResourceForm()
        {
            InitializeComponent();
        }

        private void OnOK(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
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
            label1 = new Label();
            m_nameTextBox = new TextBox();
            m_errorProvider = new ErrorProvider(components);
            m_okButton = new Button();
            m_cancelButton = new Button();
            label2 = new Label();
            m_resourceTypeTreeView = new ResourceTypeTreeView();
            ((ISupportInitialize)m_errorProvider).BeginInit();
            SuspendLayout();
            label1.AutoSize = true;
            label1.Location = new Point(12, 17);
            label1.Name = "label1";
            label1.Size = new Size(38, 13);
            label1.TabIndex = 0;
            label1.Text = "Name:";
            m_nameTextBox.Location = new Point(56, 14);
            m_nameTextBox.Name = "m_nameTextBox";
            m_nameTextBox.Size = new Size(241, 20);
            m_nameTextBox.TabIndex = 1;
            m_errorProvider.ContainerControl = (ContainerControl)this;
            m_okButton.Location = new Point(335, 12);
            m_okButton.Name = "m_okButton";
            m_okButton.Size = new Size(75, 23);
            m_okButton.TabIndex = 4;
            m_okButton.Text = "OK";
            m_okButton.UseVisualStyleBackColor = true;
            m_okButton.Click += new EventHandler(OnOK);
            m_cancelButton.DialogResult = DialogResult.Cancel;
            m_cancelButton.Location = new Point(335, 41);
            m_cancelButton.Name = "m_cancelButton";
            m_cancelButton.Size = new Size(75, 23);
            m_cancelButton.TabIndex = 5;
            m_cancelButton.Text = "Cancel";
            m_cancelButton.UseVisualStyleBackColor = true;
            m_cancelButton.Click += new EventHandler(OnCancel);
            label2.AutoSize = true;
            label2.Location = new Point(12, 41);
            label2.Name = "label2";
            label2.Size = new Size(34, 13);
            label2.TabIndex = 2;
            label2.Text = "Type:";
            m_resourceTypeTreeView.Bundle = (IResourceBundle)null;
            m_resourceTypeTreeView.Location = new Point(56, 41);
            m_resourceTypeTreeView.Name = "m_resourceTypeTreeView";
            m_resourceTypeTreeView.SelectedType = (IResourceType)null;
            m_resourceTypeTreeView.Size = new Size(241, 220);
            m_resourceTypeTreeView.TabIndex = 6;
            AcceptButton = (IButtonControl)m_okButton;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = (IButtonControl)m_cancelButton;
            ClientSize = new Size(422, 273);
            ControlBox = false;
            Controls.Add((Control)m_resourceTypeTreeView);
            Controls.Add((Control)label2);
            Controls.Add((Control)m_cancelButton);
            Controls.Add((Control)m_okButton);
            Controls.Add((Control)m_nameTextBox);
            Controls.Add((Control)label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = nameof(NewResourceForm);
            StartPosition = FormStartPosition.CenterParent;
            Text = "New Resource";
            ((ISupportInitialize)m_errorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}