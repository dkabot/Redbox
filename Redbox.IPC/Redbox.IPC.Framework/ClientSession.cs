using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Redbox.Core;
using Redbox.IPC.Framework.NamedPipes;
using Redbox.IPC.Framework.Remoting;
using Redbox.IPC.Framework.Sockets;

namespace Redbox.IPC.Framework
{
    public abstract class ClientSession : IDisposable
    {
        private readonly AutoResetEvent m_resetEvent = new AutoResetEvent(false);
        private StringBuilder m_readBuilder;

        protected ClientSession(IPCProtocol protocol)
        {
            Protocol = protocol;
        }

        public int Timeout { get; set; }

        public bool IsDisposed { get; protected set; }

        public string HostName { get; protected set; }

        public bool IsConnected { get; internal set; }

        public IPCProtocol Protocol { get; protected set; }

        protected abstract Stream Stream { get; }

        protected byte[] ReadBuffer { get; set; }

        protected internal ClientSessionPool OwningPool { get; set; }

        private StringBuilder ReadBuilder
        {
            get
            {
                if (m_readBuilder == null)
                    m_readBuilder = new StringBuilder();
                return m_readBuilder;
            }
        }

        public virtual void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            Close();
            try
            {
                m_resetEvent.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in Dispose closing the m_resetEvent.", ex);
            }

            if (Disposed == null)
                return;
            Disposed(this, EventArgs.Empty);
        }

        public static ClientSession GetClientSession(IPCProtocol protocol, int? timeout)
        {
            switch (protocol.Channel)
            {
                case ChannelType.Socket:
                    return GetTcpClientSession(protocol, timeout);
                case ChannelType.NamedPipe:
                    return GetNamedPipesClientSession(protocol);
                case ChannelType.Remoting:
                    return new RemotingClientSession(protocol, timeout ?? 5000);
                default:
                    LogHelper.Instance.Log(
                        string.Format("Unrecognized channel type {0}; please fix your protocol string.",
                            protocol.Channel), LogEntryType.Error);
                    return null;
            }
        }

        public abstract bool Connect();

        public abstract void ConnectThrowOnError();

        public virtual void Close()
        {
            if (!IsConnected)
                return;
            try
            {
                if (Protocol.Channel != ChannelType.Socket && Protocol.Channel != ChannelType.NamedPipe)
                    return;
                ExecuteCommand("quit");
            }
            catch (IOException ex)
            {
            }
            finally
            {
                IsConnected = false;
                CustomClose();
            }
        }

        public bool IsStatusOk(List<string> messages)
        {
            var flag = false;
            if (messages.Count > 0)
                flag = messages[messages.Count - 1].StartsWith("203 Command");
            return flag;
        }

        public virtual List<string> ExecuteCommand(string command)
        {
            Write(command);
            return ConsumeMessages();
        }

        public event EventHandler Disposed;

        public event Action<string> ServerEvent;

        protected List<string> ConsumeMessages()
        {
            var messages = new List<string>();
            try
            {
                ResetReadBuilder();
                Read(messages);
                if (!m_resetEvent.WaitOne(Timeout))
                {
                    messages.Clear();
                    var errors = new ErrorList();
                    errors.Add(Error.NewError("J888", "Timeout threshold exceeded.",
                        "Reissue the command when the service is not as busy."));
                    ProtocolHelper.FormatErrors(errors, messages);
                    messages.Add("545 Command failed.");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in ClientSession.ConsumeMessages.", ex);
                throw;
            }

            return messages;
        }

        protected internal abstract bool IsConnectionAvailable();

        protected internal abstract void CustomClose();

        protected abstract int GetAvailableData();

        protected abstract bool CanRead();

        internal void ProcessServerEvent(string line)
        {
            if (ServerEvent == null)
                return;
            var num = line.IndexOf("]");
            if (num == -1)
                return;
            ServerEvent(line.Substring(num + 1).Trim());
        }

        private static ClientSession GetTcpClientSession(IPCProtocol protocol, int? timeout)
        {
            return new TcpClientSession(protocol, timeout);
        }

        private static ClientSession GetNamedPipesClientSession(IPCProtocol protocol)
        {
            return new NamedPipeClientSession(protocol);
        }

        private void Read(List<string> messages)
        {
            Stream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, EndReadCallback, messages);
        }

        private void Write(string s)
        {
            if (!Stream.CanWrite)
                return;
            if (!s.EndsWith(Environment.NewLine))
                s = string.Format("{0}{1}", s, Environment.NewLine);
            Stream.Write(Encoding.ASCII.GetBytes(s), 0, s.Length);
        }

        private void ResetReadBuilder()
        {
            if (ReadBuilder.Length == 0)
                return;
            ReadBuilder.Length = 0;
        }

        private string GetNextBufferLine()
        {
            var str1 = ReadBuilder.ToString();
            var num = str1.IndexOf(Environment.NewLine);
            if (num == -1)
                return string.Empty;
            var str2 = str1.Substring(0, num + Environment.NewLine.Length);
            ReadBuilder.Remove(0, num + Environment.NewLine.Length);
            return str2.Trim();
        }

        private bool BufferHasMoreLines()
        {
            return ReadBuilder.ToString().IndexOf(Environment.NewLine) != -1;
        }

        private void EndReadCallback(IAsyncResult result)
        {
            try
            {
                var asyncState = (List<string>)result.AsyncState;
                var count = Stream.EndRead(result);
                if (count == 0)
                {
                    SafeSetEvent();
                }
                else
                {
                    var str = Encoding.ASCII.GetString(ReadBuffer, 0, count);
                    ReadBuilder.Append(str);
                    if (str.IndexOf(Environment.NewLine) == -1 && str.IndexOf("\n") == -1)
                        Read(asyncState);
                    else if (!ProcessReadBuilder(asyncState))
                        Read(asyncState);
                    else
                        SafeSetEvent();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in ClientSession.EndReadCallback", ex);
                SafeSetEvent();
            }
        }

        private bool ProcessReadBuilder(ICollection<string> messages)
        {
            string nextBufferLine;
            do
            {
                nextBufferLine = GetNextBufferLine();
                if (nextBufferLine == null || (nextBufferLine == string.Empty && !BufferHasMoreLines()))
                    return false;
                if (nextBufferLine.StartsWith("[MSG]"))
                    ProcessServerEvent(nextBufferLine);
                else
                    messages.Add(nextBufferLine);
            } while (!nextBufferLine.StartsWith("Welcome!") && !nextBufferLine.StartsWith("Goodbye!") &&
                     !nextBufferLine.StartsWith("545 Command") && !nextBufferLine.StartsWith("203 Command"));

            return true;
        }

        private void SafeSetEvent()
        {
            try
            {
                m_resetEvent.Set();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in ClientSession.SafeSetEvent.", ex);
            }
        }
    }
}