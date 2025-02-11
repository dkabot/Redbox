using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class ChannelResponse : IChannelResponse, IDisposable
{
    private static int IDCount;
    internal readonly byte[] Buffer;
    internal readonly string ID;
    private readonly List<byte> PortResponse = new();
    private readonly EventWaitHandle ReadEvent = new(false, EventResetMode.ManualReset);
    private bool Disposed;

    internal ChannelResponse()
        : this(0)
    {
    }

    internal ChannelResponse(int bufferSize)
    {
        Error = ErrorCodes.Success;
        Buffer = bufferSize <= 0 ? null : new byte[bufferSize];
        ID = string.Format("ChannelResponse-{0}", Interlocked.Increment(ref IDCount));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public int GetIndex(byte b)
    {
        var array = PortResponse.ToArray();
        if (array == null || array.Length == 0)
            return -1;
        for (var index = 0; index < array.Length; ++index)
            if (array[index] == b)
                return index;
        return -1;
    }

    public bool CommOk => Error == ErrorCodes.Success;

    public ErrorCodes Error { get; internal set; }

    public byte[] RawResponse => PortResponse.ToArray();

    public bool ReponseValid { get; internal set; }

    ~ChannelResponse()
    {
        Dispose(false);
    }

    public void DumpToLog()
    {
        LogHelper.Instance.Log(" -- Core Response dump -- ");
        if (PortResponse.Count == 0)
        {
            LogHelper.Instance.Log(" there is no response to dump.");
        }
        else
        {
            var stringBuilder = new StringBuilder();
            for (var index = 0; index < PortResponse.Count; ++index)
            {
                stringBuilder.AppendFormat("{0:x2} ", PortResponse[index]);
                if (index % 16 == 0)
                    stringBuilder.AppendLine();
            }

            LogHelper.Instance.Log(stringBuilder.ToString());
        }
    }

    internal void Accumulate(int len)
    {
        Accumulate(Buffer, len);
    }

    internal void Accumulate(byte[] bytes, int len)
    {
        for (var index = 0; index < len; ++index)
            PortResponse.Add(bytes[index]);
    }

    internal bool Wait(int timeout)
    {
        return ReadEvent.WaitOne(timeout);
    }

    internal void ReadEnd()
    {
        if (Disposed)
            return;
        ReadEvent.Set();
    }

    private void Dispose(bool disposing)
    {
        if (Disposed)
            return;
        Disposed = true;
        if (!disposing)
            return;
        ReadEvent.Close();
        PortResponse.Clear();
    }
}