using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal struct MotorWaitDecoder
    {
        internal int StatusCode { get; private set; }

        internal ErrorCodes Error { get; private set; }

        internal string Diagnostic
        {
            get
            {
                switch (StatusCode)
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
                        return StatusCode.ToString();
                }
            }
        }

        internal string FormatError(Axis axis, string response)
        {
            return string.Format("Unable to reach target on axis {0}. Response {1} ( diagnostic = {2} )",
                axis.ToString().ToUpper(), response, Diagnostic);
        }

        internal bool MotorRunning(string response, MoveOperation operation)
        {
            int result;
            if (!int.TryParse(response, out result))
            {
                StatusCode = 999;
                Error = ErrorCodes.MotorError;
                return false;
            }

            StatusCode = result;
            Error = ErrorCodes.Success;
            var flag = StatusCode != 0;
            if (16 == StatusCode)
            {
                if (operation == MoveOperation.Dropback)
                {
                    Error = ErrorCodes.Success;
                    StatusCode = 0;
                    flag = false;
                }
                else if (MoveOperation.Normal == operation)
                {
                    Error = ErrorCodes.LowerLimitError;
                    flag = false;
                }
            }

            return flag;
        }
    }
}