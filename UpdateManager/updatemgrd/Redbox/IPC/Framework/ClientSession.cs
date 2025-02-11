using Redbox.Core;
using Redbox.IPC.Framework.NamedPipes;
using Redbox.IPC.Framework.Remoting;
using Redbox.IPC.Framework.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Redbox.IPC.Framework
{
    internal abstract class ClientSession : IDisposable
    {
        private StringBuilder m_readBuilder;
        private readonly AutoResetEvent m_resetEvent = new AutoResetEvent(false);

        public static ClientSession GetClientSession(IPCProtocol protocol, int? timeout)
        {
            switch (protocol.Channel)
            {
                case ChannelType.Socket:
                    return ClientSession.GetTcpClientSession(protocol, timeout);
                case ChannelType.NamedPipe:
                    return ClientSession.GetNamedPipesClientSession(protocol);
                case ChannelType.Remoting:
                    return (ClientSession)new RemotingClientSession(protocol, timeout ?? 5000);
                default:
                    LogHelper.Instance.Log(string.Format("Unrecognized channel type {0}; please fix your protocol string.", (object)protocol.Channel), LogEntryType.Error);
                    return (ClientSession)null;
            }
        }

        public abstract bool Connect();

        public abstract void ConnectThrowOnError();

        public virtual void Close()
        {
            if (!this.IsConnected)
                return;
            try
            {
                if (this.Protocol.Channel != ChannelType.Socket && this.Protocol.Channel != ChannelType.NamedPipe)
                    return;
                this.ExecuteCommand("quit");
            }
            catch (IOException ex)
            {
            }
            finally
            {
                this.IsConnected = false;
                this.CustomClose();
            }
        }

        public virtual void Dispose()
        {
            if (this.IsDisposed)
                return;
            this.IsDisposed = true;
            this.Close();
            try
            {
                this.m_resetEvent.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in Dispose closing the m_resetEvent.", ex);
            }
            if (this.Disposed == null)
                return;
            this.Disposed((object)this, EventArgs.Empty);
        }

        public bool IsStatusOk(List<string> messages)
        {
            bool flag = false;
            if (messages.Count > 0)
            {
                List<string> stringList = messages;
                flag = stringList[stringList.Count - 1].StartsWith("203 Command");
            }
            return flag;
        }

        public virtual List<string> ExecuteCommand(string command)
        {
            this.Write(command);
            return this.ConsumeMessages();
        }

        public int Timeout { get; set; }

        public bool IsDisposed { get; protected set; }

        public string HostName { get; protected set; }

        public bool IsConnected { get; internal set; }

        public event EventHandler Disposed;

        public event Action<string> ServerEvent;

        public IPCProtocol Protocol { get; protected set; }

        protected ClientSession(IPCProtocol protocol) => this.Protocol = protocol;

        protected List<string> ConsumeMessages()
        {
            List<string> messages = new List<string>();
            try
            {
                this.ResetReadBuilder();
                this.Read(messages);
                if (!this.m_resetEvent.WaitOne(this.Timeout))
                {
                    messages.Clear();
                    ErrorList errors = new ErrorList();
                    errors.Add(Error.NewError("J888", "Timeout threshold exceeded.", "Reissue the command when the service is not as busy."));
                    ProtocolHelper.FormatErrors(errors, (IList<string>)messages);
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

        protected abstract Stream Stream { get; }

        protected byte[] ReadBuffer { get; set; }

        protected internal ClientSessionPool OwningPool { get; set; }

        internal void ProcessServerEvent(string line)
        {
            if (this.ServerEvent == null)
                return;
            int num = line.IndexOf("]");
            if (num == -1)
                return;
            this.ServerEvent(line.Substring(num + 1).Trim());
        }

        private static ClientSession GetTcpClientSession(IPCProtocol protocol, int? timeout)
        {
            return (ClientSession)new TcpClientSession(protocol, timeout);
        }

        private static ClientSession GetNamedPipesClientSession(IPCProtocol protocol)
        {
            return (ClientSession)new NamedPipeClientSession(protocol);
        }

        private void Read(List<string> messages)
        {
            this.Stream.BeginRead(this.ReadBuffer, 0, this.ReadBuffer.Length, new AsyncCallback(this.EndReadCallback), (object)messages);
        }

        private void Write(string s)
        {
            if (!this.Stream.CanWrite)
                return;
            if (!s.EndsWith(Environment.NewLine))
                s = string.Format("{0}{1}", (object)s, (object)Environment.NewLine);
            this.Stream.Write(Encoding.ASCII.GetBytes(s), 0, s.Length);
        }

        private void ResetReadBuilder()
        {
            if (this.ReadBuilder.Length == 0)
                return;
            this.ReadBuilder.Length = 0;
        }

        private string GetNextBufferLine()
        {
            string str1 = this.ReadBuilder.ToString();
            int num = str1.IndexOf(Environment.NewLine);
            if (num == -1)
                return string.Empty;
            string str2 = str1.Substring(0, num + Environment.NewLine.Length);
            this.ReadBuilder.Remove(0, num + Environment.NewLine.Length);
            return str2.Trim();
        }

        private bool BufferHasMoreLines()
        {
            return this.ReadBuilder.ToString().IndexOf(Environment.NewLine) != -1;
        }

        private void EndReadCallback(IAsyncResult result)
        {
            try
            {
                List<string> asyncState = (List<string>)result.AsyncState;
                int count = this.Stream.EndRead(result);
                if (count == 0)
                {
                    this.SafeSetEvent();
                }
                else
                {
                    string str = Encoding.ASCII.GetString(this.ReadBuffer, 0, count);
                    this.ReadBuilder.Append(str);
                    if (str.IndexOf(Environment.NewLine) == -1 && str.IndexOf("\n") == -1)
                        this.Read(asyncState);
                    else if (!this.ProcessReadBuilder((ICollection<string>)asyncState))
                        this.Read(asyncState);
                    else
                        this.SafeSetEvent();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in ClientSession.EndReadCallback", ex);
                this.SafeSetEvent();
            }
        }

        private bool ProcessReadBuilder(ICollection<string> messages)
        {
            string nextBufferLine;
            do
            {
                nextBufferLine = this.GetNextBufferLine();
                if (nextBufferLine == null || nextBufferLine == string.Empty && !this.BufferHasMoreLines())
                    return false;
                if (nextBufferLine.StartsWith("[MSG]"))
                    this.ProcessServerEvent(nextBufferLine);
                else
                    messages.Add(nextBufferLine);
            }
            while (!nextBufferLine.StartsWith("Welcome!") && !nextBufferLine.StartsWith("Goodbye!") && !nextBufferLine.StartsWith("545 Command") && !nextBufferLine.StartsWith("203 Command"));
            return true;
        }

        private void SafeSetEvent()
        {
            try
            {
                this.m_resetEvent.Set();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in ClientSession.SafeSetEvent.", ex);
            }
        }

        private StringBuilder ReadBuilder
        {
            get
            {
                if (this.m_readBuilder == null)
                    this.m_readBuilder = new StringBuilder();
                return this.m_readBuilder;
            }
        }
    }
}
