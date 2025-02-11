using Redbox.Core;
using System;
using System.IO;
using System.IO.Pipes;

namespace Redbox.IPC.Framework.NamedPipes
{
    internal class NamedPipeServerSession : ServerSession
    {
        public NamedPipeServerSession(Stream stream, IPCServiceHost serviceHost)
          : base(serviceHost)
        {
            this.NegotiateSecurityLayer(stream);
        }

        protected override bool CloseClientsInternal() => true;

        protected override string GetRemoteHostIP()
        {
            throw new NotSupportedException("Accessing remote endpoint ip address is not supported in Named Pipe Server Session.");
        }

        protected override bool IsConnectedInternal()
        {
            try
            {
                return this.Stream is NamedPipeServerStream stream && stream.IsConnected;
            }
            catch (IOException ex)
            {
                LogHelper.Instance.Log("NamedPipeServerSession.CloseStreamsInternal encountered an I/O Exception.", (Exception)ex);
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("NamedPipeServerSession.IsConnectedInternal() encountered an unhandled exception", ex);
                return false;
            }
        }

        protected override bool CloseStreamsInternal()
        {
            try
            {
                if (!(this.Stream is NamedPipeServerStream stream))
                    return false;
                if (stream.IsConnected)
                {
                    stream.WaitForPipeDrain();
                    stream.Disconnect();
                }
                stream.Close();
                return true;
            }
            catch (IOException ex)
            {
                LogHelper.Instance.Log("NamedPipeServerSession.CloseStreamsInternal encountered an I/O Exception.", (Exception)ex);
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("NamedPipeServerSession.CloseStreamsInternal() encountered an unhandled exception", ex);
                return false;
            }
        }
    }
}
