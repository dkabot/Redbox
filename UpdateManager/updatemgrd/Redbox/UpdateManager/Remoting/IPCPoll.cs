using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.UpdateManager.Remoting
{
    internal class IPCPoll : IPoll
    {
        private string m_storeNumber;
        private string m_url;
        private TimeSpan m_timeout;
        private int m_version;

        public static IPCPoll Instance => Singleton<IPCPoll>.Instance;

        public void Initialize(string storeNumber, string url, TimeSpan timeout, int version)
        {
            this.m_timeout = timeout;
            this.m_url = url;
            this.m_storeNumber = storeNumber;
            this.m_version = version;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList Poll(
          List<PollRequest> pollRequestList,
          out List<PollReply> pollReplyList)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errors = new Redbox.UpdateManager.ComponentModel.ErrorList();
            pollReplyList = new List<PollReply>();
            try
            {
                if (string.IsNullOrEmpty(this.m_storeNumber))
                {
                    errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("WCFP01", "WCF Poll has not been initialized", "Please initialize the WCF Poll class"));
                    return errors;
                }
                if (string.IsNullOrEmpty(this.m_url))
                {
                    errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("WCFP02", "WCF Poll has not been initialized", "Please initialize the WCF Poll class"));
                    return errors;
                }
                LogHelper.Instance.Log("Timeout for IPC poll is " + (object)this.m_timeout.TotalSeconds + " seconds.", this.m_timeout < TimeSpan.FromSeconds(30.0) ? LogEntryType.Info : LogEntryType.Debug);
                using (Redbox.UpdateService.Client.UpdateService service = IPCPoll.GetService(this.m_url, this.m_timeout))
                {
                    ClientCommandResult clientCommandResult = service.Poll(this.m_storeNumber, this.m_version, pollRequestList, out pollReplyList);
                    errors.AddRange(IPCPoll.IPC((IEnumerable<Redbox.IPC.Framework.Error>)clientCommandResult.Errors));
                }
            }
            catch (Exception ex)
            {
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("IPCP01", "An unhandled exception occurred while doing an IPC poll.", ex));
            }
            errors.ToLogHelper();
            return errors;
        }

        private static Redbox.UpdateService.Client.UpdateService GetService(
          string url,
          TimeSpan timeout)
        {
            return Redbox.UpdateService.Client.UpdateService.GetService(url, Convert.ToInt32(timeout.TotalMilliseconds));
        }

        private static IEnumerable<Redbox.UpdateManager.ComponentModel.Error> IPC(
          IEnumerable<Redbox.IPC.Framework.Error> errors)
        {
            return errors.Select<Redbox.IPC.Framework.Error, Redbox.UpdateManager.ComponentModel.Error>((Func<Redbox.IPC.Framework.Error, Redbox.UpdateManager.ComponentModel.Error>)(each => !each.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewError(each.Code, each.Description, each.Details) : Redbox.UpdateManager.ComponentModel.Error.NewWarning(each.Code, each.Description, each.Details)));
        }

        private IPCPoll()
        {
        }
    }
}
