using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal struct ArcusMotorStatus
    {
        private MoveOperation Operation;

        internal int ArcusStatusCode { get; private set; }

        internal ErrorCodes Error { get; private set; }

        internal int Timeout { get; private set; }

        internal string Diagnostic
        {
            get
            {
                switch (ArcusStatusCode)
                {
                    case 0:
                        return "Motor stopped";
                    case 1:
                        return "Accelerating";
                    case 2:
                        return "Decelerating";
                    case 4:
                        return "Constant speed";
                    case 8:
                        return "Upper Limit Error";
                    case 16:
                        return "Lower Limit Error";
                    default:
                        return ArcusStatusCode.ToString();
                }
            }
        }

        internal bool MotorRunning { get; private set; }

        internal Axis[] Axes { get; private set; }

        internal void TestResponse(ArcusCommandResponse response)
        {
            TestResponse(response.Response);
        }

        internal void TestResponse(string rawResponse)
        {
            MotorRunning = true;
            int result;
            if (!int.TryParse(rawResponse, out result))
            {
                ArcusStatusCode = 999;
                MotorRunning = false;
                Error = ErrorCodes.MotorError;
            }
            else
            {
                ArcusStatusCode = result;
                Error = ErrorCodes.Success;
                MotorRunning = ArcusStatusCode != 0;
                switch (Operation)
                {
                    case MoveOperation.Dropback:
                        if (16 != ArcusStatusCode)
                            break;
                        Error = ErrorCodes.Success;
                        ArcusStatusCode = 0;
                        MotorRunning = false;
                        break;
                    case MoveOperation.Normal:
                        if (16 != ArcusStatusCode)
                            break;
                        Error = ErrorCodes.LowerLimitError;
                        MotorRunning = false;
                        break;
                }
            }
        }

        internal ArcusMotorStatus(string instruction, int timeout)
            : this()
        {
            Operation = MoveOperation.Normal;
            Timeout = timeout;
            Decode(instruction);
        }

        private ErrorCodes ComputeError()
        {
            if (ArcusStatusCode < 8)
                return ErrorCodes.Success;
            if (8 == ArcusStatusCode)
                return ErrorCodes.UpperLimitError;
            return 16 == ArcusStatusCode ? ErrorCodes.LowerLimitError : ErrorCodes.MotorError;
        }

        private void Decode(string waitCommand)
        {
            if (waitCommand.Equals("WAITYDB", StringComparison.CurrentCultureIgnoreCase))
            {
                Operation = MoveOperation.Dropback;
                Axes = new Axis[1] { Axis.Y };
            }
            else
            {
                ComputeAxis(!waitCommand.StartsWith("WAITH")
                    ? waitCommand.Substring("WAIT".Length)
                    : waitCommand.Substring("WAITH".Length));
            }
        }

        private void ComputeAxis(string axisAsString)
        {
            if ("X".Equals(axisAsString.ToUpper()))
                Axes = new Axis[1];
            else if ("Y".Equals(axisAsString.ToUpper()))
                Axes = new Axis[1] { Axis.Y };
            else if ("XY".Equals(axisAsString.ToUpper()))
                Axes = new Axis[2] { Axis.Y, Axis.X };
            else
                Axes = null;
        }
    }
}