using System.Runtime.Remoting.Channels;

namespace Redbox.IPC.Framework.Remoting
{
    public class RemotingServerChannelSinkProvider : IServerChannelSinkProvider
    {
        public RemotingServerChannelSinkProvider(
            RemotingServiceHost host,
            IServerFormatterSinkProvider next)
        {
            Next = next;
            RemotingHost = host;
        }

        private RemotingServiceHost RemotingHost { get; }

        public void GetChannelData(IChannelDataStore channelData)
        {
        }

        public IServerChannelSink CreateSink(IChannelReceiver channel)
        {
            return new RemotingServerSink(RemotingHost, Next.CreateSink(channel));
        }

        public IServerChannelSinkProvider Next { get; set; }
    }
}