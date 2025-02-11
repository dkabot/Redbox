using System;
using System.Reflection;
using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework.Pipes;

namespace Redbox.HAL.IPC.Framework.Server
{
    public sealed class IpcServiceHostFactory : IIpcServiceHostFactory
    {
        private readonly IpcHostVersion Version;

        public IpcServiceHostFactory()
            : this(IpcHostVersion.Legacy)
        {
        }

        public IpcServiceHostFactory(IpcHostVersion version)
        {
            Version = version;
        }

        public IIpcServiceHost Create(IIpcProtocol protocol)
        {
            throw new NotImplementedException();
        }

        public IIpcServiceHost Create(IIpcProtocol protocol, IHostInfo info)
        {
            if (protocol.Channel == ChannelType.NamedPipe)
                return new NamedPipeServiceHost(protocol, info);
            if (protocol.Channel == ChannelType.Socket)
                return CreateTcpHost(protocol, info);
            LogHelper.Instance.Log(LogEntryType.Error,
                "Unsupported channel type '{0}' specified; try using sockets or pipes in your protocol uri.",
                protocol.Channel);
            return null;
        }

        public IHostInfo Create(Assembly a)
        {
            return new HostInfo(a);
        }

        private IIpcServiceHost CreateTcpHost(IIpcProtocol protocol, IHostInfo info)
        {
            return IpcHostVersion.Modern != Version
                ? new TcpServiceHost(protocol, info)
                : (IIpcServiceHost)new Sockets.TcpServiceHost(protocol, info);
        }
    }
}