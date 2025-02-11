using System;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

namespace Redbox.IPC.Framework.Remoting
{
    public class RemotingServerSink :
        BaseChannelSinkWithProperties,
        IServerChannelSink,
        IChannelSinkBase
    {
        public RemotingServerSink(RemotingServiceHost serviceHost, IServerChannelSink nextSink)
        {
            NextChannelSink = nextSink;
            RemotingHost = serviceHost;
        }

        private RemotingServiceHost RemotingHost { get; }

        public ServerProcessing ProcessMessage(
            IServerChannelSinkStack sinkStack,
            IMessage requestMsg,
            ITransportHeaders requestHeaders,
            Stream requestStream,
            out IMessage responseMsg,
            out ITransportHeaders responseHeaders,
            out Stream responseStream)
        {
            CallContext.SetData("RemoteHostIP", requestHeaders["__IPAddress"]);
            CallContext.SetData("RemotingHost", RemotingHost);
            sinkStack.Push(this, null);
            return NextChannelSink.ProcessMessage(sinkStack, requestMsg, requestHeaders, requestStream, out responseMsg,
                out responseHeaders, out responseStream);
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
            return null;
        }

        public IServerChannelSink NextChannelSink { get; }
    }
}