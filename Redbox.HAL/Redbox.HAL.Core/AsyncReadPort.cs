using System;
using System.IO.Ports;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class AsyncReadPort : RedboxSerialPort
{
    internal AsyncReadPort(SerialPort port)
        : base(port)
    {
    }

    protected override IChannelResponse OnSendReceive(byte[] command, int readTimeout)
    {
        var response = new ChannelResponse(Port.ReadBufferSize);
        if (EnableDebugging)
            LogHelper.Instance.Log("[AsyncReadPort, {0}] SendReceive response id = {1}", DisplayName, response.ID);
        try
        {
            Port.Write(command, 0, command.Length);
            if (WriteTerminator != null)
                Port.Write(WriteTerminator, 0, WriteTerminator.Length);
            RuntimeService.SpinWait(WritePause);
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log(string.Format("[AsyncReadPort, {0}] WriteCommand caught an exception.", DisplayName),
                ex);
            response.Error = ErrorCodes.CommunicationError;
            return response;
        }

        ReadInner(response);
        try
        {
            if (!response.Wait(readTimeout))
            {
                LogHelper.Instance.Log(LogEntryType.Error,
                    "[AsyncReadPort, {0}] Communication ERROR: port read timed out", DisplayName);
                response.Error = ErrorCodes.CommunicationError;
            }
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log(LogEntryType.Error,
                string.Format("[AsyncPort, {0}] Send/Recv caught an exception.", DisplayName), ex);
            response.Error = ErrorCodes.CommunicationError;
        }

        return response;
    }

    private void ReadInner(ChannelResponse response)
    {
        if (EnableDebugging)
            LogHelper.Instance.Log("[AsyncReadPort, {0}] Read inner id = {1}", DisplayName, response.ID);
        try
        {
            Port.BaseStream.BeginRead(response.Buffer, 0, response.Buffer.Length, EndReadCallback, response);
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log(
                string.Format("[AsyncReadPort, {0}] ReadInner caught an exception id = {1}", DisplayName, response.ID),
                ex);
        }
    }

    private void EndReadCallback(IAsyncResult result)
    {
        ChannelResponse asyncState;
        int len;
        try
        {
            asyncState = (ChannelResponse)result.AsyncState;
            len = Port.BaseStream.EndRead(result);
            if (EnableDebugging)
                LogHelper.Instance.Log(
                    "[AsyncReadPort, {0}] EndReadCallback bytesRead = {1}, current buffer size = {2} ID  = {3}",
                    DisplayName, len, asyncState.RawResponse.Length, asyncState.ID);
            if (len == 0)
            {
                asyncState.ReponseValid = ValidateResponse(asyncState);
                asyncState.ReadEnd();
                return;
            }

            Port.BaseStream.Flush();
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log(
                string.Format("[AsyncReadPort, {0}] EndReadCallback caught an exception.", DisplayName), ex);
            return;
        }

        asyncState.Accumulate(len);
        if (!ValidateResponse(asyncState))
        {
            ReadInner(asyncState);
        }
        else
        {
            asyncState.ReponseValid = true;
            asyncState.ReadEnd();
        }
    }
}