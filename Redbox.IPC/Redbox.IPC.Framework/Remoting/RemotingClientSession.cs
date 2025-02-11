using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using Redbox.Core;

namespace Redbox.IPC.Framework.Remoting
{
    public class RemotingClientSession : ClientSession
    {
        private static readonly object m_lockObject = new object();

        public RemotingClientSession(IPCProtocol protocol, int timeout)
            : base(protocol)
        {
            Timeout = timeout;
            Url = string.Format("tcp://{0}:{1}/IPCFramework/CommandService.rem", Protocol.Host, Protocol.Port);
        }

        protected override Stream Stream => throw new NotImplementedException();

        private string Url { get; }

        public override bool Connect()
        {
            IsConnected = true;
            return true;
        }

        public override void ConnectThrowOnError()
        {
            IsConnected = true;
        }

        public override void Close()
        {
        }

        public override void Dispose()
        {
            if (OwningPool != null)
                OwningPool.ReturnSession(this);
            base.Dispose();
        }

        public override List<string> ExecuteCommand(string command)
        {
            if (IsRegistrationRequired())
                RegisterChannel();
            var str =
                ((IRemotingHost)new RemotingClientProxy(typeof(IRemotingHost), Url, Timeout).GetTransparentProxy())
                .ExecuteCommand(command);
            if (string.IsNullOrEmpty(str))
                return new List<string>();
            return new List<string>(str.Split(new string[1]
            {
                "\r\n"
            }, StringSplitOptions.RemoveEmptyEntries));
        }

        protected internal override bool IsConnectionAvailable()
        {
            return true;
        }

        protected internal override void CustomClose()
        {
        }

        protected override int GetAvailableData()
        {
            throw new NotImplementedException();
        }

        protected override bool CanRead()
        {
            throw new NotImplementedException();
        }

        private void RegisterChannel()
        {
            lock (m_lockObject)
            {
                if (!IsRegistrationRequired())
                    return;
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["CallRemotingConfigure"]))
                    RemotingConfiguration.Configure(null, Protocol.IsSecure);
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["CustomErrorsMode"]))
                    RemotingConfiguration.CustomErrorsMode =
                        Enum<CustomErrorsModes>.ParseIgnoringCase(ConfigurationManager.AppSettings["CustomErrorsMode"],
                            CustomErrorsModes.RemoteOnly);
                LogHelper.Instance.Log("CustomErrors enabled = {0}, {1}",
                    RemotingConfiguration.CustomErrorsEnabled(false), RemotingConfiguration.CustomErrorsMode);
                var sinkProvider = new BinaryClientFormatterSinkProvider();
                sinkProvider.Next = new RemotingClientChannelSinkProvider(sinkProvider.Next);
                ChannelServices.RegisterChannel(new TcpClientChannel(GetProperties(), sinkProvider), Protocol.IsSecure);
            }
        }

        private static bool IsRegistrationRequired()
        {
            return ChannelServices.GetChannel("ipc_tcp") == null;
        }

        private ListDictionary GetProperties()
        {
            return new ListDictionary
            {
                {
                    "port",
                    "0"
                },
                {
                    "timeout",
                    Timeout
                },
                {
                    "name",
                    "ipc_tcp"
                }
            };
        }
    }
}