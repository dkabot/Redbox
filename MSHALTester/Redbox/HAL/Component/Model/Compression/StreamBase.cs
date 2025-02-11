using System;
using System.IO;

namespace Redbox.HAL.Component.Model.Compression;

internal abstract class StreamBase : IDisposable
{
    internal long Length => Stream != null ? Stream.Length : 0L;

    internal long Position => Stream != null ? Stream.Position : 0L;

    internal bool CanRead => Stream != null && Stream.CanRead;

    internal bool CanWrite => Stream != null && Stream.CanWrite;

    internal bool CanSeek => Stream != null && Stream.CanSeek;

    internal Stream Stream { get; set; }

    public void Dispose()
    {
        if (Stream == null)
            return;
        Stream.Dispose();
    }

    public static implicit operator Stream(StreamBase wrapper)
    {
        return wrapper.Stream;
    }

    internal static MemoryStream New()
    {
        return new MemoryStream();
    }

    internal static MemoryStream NewOn(byte[] buffer)
    {
        return new MemoryStream(buffer);
    }

    internal static FileStream CreateFile(string path)
    {
        return new FileStream(path, FileMode.Create);
    }

    internal static FileStream OpenFile(string path)
    {
        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    internal void Flush()
    {
        if (Stream == null)
            return;
        Stream.Flush();
    }

    internal void Close()
    {
        if (Stream == null)
            return;
        Stream.Close();
    }

    internal IAsyncResult BeginRead(
        byte[] array,
        int offset,
        int numBytes,
        AsyncCallback userCallback,
        object stateObject)
    {
        return Stream != null ? Stream.BeginRead(array, offset, numBytes, userCallback, stateObject) : null;
    }

    internal void EndRead(IAsyncResult asyncResult)
    {
        if (Stream == null)
            return;
        Stream.EndRead(asyncResult);
    }

    internal IAsyncResult BeginWrite(
        byte[] array,
        int offset,
        int numBytes,
        AsyncCallback userCallback,
        object stateObject)
    {
        return Stream != null ? Stream.BeginWrite(array, offset, numBytes, userCallback, stateObject) : null;
    }

    internal long Seek(long offset, SeekOrigin origin)
    {
        return Stream != null ? Stream.Seek(offset, origin) : 0L;
    }

    internal void EndWrite(IAsyncResult asyncResult)
    {
        if (Stream == null)
            return;
        Stream.EndWrite(asyncResult);
    }

    internal int Read(byte[] array, int offset, int count)
    {
        return Stream != null ? Stream.Read(array, offset, count) : 0;
    }

    internal void Write(byte[] array, int offset, int count)
    {
        if (Stream == null)
            return;
        Stream.Write(array, offset, count);
    }

    internal void WriteByte(byte value)
    {
        if (Stream == null)
            return;
        Stream.WriteByte(value);
    }

    internal int ReadByte()
    {
        return this.Stream != null ? this.Stream.ReadByte() : 0;
    }
}