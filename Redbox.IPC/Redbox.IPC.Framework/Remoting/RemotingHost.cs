using System;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Redbox.Core;

namespace Redbox.IPC.Framework.Remoting
{
    public class RemotingHost : MarshalByRefObject, IRemotingHost
    {
        private static long m_connectionCount;

        public string ExecuteCommand(string commandString)
        {
            using (var timer = new ExecutionTimer())
            {
                try
                {
                    Interlocked.Add(ref Statistics.Instance.NumberOfBytesReceived, commandString.Length);
                    var data = (RemotingServiceHost)CallContext.GetData(nameof(RemotingHost));
                    if (Interlocked.Read(ref m_connectionCount) >= data.MaxThreads)
                        return CommandService.Instance.GetServerBusyResult(timer, data.MaxThreads);
                    Interlocked.Increment(ref m_connectionCount);
                    LogHelper.Instance.Log(
                        string.Format("There are {0} connections active", Interlocked.Read(ref m_connectionCount)),
                        LogEntryType.Debug);
                    var str = new RemotingServerSession().ProcessRequest(commandString);
                    Interlocked.Add(ref Statistics.Instance.NumberOfBytesSent, str.Length);
                    Interlocked.Decrement(ref m_connectionCount);
                    return str;
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("RemotingHost execution error", ex);
                    return null;
                }
            }
        }
    }
}