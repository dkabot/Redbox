using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Common.GUI.Functions
{
    public class IncrementalMoveForm : Form
    {
        private readonly ILogger Log;
        private Button button1;
        private Button button2;
        private Button button3;
        private IContainer components;
        private Label label1;
        private ErrorProvider m_errorProvider;
        private TextBox m_unitsTextBox;
        private RadioButton m_xAxisRadio;
        private RadioButton m_yAxisRadio;
        private TextBox textBox1;

        public IncrementalMoveForm()
        {
            InitializeComponent();
        }

        public IncrementalMoveForm(HardwareService service)
            : this()
        {
            Service = service;
            Log = ServiceLocator.Instance.GetService<ILogger>();
        }

        public int Units
        {
            get
            {
                int result;
                if (int.TryParse(m_unitsTextBox.Text, out result))
                    return result;
                m_errorProvider.SetError(m_unitsTextBox, "Please type a number.");
                return 0;
            }
        }

        public string Axis => m_xAxisRadio.Checked ? "X" : "Y";

        private HardwareService Service { get; }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void IncrementalMoveForm_Load(object sender, EventArgs e)
        {
        }

        private void m_xAxisRadio_CheckedChanged_1(object s, EventArgs e)
        {
        }

        private void button3_Click(object sender, EventArgs e)
        {
            m_errorProvider.Clear();
            var ignoringCase = Enum<MoveAxis>.ParseIgnoringCase(Axis, MoveAxis.X);
            try
            {
                if (MoveAxis.Y == ignoringCase)
                {
                    if (Math.Abs(Units) > 200)
                    {
                        m_errorProvider.SetError(m_unitsTextBox, "Move cannot be more than 200 units.");
                        return;
                    }
                }
                else if (ignoringCase == MoveAxis.X && Math.Abs(Units) > 50)
                {
                    m_errorProvider.SetError(m_unitsTextBox, "Move cannot be more than 50 units.");
                    return;
                }

                var position = new MotorPosition(ignoringCase);
                var moveHelper = new MoveHelper(Service);
                moveHelper.GetPosition(ref position);
                if (!position.ReadOk)
                    return;
                var units = Units + position.Position;
                if (moveHelper.MoveAbs(ignoringCase, units, false).MoveOk)
                    return;
                Log.Log("Execute immediate failed ... ", LogEntryType.Error);
            }
            catch (Exception ex)
            {
                Log.Log("*** exception ***", ex);
            }
        }

        private void IncrementalMoveForm_Load_1(object sender, EventArgs e)
        {
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
            label1 = new Label();
            m_unitsTextBox = new TextBox();
            button1 = new Button();
            button2 = new Button();
            m_errorProvider = new ErrorProvider(components);
            m_xAxisRadio = new RadioButton();
            m_yAxisRadio = new RadioButton();
            button3 = new Button();
            textBox1 = new TextBox();
            ((ISupportInitialize)m_errorProvider).BeginInit();
            SuspendLayout();
            label1.AutoSize = true;
            label1.Location = new Point(16, 85);
            label1.Name = "label1";
            label1.Size = new Size(31, 13);
            label1.TabIndex = 0;
            label1.Text = "Units";
            m_unitsTextBox.Location = new Point(53, 78);
            m_unitsTextBox.Name = "m_unitsTextBox";
            m_unitsTextBox.Size = new Size(87, 20);
            m_unitsTextBox.TabIndex = 1;
            button1.Location = new Point(19, 136);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 5;
            button1.Text = "Done";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            button2.Location = new Point(111, 136);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 6;
            button2.Text = "Cancel";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            m_errorProvider.ContainerControl = this;
            m_xAxisRadio.AutoSize = true;
            m_xAxisRadio.Checked = true;
            m_xAxisRadio.Location = new Point(19, 101);
            m_xAxisRadio.Name = "m_xAxisRadio";
            m_xAxisRadio.Size = new Size(53, 17);
            m_xAxisRadio.TabIndex = 7;
            m_xAxisRadio.TabStop = true;
            m_xAxisRadio.Text = "X axis";
            m_xAxisRadio.UseVisualStyleBackColor = true;
            m_yAxisRadio.AutoSize = true;
            m_yAxisRadio.Location = new Point(87, 101);
            m_yAxisRadio.Name = "m_yAxisRadio";
            m_yAxisRadio.Size = new Size(53, 17);
            m_yAxisRadio.TabIndex = 8;
            m_yAxisRadio.TabStop = true;
            m_yAxisRadio.Text = "Y axis";
            m_yAxisRadio.UseVisualStyleBackColor = true;
            button3.Location = new Point(146, 76);
            button3.Name = "button3";
            button3.Size = new Size(75, 23);
            button3.TabIndex = 9;
            button3.Text = "Move";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            textBox1.BackColor = Color.Yellow;
            textBox1.Location = new Point(19, 12);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.Size = new Size(212, 48);
            textBox1.TabIndex = 10;
            textBox1.Text =
                "WARNING: clicking 'MOVE' will move the drum or picker *WITHOUT PERFORMING A SENSOR CHECK*!";
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(275, 171);
            Controls.Add(textBox1);
            Controls.Add(button3);
            Controls.Add(m_yAxisRadio);
            Controls.Add(m_xAxisRadio);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(m_unitsTextBox);
            Controls.Add(label1);
            Name = nameof(IncrementalMoveForm);
            Text = "Move Units Form";
            Load += IncrementalMoveForm_Load_1;
            ((ISupportInitialize)m_errorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}