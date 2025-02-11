using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using Redbox.Core;

namespace Redbox.IPC.Framework
{
    public abstract class ServerSession : ISession, IMessageSink
    {
        private readonly object m_syncLock = new object();
        private Action<ISession> _beforeCommandAction;
        private Action<string, string> _paramAction;
        private ParameterDictionary m_context;
        private List<string> m_messageSinkQueue;
        private IDictionary<string, object> m_properties;
        private StringBuilder m_readBuilder;

        protected ServerSession(IPCServiceHost serviceHost)
        {
            ServiceHost = serviceHost;
        }

        public List<string> Filters { get; } = new List<string>();

        protected byte[] ReadBuffer { get; set; }

        protected Stream Stream { get; set; }

        private IPCServiceHost ServiceHost { get; }

        private List<string> MessageSinkQueue
        {
            get
            {
                if (m_messageSinkQueue == null)
                    m_messageSinkQueue = new List<string>();
                return m_messageSinkQueue;
            }
        }

        private StringBuilder ReadBuilder
        {
            get
            {
                if (m_readBuilder == null)
                    m_readBuilder = new StringBuilder();
                return m_readBuilder;
            }
        }

        private bool IsSecure => ServiceHost.EncryptionProtocol != SslProtocols.None && ServiceHost.Certificate != null;

        public void Start()
        {
            Write(string.Format("Welcome! {0}, Version {1}, {2}", ServiceHost.Name, ServiceHost.Version,
                ServiceHost.Copyright));
            Read();
        }

        public void SetFilters(List<string> filters)
        {
            Filters.Clear();
            filters.ForEach(Filters.Add);
        }

        public void SetParamAction(Action<string, string> paramAction)
        {
            _paramAction = paramAction;
        }

        public void SetBeforeCommandAction(Action<ISession> beforeCommandAction)
        {
            _beforeCommandAction = beforeCommandAction;
        }

        public bool Send(string message)
        {
            var message1 = string.Format("[MSG] {0}", message);
            LogHelper.Instance.Log(message1, LogEntryType.Info);
            EnqueueSinkMessage(message1);
            return true;
        }

        public ParameterDictionary Context
        {
            get
            {
                if (m_context == null)
                    m_context = new ParameterDictionary();
                return m_context;
            }
        }

        public IDictionary<string, object> Properties
        {
            get
            {
                if (m_properties == null)
                    m_properties = new Dictionary<string, object>();
                return m_properties;
            }
        }

        public bool EnableFilters { get; set; }

        public bool IsConnected()
        {
            return IsConnectedInternal();
        }

        public event EventHandler Disconnect;

        public bool CloseClients()
        {
            return CloseClientsInternal();
        }

        public bool CloseStreams()
        {
            return CloseStreamsInternal();
        }

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

        protected abstract bool IsConnectedInternal();

        protected abstract bool CloseStreamsInternal();

        protected abstract bool CloseClientsInternal();

        protected abstract string GetRemoteHostIP();

        private void Read()
        {
            try
            {
                ReadBuffer = BufferPool.Instance.Checkout();
                Stream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, EndReadCallback, null);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in Read.", ex);
                InternalClose();
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
                LogHelper.Instance.Log("An unhandled exception was raised in Write.", ex);
                InternalClose();
            }
        }

        private void EndReadCallback(IAsyncResult result)
        {
            try
            {
                var count = Stream.EndRead(result);
                if (count == 0)
                {
                    InternalClose();
                }
                else
                {
                    Interlocked.Add(ref Statistics.Instance.NumberOfBytesReceived, count);
                    ReadBuilder.Append(Encoding.ASCII.GetString(ReadBuffer, 0, count));
                    if (ServiceHost.WorkerPool.QueueWorkItem(o => ProcessReadBuilder()))
                        return;
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        try
                        {
                            using (var timer = new ExecutionTimer())
                            {
                                var serverBusyResult =
                                    CommandService.Instance.GetServerBusyResult(timer,
                                        ServiceHost.WorkerPool.MaximumThreads);
                                LogHelper.Instance.Log(serverBusyResult);
                                Write(serverBusyResult);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Instance.Log(
                                "An unhandled exception was raised in ProcessReadBuilder, throttling client call.", ex);
                        }
                        finally
                        {
                            LogHelper.Instance.Log("Call Read from ProcessReadBuilder 5.", LogEntryType.Debug);
                            Read();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in EndReadCallback.", ex);
                InternalClose();
            }
            finally
            {
                BufferPool.Instance.Checkin(ReadBuffer);
            }
        }

        private void ProcessReadBuilder()
        {
            string nextBufferLine;
            try
            {
                nextBufferLine = GetNextBufferLine();
                if (string.IsNullOrEmpty(nextBufferLine))
                    if (NoMoreBufferLines())
                    {
                        LogHelper.Instance.Log("Call Read from ProcessReadBuilder 1.", LogEntryType.Debug);
                        Read();
                        return;
                    }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(
                    "An unhandled exception was raised in ProcessReadBuilder near GetNextBufferLine.", ex);
                LogHelper.Instance.Log("Call Read from ProcessReadBuilder 2.", LogEntryType.Debug);
                Read();
                return;
            }

            if (string.IsNullOrEmpty(nextBufferLine))
            {
                LogHelper.Instance.Log("Call Read from ProcessReadBuilder 3.", LogEntryType.Debug);
                Read();
            }
            else if (nextBufferLine.StartsWith("quit", StringComparison.CurrentCultureIgnoreCase))
            {
                LogHelper.Instance.Log(nextBufferLine, LogEntryType.Debug);
                Write("Goodbye!");
                InternalClose();
            }
            else
            {
                try
                {
                    var originIP = string.Empty;
                    try
                    {
                        originIP = GetRemoteHostIP();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log("Error accessing remote host ip.", ex);
                    }

                    if (_beforeCommandAction != null)
                        _beforeCommandAction(this);
                    var commandResult = CommandService.Instance.Execute(this, originIP, nextBufferLine, Filters,
                        EnableFilters, _paramAction);
                    if (commandResult == null)
                        return;
                    ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
                    FlushSinkQueue(commandResult.Messages);
                    Write(commandResult.ToString());
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log(
                        "An unhandled exception was raised in ProcessReadBuilder thread pool work item, executing command.",
                        ex);
                }
                finally
                {
                    LogHelper.Instance.Log("Call Read from ProcessReadBuilder 4.", LogEntryType.Debug);
                    Read();
                }
            }
        }

        private void InternalClose()
        {
            try
            {
                if (Disconnect != null)
                    Disconnect(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in Disconnect event handler.", ex);
            }

            try
            {
                CloseClients();
                CloseStreams();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in TcpServerSession.InternalClose.", ex);
            }
            finally
            {
                BufferPool.Instance.Checkin(ReadBuffer);
                if (ServiceHost != null)
                    ServiceHost.UnregisterSession(this);
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

        private bool NoMoreBufferLines()
        {
            return ReadBuilder.ToString().IndexOf(Environment.NewLine) == -1;
        }

        private void ResetReadBuilder()
        {
            ReadBuilder.Length = 0;
        }
    }
}