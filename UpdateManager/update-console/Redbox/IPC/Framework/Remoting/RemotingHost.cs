using Redbox.Core;
using System;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace Redbox.IPC.Framework.Remoting
{
    internal class RemotingHost : MarshalByRefObject, IRemotingHost
    {
        private static long m_connectionCount;

        public string ExecuteCommand(string commandString)
        {
            using (ExecutionTimer timer = new ExecutionTimer())
            {
                try
                {
                    Interlocked.Add(ref Statistics.Instance.NumberOfBytesReceived, (long)commandString.Length);
                    RemotingServiceHost data = (RemotingServiceHost)CallContext.GetData(nameof(RemotingHost));
                    if (Interlocked.Read(ref RemotingHost.m_connectionCount) >= (long)data.MaxThreads)
                        return CommandService.Instance.GetServerBusyResult(timer, data.MaxThreads);
                    Interlocked.Increment(ref RemotingHost.m_connectionCount);
                    LogHelper.Instance.Log(string.Format("There are {0} connections active", (object)Interlocked.Read(ref RemotingHost.m_connectionCount)), LogEntryType.Debug);
                    string str = new RemotingServerSession().ProcessRequest(commandString);
                    Interlocked.Add(ref Statistics.Instance.NumberOfBytesSent, (long)str.Length);
                    Interlocked.Decrement(ref RemotingHost.m_connectionCount);
                    return str;
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("RemotingHost execution error", ex);
                    return (string)null;
                }
            }
        }
    }
}
