using System;
using System.IO;
using System.IO.Pipes;
using Redbox.Core;

namespace Redbox.IPC.Framework.NamedPipes
{
    public class NamedPipeClientSession : ClientSession
    {
        private NamedPipeClientStream m_stream;

        protected internal NamedPipeClientSession(IPCProtocol protocol)
            : base(protocol)
        {
            var serverName = protocol.Host;
            if (serverName.Equals("localhost", StringComparison.CurrentCultureIgnoreCase) ||
                serverName.Equals("127.0.0.1"))
                serverName = ".";
            m_stream = new NamedPipeClientStream(serverName, protocol.Port, PipeDirection.InOut,
                PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            ReadBuffer = new byte[1024];
        }

        public string PipeName { get; private set; }

        protected override Stream Stream
        {
            get
            {
                if (m_stream == null)
                {
                    var pipeClientStream = new NamedPipeClientStream(HostName, PipeName, PipeDirection.InOut);
                    pipeClientStream.ReadTimeout = Timeout;
                    m_stream = pipeClientStream;
                    ReadBuffer = new byte[m_stream.InBufferSize];
                }

                return m_stream;
            }
        }

        public override bool Connect()
        {
            try
            {
                ConnectThrowOnError();
                return true;
            }
            catch (IOException ex)
            {
                return false;
            }
        }

        public override void ConnectThrowOnError()
        {
            var flag = false;
            for (var index = 0; index < 3; ++index)
                if (!flag)
                    try
                    {
                        if (!m_stream.IsConnected)
                        {
                            m_stream.Connect(5000);
                            if (m_stream.IsConnected)
                                flag = true;
                        }
                    }
                    catch (TimeoutException ex)
                    {
                        LogHelper.Instance.Log(
                            string.Format("NamedPipeclient.Connect() timed out; still have {0} re-connect attempts.",
                                3 - index), LogEntryType.Info);
                    }
                else
                    break;

            if (!flag)
                throw new TimeoutException("Failed to connect to pipe server after 3 tries; maybe it's down?");
            ConsumeMessages();
            IsConnected = true;
        }

        protected internal override bool IsConnectionAvailable()
        {
            return ((PipeStream)Stream).IsConnected;
        }

        protected internal override void CustomClose()
        {
        }

        protected override bool CanRead()
        {
            return Stream.CanRead;
        }

        protected override int GetAvailableData()
        {
            return !((PipeStream)Stream).IsMessageComplete ? 0 : 1;
        }
    }
}