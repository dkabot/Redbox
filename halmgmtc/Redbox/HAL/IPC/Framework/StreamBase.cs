using System;
using System.IO;

namespace Redbox.HAL.IPC.Framework
{
    internal abstract class StreamBase : IDisposable
    {
        public static implicit operator Stream(StreamBase wrapper) => wrapper.Stream;

        public void Dispose()
        {
            if (this.Stream == null)
                return;
            this.Stream.Dispose();
        }

        internal static MemoryStream New() => new MemoryStream();

        internal static MemoryStream NewOn(byte[] buffer) => new MemoryStream(buffer);

        internal static FileStream CreateFile(string path) => new FileStream(path, FileMode.Create);

        internal static FileStream OpenFile(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        internal long Length => this.Stream != null ? this.Stream.Length : 0L;

        internal long Position => this.Stream != null ? this.Stream.Position : 0L;

        internal bool CanRead => this.Stream != null && this.Stream.CanRead;

        internal bool CanWrite => this.Stream != null && this.Stream.CanWrite;

        internal bool CanSeek => this.Stream != null && this.Stream.CanSeek;

        internal void Flush()
        {
            if (this.Stream == null)
                return;
            this.Stream.Flush();
        }

        internal void Close()
        {
            if (this.Stream == null)
                return;
            this.Stream.Close();
        }

        internal IAsyncResult BeginRead(
          byte[] array,
          int offset,
          int numBytes,
          AsyncCallback userCallback,
          object stateObject)
        {
            return this.Stream != null ? this.Stream.BeginRead(array, offset, numBytes, userCallback, stateObject) : (IAsyncResult)null;
        }

        internal void EndRead(IAsyncResult asyncResult)
        {
            if (this.Stream == null)
                return;
            this.Stream.EndRead(asyncResult);
        }

        internal IAsyncResult BeginWrite(
          byte[] array,
          int offset,
          int numBytes,
          AsyncCallback userCallback,
          object stateObject)
        {
            return this.Stream != null ? this.Stream.BeginWrite(array, offset, numBytes, userCallback, stateObject) : (IAsyncResult)null;
        }

        internal long Seek(long offset, SeekOrigin origin)
        {
            return this.Stream != null ? this.Stream.Seek(offset, origin) : 0L;
        }

        internal void EndWrite(IAsyncResult asyncResult)
        {
            if (this.Stream == null)
                return;
            this.Stream.EndWrite(asyncResult);
        }

        internal int Read(byte[] array, int offset, int count)
        {
            return this.Stream != null ? this.Stream.Read(array, offset, count) : 0;
        }

        internal void Write(byte[] array, int offset, int count)
        {
            if (this.Stream == null)
                return;
            this.Stream.Write(array, offset, count);
        }

        internal void WriteByte(byte value)
        {
            if (this.Stream == null)
                return;
            this.Stream.WriteByte(value);
        }

        internal int ReadByte() => this.Stream != null ? this.Stream.ReadByte() : 0;

        internal Stream Stream { get; set; }
    }
}
