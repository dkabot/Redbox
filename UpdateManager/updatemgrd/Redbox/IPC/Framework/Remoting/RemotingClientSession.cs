using Redbox.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Redbox.IPC.Framework.Remoting
{
    internal class RemotingClientSession : ClientSession
    {
        private static readonly object m_lockObject = new object();

        public RemotingClientSession(IPCProtocol protocol, int timeout)
          : base(protocol)
        {
            this.Timeout = timeout;
            this.Url = string.Format("tcp://{0}:{1}/IPCFramework/CommandService.rem", (object)this.Protocol.Host, (object)this.Protocol.Port);
        }

        public override bool Connect()
        {
            this.IsConnected = true;
            return true;
        }

        public override void ConnectThrowOnError() => this.IsConnected = true;

        public override void Close()
        {
        }

        public override void Dispose()
        {
            if (this.OwningPool != null)
                this.OwningPool.ReturnSession((ClientSession)this);
            base.Dispose();
        }

        public override List<string> ExecuteCommand(string command)
        {
            if (RemotingClientSession.IsRegistrationRequired())
                this.RegisterChannel();
            string str = ((IRemotingHost)new RemotingClientProxy(typeof(IRemotingHost), this.Url, this.Timeout).GetTransparentProxy()).ExecuteCommand(command);
            if (string.IsNullOrEmpty(str))
                return new List<string>();
            return new List<string>((IEnumerable<string>)str.Split(new string[1]
            {
        "\r\n"
            }, StringSplitOptions.RemoveEmptyEntries));
        }

        protected internal override bool IsConnectionAvailable() => true;

        protected internal override void CustomClose()
        {
        }

        protected override int GetAvailableData() => throw new NotImplementedException();

        protected override bool CanRead() => throw new NotImplementedException();

        protected override Stream Stream => throw new NotImplementedException();

        private void RegisterChannel()
        {
            lock (RemotingClientSession.m_lockObject)
            {
                if (!RemotingClientSession.IsRegistrationRequired())
                    return;
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["CallRemotingConfigure"]))
                    RemotingConfiguration.Configure((string)null, this.Protocol.IsSecure);
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["CustomErrorsMode"]))
                    RemotingConfiguration.CustomErrorsMode = Enum<CustomErrorsModes>.ParseIgnoringCase(ConfigurationManager.AppSettings["CustomErrorsMode"], CustomErrorsModes.RemoteOnly);
                LogHelper.Instance.Log("CustomErrors enabled = {0}, {1}", (object)RemotingConfiguration.CustomErrorsEnabled(false), (object)RemotingConfiguration.CustomErrorsMode);
                BinaryClientFormatterSinkProvider sinkProvider = new BinaryClientFormatterSinkProvider();
                sinkProvider.Next = (IClientChannelSinkProvider)new RemotingClientChannelSinkProvider(sinkProvider.Next);
                ChannelServices.RegisterChannel((IChannel)new TcpClientChannel((IDictionary)this.GetProperties(), (IClientChannelSinkProvider)sinkProvider), this.Protocol.IsSecure);
            }
        }

        private static bool IsRegistrationRequired() => ChannelServices.GetChannel("ipc_tcp") == null;

        private ListDictionary GetProperties()
        {
            return new ListDictionary()
      {
        {
          (object) "port",
          (object) "0"
        },
        {
          (object) "timeout",
          (object) this.Timeout
        },
        {
          (object) "name",
          (object) "ipc_tcp"
        }
      };
        }

        private string Url { set; get; }
    }
}
