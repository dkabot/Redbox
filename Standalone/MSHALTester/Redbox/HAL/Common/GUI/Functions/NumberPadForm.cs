using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions;

public class NumberPadForm : Form
{
    private IContainer components;
    private Button m_cancelButton;
    private NumberPad m_numberPad;
    private Button m_okButton;

    public NumberPadForm()
    {
        InitializeComponent();
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Number
    {
        get => m_numberPad.Number;
        set => m_numberPad.Text = value;
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
        m_numberPad = new NumberPad();
        m_okButton = new Button();
        m_cancelButton = new Button();
        SuspendLayout();
        m_numberPad.Dock = DockStyle.Fill;
        m_numberPad.Location = new Point(3, 3);
        m_numberPad.Name = "m_numberPad";
        m_numberPad.Size = new Size(378, 260);
        m_numberPad.TabIndex = 0;
        m_okButton.Location = new Point(289, 152);
        m_okButton.Name = "m_okButton";
        m_okButton.Size = new Size(89, 48);
        m_okButton.TabIndex = 1;
        m_okButton.Text = "OK";
        m_okButton.UseVisualStyleBackColor = true;
        m_okButton.Click += OnOK;
        m_cancelButton.DialogResult = DialogResult.Cancel;
        m_cancelButton.Location = new Point(289, 206);
        m_cancelButton.Name = "m_cancelButton";
        m_cancelButton.Size = new Size(89, 54);
        m_cancelButton.TabIndex = 2;
        m_cancelButton.Text = "Cancel";
        m_cancelButton.UseVisualStyleBackColor = true;
        m_cancelButton.Click += OnCancel;
        AcceptButton = m_okButton;
        AutoScaleDimensions = new SizeF(6f, 13f);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = m_cancelButton;
        ClientSize = new Size(384, 266);
        ControlBox = false;
        Controls.Add(m_cancelButton);
        Controls.Add(m_okButton);
        Controls.Add(m_numberPad);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Name = nameof(NumberPadForm);
        Padding = new Padding(3);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Enter Number";
        ResumeLayout(false);
    }
}