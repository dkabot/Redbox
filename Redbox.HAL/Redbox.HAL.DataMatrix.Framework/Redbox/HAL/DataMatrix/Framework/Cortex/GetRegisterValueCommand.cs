using System;
using System.Globalization;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    internal sealed class GetRegisterValueCommand : TextCommand
    {
        private readonly CortexRegister Register;

        internal GetRegisterValueCommand(CortexRegister register)
        {
            Register = register;
        }

        protected override string Prefix => "G";

        protected override string Data => string.Format("({0})", Register.Register);

        internal bool IsCorrectlySet => OnCheckExpected();

        private bool OnCheckExpected()
        {
            var parsedPackets = ParsedPackets;
            if (parsedPackets.Length != 0 && parsedPackets[0].RawPacketType == 'd')
            {
                var num = int.Parse(Register.Value);
                var s = Encoding.ASCII.GetString(parsedPackets[0].PayloadData, 0, 8);
                try
                {
                    if (int.Parse(s, NumberStyles.HexNumber) == num)
                        return true;
                    LogHelper.Instance.Log("[GetRegisterValue] register {0} expected value {1} found {2}",
                        Register.Register, Register.Value, s);
                    return false;
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log(
                        string.Format(
                            "[GetRegisterValue] caught an unhandled exception for register {0} ( payload = {1} )",
                            Register, s), ex);
                    return false;
                }
            }

            LogHelper.Instance.Log("The response packet for GetRegister is malformed ( found '{0}' packet ).",
                parsedPackets.Length == 0 ? "nil" : (object)parsedPackets[0].RawPacketType.ToString());
            return false;
        }
    }
}