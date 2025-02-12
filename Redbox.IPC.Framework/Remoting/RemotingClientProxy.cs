using System;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace Redbox.IPC.Framework.Remoting
{
    public class RemotingClientProxy : RealProxy
    {
        public RemotingClientProxy(Type type, string url, int timeout)
            : base(type)
        {
            Url = url;
            using (var enumerator = ChannelServices.RegisteredChannels
                       .Where(channel => channel.ChannelName == "ipc_tcp" && channel is IChannelSender).GetEnumerator())
            {
                if (enumerator.MoveNext())
                    SinkChain = ((IChannelSender)enumerator.Current).CreateMessageSink(Url, timeout, out _);
            }

            if (SinkChain == null)
                throw new ApplicationException("TcpChannel is not registered");
        }

        private string Url { get; }

        private IMessageSink SinkChain { get; }

        public override IMessage Invoke(IMessage msg)
        {
            msg.Properties["__Uri"] = Url;
            return SinkChain.SyncProcessMessage(msg);
        }
    }
}