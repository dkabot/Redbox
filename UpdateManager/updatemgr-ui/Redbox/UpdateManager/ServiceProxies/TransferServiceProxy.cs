using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.ServiceProxies
{
    internal class TransferServiceProxy : ITransferService
    {
        private string m_url;

        public static TransferServiceProxy Instance => Singleton<TransferServiceProxy>.Instance;

        public void Initialize(string url)
        {
            this.m_url = url;
            ServiceLocator.Instance.AddService(typeof(ITransferService), (object)this);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList CancelAll()
        {
            return TransferServiceProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)this.ExecuteCommandString("transfer cancel-all").Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList GetRepositoriesInTransit(
          out HashSet<string> inTransit)
        {
            inTransit = new HashSet<string>();
            ClientCommandResult clientCommandResult = this.ExecuteCommandString("transfer repositories-in-transit");
            if (clientCommandResult.Success)
                inTransit = new HashSet<string>((IEnumerable<string>)clientCommandResult.CommandMessages[0].ToObject<List<string>>());
            return TransferServiceProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList GetJobs(
          out List<ITransferJob> jobs,
          bool allUsers)
        {
            jobs = new List<ITransferJob>();
            ClientCommandResult clientCommandResult = this.ExecuteCommandString(string.Format("transfer list allUsers: {0}", (object)allUsers));
            if (clientCommandResult.Success)
            {
                List<TransferJobProxy> transferJobProxyList = clientCommandResult.CommandMessages[0].ToObject<List<TransferJobProxy>>();
                transferJobProxyList.ForEach((Action<TransferJobProxy>)(p => p.SetUrl(this.m_url)));
                jobs = transferJobProxyList.ConvertAll<ITransferJob>((Converter<TransferJobProxy, ITransferJob>)(p => (ITransferJob)p));
            }
            return TransferServiceProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList CreateDownloadJob(
          string name,
          out ITransferJob job)
        {
            job = (ITransferJob)null;
            ClientCommandResult clientCommandResult = this.ExecuteCommandString(string.Format("transfer create-download name: '{0}'", (object)name));
            if (clientCommandResult.Success)
            {
                TransferJobProxy transferJobProxy = clientCommandResult.CommandMessages[0].ToObject<TransferJobProxy>();
                transferJobProxy.SetUrl(this.m_url);
                job = (ITransferJob)transferJobProxy;
            }
            return TransferServiceProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList CreateUploadJob(
          string name,
          out ITransferJob job)
        {
            job = (ITransferJob)null;
            ClientCommandResult clientCommandResult = this.ExecuteCommandString(string.Format("transfer create-upload name: '{0}'", (object)name));
            if (clientCommandResult.Success)
            {
                TransferJobProxy transferJobProxy = clientCommandResult.CommandMessages[0].ToObject<TransferJobProxy>();
                transferJobProxy.SetUrl(this.m_url);
                job = (ITransferJob)transferJobProxy;
            }
            return TransferServiceProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList GetJob(Guid id, out ITransferJob job)
        {
            job = (ITransferJob)null;
            ClientCommandResult clientCommandResult = this.ExecuteCommandString(string.Format("transfer get-job id: '{0}'", (object)id));
            if (clientCommandResult.Success)
            {
                TransferJobProxy transferJobProxy = clientCommandResult.CommandMessages[0].ToObject<TransferJobProxy>();
                transferJobProxy.SetUrl(this.m_url);
                job = (ITransferJob)transferJobProxy;
            }
            return TransferServiceProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList AreJobsRunning(out bool isRunning)
        {
            isRunning = false;
            ClientCommandResult clientCommandResult = this.ExecuteCommandString("transfer are-jobs-running");
            if (clientCommandResult.Success)
                isRunning = clientCommandResult.CommandMessages[0].ToObject<bool>();
            return TransferServiceProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors);
        }

        public bool SetMaxBandwidthWhileWithInSchedule(int i)
        {
            int num = this.MaxBandwidthWhileWithInSchedule != i ? 1 : 0;
            this.MaxBandwidthWhileWithInSchedule = i;
            return num != 0;
        }

        public int MaxBandwidthWhileWithInSchedule
        {
            get => BandwidthUsageSettings.MaxBandwidthWhileWithInSchedule;
            private set => BandwidthUsageSettings.MaxBandwidthWhileWithInSchedule = value;
        }

        public bool SetMaxBandwidthWhileOutsideOfSchedule(int max)
        {
            bool flag = false;
            try
            {
                flag = this.MaxBandwidthWhileOutsideOfSchedule != max;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error Accessing BITS settings.", ex);
            }
            this.MaxBandwidthWhileOutsideOfSchedule = max;
            return flag;
        }

        public int MaxBandwidthWhileOutsideOfSchedule
        {
            get => BandwidthUsageSettings.MaxBandwidthWhileWithInSchedule;
            private set => BandwidthUsageSettings.MaxBandwidthWhileWithInSchedule = value;
        }

        public bool SetStartOfScheduleInHoursFromMidnight(byte b)
        {
            bool flag = false;
            try
            {
                flag = (int)this.StartOfScheduleInHoursFromMidnight != (int)b;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error Accessing BITS settings.", ex);
            }
            this.StartOfScheduleInHoursFromMidnight = b;
            return flag;
        }

        public byte StartOfScheduleInHoursFromMidnight
        {
            get => BandwidthUsageSettings.StartOfScheduleInHoursFromMidnight;
            private set => BandwidthUsageSettings.StartOfScheduleInHoursFromMidnight = value;
        }

        public bool SetEndOfScheduleInHoursFromMidnight(byte b)
        {
            bool flag = false;
            try
            {
                flag = (int)this.EndOfScheduleInHoursFromMidnight != (int)b;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error Accessing BITS settings.", ex);
            }
            this.EndOfScheduleInHoursFromMidnight = b;
            return flag;
        }

        public byte EndOfScheduleInHoursFromMidnight
        {
            get => BandwidthUsageSettings.EndOfScheduleInHoursFromMidnight;
            private set => BandwidthUsageSettings.EndOfScheduleInHoursFromMidnight = value;
        }

        public bool SetUseSystemMaxOutsideOfSchedule(bool flag)
        {
            bool flag1 = false;
            try
            {
                flag1 = this.UseSystemMaxOutsideOfSchedule != flag;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error Accessing BITS settings.", ex);
            }
            this.UseSystemMaxOutsideOfSchedule = flag;
            return flag1;
        }

        public bool UseSystemMaxOutsideOfSchedule
        {
            get => BandwidthUsageSettings.UseSystemMaxOutsideOfSchedule;
            private set => BandwidthUsageSettings.UseSystemMaxOutsideOfSchedule = value;
        }

        public bool SetEnableMaximumBandwitdthThrottle(bool flag)
        {
            bool flag1 = false;
            try
            {
                flag1 = this.EnableMaximumBandwitdthThrottle != flag;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error Accessing BITS settings.", ex);
            }
            this.EnableMaximumBandwitdthThrottle = flag;
            return flag1;
        }

        public bool EnableMaximumBandwitdthThrottle
        {
            get => BandwidthUsageSettings.EnableMaximumBandwitdthThrottle;
            private set => BandwidthUsageSettings.EnableMaximumBandwitdthThrottle = value;
        }

        private ClientCommandResult ExecuteCommandString(string command)
        {
            using (UpdateManagerService updateManagerService = new UpdateManagerService(this.m_url))
                return updateManagerService.ExecuteCommandString(command);
        }

        private static Redbox.UpdateManager.ComponentModel.ErrorList ConvertFromIPCErrors(
          List<Redbox.IPC.Framework.Error> ipc)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errors = new Redbox.UpdateManager.ComponentModel.ErrorList();
            ipc.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errors.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details))));
            return errors;
        }

        private TransferServiceProxy()
        {
        }
    }
}
