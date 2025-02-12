using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Client;

namespace Redbox.HAL.Common.GUI.Functions
{
    public class PickerSensorsBar : UserControl
    {
        public delegate void BarToggle(string msg, string response);

        public delegate void SensorOperationHandler(bool readError);

        private readonly ButtonAspectsManager ButtonManager;
        private readonly Dictionary<int, Panel> m_sensors = new Dictionary<int, Panel>();
        private IContainer components;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Button m_offButton;
        private Button m_onButton;
        private Button m_readButton;
        private Panel m_sensor1Panel;
        private Panel m_sensor2Panel;
        private Panel m_sensor3Panel;
        private Panel m_sensor4Panel;
        private Panel m_sensor5Panel;
        private Panel m_sensor6Panel;

        public PickerSensorsBar(HardwareService service)
        {
            InitializeComponent();
            m_sensors[0] = m_sensor1Panel;
            m_sensors[1] = m_sensor2Panel;
            m_sensors[2] = m_sensor3Panel;
            m_sensors[3] = m_sensor4Panel;
            m_sensors[4] = m_sensor5Panel;
            m_sensors[5] = m_sensor6Panel;
            Service = service;
            ButtonManager = new ButtonAspectsManager();
        }

        public HardwareService Service { get; set; }

        public event SensorOperationHandler ReadEvents;

        public event BarToggle BarEvents;

        private void ExecuteBarInstruction(object sender, EventArgs e)
        {
            using (ButtonManager.MakeAspect(sender))
            {
                var tag = ((Control)sender).Tag as string;
                var job = CommonFunctions.ExecuteInstruction(Service, tag);
                if (job == null)
                    return;
                var topOfStack = job.GetTopOfStack();
                if (BarEvents == null)
                    return;
                BarEvents(tag, topOfStack);
            }
        }

        private void Read_Click(object sender, EventArgs e)
        {
            using (ButtonManager.MakeAspect(sender))
            {
                ReadSensorsInner();
            }
        }

        private void ReadSensorsInner()
        {
            var readError = false;
            try
            {
                var hardwareJob = CommonFunctions.ExecuteInstruction(Service, "SENSOR READ PICKER-SENSOR=1..6");
                if (hardwareJob == null)
                    return;
                Stack<string> stack;
                if (!hardwareJob.GetStack(out stack).Success)
                {
                    readError = true;
                }
                else if (stack.Count == 0)
                {
                    readError = true;
                }
                else if (int.Parse(stack.Pop()) == 0)
                {
                    readError = true;
                }
                else
                {
                    var sensors = new bool[6];
                    for (var index = 5; index >= 0; --index)
                    {
                        var str = stack.Pop();
                        sensors[index] = str.Contains("BLOCKED");
                    }

                    DisplaySensors(sensors);
                }
            }
            finally
            {
                if (ReadEvents != null)
                    ReadEvents(readError);
            }
        }

        private void DisplaySensors(bool[] sensors)
        {
            for (var key = 0; key < sensors.Length; ++key)
                m_sensors[key].BackColor = sensors[key] ? Color.Green : Color.WhiteSmoke;
            Application.DoEvents();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_sensor1Panel = new Panel();
            m_sensor2Panel = new Panel();
            m_sensor3Panel = new Panel();
            m_sensor4Panel = new Panel();
            m_sensor5Panel = new Panel();
            m_sensor6Panel = new Panel();
            m_readButton = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            m_onButton = new Button();
            m_offButton = new Button();
            SuspendLayout();
            m_sensor1Panel.BorderStyle = BorderStyle.FixedSingle;
            m_sensor1Panel.Enabled = false;
            m_sensor1Panel.Location = new Point(14, 16);
            m_sensor1Panel.Name = "m_sensor1Panel";
            m_sensor1Panel.Size = new Size(40, 35);
            m_sensor1Panel.TabIndex = 0;
            m_sensor2Panel.BorderStyle = BorderStyle.FixedSingle;
            m_sensor2Panel.Location = new Point(70, 16);
            m_sensor2Panel.Name = "m_sensor2Panel";
            m_sensor2Panel.Size = new Size(40, 35);
            m_sensor2Panel.TabIndex = 1;
            m_sensor3Panel.BorderStyle = BorderStyle.FixedSingle;
            m_sensor3Panel.Location = new Point(sbyte.MaxValue, 16);
            m_sensor3Panel.Name = "m_sensor3Panel";
            m_sensor3Panel.Size = new Size(40, 35);
            m_sensor3Panel.TabIndex = 2;
            m_sensor4Panel.BorderStyle = BorderStyle.FixedSingle;
            m_sensor4Panel.Location = new Point(184, 16);
            m_sensor4Panel.Name = "m_sensor4Panel";
            m_sensor4Panel.Size = new Size(40, 35);
            m_sensor4Panel.TabIndex = 3;
            m_sensor5Panel.BorderStyle = BorderStyle.FixedSingle;
            m_sensor5Panel.Location = new Point(240, 16);
            m_sensor5Panel.Name = "m_sensor5Panel";
            m_sensor5Panel.Size = new Size(40, 35);
            m_sensor5Panel.TabIndex = 4;
            m_sensor6Panel.BorderStyle = BorderStyle.FixedSingle;
            m_sensor6Panel.Location = new Point(295, 16);
            m_sensor6Panel.Name = "m_sensor6Panel";
            m_sensor6Panel.Size = new Size(40, 35);
            m_sensor6Panel.TabIndex = 5;
            m_readButton.BackColor = Color.LightGray;
            m_readButton.Location = new Point(14, 57);
            m_readButton.Name = "m_readButton";
            m_readButton.Size = new Size(75, 45);
            m_readButton.TabIndex = 6;
            m_readButton.Text = "Read";
            m_readButton.UseVisualStyleBackColor = false;
            m_readButton.Click += Read_Click;
            label1.AutoSize = true;
            label1.Location = new Point(30, 0);
            label1.Name = "label1";
            label1.Size = new Size(13, 13);
            label1.TabIndex = 9;
            label1.Text = "1";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            label2.AutoSize = true;
            label2.Location = new Point(86, 0);
            label2.Name = "label2";
            label2.Size = new Size(13, 13);
            label2.TabIndex = 10;
            label2.Text = "2";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            label3.AutoSize = true;
            label3.Location = new Point(140, 0);
            label3.Name = "label3";
            label3.Size = new Size(13, 13);
            label3.TabIndex = 11;
            label3.Text = "3";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            label4.AutoSize = true;
            label4.Location = new Point(198, 0);
            label4.Name = "label4";
            label4.Size = new Size(13, 13);
            label4.TabIndex = 12;
            label4.Text = "4";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            label5.AutoSize = true;
            label5.Location = new Point(byte.MaxValue, 0);
            label5.Name = "label5";
            label5.Size = new Size(13, 13);
            label5.TabIndex = 13;
            label5.Text = "5";
            label5.TextAlign = ContentAlignment.MiddleCenter;
            label6.AutoSize = true;
            label6.Location = new Point(311, 0);
            label6.Name = "label6";
            label6.Size = new Size(13, 13);
            label6.TabIndex = 14;
            label6.Text = "6";
            label6.TextAlign = ContentAlignment.MiddleCenter;
            m_onButton.BackColor = Color.LightGray;
            m_onButton.Location = new Point(sbyte.MaxValue, 57);
            m_onButton.Name = "m_onButton";
            m_onButton.Size = new Size(75, 45);
            m_onButton.TabIndex = 15;
            m_onButton.Tag = "SENSOR PICKER-ON";
            m_onButton.Text = "On";
            m_onButton.UseVisualStyleBackColor = false;
            m_onButton.Click += ExecuteBarInstruction;
            m_offButton.BackColor = Color.LightGray;
            m_offButton.Location = new Point(240, 57);
            m_offButton.Name = "m_offButton";
            m_offButton.Size = new Size(75, 45);
            m_offButton.TabIndex = 16;
            m_offButton.Tag = "SENSOR PICKER-OFF";
            m_offButton.Text = "Off";
            m_offButton.UseVisualStyleBackColor = false;
            m_offButton.Click += ExecuteBarInstruction;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(m_offButton);
            Controls.Add(m_onButton);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(m_readButton);
            Controls.Add(m_sensor6Panel);
            Controls.Add(m_sensor5Panel);
            Controls.Add(m_sensor4Panel);
            Controls.Add(m_sensor3Panel);
            Controls.Add(m_sensor2Panel);
            Controls.Add(m_sensor1Panel);
            Name = nameof(PickerSensorsBar);
            Size = new Size(349, 113);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}