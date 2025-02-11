using Redbox.Core;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Redbox.IPC.Framework.Remoting
{
    internal class RemotingServiceHost : IPCServiceHost
    {
        private TcpServerChannel m_tcpChannel;

        public RemotingServiceHost(int maxThreads) => this.MaxThreads = maxThreads;

        public override bool Start()
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["CallRemotingConfigure"]))
            {
                LogHelper.Instance.Log("Before configure - CustomErrors enabled = {0}, {1}", (object)RemotingConfiguration.CustomErrorsEnabled(false), (object)RemotingConfiguration.CustomErrorsMode);
                RemotingConfiguration.Configure((string)null, this.Protocol.IsSecure);
                LogHelper.Instance.Log("After configure - CustomErrors enabled = {0}, {1}", (object)RemotingConfiguration.CustomErrorsEnabled(false), (object)RemotingConfiguration.CustomErrorsMode);
            }
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["CustomErrorsMode"]))
                RemotingConfiguration.CustomErrorsMode = Enum<CustomErrorsModes>.ParseIgnoringCase(ConfigurationManager.AppSettings["CustomErrorsMode"], CustomErrorsModes.RemoteOnly);
            LogHelper.Instance.Log("CustomErrors enabled = {0}, {1}", (object)RemotingConfiguration.CustomErrorsEnabled(false), (object)RemotingConfiguration.CustomErrorsMode);
            ListDictionary properties = new ListDictionary()
      {
        {
          (object) "port",
          (object) this.Protocol.Port
        },
        {
          (object) "name",
          (object) "ipc_tcp"
        }
      };
            LogHelper.Instance.Log("Creating tcp channel for remoting");
            this.m_tcpChannel = new TcpServerChannel((IDictionary)properties, (IServerChannelSinkProvider)new RemotingServerChannelSinkProvider(this, (IServerFormatterSinkProvider)new BinaryServerFormatterSinkProvider()));
            LogHelper.Instance.Log("Registering channel with channel services");
            ChannelServices.RegisterChannel((IChannel)this.m_tcpChannel, this.Protocol.IsSecure);
            WellKnownServiceTypeEntry entry = new WellKnownServiceTypeEntry(typeof(RemotingHost), "CommandService.rem", WellKnownObjectMode.SingleCall);
            RemotingConfiguration.ApplicationName = "IPCFramework";
            RemotingConfiguration.RegisterWellKnownServiceType(entry);
            this.Alive = true;
            Statistics.Instance.ServerStartTime = DateTime.Now;
            LogHelper.Instance.Log("Begin processing incoming requests.");
            return true;
        }

        public override void Stop()
        {
            if (!this.Alive)
                return;
            this.m_tcpChannel.StopListening((object)null);
            ChannelServices.UnregisterChannel((IChannel)this.m_tcpChannel);
            this.Alive = false;
        }

        public int MaxThreads { get; set; }
    }
}
