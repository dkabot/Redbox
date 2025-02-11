using System.ComponentModel;
using System.Xml;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Controller.Framework
{
    public sealed class MotorSpeed
    {
        public MotorSpeed(int low, int high, int accelerationTime, MotorGear gear)
        {
            Low = low;
            High = high;
            Gear = gear;
            AccelerationTime = accelerationTime;
        }

        [DisplayName("Low Speed")]
        [Description("The low (start) speed of the motor specified in pulses per second.")]
        public int Low { get; }

        [DisplayName("High Speed")]
        [Description("The high (target) speed of the motor specified in pulses per second.")]
        public int High { get; }

        [Browsable(false)] public MotorGear Gear { get; }

        [DisplayName("Acceleration Time (in milliseconds)")]
        [Description("The amount of time in milliseconds between acceleration from the low speed to the high speed.")]
        public int AccelerationTime { get; }

        public override string ToString()
        {
            return "(Motor Speed Properties)";
        }

        public static MotorSpeed FromXmlNode(XmlNode node, MotorGear gear)
        {
            var nodeValue1 = node.SelectSingleNode("Low").GetNodeValue(500);
            var nodeValue2 = node.SelectSingleNode("High").GetNodeValue(3000);
            var nodeValue3 = node.SelectSingleNode("AccelerationTime").GetNodeValue(300);
            var high = nodeValue2;
            var accelerationTime = nodeValue3;
            var gear1 = gear;
            return new MotorSpeed(nodeValue1, high, accelerationTime, gear1);
        }

        public void SaveToXml(XmlNode node)
        {
            if (node == null)
                return;
            node.SelectSingleNodeAndSetValue("Low", Low);
            node.SelectSingleNodeAndSetValue("High", High);
            node.SelectSingleNodeAndSetValue("AccelerationTime", AccelerationTime);
        }

        public string GetHighSpeedCommand()
        {
            return string.Format("HSPD {0}", High * Gear.GetStepRatio());
        }

        public string GetLowSpeedCommand()
        {
            return string.Format("LSPD {0}", Low * Gear.GetStepRatio());
        }

        public string GetAccelerationCommand()
        {
            return string.Format("ACCEL {0}", AccelerationTime);
        }
    }
}