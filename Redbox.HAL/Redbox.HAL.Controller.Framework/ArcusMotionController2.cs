using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class ArcusMotionController2 : AbstractMotionController
    {
        private const string HaltMotorCommand = "RSTOP";
        private const string NopCommand = "NOP";
        private readonly IDeviceDescriptor ArcusDescriptor;
        private readonly List<string> ErrorCorrectPreamble = new List<string>();
        private readonly List<string> InitCommands = new List<string>();

        private readonly string[] InvalidQueryResponse = new string[1]
        {
            "INVALID QUERY RESPONSE"
        };

        private readonly List<string> MovePreamble = new List<string>();
        private readonly ICommPort Port;
        private bool DebugEnabled;
        private bool UseSmoothMove;

        internal ArcusMotionController2()
        {
            ArcusDescriptor = new ArcusDeviceDescriptor(ServiceLocator.Instance.GetService<IUsbDeviceService>());
            var port = new SerialPort(ControllerConfiguration.Instance.MotionControllerPortName, 115200, Parity.None, 8,
                StopBits.One);
            Port = ServiceLocator.Instance.GetService<IPortManagerService>().Create(port);
            Port.WriteTerminator = new byte[2]
            {
                13,
                0
            };
            Port.WritePause = ControllerConfiguration.Instance.ArcusWritePause;
            Port.WriteTimeout = ControllerConfiguration.Instance.MotionControllerTimeout;
            Port.ValidateResponse = response => response.GetIndex(4) != -1;
            Port.DisplayName = "Motion Control";
            Port.EnableDebugging = DebugEnabled;
            LogHelper.Instance.Log("[ArcusMotionController2] ctor");
        }

        internal override void OnConfigurationLoad()
        {
            LogHelper.Instance.Log("[ArcusMotionControl] Notify configuration loaded.");
            ComputeMoveCommands();
            UseSmoothMove = ControllerConfiguration.Instance.ArcusSmoothMove;
            DebugEnabled = ControllerConfiguration.Instance.EnableArcusTrace;
        }

        internal override void OnConfigurationChangeStart()
        {
        }

        internal override void OnConfigurationChangeEnd()
        {
            LogHelper.Instance.Log("[ArcusMotionControl] On configuration end.");
            ComputeMoveCommands();
            UseSmoothMove = ControllerConfiguration.Instance.ArcusSmoothMove;
            DebugEnabled = ControllerConfiguration.Instance.EnableArcusTrace;
        }

        internal override ErrorCodes MoveToTarget(ref MoveTarget target)
        {
            var errorCodes = ClearYMotorError();
            return errorCodes != ErrorCodes.Success ? errorCodes : MoveAbsolute(ref target);
        }

        internal override ErrorCodes HomeAxis(Axis axis)
        {
            return axis != Axis.X ? HomeYAxis() : HomeXAxis();
        }

        internal override ErrorCodes MoveToVend(MoveMode mode)
        {
            var vendYposition = ControllerConfiguration.Instance.VendYPosition;
            if (ControllerConfiguration.Instance.VendPositionReceiveOffset != 0)
                switch (mode)
                {
                    case MoveMode.Put:
                        vendYposition += ControllerConfiguration.Instance.VendPositionReceiveOffset;
                        break;
                    case MoveMode.Get:
                        vendYposition -= ControllerConfiguration.Instance.VendPositionReceiveOffset;
                        break;
                }

            if (ControllerConfiguration.Instance.QueryPositionForVendMove)
            {
                var controllerPosition = ReadPositions();
                if (controllerPosition.ReadOk && controllerPosition.YCoordinate.Value == vendYposition)
                {
                    if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
                        LogHelper.Instance.Log("[ArcusMotionController2] Picker currently at position Y = {0}",
                            controllerPosition.YCoordinate.Value);
                    return ErrorCodes.Success;
                }
            }

            var vend = ClearYMotorError();
            if (vend != ErrorCodes.Success)
                return vend;
            var target = new MoveTarget
            {
                Axis = Axis.Y,
                YCoordinate = vendYposition
            };
            return MoveAbsolute(ref target);
        }

        internal override bool CommunicationOk()
        {
            var str = SendReciveRaw("$");
            if (str == null)
                return false;
            LogHelper.Instance.Log(LogEntryType.Debug, "Arcus comm ok: command returns {0}", str);
            return str.StartsWith("OK");
        }

        internal override IControllerPosition ReadPositions()
        {
            var controllerPosition = new ArcusControllerPosition
            {
                XCoordinate = new int?(),
                YCoordinate = new int?()
            };
            controllerPosition.XCoordinate = DecodePositionResponse(Axis.X);
            controllerPosition.YCoordinate = DecodePositionResponse(Axis.Y);
            return controllerPosition;
        }

        internal override bool OnShutdown()
        {
            return Port.Close();
        }

        internal override IMotionControlLimitResponse ReadLimits()
        {
            var queryResponse = SendReciveRaw("???");
            var arcusLimitResponse = new ArcusLimitResponse(queryResponse);
            if (!arcusLimitResponse.ReadOk)
                WriteToLog("[ReadLimits] The response is incorrect {0}",
                    queryResponse == null ? "NONE" : (object)queryResponse);
            return arcusLimitResponse;
        }

        internal override bool OnResetDeviceDriver()
        {
            OnShutdown();
            var flag = ArcusDescriptor.ResetDriver();
            LogHelper.Instance.WithContext("Arcus device reset returned {0}", flag.ToString().ToUpper());
            return flag;
        }

        internal override bool OnStartup()
        {
            if (!Port.Open())
            {
                LogHelper.Instance.Log(LogEntryType.Error, "[ArcusMotionControl] Unable to open port {0}.",
                    Port.DisplayName);
                Port.Close();
                return false;
            }

            InitCommands.ForEach(each => SendReciveRaw(each));
            return true;
        }

        private void ComputeMoveCommands()
        {
            LogHelper.Instance.Log("Re-computing move commands.");
            InitCommands.Clear();
            InitCommands.AddRange(new string[19]
            {
                "$",
                "$",
                "$",
                "RSTOP",
                "MECLEARX",
                "MECLEARY",
                "MECLEARZ",
                "MECLEARU",
                "I1=4204544",
                "I2=3136",
                "I3=" + ControllerConfiguration.Instance.GearX.GetEncoderRatio(),
                "I4=0",
                "I5=0",
                "I6=0",
                "I7=10",
                "I8=0",
                "I9=2000",
                "I10=0",
                "I11=2"
            });
            ErrorCorrectPreamble.Clear();
            ErrorCorrectPreamble.Add(string.Format("LSPD {0}",
                (int)ControllerConfiguration.Instance.GearX.GetStepRatio() * 100));
            ErrorCorrectPreamble.Add(string.Format("I7={0}",
                ControllerConfiguration.Instance.WidenArcusTolerance ? 5 : 2));
            MovePreamble.Clear();
            MovePreamble.AddRange(new string[10]
            {
                "$",
                "RSTOP",
                "MECLEARY",
                "MECLEARX",
                ControllerConfiguration.Instance.MoveAxisXYSpeed.GetHighSpeedCommand(),
                ControllerConfiguration.Instance.MoveAxisXYSpeed.GetLowSpeedCommand(),
                ControllerConfiguration.Instance.MoveAxisXYSpeed.GetAccelerationCommand(),
                "ABS",
                "I7=15",
                "I2=3136"
            });
        }

        private ErrorCodes OnWaitMotor(int timeout, Axis axis)
        {
            return OnWaitMotor(timeout, axis, MoveOperation.Normal);
        }

        private ErrorCodes OnWaitMotor(int timeout, Axis axis, MoveOperation operation)
        {
            var service1 = ServiceLocator.Instance.GetService<IRuntimeService>();
            var timespan = new TimeSpan(0, 0, 0, 0, ControllerConfiguration.Instance.ArcusMotorQueryPause);
            var service2 = ServiceLocator.Instance.GetService<IDoorSensorService>();
            using (var executionTimer = new ExecutionTimer())
            {
                var command = "MST" + axis.ToString().ToUpper();
                string response;
                MotorWaitDecoder motorWaitDecoder;
                do
                {
                    var doorSensorResult = service2.Query();
                    if (doorSensorResult != DoorSensorResult.Ok)
                    {
                        WriteToLog(string.Format("Door sensor query returned {0}: halting motion.",
                            doorSensorResult.ToString()));
                        HaltMotor();
                        return ErrorCodes.DoorOpen;
                    }

                    response = SendReciveRaw(command);
                    if (response == null)
                        return ErrorCodes.ArcusNotResponsive;
                    motorWaitDecoder = new MotorWaitDecoder();
                    if (!motorWaitDecoder.MotorRunning(response, operation))
                    {
                        if (motorWaitDecoder.Error != ErrorCodes.Success)
                        {
                            var str = motorWaitDecoder.FormatError(axis, response);
                            LogHelper.Instance.Log(str, LogEntryType.Error);
                            WriteToLog(str);
                        }

                        return motorWaitDecoder.Error;
                    }

                    service1.SpinWait(timespan);
                } while (executionTimer.ElapsedMilliseconds < timeout);

                var str1 = motorWaitDecoder.FormatError(axis, response);
                LogHelper.Instance.Log(str1, LogEntryType.Error);
                WriteToLog(str1);
                HaltMotor();
                return ErrorCodes.Timeout;
            }
        }

        private string HaltMotor()
        {
            return SendReciveRaw("RSTOP");
        }

        private ErrorCodes HomeXAxis()
        {
            var strArray1 = new string[11]
            {
                "$",
                "RSTOP",
                "MECLEARX",
                "I1=4204544",
                ControllerConfiguration.Instance.InitAxisXSpeed.GetHighSpeedCommand(),
                ControllerConfiguration.Instance.InitAxisXSpeed.GetLowSpeedCommand(),
                ControllerConfiguration.Instance.InitAxisXSpeed.GetAccelerationCommand(),
                "INC",
                "EX=0",
                "PX=0",
                "I2=3072"
            };
            foreach (var command in strArray1)
                if (SendReciveRaw(command) == null)
                    return ErrorCodes.ArcusNotResponsive;
            var errorCodes1 = OnWaitMotor(5000, Axis.X);
            if (errorCodes1 != ErrorCodes.Success)
                return errorCodes1;
            if (SendReciveRaw("HOMEX-") == null)
                return ErrorCodes.ArcusNotResponsive;
            var errorCodes2 = OnWaitMotor(64000, Axis.X);
            if (errorCodes2 != ErrorCodes.Success)
                return errorCodes2;
            var strArray2 = new string[2]
            {
                "I2=3136",
                "I7=15"
            };
            foreach (var command in strArray2)
                if (SendReciveRaw(command) == null)
                    return ErrorCodes.ArcusNotResponsive;
            var errorCodes3 = OnWaitMotor(5000, Axis.X);
            if (errorCodes3 != ErrorCodes.Success)
                return errorCodes3;
            return SendReciveRaw("MECLEARX") != null ? ErrorCodes.Success : ErrorCodes.ArcusNotResponsive;
        }

        private ErrorCodes HomeYAxis()
        {
            var strArray = new string[9]
            {
                "$",
                "RSTOP",
                "MECLEARY",
                ControllerConfiguration.Instance.InitAxisYSpeed.GetHighSpeedCommand(),
                ControllerConfiguration.Instance.InitAxisYSpeed.GetLowSpeedCommand(),
                ControllerConfiguration.Instance.InitAxisYSpeed.GetAccelerationCommand(),
                "INC",
                "EY=0",
                "PY=0"
            };
            foreach (var command in strArray)
                if (SendReciveRaw(command) == null)
                    return ErrorCodes.ArcusNotResponsive;
            if (ControllerConfiguration.Instance.HomeYDropBack != 0)
            {
                var num = ControllerConfiguration.Instance.HomeYDropBack;
                if (Math.Sign(num) == 1)
                    num = -num;
                if (SendReciveRaw("MIOY") == null || SendReciveRaw(string.Format("Y{0}", num)) == null)
                    return ErrorCodes.ArcusNotResponsive;
                var errorCodes = OnWaitMotor(6000, Axis.Y, MoveOperation.Dropback);
                if (errorCodes != ErrorCodes.Success)
                    return errorCodes;
                if (SendReciveRaw("MECLEARY") == null)
                    return ErrorCodes.ArcusNotResponsive;
            }

            return SendReciveRaw("HOMEY+") != null ? OnWaitMotor(60000, Axis.Y) : ErrorCodes.ArcusNotResponsive;
        }

        private string SendReciveRaw(string command)
        {
            if (string.IsNullOrEmpty(command) || command.Equals("NOP", StringComparison.CurrentCultureIgnoreCase))
                return "OK";
            var flag = LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug) || DebugEnabled;
            var stringBuilder = flag ? new StringBuilder() : null;
            if (flag)
                stringBuilder.AppendFormat("ArcusMotionController: Instruction={0}", command);
            using (var channelResponse =
                   Port.SendRecv(command, ControllerConfiguration.Instance.MotionControllerTimeout))
            {
                if (!channelResponse.CommOk)
                {
                    LogHelper.Instance.Log("ArcusMotionControl.SendCommand(): channel error returned {0}",
                        channelResponse.Error);
                    WriteToLog("[ArcusMotionController] Send command {0}; error = {1}", command,
                        ErrorCodes.ArcusNotResponsive.ToString());
                    if (flag)
                    {
                        stringBuilder.AppendFormat(", Error = {0}", ErrorCodes.ArcusNotResponsive.ToString());
                        WriteToLog(stringBuilder.ToString());
                    }

                    return null;
                }

                var index = channelResponse.GetIndex(13);
                var count = index == -1 ? channelResponse.RawResponse.Length - 1 : index;
                var str = Encoding.ASCII.GetString(channelResponse.RawResponse, 0, count);
                if (flag)
                {
                    stringBuilder.AppendFormat(", Response={0}", str);
                    WriteToLog(stringBuilder.ToString());
                }

                return str;
            }
        }

        private int? DecodePositionResponse(Axis axis)
        {
            var str = SendReciveRaw(axis == Axis.X ? "EX" : "PY");
            int result;
            return str != null && int.TryParse(str.Substring(str.IndexOf('=') + 1), out result) ? result : new int?();
        }

        private ErrorCodes MoveAbsolute(ref MoveTarget target)
        {
            foreach (var command in MovePreamble)
                if (SendReciveRaw(command) == null)
                    return ErrorCodes.ArcusNotResponsive;
            var errorCodes1 = ErrorCodes.Success;
            switch (target.Axis)
            {
                case Axis.X:
                    errorCodes1 = OnAxisXMove(ref target);
                    break;
                case Axis.Y:
                    errorCodes1 = OnAxisYMove(ref target);
                    break;
                case Axis.XY:
                    errorCodes1 = OnMultiAxisMove(ref target);
                    break;
            }

            var errorCodes2 = IsMotorErrored(Axis.XY);
            if (errorCodes2 != ErrorCodes.Success)
                return errorCodes2;
            if (ControllerConfiguration.Instance.PrintEncoderPositionAfterMove2)
            {
                var controllerPosition = ReadPositions();
                if (controllerPosition.ReadOk)
                    LogHelper.Instance.Log("MoveAbsoluteInternal: encoder positions (x,y) = {0}, {1}.",
                        controllerPosition.XCoordinate.Value, controllerPosition.YCoordinate.Value);
                else
                    LogHelper.Instance.Log("Unable to determine encoder position from ARCUS.");
            }

            return errorCodes1;
        }

        private ErrorCodes OnMultiAxisMove(ref MoveTarget target)
        {
            var moveTimeout = ControllerConfiguration.Instance.MoveTimeout;
            if (!UseSmoothMove)
            {
                if (SendReciveRaw(string.Format("X{0}", target.XCoordinate.Value)) == null ||
                    SendReciveRaw(string.Format("Y{0}", target.YCoordinate.Value)) == null)
                    return ErrorCodes.ArcusNotResponsive;
            }
            else if (SendReciveRaw(string.Format("Y{0}X{1}", target.YCoordinate.Value, target.XCoordinate.Value)) ==
                     null)
            {
                return ErrorCodes.ArcusNotResponsive;
            }

            var errorCodes1 = OnWaitMotor(moveTimeout, Axis.Y);
            if (errorCodes1 != ErrorCodes.Success)
                return errorCodes1;
            var errorCodes2 = OnWaitMotor(moveTimeout, Axis.X);
            if (errorCodes2 != ErrorCodes.Success)
                return errorCodes2;
            foreach (var command in ErrorCorrectPreamble)
                if (SendReciveRaw(command) == null)
                    return ErrorCodes.ArcusNotResponsive;
            if (SendReciveRaw(string.Format("Y{0}X{1}", target.YCoordinate.Value, target.XCoordinate.Value)) == null)
                return ErrorCodes.ArcusNotResponsive;
            var errorCodes3 = OnWaitMotor(moveTimeout, Axis.Y);
            if (errorCodes3 != ErrorCodes.Success)
                return errorCodes3;
            var errorCodes4 = OnWaitMotor(moveTimeout, Axis.X);
            if (errorCodes4 != ErrorCodes.Success)
                return errorCodes4;
            return SendReciveRaw("I7=10") != null ? ErrorCodes.Success : ErrorCodes.ArcusNotResponsive;
        }

        private ErrorCodes OnAxisYMove(ref MoveTarget target)
        {
            return SendReciveRaw(string.Format("Y{0}", target.YCoordinate.Value)) != null
                ? OnWaitMotor(ControllerConfiguration.Instance.MoveTimeout, Axis.Y)
                : ErrorCodes.ArcusNotResponsive;
        }

        private ErrorCodes OnAxisXMove(ref MoveTarget target)
        {
            var moveTimeout = ControllerConfiguration.Instance.MoveTimeout;
            var command1 = string.Format("X{0}", target.XCoordinate.Value);
            if (SendReciveRaw(command1) == null)
                return ErrorCodes.ArcusNotResponsive;
            var errorCodes1 = OnWaitMotor(moveTimeout, Axis.X);
            if (errorCodes1 != ErrorCodes.Success)
                return errorCodes1;
            foreach (var command2 in ErrorCorrectPreamble)
                if (SendReciveRaw(command2) == null)
                    return ErrorCodes.ArcusNotResponsive;
            if (SendReciveRaw(command1) == null)
                return ErrorCodes.ArcusNotResponsive;
            var errorCodes2 = OnWaitMotor(moveTimeout, Axis.X);
            if (errorCodes2 != ErrorCodes.Success)
                return errorCodes2;
            return SendReciveRaw("I7=10") != null ? ErrorCodes.Success : ErrorCodes.ArcusNotResponsive;
        }

        private ErrorCodes ClearYMotorError()
        {
            var errorCodes = IsMotorErrored(Axis.Y);
            if (ErrorCodes.MotorError != errorCodes)
                return errorCodes;
            var num = (int)HomeAxis(Axis.Y);
            return IsMotorErrored(Axis.Y);
        }

        private ErrorCodes IsMotorErrored(Axis axis)
        {
            if (Axis.XY != axis && Axis.Y != axis)
                return ErrorCodes.Success;
            var s = SendReciveRaw("MIOY");
            int result;
            if (s == null || SendReciveRaw("MECLEARY") == null || !int.TryParse(s, out result))
                return ErrorCodes.ArcusNotResponsive;
            return result <= 0 || result >= 8 ? ErrorCodes.Success : ErrorCodes.MotorError;
        }
    }
}