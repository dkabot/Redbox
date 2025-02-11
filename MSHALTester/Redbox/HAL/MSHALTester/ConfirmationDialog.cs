using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.MSHALTester;

public class ConfirmationDialog : Form
{
    private IContainer components;
    private Button m_cancelButton;
    private TextBox m_confirmationBox;
    private string m_confirmText;
    private ErrorProvider m_errorProvider;
    private Label m_label1;
    private Button m_okButton;

    public ConfirmationDialog(string confirmText)
    {
        InitializeComponent();
        Setup(confirmText);
    }

    public ConfirmationDialog()
    {
        InitializeComponent();
        Setup("test");
    }

    private void Setup(string confirmText)
    {
        m_confirmText = string.IsNullOrEmpty(confirmText) ? "test" : confirmText;
        m_label1.Text = string.Format("Enter the word '{0}' (without quotes)", m_confirmText);
    }

    private void m_okButton_Click(object sender, EventArgs e)
    {
        m_errorProvider.Clear();
        if (string.IsNullOrEmpty(m_confirmationBox.Text))
        {
            m_errorProvider.SetError(m_confirmationBox, "Enter a confirmation.");
        }
        else if (!m_confirmationBox.Text.Equals(m_confirmText, StringComparison.CurrentCultureIgnoreCase))
        {
            m_errorProvider.SetError(m_confirmationBox, "Enter the correct confirmation.");
        }
        else
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    private void m_cancelButton_Click(object sender, EventArgs e)
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
        components = new Container();
        m_confirmationBox = new TextBox();
        m_label1 = new Label();
        m_okButton = new Button();
        m_cancelButton = new Button();
        m_errorProvider = new ErrorProvider(components);
        ((ISupportInitialize)m_errorProvider).BeginInit();
        SuspendLayout();
        m_confirmationBox.Location = new Point(26, 52);
        m_confirmationBox.Name = "m_confirmationBox";
        m_confirmationBox.Size = new Size(197, 20);
        m_confirmationBox.TabIndex = 0;
        m_label1.AutoSize = true;
        m_label1.Location = new Point(23, 26);
        m_label1.Name = "m_label1";
        m_label1.Size = new Size(202, 13);
        m_label1.TabIndex = 1;
        m_label1.Text = "Enter the word 'test' (without the quotes) :";
        m_okButton.Location = new Point(26, 96);
        m_okButton.Name = "m_okButton";
        m_okButton.Size = new Size(105, 60);
        m_okButton.TabIndex = 2;
        m_okButton.Text = "Ok";
        m_okButton.UseVisualStyleBackColor = true;
        m_okButton.Click += m_okButton_Click;
        m_cancelButton.Location = new Point(158, 96);
        m_cancelButton.Name = "m_cancelButton";
        m_cancelButton.Size = new Size(105, 60);
        m_cancelButton.TabIndex = 3;
        m_cancelButton.Text = "Cancel";
        m_cancelButton.UseVisualStyleBackColor = true;
        m_cancelButton.Click += m_cancelButton_Click;
        m_errorProvider.ContainerControl = this;
        AutoScaleDimensions = new SizeF(6f, 13f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(298, 186);
        Controls.Add(m_cancelButton);
        Controls.Add(m_okButton);
        Controls.Add(m_label1);
        Controls.Add(m_confirmationBox);
        Name = nameof(ConfirmationDialog);
        Text = nameof(ConfirmationDialog);
        ((ISupportInitialize)m_errorProvider).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}