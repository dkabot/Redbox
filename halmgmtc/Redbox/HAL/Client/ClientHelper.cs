using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Redbox.IPC.Framework;
using ConversionHelper = Redbox.HAL.Component.Model.Extensions.ConversionHelper;

namespace Redbox.HAL.Client
{
    public sealed class ClientHelper : IDisposable
    {
        public delegate void InitComplete();

        private readonly ManualResetEvent m_resetEvent = new ManualResetEvent(false);
        private readonly IClientOutputSink Sink;
        private bool Disposed;

        public ClientHelper(IClientOutputSink sink, HardwareService service)
        {
            Sink = sink == null ? NullSink.Instance : sink;
            Service = service;
        }

        public ClientHelper(HardwareService service)
            : this(NullSink.Instance, service)
        {
        }

        public HardwareService Service { get; private set; }

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            m_resetEvent.Close();
        }

        public bool BootstrapInitRunning()
        {
            if (Service == null)
            {
                Sink.WriteMessage("The service is not configured.");
                return false;
            }

            string status;
            if (!Service.GetInitStatus(out status).Success)
            {
                Sink.WriteMessage("Unable to determine init status.");
                return false;
            }

            if (!("COMPLETED" == status) && !("ERRORED" == status))
                return true;
            Sink.WriteMessage("Boot init ended with status: {0}", status);
            return false;
        }

        public bool WaitforInit(int pause)
        {
            return WaitForInitInner(pause);
        }

        public bool WaitforInit()
        {
            return WaitForInitInner(5000);
        }

        public bool WaitforInit(InitComplete onDone)
        {
            var num = WaitForInitInner(5000) ? 1 : 0;
            if (onDone == null)
                return num != 0;
            onDone();
            return num != 0;
        }

        public HardwareService Connect()
        {
            return ConnectInner(Constants.HALIPCStrings.TcpServer);
        }

        public HardwareService Connect(string ipc)
        {
            return !string.IsNullOrEmpty(ipc)
                ? ConnectInner(ipc)
                : throw new UriFormatException("The URI string is null or empty - please re-configure.");
        }

        public bool TestCommunication()
        {
            if (Service == null)
                ConnectInner(Constants.HALIPCStrings.TcpServer);
            return TestServiceConnection(Service);
        }

        public bool WaitForJob(HardwareJob job, out HardwareJobStatus endStatus)
        {
            endStatus = HardwareJobStatus.Completed;
            try
            {
                var _s0 = HardwareJobStatus.Completed;
                var waitForJob = true;
                job.StatusChanged += (j, status) =>
                {
                    _s0 = status;
                    waitForJob = status != HardwareJobStatus.Stopped && status != HardwareJobStatus.Errored &&
                                 status != HardwareJobStatus.Garbage && status != HardwareJobStatus.Completed &&
                                 status != 0;
                };
                job.Connect();
                job.Pend();
                while (waitForJob)
                    m_resetEvent.WaitOne(250, false);
                endStatus = _s0;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                job.Disconnect();
            }
        }

        public bool ExecuteImmediate(string inst)
        {
            return Service != null && Service.ExecuteImmediate(inst, out _).Success;
        }

        public T ExecuteImmediateAndGetResult<T>(HardwareService service, string inst)
        {
            HardwareJob job;
            var hardwareCommandResult = service.ExecuteImmediate(inst, out job);
            return hardwareCommandResult == null || !hardwareCommandResult.Success
                ? default
                : (T)ConversionHelper.ChangeType(GetStackEntriesInner(job, 1)[0], typeof(T));
        }

        private bool WaitForInitInner(int pause)
        {
            Sink.WriteMessage("Checking for bootstrap init.");
            while (BootstrapInitRunning())
            {
                Sink.WriteMessage("{0}: still waiting for bootstrap init.", DateTime.Now.ToLongTimeString());
                Thread.Sleep(pause);
            }

            Sink.WriteMessage("Init done");
            return true;
        }

        private HardwareService ConnectInner(string ipcUrl)
        {
            try
            {
                var service = new HardwareService(IPCProtocol.Parse(ipcUrl));
                return TestServiceConnection(service) ? service : null;
            }
            catch (UriFormatException ex)
            {
                return Service = null;
            }
        }

        private bool TestServiceConnection(HardwareService service)
        {
            if (service == null)
                return false;
            var hardwareCommandResult = service.ExecuteServiceCommand("SERVICE test-comm", 5000);
            return hardwareCommandResult.Success && hardwareCommandResult.CommandMessages[0] == "ACK";
        }

        private string FindLocalIP()
        {
            foreach (var address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                if (address.AddressFamily == AddressFamily.InterNetwork)
                    return address.ToString();
            return null;
        }

        private string[] GetStackEntriesInner(HardwareJob job, int depth)
        {
            var stackEntriesInner = new string[depth];
            for (var index = 0; index < depth; ++index)
                stackEntriesInner[index] = string.Empty;
            Stack<string> stack;
            if (job.GetStack(out stack).Success && stack.Count >= depth)
                for (var index = 0; index < depth; ++index)
                    stackEntriesInner[index] = stack.Pop();
            return stackEntriesInner;
        }
    }
}