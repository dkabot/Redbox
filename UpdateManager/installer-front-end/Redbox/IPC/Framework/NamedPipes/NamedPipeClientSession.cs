using Redbox.Core;
using System;
using System.IO;
using System.IO.Pipes;

namespace Redbox.IPC.Framework.NamedPipes
{
    internal class NamedPipeClientSession : ClientSession
    {
        private NamedPipeClientStream m_stream;

        public override bool Connect()
        {
            try
            {
                this.ConnectThrowOnError();
                return true;
            }
            catch (IOException ex)
            {
                return false;
            }
        }

        public override void ConnectThrowOnError()
        {
            bool flag = false;
            for (int index = 0; index < 3; ++index)
            {
                if (!flag)
                {
                    try
                    {
                        if (!this.m_stream.IsConnected)
                        {
                            this.m_stream.Connect(5000);
                            if (this.m_stream.IsConnected)
                                flag = true;
                        }
                    }
                    catch (TimeoutException ex)
                    {
                        LogHelper.Instance.Log(string.Format("NamedPipeclient.Connect() timed out; still have {0} re-connect attempts.", (object)(3 - index)), LogEntryType.Info);
                    }
                }
                else
                    break;
            }
            if (!flag)
                throw new TimeoutException("Failed to connect to pipe server after 3 tries; maybe it's down?");
            this.ConsumeMessages();
            this.IsConnected = true;
        }

        public string PipeName { get; private set; }

        protected internal NamedPipeClientSession(IPCProtocol protocol)
          : base(protocol)
        {
            string serverName = protocol.Host;
            if (serverName.Equals("localhost", StringComparison.CurrentCultureIgnoreCase) || serverName.Equals("127.0.0.1"))
                serverName = ".";
            this.m_stream = new NamedPipeClientStream(serverName, protocol.Port, PipeDirection.InOut, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            this.ReadBuffer = new byte[1024];
        }

        protected internal override bool IsConnectionAvailable()
        {
            return ((PipeStream)this.Stream).IsConnected;
        }

        protected internal override void CustomClose()
        {
        }

        protected override bool CanRead() => this.Stream.CanRead;

        protected override int GetAvailableData()
        {
            return !((PipeStream)this.Stream).IsMessageComplete ? 0 : 1;
        }

        protected override Stream Stream
        {
            get
            {
                if (this.m_stream == null)
                {
                    NamedPipeClientStream pipeClientStream = new NamedPipeClientStream(this.HostName, this.PipeName, PipeDirection.InOut);
                    pipeClientStream.ReadTimeout = this.Timeout;
                    this.m_stream = pipeClientStream;
                    this.ReadBuffer = new byte[this.m_stream.InBufferSize];
                }
                return (Stream)this.m_stream;
            }
        }
    }
}
