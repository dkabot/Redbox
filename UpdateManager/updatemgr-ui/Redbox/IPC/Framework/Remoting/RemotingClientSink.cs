using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

namespace Redbox.IPC.Framework.Remoting
{
    internal class RemotingClientSink :
      BaseChannelSinkWithProperties,
      IClientChannelSink,
      IChannelSinkBase
    {
        public RemotingClientSink(IClientChannelSink nextSink, object remoteChannelData)
        {
            this.NextChannelSink = nextSink;
            MethodInfo setMethod = this.NextChannelSink.GetType().GetProperty("Item").GetSetMethod();
            setMethod.Invoke((object)this.NextChannelSink, new object[2]
            {
        (object) "timeout",
        remoteChannelData
            });
            setMethod.Invoke((object)this.NextChannelSink, (object[])new string[2]
            {
        "connectiongroupname",
        remoteChannelData.ToString()
            });
        }

        public void ProcessMessage(
          IMessage msg,
          ITransportHeaders requestHeaders,
          Stream requestStream,
          out ITransportHeaders responseHeaders,
          out Stream responseStream)
        {
            this.NextChannelSink.ProcessMessage(msg, requestHeaders, requestStream, out responseHeaders, out responseStream);
        }

        public void AsyncProcessRequest(
          IClientChannelSinkStack sinkStack,
          IMessage msg,
          ITransportHeaders headers,
          Stream stream)
        {
            this.NextChannelSink.AsyncProcessRequest(sinkStack, msg, headers, stream);
        }

        public void AsyncProcessResponse(
          IClientResponseChannelSinkStack sinkStack,
          object state,
          ITransportHeaders headers,
          Stream stream)
        {
            this.NextChannelSink.AsyncProcessResponse(sinkStack, state, headers, stream);
        }

        public Stream GetRequestStream(IMessage msg, ITransportHeaders headers) => (Stream)null;

        public IClientChannelSink NextChannelSink { get; private set; }
    }
}
