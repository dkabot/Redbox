using System;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework;
using Redbox.HAL.IPC.Framework.Server;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Win32.Commands
{
    public sealed class IPCProxy : IDisposable
    {
        private readonly IIpcProtocol Protocol;
        private bool Disposed;
        private IIpcServiceHost m_listener;

        private IPCProxy(IIpcProtocol protocol)
        {
            Protocol = protocol;
        }

        public bool ProtocolValid => Protocol != null;

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            StopListener();
        }

        public static void InitializeServices(IpcHostVersion version)
        {
            ServiceLocator.Instance.AddService<IIpcServiceHostFactory>(new IpcServiceHostFactory(version));
        }

        public static IPCProxy Parse(string protocol)
        {
            IIpcProtocol protocol1;
            try
            {
                protocol1 = IPCProtocol.Parse(protocol);
            }
            catch (UriFormatException ex)
            {
                LogHelper.Instance.Log("ProtocolURI is malformed.", ex);
                protocol1 = null;
            }

            return new IPCProxy(protocol1);
        }

        public void StartListener()
        {
            if (Protocol == null)
                throw new Exception("Expected a parsed protocol.");
            var assembly = typeof(ProgramCommand).Assembly;
            var service = ServiceLocator.Instance.GetService<IIpcServiceHostFactory>();
            var info = service.Create(assembly);
            m_listener = service.Create(Protocol, info);
            m_listener.Start();
        }

        public void StopListener()
        {
            if (m_listener == null)
                return;
            m_listener.Stop();
        }

        public bool ExecuteStartupFile(string serviceStartup)
        {
            if (File.Exists(serviceStartup))
                using (var reader = new StreamReader(serviceStartup))
                {
                    new BatchCommandRunner(reader).Start();
                }

            return true;
        }
    }
}