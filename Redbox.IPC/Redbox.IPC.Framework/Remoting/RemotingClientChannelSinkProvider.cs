using System.Runtime.Remoting.Channels;

namespace Redbox.IPC.Framework.Remoting
{
    public class RemotingClientChannelSinkProvider : IClientChannelSinkProvider
    {
        public RemotingClientChannelSinkProvider(IClientChannelSinkProvider nextProvider)
        {
            Next = nextProvider;
        }

        public IClientChannelSink CreateSink(
            IChannelSender channel,
            string url,
            object remoteChannelData)
        {
            return new RemotingClientSink(Next.CreateSink(channel, url, remoteChannelData), remoteChannelData);
        }

        public IClientChannelSinkProvider Next { set; get; }
    }
}