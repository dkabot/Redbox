using System;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework
{
    internal abstract class AbstractIPCResponse : IIPCResponse, IDisposable
    {
        protected readonly bool LogDetails;
        protected readonly StringBuilder ReadBuilder = new StringBuilder();
        private bool Disposed;

        protected AbstractIPCResponse()
            : this(false)
        {
        }

        protected AbstractIPCResponse(bool logDetails)
        {
            LogDetails = logDetails;
        }

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            OnDispose(true);
            GC.SuppressFinalize(this);
        }

        public bool Accumulate(byte[] rawResponse)
        {
            return Accumulate(rawResponse, 0, rawResponse.Length);
        }

        public bool Accumulate(byte[] bytes, int start, int length)
        {
            ReadBuilder.Append(Encoding.ASCII.GetString(bytes, start, length));
            return IsComplete = OnTestResponse();
        }

        public void Clear()
        {
            IsComplete = false;
            if (ReadBuilder.Length > 0)
                ReadBuilder.Length = 0;
            OnClear();
        }

        public bool IsComplete { get; protected set; }

        protected abstract void OnClear();

        protected virtual void OnDispose(bool fromDispose)
        {
        }

        protected abstract bool OnTestResponse();

        protected string GetNextBufferLine()
        {
            var str1 = ReadBuilder.ToString();
            var num = str1.IndexOf(Environment.NewLine);
            if (-1 == num)
                return string.Empty;
            var str2 = str1.Substring(0, num + Environment.NewLine.Length);
            ReadBuilder.Remove(0, num + Environment.NewLine.Length);
            return str2.Trim();
        }

        protected bool BufferHasMoreLines()
        {
            return ReadBuilder.ToString().IndexOf(Environment.NewLine) == -1;
        }
    }
}