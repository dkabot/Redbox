using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions;

public class LocationNumberPad : Form
{
    private Button button1;
    private Button button10;
    private Button button11;
    private Button button12;
    private Button button13;
    private Button button2;
    private Button button3;
    private Button button4;
    private Button button5;
    private Button button6;
    private Button button7;
    private Button button8;
    private Button button9;
    private IContainer components;
    private TextBox m_displayTextBox;

    public LocationNumberPad()
    {
        InitializeComponent();
    }

    public int Number
    {
        get
        {
            int result;
            return int.TryParse(m_displayTextBox.Text, out result) ? result : -1;
        }
    }

    private void OnNumberButtonPush(object sender, EventArgs e)
    {
        m_displayTextBox.Text += GetNumber(sender);
    }

    private string GetNumber(object sender)
    {
        return !(sender is Button button) || !(button.Tag is string tag) ? string.Empty : tag;
    }

    private void button11_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(m_displayTextBox.Text))
            DialogResult = DialogResult.Cancel;
        else
            DialogResult = DialogResult.OK;
        Close();
    }

    private void button12_Click(object sender, EventArgs e)
    {
        m_displayTextBox.Text = string.Empty;
    }

    private void button10_Click(object sender, EventArgs e)
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
        button1 = new Button();
        button2 = new Button();
        button3 = new Button();
        button4 = new Button();
        button5 = new Button();
        button7 = new Button();
        button8 = new Button();
        button9 = new Button();
        button6 = new Button();
        m_displayTextBox = new TextBox();
        button11 = new Button();
        button12 = new Button();
        button10 = new Button();
        button13 = new Button();
        SuspendLayout();
        button1.BackColor = Color.LightGray;
        button1.Location = new Point(12, 62);
        button1.Name = "button1";
        button1.Size = new Size(75, 49);
        button1.TabIndex = 0;
        button1.Tag = "1";
        button1.Text = "1";
        button1.UseVisualStyleBackColor = false;
        button1.Click += OnNumberButtonPush;
        button2.BackColor = Color.LightGray;
        button2.Location = new Point(105, 62);
        button2.Name = "button2";
        button2.Size = new Size(75, 49);
        button2.TabIndex = 1;
        button2.Tag = "2";
        button2.Text = "2";
        button2.UseVisualStyleBackColor = false;
        button2.Click += OnNumberButtonPush;
        button3.BackColor = Color.LightGray;
        button3.Location = new Point(197, 62);
        button3.Name = "button3";
        button3.Size = new Size(75, 49);
        button3.TabIndex = 2;
        button3.Tag = "3";
        button3.Text = "3";
        button3.UseVisualStyleBackColor = false;
        button3.Click += OnNumberButtonPush;
        button4.BackColor = Color.LightGray;
        button4.Location = new Point(12, 130);
        button4.Name = "button4";
        button4.Size = new Size(75, 49);
        button4.TabIndex = 3;
        button4.Tag = "4";
        button4.Text = "4";
        button4.UseVisualStyleBackColor = false;
        button4.Click += OnNumberButtonPush;
        button5.BackColor = Color.LightGray;
        button5.Location = new Point(105, 130);
        button5.Name = "button5";
        button5.Size = new Size(75, 49);
        button5.TabIndex = 4;
        button5.Tag = "5";
        button5.Text = "5";
        button5.UseVisualStyleBackColor = false;
        button5.Click += OnNumberButtonPush;
        button7.BackColor = Color.LightGray;
        button7.Location = new Point(12, 204);
        button7.Name = "button7";
        button7.Size = new Size(75, 49);
        button7.TabIndex = 6;
        button7.Tag = "7";
        button7.Text = "7";
        button7.UseVisualStyleBackColor = false;
        button7.Click += OnNumberButtonPush;
        button8.BackColor = Color.LightGray;
        button8.Location = new Point(105, 204);
        button8.Name = "button8";
        button8.Size = new Size(75, 49);
        button8.TabIndex = 7;
        button8.Tag = "8";
        button8.Text = "8";
        button8.UseVisualStyleBackColor = false;
        button8.Click += OnNumberButtonPush;
        button9.BackColor = Color.LightGray;
        button9.Location = new Point(197, 204);
        button9.Name = "button9";
        button9.Size = new Size(75, 49);
        button9.TabIndex = 8;
        button9.Tag = "9";
        button9.Text = "9";
        button9.UseVisualStyleBackColor = false;
        button9.Click += OnNumberButtonPush;
        button6.BackColor = Color.LightGray;
        button6.Location = new Point(197, 130);
        button6.Name = "button6";
        button6.Size = new Size(75, 49);
        button6.TabIndex = 9;
        button6.Tag = "6";
        button6.Text = "6";
        button6.UseVisualStyleBackColor = false;
        button6.Click += OnNumberButtonPush;
        m_displayTextBox.BackColor = SystemColors.ControlLightLight;
        m_displayTextBox.Location = new Point(12, 21);
        m_displayTextBox.Name = "m_displayTextBox";
        m_displayTextBox.ReadOnly = true;
        m_displayTextBox.Size = new Size(270, 20);
        m_displayTextBox.TabIndex = 11;
        button11.BackColor = Color.LightGray;
        button11.Location = new Point(314, 62);
        button11.Name = "button11";
        button11.Size = new Size(75, 49);
        button11.TabIndex = 12;
        button11.Text = "Ok";
        button11.UseVisualStyleBackColor = false;
        button11.Click += button11_Click;
        button12.BackColor = Color.LightGray;
        button12.Location = new Point(314, 204);
        button12.Name = "button12";
        button12.Size = new Size(75, 49);
        button12.TabIndex = 13;
        button12.Text = "Clear";
        button12.UseVisualStyleBackColor = false;
        button12.Click += button12_Click;
        button10.BackColor = Color.LightGray;
        button10.Location = new Point(314, 130);
        button10.Name = "button10";
        button10.Size = new Size(75, 49);
        button10.TabIndex = 14;
        button10.Text = "Cancel";
        button10.UseVisualStyleBackColor = false;
        button10.Click += button10_Click;
        button13.BackColor = Color.LightGray;
        button13.Location = new Point(105, 271);
        button13.Name = "button13";
        button13.Size = new Size(75, 49);
        button13.TabIndex = 15;
        button13.Tag = "0";
        button13.Text = "0";
        button13.UseVisualStyleBackColor = false;
        button13.Click += OnNumberButtonPush;
        AutoScaleDimensions = new SizeF(6f, 13f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(419, 333);
        Controls.Add(button13);
        Controls.Add(button10);
        Controls.Add(button12);
        Controls.Add(button11);
        Controls.Add(m_displayTextBox);
        Controls.Add(button6);
        Controls.Add(button9);
        Controls.Add(button8);
        Controls.Add(button7);
        Controls.Add(button5);
        Controls.Add(button4);
        Controls.Add(button3);
        Controls.Add(button2);
        Controls.Add(button1);
        Name = "NumberPad";
        Text = "NumberPad";
        ResumeLayout(false);
        PerformLayout();
    }
}