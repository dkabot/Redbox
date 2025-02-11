using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

internal sealed class ByteAccumulator : IIPCResponse, IDisposable
{
    internal readonly List<byte> Accumulator = new();
    private bool Disposed;

    public void Dispose()
    {
        if (Disposed)
            return;
        Disposed = true;
        Accumulator.Clear();
        GC.SuppressFinalize(this);
    }

    public bool Accumulate(byte[] rawResponse)
    {
        return Accumulate(rawResponse, 0, rawResponse.Length);
    }

    public bool Accumulate(byte[] bytes, int start, int length)
    {
        for (var index = start; index < length; ++index)
            Accumulator.Add(bytes[index]);
        return true;
    }

    public void Clear()
    {
        Accumulator.Clear();
        IsComplete = false;
    }

    public bool IsComplete { get; private set; }

    public bool OnReadComplete()
    {
        return IsComplete = true;
    }
}