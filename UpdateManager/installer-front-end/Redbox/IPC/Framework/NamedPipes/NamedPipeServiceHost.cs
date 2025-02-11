using Redbox.Core;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace Redbox.IPC.Framework.NamedPipes
{
    internal class NamedPipeServiceHost : IPCServiceHost
    {
        public override bool Start()
        {
            this.Alive = true;
            LogHelper.Instance.Log("Start Worker Pool threads.");
            this.WorkerPool.Start();
            LogHelper.Instance.Log(string.Format("Pipe Server on host {0} accepting connections on pipe {1}", (object)this.Protocol.Host, (object)this.Protocol.Port), LogEntryType.Info);
            try
            {
                while (this.Alive)
                {
                    NamedPipeServerStream pipeServerStream = new NamedPipeServerStream(this.Protocol.Port, PipeDirection.InOut, -1, PipeTransmissionMode.Byte, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
                    pipeServerStream.WaitForConnection();
                    ServerSession serverSession = (ServerSession)new NamedPipeServerSession((Stream)pipeServerStream, (IPCServiceHost)this);
                    this.RegiserSession((ISession)serverSession);
                    serverSession.Start();
                }
            }
            catch (ThreadInterruptedException ex)
            {
                LogHelper.Instance.Log("Server thread interrupted", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Caught exception in NamedPipeServiceListener.Start", ex);
            }
            return true;
        }

        public override void Stop()
        {
            LogHelper.Instance.Log(string.Format("Attempting to Stop NamedPipeServiceHost on host {0} with pipe {1}", (object)this.Protocol.Host, (object)this.Protocol.Port), LogEntryType.Info);
            this.Alive = false;
            this.WorkerPool.Shutdown();
            BufferPool.Instance.Shutdown();
        }
    }
}
