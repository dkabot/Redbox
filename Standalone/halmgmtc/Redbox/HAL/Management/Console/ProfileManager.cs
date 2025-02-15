using System;
using Redbox.HAL.Client;
using Redbox.HAL.Core;
using Redbox.HAL.Management.Console.Properties;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Management.Console
{
    public class ProfileManager
    {
        private readonly string DefaultProtocol = Settings.Default.DefaultConnectionURL;
        private ClientHelper Helper;
        private HardwareService m_service;

        private ProfileManager()
        {
        }

        public static ProfileManager Instance => Singleton<ProfileManager>.Instance;

        public bool IsConnected { get; private set; }

        public HardwareService Service
        {
            get
            {
                if (m_service != null)
                    return m_service;
                OutputWindow.Instance.Append("The service isn't started");
                return null;
            }
            private set => m_service = value;
        }

        public void OnConnect(object sender, EventArgs e)
        {
            Connect();
        }

        public void OnDisconnect(object sender, EventArgs e)
        {
            Disconnect();
        }

        public bool Connect()
        {
            if (Service == null)
            {
                Service = new HardwareService(IPCProtocol.Parse(DefaultProtocol));
                Helper = new ClientHelper(Service);
            }

            if (!Helper.TestCommunication())
                return false;
            IsConnected = true;
            if (Connected != null)
                Connected(this, EventArgs.Empty);
            return true;
        }

        public void Disconnect()
        {
            IsConnected = false;
            JobHelper.DisconnectJobs();
            Service = null;
            if (Disconnected == null)
                return;
            Disconnected(this, EventArgs.Empty);
        }

        public bool GetJob(string jobID, out HardwareJob job)
        {
            job = null;
            if (!IsConnected || string.IsNullOrEmpty(jobID))
                return false;
            var job1 = Instance.Service.GetJob(jobID, out job);
            if (job1.Success)
                return true;
            OutputWindow.Instance.Append(string.Format("Failed to get job {0}", jobID));
            CommonFunctions.ProcessCommandResult(job1);
            return false;
        }

        public event EventHandler Connected;

        public event EventHandler Disconnected;
    }
}