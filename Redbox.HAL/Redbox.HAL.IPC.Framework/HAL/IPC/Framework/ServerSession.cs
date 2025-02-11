using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework
{
    public abstract class ServerSession : ISession, IMessageSink
    {
        private readonly object m_syncLock = new object();
        private readonly List<string> MessageSinkQueue = new List<string>();
        private readonly StringBuilder ReadBuilder = new StringBuilder();
        private readonly IPCServiceHost ServiceHost;

        public ServerSession(IPCServiceHost serviceHost, int bufferSize)
        {
            ServiceHost = serviceHost;
            ReadBuffer = new byte[bufferSize];
            LogDetailedMessages = false;
        }

        public IDictionary<string, string> Context { get; } = new Dictionary<string, string>();

        protected abstract int ReadBufferSize { get; }

        protected Stream Stream { get; set; }

        protected byte[] ReadBuffer { get; set; }

        private bool IsSecure => ServiceHost.EncryptionProtocol != SslProtocols.None && ServiceHost.Certificate != null;

        public void Start()
        {
            Write(string.Format("Welcome! {0}, Version {1}, {2}", ServiceHost.HostInfo.Product,
                ServiceHost.HostInfo.Version, ServiceHost.HostInfo.Copyright));
            Read();
        }

        public bool Send(string message)
        {
            var str = string.Format("[MSG] {0}", message);
            if (LogDetailedMessages)
                LogHelper.Instance.Log(str);
            EnqueueSinkMessage(str);
            return true;
        }

        public event EventHandler Disconnect;

        public bool LogDetailedMessages { get; set; }

        public bool CloseClients()
        {
            return CloseClientsInternal();
        }

        public bool CloseStreams()
        {
            return CloseStreamsInternal();
        }

        protected abstract bool CloseClientsInternal();

        protected abstract bool CloseStreamsInternal();

        protected void NegotiateSecurityLayer(Stream stream)
        {
            if (!IsSecure)
            {
                Stream = stream;
            }
            else
            {
                var sslStream1 = new SslStream(stream, false);
                sslStream1.ReadTimeout = 30000;
                sslStream1.WriteTimeout = 30000;
                var sslStream2 = sslStream1;
                sslStream2.AuthenticateAsServer(ServiceHost.Certificate, false, SslProtocols.Tls, true);
                Stream = sslStream2;
            }
        }

        private void RaiseDisconnect()
        {
            if (Disconnect == null)
                return;
            Disconnect(this, EventArgs.Empty);
        }

        private void Read()
        {
            try
            {
                Stream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, EndReadCallback, null);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("ServerSession.Read caught an exception", ex);
                QuitSession(null);
            }
        }

        private void Write(string s)
        {
            if (!Stream.CanWrite)
                return;
            try
            {
                if (!s.EndsWith(Environment.NewLine))
                    s = string.Format("{0}{1}", s, Environment.NewLine);
                Stream.Write(Encoding.ASCII.GetBytes(s), 0, s.Length);
                Interlocked.Add(ref Statistics.Instance.NumberOfBytesSent, s.Length);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("There was an unhandled exception writing to the client session.", ex);
                QuitSession(null);
            }
        }

        private void EndReadCallback(IAsyncResult result)
        {
            try
            {
                var count = Stream.EndRead(result);
                if (count == 0)
                    return;
                Interlocked.Add(ref Statistics.Instance.NumberOfBytesReceived, count);
                ReadBuilder.Append(Encoding.ASCII.GetString(ReadBuffer, 0, count));
                if (!HasMoreBufferLines())
                    ThreadPool.QueueUserWorkItem(o => ProcessReadBuilder());
                else
                    Read();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("ClientSession.Execute has caught an unhandled exception.", ex);
                QuitSession(null);
            }
        }

        private void ProcessReadBuilder()
        {
            while (true)
            {
                string str;
                do
                {
                    var nextBufferLine = GetNextBufferLine();
                    if (nextBufferLine == string.Empty && HasMoreBufferLines())
                    {
                        Read();
                        return;
                    }

                    if (nextBufferLine.StartsWith("quit", StringComparison.CurrentCultureIgnoreCase))
                    {
                        QuitSession("Goodbye!");
                        return;
                    }

                    var commandResult = CommandService.Instance.Execute(this, nextBufferLine);
                    ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
                    FlushSinkQueue(commandResult.Messages);
                    str = commandResult.ToString();
                    Write(str);
                } while (!LogDetailedMessages);

                LogHelper.Instance.Log(str, LogEntryType.Debug);
            }
        }

        private bool QuitSession(string msg)
        {
            if (msg != null)
            {
                if (LogDetailedMessages)
                    LogHelper.Instance.Log(msg, LogEntryType.Debug);
                Write(msg);
            }

            RaiseDisconnect();
            var flag = false;
            try
            {
                CloseClients();
                CloseStreams();
                flag = true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Caught exception in QuitSession", ex);
            }

            try
            {
                ServiceHost.Unregister(this);
                return flag;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("QuitSession: unable to unregister session.", ex);
                return false;
            }
        }

        private void EnqueueSinkMessage(string message)
        {
            lock (m_syncLock)
            {
                MessageSinkQueue.Add(message);
            }
        }

        private void FlushSinkQueue(ICollection<string> messages)
        {
            lock (m_syncLock)
            {
                while (MessageSinkQueue.Count > 0)
                {
                    var messageSink = MessageSinkQueue[0];
                    MessageSinkQueue.RemoveAt(0);
                    messages.Add(messageSink);
                }
            }
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

        private bool HasMoreBufferLines()
        {
            return ReadBuilder.ToString().IndexOf(Environment.NewLine) == -1;
        }
    }
}