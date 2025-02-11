using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace Redbox.IPC.Framework.Remoting
{
    internal class RemotingClientProxy : RealProxy
    {
        public RemotingClientProxy(Type type, string url, int timeout)
          : base(type)
        {
            this.Url = url;
            using (IEnumerator<IChannel> enumerator = ((IEnumerable<IChannel>)ChannelServices.RegisteredChannels).Where<IChannel>((Func<IChannel, bool>)(channel => channel.ChannelName == "ipc_tcp" && channel is IChannelSender)).GetEnumerator())
            {
                if (enumerator.MoveNext())
                    this.SinkChain = ((IChannelSender)enumerator.Current).CreateMessageSink(this.Url, (object)timeout, out string _);
            }
            if (this.SinkChain == null)
                throw new ApplicationException("TcpChannel is not registered");
        }

        public override IMessage Invoke(IMessage msg)
        {
            msg.Properties[(object)"__Uri"] = (object)this.Url;
            return this.SinkChain.SyncProcessMessage(msg);
        }

        private string Url { set; get; }

        private IMessageSink SinkChain { set; get; }
    }
}
