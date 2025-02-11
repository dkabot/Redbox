using Redbox.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Redbox.IPC.Framework
{
    internal abstract class ServerSession : ISession, IMessageSink
    {
        private Action<string, string> _paramAction;
        private Action<ISession> _beforeCommandAction;
        private List<string> _filters = new List<string>();
        private bool _enableFilters;
        private StringBuilder m_readBuilder;
        private ParameterDictionary m_context;
        private List<string> m_messageSinkQueue;
        private IDictionary<string, object> m_properties;
        private readonly object m_syncLock = new object();

        public void Start()
        {
            this.Write(string.Format("Welcome! {0}, Version {1}, {2}", (object)this.ServiceHost.Name, (object)this.ServiceHost.Version, (object)this.ServiceHost.Copyright));
            this.Read();
        }

        public void SetFilters(List<string> filters)
        {
            this._filters.Clear();
            filters.ForEach(new Action<string>(this._filters.Add));
        }

        public void SetParamAction(Action<string, string> paramAction)
        {
            this._paramAction = paramAction;
        }

        public void SetBeforeCommandAction(Action<ISession> beforeCommandAction)
        {
            this._beforeCommandAction = beforeCommandAction;
        }

        public bool Send(string message)
        {
            string message1 = string.Format("[MSG] {0}", (object)message);
            LogHelper.Instance.Log(message1, LogEntryType.Info);
            this.EnqueueSinkMessage(message1);
            return true;
        }

        public ParameterDictionary Context
        {
            get
            {
                if (this.m_context == null)
                    this.m_context = new ParameterDictionary();
                return this.m_context;
            }
        }

        public IDictionary<string, object> Properties
        {
            get
            {
                if (this.m_properties == null)
                    this.m_properties = (IDictionary<string, object>)new Dictionary<string, object>();
                return this.m_properties;
            }
        }

        public List<string> Filters => this._filters;

        public bool EnableFilters
        {
            get => this._enableFilters;
            set => this._enableFilters = value;
        }

        public bool CloseClients() => this.CloseClientsInternal();

        public bool CloseStreams() => this.CloseStreamsInternal();

        public bool IsConnected() => this.IsConnectedInternal();

        public event EventHandler Disconnect;

        protected ServerSession(IPCServiceHost serviceHost) => this.ServiceHost = serviceHost;

        protected void NegotiateSecurityLayer(Stream stream)
        {
            if (!this.IsSecure)
            {
                this.Stream = stream;
            }
            else
            {
                SslStream sslStream1 = new SslStream(stream, false);
                sslStream1.ReadTimeout = 30000;
                sslStream1.WriteTimeout = 30000;
                SslStream sslStream2 = sslStream1;
                sslStream2.AuthenticateAsServer((X509Certificate)this.ServiceHost.Certificate, false, SslProtocols.Tls, true);
                this.Stream = (Stream)sslStream2;
            }
        }

        protected abstract bool IsConnectedInternal();

        protected abstract bool CloseStreamsInternal();

        protected abstract bool CloseClientsInternal();

        protected abstract string GetRemoteHostIP();

        protected byte[] ReadBuffer { get; set; }

        protected Stream Stream { get; set; }

        private void Read()
        {
            try
            {
                this.ReadBuffer = BufferPool.Instance.Checkout();
                this.Stream.BeginRead(this.ReadBuffer, 0, this.ReadBuffer.Length, new AsyncCallback(this.EndReadCallback), (object)null);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in Read.", ex);
                this.InternalClose();
            }
        }

        private void Write(string s)
        {
            if (!this.Stream.CanWrite)
                return;
            try
            {
                if (!s.EndsWith(Environment.NewLine))
                    s = string.Format("{0}{1}", (object)s, (object)Environment.NewLine);
                this.Stream.Write(Encoding.ASCII.GetBytes(s), 0, s.Length);
                Interlocked.Add(ref Statistics.Instance.NumberOfBytesSent, (long)s.Length);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in Write.", ex);
                this.InternalClose();
            }
        }

        private void EndReadCallback(IAsyncResult result)
        {
            try
            {
                int count = this.Stream.EndRead(result);
                if (count == 0)
                {
                    this.InternalClose();
                }
                else
                {
                    Interlocked.Add(ref Statistics.Instance.NumberOfBytesReceived, (long)count);
                    this.ReadBuilder.Append(Encoding.ASCII.GetString(this.ReadBuffer, 0, count));
                    if (this.ServiceHost.WorkerPool.QueueWorkItem((WorkItem)(o => this.ProcessReadBuilder())))
                        return;
                    ThreadPool.QueueUserWorkItem((WaitCallback)(o =>
                    {
                        try
                        {
                            using (ExecutionTimer timer = new ExecutionTimer())
                            {
                                string serverBusyResult = CommandService.Instance.GetServerBusyResult(timer, this.ServiceHost.WorkerPool.MaximumThreads);
                                LogHelper.Instance.Log(serverBusyResult);
                                this.Write(serverBusyResult);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Instance.Log("An unhandled exception was raised in ProcessReadBuilder, throttling client call.", ex);
                        }
                        finally
                        {
                            LogHelper.Instance.Log("Call Read from ProcessReadBuilder 5.", LogEntryType.Debug);
                            this.Read();
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in EndReadCallback.", ex);
                this.InternalClose();
            }
            finally
            {
                BufferPool.Instance.Checkin(this.ReadBuffer);
            }
        }

        private void ProcessReadBuilder()
        {
            string nextBufferLine;
            try
            {
                nextBufferLine = this.GetNextBufferLine();
                if (string.IsNullOrEmpty(nextBufferLine))
                {
                    if (this.NoMoreBufferLines())
                    {
                        LogHelper.Instance.Log("Call Read from ProcessReadBuilder 1.", LogEntryType.Debug);
                        this.Read();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in ProcessReadBuilder near GetNextBufferLine.", ex);
                LogHelper.Instance.Log("Call Read from ProcessReadBuilder 2.", LogEntryType.Debug);
                this.Read();
                return;
            }
            if (string.IsNullOrEmpty(nextBufferLine))
            {
                LogHelper.Instance.Log("Call Read from ProcessReadBuilder 3.", LogEntryType.Debug);
                this.Read();
            }
            else if (nextBufferLine.StartsWith("quit", StringComparison.CurrentCultureIgnoreCase))
            {
                LogHelper.Instance.Log(nextBufferLine, LogEntryType.Debug);
                this.Write("Goodbye!");
                this.InternalClose();
            }
            else
            {
                try
                {
                    string originIP = string.Empty;
                    try
                    {
                        originIP = this.GetRemoteHostIP();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log("Error accessing remote host ip.", ex);
                    }
                    if (this._beforeCommandAction != null)
                        this._beforeCommandAction((ISession)this);
                    CommandResult commandResult = CommandService.Instance.Execute((ISession)this, originIP, nextBufferLine, this.Filters, this.EnableFilters, this._paramAction);
                    if (commandResult == null)
                        return;
                    ProtocolHelper.FormatErrors(commandResult.Errors, (IList<string>)commandResult.Messages);
                    this.FlushSinkQueue((ICollection<string>)commandResult.Messages);
                    this.Write(commandResult.ToString());
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("An unhandled exception was raised in ProcessReadBuilder thread pool work item, executing command.", ex);
                }
                finally
                {
                    LogHelper.Instance.Log("Call Read from ProcessReadBuilder 4.", LogEntryType.Debug);
                    this.Read();
                }
            }
        }

        private void InternalClose()
        {
            try
            {
                if (this.Disconnect != null)
                    this.Disconnect((object)this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in Disconnect event handler.", ex);
            }
            try
            {
                this.CloseClients();
                this.CloseStreams();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in TcpServerSession.InternalClose.", ex);
            }
            finally
            {
                BufferPool.Instance.Checkin(this.ReadBuffer);
                if (this.ServiceHost != null)
                    this.ServiceHost.UnregisterSession((ISession)this);
            }
        }

        private void EnqueueSinkMessage(string message)
        {
            lock (this.m_syncLock)
                this.MessageSinkQueue.Add(message);
        }

        private void FlushSinkQueue(ICollection<string> messages)
        {
            lock (this.m_syncLock)
            {
                while (this.MessageSinkQueue.Count > 0)
                {
                    string messageSink = this.MessageSinkQueue[0];
                    this.MessageSinkQueue.RemoveAt(0);
                    messages.Add(messageSink);
                }
            }
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

        private bool NoMoreBufferLines()
        {
            return this.ReadBuilder.ToString().IndexOf(Environment.NewLine) == -1;
        }

        private void ResetReadBuilder() => this.ReadBuilder.Length = 0;

        private IPCServiceHost ServiceHost { get; set; }

        private List<string> MessageSinkQueue
        {
            get
            {
                if (this.m_messageSinkQueue == null)
                    this.m_messageSinkQueue = new List<string>();
                return this.m_messageSinkQueue;
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

        private bool IsSecure
        {
            get
            {
                return this.ServiceHost.EncryptionProtocol != SslProtocols.None && this.ServiceHost.Certificate != null;
            }
        }
    }
}
