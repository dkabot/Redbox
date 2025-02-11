using System.Runtime.Remoting.Channels;

namespace Redbox.IPC.Framework.Remoting
{
    internal class RemotingClientChannelSinkProvider : IClientChannelSinkProvider
    {
        public RemotingClientChannelSinkProvider(IClientChannelSinkProvider nextProvider)
        {
            this.Next = nextProvider;
        }

        public IClientChannelSink CreateSink(
          IChannelSender channel,
          string url,
          object remoteChannelData)
        {
            return (IClientChannelSink)new RemotingClientSink(this.Next.CreateSink(channel, url, remoteChannelData), remoteChannelData);
        }

        public IClientChannelSinkProvider Next { set; get; }
    }
}
