using System.Collections.Generic;
using Redbox.HAL.Client;

namespace Redbox.HAL.Common.GUI.Functions
{
    public sealed class MoveHelper
    {
        private readonly HardwareService Service;
        private readonly int Timeout;

        public MoveHelper(HardwareService service, int timeout)
        {
            Service = service;
            Timeout = timeout;
        }

        public MoveHelper(HardwareService service)
            : this(service, 64000)
        {
        }

        public MoveHelperResult MoveTo(int deck, int slot, string mode)
        {
            HardwareJob job;
            return new MoveHelperResult(
                Service.ExecuteImmediate(string.Format("MOVE DECK={0} SLOT={1} MODE={2}", deck, slot, mode), Timeout,
                    out job).Success
                    ? job.GetTopOfStack()
                    : "SERVICE DOWN");
        }

        public MoveHelperResult MoveTo(int deck, int slot)
        {
            return MoveTo(deck, slot, "NORMAL");
        }

        public MoveHelperResult MoveTo(int deck, int slot, bool readLimits)
        {
            var moveHelperResult = MoveTo(deck, slot);
            if (readLimits)
                moveHelperResult.Limits = ReadLimits();
            return moveHelperResult;
        }

        public void GetPosition(ref MotorPosition position)
        {
            HardwareJob job;
            var hardwareCommandResult = Service.ExecuteImmediate(
                string.Format("MOTIONCONTROL {0}", position.Axis == MoveAxis.X ? "XPOS" : (object)"YPOS"), out job);
            position.ReadOk = hardwareCommandResult.Success;
            Stack<string> stack;
            if (!hardwareCommandResult.Success || !job.GetStack(out stack).Success || stack.Count < 1)
                return;
            int result;
            if (int.TryParse(stack.Pop(), out result))
            {
                position.ReadOk = true;
                position.Position = result;
            }
            else
            {
                position.ReadOk = false;
            }
        }

        public MoveHelperResult MoveAbs(MoveAxis axis, int units)
        {
            return MoveAbs(axis, units, true);
        }

        public MoveHelperResult MoveAbs(MoveAxis axis, int units, bool checkSensors)
        {
            HardwareJob job;
            return new MoveHelperResult(
                Service.ExecuteImmediate(
                    string.Format("MOVEABS AXIS={0} UNITS={1} SENSOR-CHECK={2}", axis.ToString().ToUpper(),
                        units.ToString(), checkSensors.ToString().ToUpper()), Timeout, out job).Success
                    ? job.GetTopOfStack()
                    : "SERVICE DOWN");
        }

        public string[] ReadLimits()
        {
            var stringList = new List<string>();
            var hardwareJob = CommonFunctions.ExecuteInstruction(Service, "MOTIONCONTROL READLIMITS");
            Stack<string> stack;
            if (hardwareJob == null || hardwareJob.GetStack(out stack).Errors.Count > 0)
                return stringList.ToArray();
            var s = stack.Peek();
            try
            {
                var num = int.Parse(s);
                stack.Pop();
                for (var index = 0; index < num; ++index)
                    stringList.Add(stack.Pop());
            }
            catch
            {
                stringList.Clear();
            }

            return stringList.ToArray();
        }
    }
}