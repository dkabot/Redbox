using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console
{
    public class SensorView : UserControl
    {
        private static SensorView m_instance;
        private IContainer components;
        private Panel m_panel;
        private Button m_sensorReadButton;
        private bool[] m_sensors = new bool[6];

        private SensorView()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            m_sensorReadButton.Click += CommonFunctions.OnReadPickerSensors;
            m_sensorReadButton.EnabledChanged += OnSensorButtonEnableStateChanged;
            EnvironmentHelper.ExecutingImmediateStausChanged += OnExecuteImmediateStatusChanged;
            m_panel.BackColor = Color.WhiteSmoke;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool[] Sensors
        {
            get => m_sensors;
            set
            {
                m_sensors = value;
                RefreshSensors();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static SensorView Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new SensorView();
                return m_instance;
            }
        }

        public void ResetSensors()
        {
            for (var index = 0; index < Sensors.Length; ++index)
                Sensors[index] = false;
            RefreshSensors();
        }

        public void RefreshSensors()
        {
            m_panel.Invalidate();
        }

        private void OnExecuteImmediateStatusChanged(object sender, BoolEventArgs e)
        {
            m_sensorReadButton.Enabled = !e.State;
        }

        private void OnSensorButtonEnableStateChanged(object sender, EventArgs e)
        {
            if (m_sensorReadButton.Enabled)
                m_sensorReadButton.FlatStyle = FlatStyle.Standard;
            else
                m_sensorReadButton.FlatStyle = FlatStyle.Flat;
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(SystemBrushes.ControlLightLight, 0, 0, m_panel.Width, m_panel.Height);
            var num1 = 1;
            var num2 = 28;
            var num3 = Math.Max(m_panel.Width, 180) / Sensors.Length;
            var x = (num3 - num2) / 2;
            var y = m_panel.Height / 2f;
            if (m_panel.Width >= num2 * 6 + num3 * 6)
                ;
            foreach (var sensor in Sensors)
            {
                var num4 = sensor ? 1 : 0;
                e.Graphics.DrawString(num1.ToString(), Font, Brushes.Black, x + 8, y - 19f);
                e.Graphics.FillRectangle(Brushes.DarkGray, x + 2, y + 2f, num2, num2);
                e.Graphics.FillRectangle(Brushes.Black, x, y, num2, num2);
                if (num4 != 0)
                    e.Graphics.FillRectangle(Brushes.Green, x, y, num2, num2);
                else
                    e.Graphics.DrawRectangle(Pens.CornflowerBlue, x, y, num2, num2);
                x += num3;
                ++num1;
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            m_panel.Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            m_panel = new Panel();
            m_sensorReadButton = new Button();
            SuspendLayout();
            m_panel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            m_panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            m_panel.BackColor = Color.WhiteSmoke;
            m_panel.Location = new Point(0, 0);
            m_panel.Name = "m_panel";
            m_panel.Size = new Size(150, 104);
            m_panel.TabIndex = 0;
            m_panel.Paint += OnPaint;
            m_sensorReadButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            m_sensorReadButton.BackColor = Color.LightGray;
            m_sensorReadButton.Location = new Point(0, 110);
            m_sensorReadButton.Name = "m_sensorReadButton";
            m_sensorReadButton.Size = new Size(147, 37);
            m_sensorReadButton.TabIndex = 1;
            m_sensorReadButton.Text = "Read Sensors";
            m_sensorReadButton.UseVisualStyleBackColor = false;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = Color.WhiteSmoke;
            Controls.Add(m_sensorReadButton);
            Controls.Add(m_panel);
            Name = nameof(SensorView);
            Resize += OnResize;
            ResumeLayout(false);
        }
    }
}