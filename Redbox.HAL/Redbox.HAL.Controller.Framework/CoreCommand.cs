using System;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class CoreCommand
    {
        private static readonly char[] CommaSplit = new char[1]
        {
            ','
        };

        internal readonly AddressSelector Address;
        internal readonly string CommandName;
        internal readonly int CommandWait;
        internal readonly int OperationTimeout;
        internal readonly ICommPort Port;
        internal readonly string ResetCommand;
        internal readonly int? StatusBit;
        internal readonly int WaitPauseTime;

        private CoreCommand(CommandType type, int? timeout, ICommPort port)
        {
            Port = port;
            CommandName = type.ToString();
            var field = typeof(CommandType).GetField(type.ToString());
            if (field == null)
            {
                LogHelper.Instance.Log("[CoreCommand] Could not locate type field on {0}", CommandName);
            }
            else
            {
                var properties = CommandPropertiesAttribute.GetProperties(field);
                if (properties == null)
                {
                    LogHelper.Instance.Log("[CoreCommand] Could not locate PropertiesAttribute on {0}", CommandName);
                }
                else
                {
                    Command = properties.Command;
                    Address = properties.Address;
                    ResetCommand = properties.ResetCommand;
                    WaitPauseTime = -1 != properties.WaitPauseTime ? properties.WaitPauseTime : 0;
                    OperationTimeout = timeout.GetValueOrDefault();
                    StatusBit = new int?();
                    if (-1 != properties.StatusBit)
                    {
                        StatusBit = properties.StatusBit;
                        if (OperationTimeout == 0 || WaitPauseTime == 0)
                            LogHelper.Instance.Log(
                                "[CoreCommand] On CommandType {0} there is a status bit but Timeout = {1} and WaitPause = {2}",
                                CommandName, OperationTimeout, WaitPauseTime);
                    }

                    CommandWait = -1 != properties.CommandWait ? properties.CommandWait : 8000;
                }
            }
        }

        internal string Command { get; }

        internal static CoreCommand Create(CommandType type, int? timeout, ICommPort port)
        {
            return new CoreCommand(type, timeout, port);
        }

        internal static CoreCommand Create(CommandType type, ICommPort port)
        {
            return new CoreCommand(type, new int?(), port);
        }

        internal CoreResponse Execute()
        {
            using (var trace = new CommandTrace(ControllerConfiguration.Instance.EnableCommandTrace))
            {
                trace.Trace("[CoreCommand] Executing command {0}", CommandName);
                var coreResponse = SendCommand(Command, trace);
                try
                {
                    if (coreResponse.CommError || !StatusBit.HasValue)
                        return coreResponse;
                    coreResponse.Error = WaitForCommand(trace);
                    if (coreResponse.TimedOut && !string.IsNullOrEmpty(ResetCommand))
                        Array.ForEach(ResetCommand.Split(CommaSplit, StringSplitOptions.RemoveEmptyEntries),
                            s => SendCommand(s, trace));
                    return coreResponse;
                }
                finally
                {
                    trace.Trace("[CoreCommand] {0} returned {1}", CommandName, coreResponse.ToString());
                }
            }
        }

        private ErrorCodes WaitForCommand(CommandTrace trace)
        {
            var service = ServiceLocator.Instance.GetService<IRuntimeService>();
            var timespan = new TimeSpan(0, 0, 0, 0, WaitPauseTime);
            trace.Enter();
            trace.Trace("[WaitForCommand] Start.");
            try
            {
                using (var executionTimer = new ExecutionTimer())
                {
                    do
                    {
                        service.SpinWait(timespan);
                        var coreResponse = SendCommand("S", trace);
                        if (coreResponse.CommError)
                            return ErrorCodes.CommunicationError;
                        trace.Trace("[WaitForCommand] {0}", coreResponse.OpCodeResponse);
                        if (!coreResponse.IsBitSet(StatusBit.Value))
                            return ErrorCodes.Success;
                    } while (executionTimer.ElapsedMilliseconds <= OperationTimeout);

                    return ErrorCodes.Timeout;
                }
            }
            finally
            {
                trace.Exit();
            }
        }

        private CoreResponse SendCommand(string command, CommandTrace trace)
        {
            trace.Enter();
            try
            {
                var coreResponse = new CoreResponse(Address);
                using (var channelResponse = Port.SendRecv(Address.ToString(), 5000))
                {
                    coreResponse.Error = !channelResponse.CommOk ? ErrorCodes.CommunicationError : ErrorCodes.Success;
                    if (!channelResponse.CommOk)
                    {
                        LogHelper.Instance.Log(" [SendCommand] Selector {0} ( port = {1} )communication error.",
                            Address.ToString(), Port.DisplayName);
                        coreResponse.Diagnostic = ComputeCommunicationError(Address);
                        return coreResponse;
                    }

                    trace.Trace("[SendCommand] Command {0} selector response {1}", command,
                        Encoding.ASCII.GetString(channelResponse.RawResponse));
                }

                using (var channelResponse = Port.SendRecv(command, CommandWait))
                {
                    coreResponse.Error = !channelResponse.CommOk ? ErrorCodes.CommunicationError : ErrorCodes.Success;
                    if (channelResponse.CommOk)
                    {
                        coreResponse.OpCodeResponse = Encoding.ASCII.GetString(channelResponse.RawResponse);
                        trace.Trace("[SendCommand] Command {0} response {1}", command, coreResponse.OpCodeResponse);
                    }
                    else
                    {
                        LogHelper.Instance.Log(
                            " [SendCommand] Command {0} on address {1} ( port = {2} ) communication error.", command,
                            Address.ToString(), Port.DisplayName);
                    }

                    return coreResponse;
                }
            }
            finally
            {
                trace.Exit();
            }
        }

        private string ComputeCommunicationError(AddressSelector address)
        {
            string str;
            switch (address)
            {
                case AddressSelector.H001:
                    str = "PCB";
                    break;
                case AddressSelector.H002:
                    str = "AUX board";
                    break;
                case AddressSelector.H101:
                    str = "SER board";
                    break;
                case AddressSelector.H555:
                    str = "QR device";
                    break;
                default:
                    str = string.Format("Unknown board {0}", address.ToString());
                    break;
            }

            var message = string.Format("{0} is not responsive.", str);
            LogHelper.Instance.Log(message, LogEntryType.Error);
            return message;
        }
    }
}