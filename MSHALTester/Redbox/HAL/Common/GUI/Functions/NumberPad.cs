using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions;

public class NumberPad : UserControl
{
    private IContainer components;
    private Button m_clearButton;
    private Button m_eightButton;
    private Button m_fiveButton;
    private Button m_fourButton;
    private Button m_nineButton;
    private Button m_oneButton;
    private Button m_sevenButton;
    private Button m_sixButton;
    private TableLayoutPanel m_tableLayoutPanel;
    private TextBox m_textBox;
    private Button m_threeButton;
    private Button m_twoButton;
    private Button m_zeroButton;

    public NumberPad()
    {
        InitializeComponent();
        m_textBox.MaxLength = 40;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Number
    {
        get => m_textBox.Text;
        set => m_textBox.Text = value;
    }

    public int MaxLength
    {
        get => m_textBox.MaxLength;
        set => m_textBox.MaxLength = value;
    }

    public void Clear()
    {
        m_textBox.Clear();
    }

    public event EventHandler NumberChanged;

    private void OnNumberPadButtonClicked(object sender, EventArgs e)
    {
        if (!(sender is Button button))
            return;
        if (button.Text == "C")
            Clear();
        else
            m_textBox.Text += button.Text;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (!e.Control)
            return;
        switch (e.KeyCode)
        {
            case Keys.C:
                e.Handled = true;
                e.SuppressKeyPress = true;
                if (m_textBox.SelectedText.Length <= 0)
                    break;
                m_textBox.Copy();
                break;
            case Keys.V:
                e.Handled = true;
                e.SuppressKeyPress = true;
                if (!Clipboard.GetDataObject().GetDataPresent(DataFormats.Text))
                    break;
                m_textBox.Paste();
                break;
            case Keys.X:
                e.Handled = true;
                e.SuppressKeyPress = true;
                if (m_textBox.SelectedText.Length <= 0)
                    break;
                m_textBox.Cut();
                break;
        }
    }

    private void OnKeyPress(object sender, KeyPressEventArgs e)
    {
        if ((e.KeyChar >= '0' && e.KeyChar <= '9') || e.KeyChar == '\b')
            return;
        e.Handled = true;
    }

    private void OnTextChanged(object sender, EventArgs e)
    {
        if (NumberChanged == null)
            return;
        NumberChanged(this, EventArgs.Empty);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        m_tableLayoutPanel = new TableLayoutPanel();
        m_sevenButton = new Button();
        m_eightButton = new Button();
        m_nineButton = new Button();
        m_fourButton = new Button();
        m_fiveButton = new Button();
        m_sixButton = new Button();
        m_oneButton = new Button();
        m_twoButton = new Button();
        m_threeButton = new Button();
        m_zeroButton = new Button();
        m_clearButton = new Button();
        m_textBox = new TextBox();
        m_tableLayoutPanel.SuspendLayout();
        SuspendLayout();
        m_tableLayoutPanel.ColumnCount = 4;
        m_tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        m_tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        m_tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        m_tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        m_tableLayoutPanel.Controls.Add(m_sevenButton, 0, 1);
        m_tableLayoutPanel.Controls.Add(m_eightButton, 1, 1);
        m_tableLayoutPanel.Controls.Add(m_nineButton, 2, 1);
        m_tableLayoutPanel.Controls.Add(m_fourButton, 0, 2);
        m_tableLayoutPanel.Controls.Add(m_fiveButton, 1, 2);
        m_tableLayoutPanel.Controls.Add(m_sixButton, 2, 2);
        m_tableLayoutPanel.Controls.Add(m_oneButton, 0, 3);
        m_tableLayoutPanel.Controls.Add(m_twoButton, 1, 3);
        m_tableLayoutPanel.Controls.Add(m_threeButton, 2, 3);
        m_tableLayoutPanel.Controls.Add(m_zeroButton, 0, 4);
        m_tableLayoutPanel.Controls.Add(m_clearButton, 3, 1);
        m_tableLayoutPanel.Controls.Add(m_textBox, 0, 0);
        m_tableLayoutPanel.Dock = DockStyle.Fill;
        m_tableLayoutPanel.Location = new Point(0, 0);
        m_tableLayoutPanel.Name = "m_tableLayoutPanel";
        m_tableLayoutPanel.RowCount = 5;
        m_tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 13.04348f));
        m_tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 21.73913f));
        m_tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 21.73913f));
        m_tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 21.73913f));
        m_tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 21.73913f));
        m_tableLayoutPanel.Size = new Size(373, 222);
        m_tableLayoutPanel.TabIndex = 0;
        m_sevenButton.Dock = DockStyle.Fill;
        m_sevenButton.Location = new Point(3, 31);
        m_sevenButton.Name = "m_sevenButton";
        m_sevenButton.Size = new Size(87, 42);
        m_sevenButton.TabIndex = 1;
        m_sevenButton.Text = "7";
        m_sevenButton.UseVisualStyleBackColor = true;
        m_sevenButton.Click += OnNumberPadButtonClicked;
        m_eightButton.Dock = DockStyle.Fill;
        m_eightButton.Location = new Point(96, 31);
        m_eightButton.Name = "m_eightButton";
        m_eightButton.Size = new Size(87, 42);
        m_eightButton.TabIndex = 2;
        m_eightButton.Text = "8";
        m_eightButton.UseVisualStyleBackColor = true;
        m_eightButton.Click += OnNumberPadButtonClicked;
        m_nineButton.Dock = DockStyle.Fill;
        m_nineButton.Location = new Point(189, 31);
        m_nineButton.Name = "m_nineButton";
        m_nineButton.Size = new Size(87, 42);
        m_nineButton.TabIndex = 3;
        m_nineButton.Text = "9";
        m_nineButton.UseVisualStyleBackColor = true;
        m_nineButton.Click += OnNumberPadButtonClicked;
        m_fourButton.Dock = DockStyle.Fill;
        m_fourButton.Location = new Point(3, 79);
        m_fourButton.Name = "m_fourButton";
        m_fourButton.Size = new Size(87, 42);
        m_fourButton.TabIndex = 5;
        m_fourButton.Text = "4";
        m_fourButton.UseVisualStyleBackColor = true;
        m_fourButton.Click += OnNumberPadButtonClicked;
        m_fiveButton.Dock = DockStyle.Fill;
        m_fiveButton.Location = new Point(96, 79);
        m_fiveButton.Name = "m_fiveButton";
        m_fiveButton.Size = new Size(87, 42);
        m_fiveButton.TabIndex = 6;
        m_fiveButton.Text = "5";
        m_fiveButton.UseVisualStyleBackColor = true;
        m_fiveButton.Click += OnNumberPadButtonClicked;
        m_sixButton.Dock = DockStyle.Fill;
        m_sixButton.Location = new Point(189, 79);
        m_sixButton.Name = "m_sixButton";
        m_sixButton.Size = new Size(87, 42);
        m_sixButton.TabIndex = 7;
        m_sixButton.Text = "6";
        m_sixButton.UseVisualStyleBackColor = true;
        m_sixButton.Click += OnNumberPadButtonClicked;
        m_oneButton.Dock = DockStyle.Fill;
        m_oneButton.Location = new Point(3, sbyte.MaxValue);
        m_oneButton.Name = "m_oneButton";
        m_oneButton.Size = new Size(87, 42);
        m_oneButton.TabIndex = 8;
        m_oneButton.Text = "1";
        m_oneButton.UseVisualStyleBackColor = true;
        m_oneButton.Click += OnNumberPadButtonClicked;
        m_twoButton.Dock = DockStyle.Fill;
        m_twoButton.Location = new Point(96, sbyte.MaxValue);
        m_twoButton.Name = "m_twoButton";
        m_twoButton.Size = new Size(87, 42);
        m_twoButton.TabIndex = 9;
        m_twoButton.Text = "2";
        m_twoButton.UseVisualStyleBackColor = true;
        m_twoButton.Click += OnNumberPadButtonClicked;
        m_threeButton.Dock = DockStyle.Fill;
        m_threeButton.Location = new Point(189, sbyte.MaxValue);
        m_threeButton.Name = "m_threeButton";
        m_threeButton.Size = new Size(87, 42);
        m_threeButton.TabIndex = 10;
        m_threeButton.Text = "3";
        m_threeButton.UseVisualStyleBackColor = true;
        m_threeButton.Click += OnNumberPadButtonClicked;
        m_tableLayoutPanel.SetColumnSpan(m_zeroButton, 3);
        m_zeroButton.Dock = DockStyle.Fill;
        m_zeroButton.Location = new Point(3, 175);
        m_zeroButton.Name = "m_zeroButton";
        m_zeroButton.Size = new Size(273, 44);
        m_zeroButton.TabIndex = 11;
        m_zeroButton.Text = "0";
        m_zeroButton.UseVisualStyleBackColor = true;
        m_zeroButton.Click += OnNumberPadButtonClicked;
        m_clearButton.Dock = DockStyle.Fill;
        m_clearButton.Location = new Point(282, 31);
        m_clearButton.Name = "m_clearButton";
        m_clearButton.Size = new Size(88, 42);
        m_clearButton.TabIndex = 4;
        m_clearButton.Text = "C";
        m_clearButton.UseVisualStyleBackColor = true;
        m_clearButton.Click += OnNumberPadButtonClicked;
        m_tableLayoutPanel.SetColumnSpan(m_textBox, 4);
        m_textBox.Dock = DockStyle.Fill;
        m_textBox.Location = new Point(3, 3);
        m_textBox.Name = "m_textBox";
        m_textBox.Size = new Size(367, 20);
        m_textBox.TabIndex = 0;
        m_textBox.TextAlign = HorizontalAlignment.Right;
        m_textBox.TextChanged += OnTextChanged;
        m_textBox.KeyDown += OnKeyDown;
        m_textBox.KeyPress += OnKeyPress;
        AutoScaleDimensions = new SizeF(6f, 13f);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(m_tableLayoutPanel);
        Name = nameof(NumberPad);
        Size = new Size(373, 222);
        m_tableLayoutPanel.ResumeLayout(false);
        m_tableLayoutPanel.PerformLayout();
        ResumeLayout(false);
    }
}