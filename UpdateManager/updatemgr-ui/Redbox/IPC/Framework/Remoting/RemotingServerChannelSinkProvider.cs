using System.Runtime.Remoting.Channels;

namespace Redbox.IPC.Framework.Remoting
{
    internal class RemotingServerChannelSinkProvider : IServerChannelSinkProvider
    {
        public RemotingServerChannelSinkProvider(
          RemotingServiceHost host,
          IServerFormatterSinkProvider next)
        {
            this.Next = (IServerChannelSinkProvider)next;
            this.RemotingHost = host;
        }

        public void GetChannelData(IChannelDataStore channelData)
        {
        }

        public IServerChannelSink CreateSink(IChannelReceiver channel)
        {
            return (IServerChannelSink)new RemotingServerSink(this.RemotingHost, this.Next.CreateSink(channel));
        }

        public IServerChannelSinkProvider Next { get; set; }

        private RemotingServiceHost RemotingHost { get; set; }
    }
}
