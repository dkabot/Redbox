using System;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

namespace Redbox.IPC.Framework.Remoting
{
    internal class RemotingServerSink :
      BaseChannelSinkWithProperties,
      IServerChannelSink,
      IChannelSinkBase
    {
        public RemotingServerSink(RemotingServiceHost serviceHost, IServerChannelSink nextSink)
        {
            this.NextChannelSink = nextSink;
            this.RemotingHost = serviceHost;
        }

        public ServerProcessing ProcessMessage(
          IServerChannelSinkStack sinkStack,
          IMessage requestMsg,
          ITransportHeaders requestHeaders,
          Stream requestStream,
          out IMessage responseMsg,
          out ITransportHeaders responseHeaders,
          out Stream responseStream)
        {
            CallContext.SetData("RemoteHostIP", requestHeaders[(object)"__IPAddress"]);
            CallContext.SetData("RemotingHost", (object)this.RemotingHost);
            sinkStack.Push((IServerChannelSink)this, (object)null);
            return this.NextChannelSink.ProcessMessage(sinkStack, requestMsg, requestHeaders, requestStream, out responseMsg, out responseHeaders, out responseStream);
        }

        public void AsyncProcessResponse(
          IServerResponseChannelSinkStack sinkStack,
          object state,
          IMessage msg,
          ITransportHeaders headers,
          Stream stream)
        {
            throw new NotImplementedException();
        }

        public Stream GetResponseStream(
          IServerResponseChannelSinkStack sinkStack,
          object state,
          IMessage msg,
          ITransportHeaders headers)
        {
            return (Stream)null;
        }

        public IServerChannelSink NextChannelSink { get; private set; }

        private RemotingServiceHost RemotingHost { get; set; }
    }
}
