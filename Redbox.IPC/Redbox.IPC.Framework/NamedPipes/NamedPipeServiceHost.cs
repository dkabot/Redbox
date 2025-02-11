using System;
using System.IO.Pipes;
using System.Threading;
using Redbox.Core;

namespace Redbox.IPC.Framework.NamedPipes
{
    public class NamedPipeServiceHost : IPCServiceHost
    {
        public override bool Start()
        {
            Alive = true;
            LogHelper.Instance.Log("Start Worker Pool threads.");
            WorkerPool.Start();
            LogHelper.Instance.Log(
                string.Format("Pipe Server on host {0} accepting connections on pipe {1}", Protocol.Host,
                    Protocol.Port), LogEntryType.Info);
            try
            {
                while (Alive)
                {
                    var pipeServerStream = new NamedPipeServerStream(Protocol.Port, PipeDirection.InOut, -1,
                        PipeTransmissionMode.Byte, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
                    pipeServerStream.WaitForConnection();
                    var serverSession = (ServerSession)new NamedPipeServerSession(pipeServerStream, this);
                    RegiserSession(serverSession);
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
            LogHelper.Instance.Log(
                string.Format("Attempting to Stop NamedPipeServiceHost on host {0} with pipe {1}", Protocol.Host,
                    Protocol.Port), LogEntryType.Info);
            Alive = false;
            WorkerPool.Shutdown();
            BufferPool.Instance.Shutdown();
        }
    }
}