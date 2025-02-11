using System;
using System.IO.Ports;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;

namespace Redbox.HAL.Core;

internal sealed class CallbackReadPort : RedboxSerialPort
{
    private readonly AtomicFlag ReadingFlag = new();
    private ChannelResponse Current;
    private byte[] ReadBuffer;

    internal CallbackReadPort(SerialPort port)
        : base(port)
    {
    }

    protected override void OnPreOpenPort()
    {
        Port.DataReceived += Port_DataReceived;
        ReadBuffer = new byte[Port.ReadBufferSize];
    }

    protected override void OnPortClosed()
    {
        ReadBuffer = null;
    }

    protected override IChannelResponse OnSendReceive(byte[] command, int readTimeout)
    {
        var channelResponse = new ChannelResponse();
        Current = channelResponse;
        try
        {
            try
            {
                if (!ReadingFlag.Set())
                {
                    LogHelper.Instance.Log(LogEntryType.Error, "[CallbackReadPort, {0}] The port is already reading.",
                        DisplayName);
                    channelResponse.Error = ErrorCodes.CommunicationError;
                    return channelResponse;
                }

                Port.Write(command, 0, command.Length);
                if (WriteTerminator != null)
                    Port.Write(WriteTerminator, 0, WriteTerminator.Length);
                RuntimeService.SpinWait(WritePause);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(string.Format("[CallbackReadPort, {0}] Write caught an exception.", DisplayName),
                    ex);
                channelResponse.Error = ErrorCodes.CommunicationError;
                return channelResponse;
            }

            if (!channelResponse.Wait(readTimeout))
            {
                channelResponse.Error = ErrorCodes.CommunicationError;
                LogHelper.Instance.Log(LogEntryType.Error,
                    "[CallbackReadPort, {0}] Communication ERROR: port read timed out", DisplayName);
            }

            return channelResponse;
        }
        finally
        {
            Current = null;
            ReadingFlag.Clear();
        }
    }

    private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            var bytesToRead = Port.BytesToRead;
            if (EnableDebugging)
                LogHelper.Instance.Log("[CallbackReadPort, {0}] data received bytes = {1} args = {2}", DisplayName,
                    bytesToRead, e.EventType);
            if (bytesToRead == 0 || Current == null)
                return;
            var num = Port.Read(ReadBuffer, 0, bytesToRead);
            if (num != bytesToRead)
                LogHelper.Instance.Log(
                    "[CallbackReadPort, {0}] !! The bytes to read {1} doesn't match the read count {2} !!", DisplayName,
                    num, bytesToRead);
            if (!ReadingFlag.IsSet)
                return;
            Current.Accumulate(ReadBuffer, bytesToRead);
            if (!ValidateResponse(Current))
                return;
            Current.ReadEnd();
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log(
                string.Format("[CallbackReadPort, {0}] data received caught an exception.", DisplayName), ex);
        }
    }
}