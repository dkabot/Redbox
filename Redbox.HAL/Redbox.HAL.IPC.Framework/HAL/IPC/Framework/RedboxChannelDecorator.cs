using System;
using System.Text;
using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework
{
    internal static class RedboxChannelDecorator
    {
        internal static bool Write(IIPCChannel channel, string line)
        {
            if (!line.EndsWith(Environment.NewLine))
                line = string.Format("{0}{1}", line, Environment.NewLine);
            var bytes = Encoding.ASCII.GetBytes(line);
            var num = channel.Write(bytes) ? 1 : 0;
            if (num == 0)
                return num != 0;
            Interlocked.Add(ref Statistics.Instance.NumberOfBytesSent, bytes.Length);
            return num != 0;
        }
    }
}