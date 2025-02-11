using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace Redbox.UpdateManager.Environment
{
    internal class StoreInfoService : IStoreInfoService, IPollRequestReply
    {
        private int _syncID;
        private StoreInfoChangeSetData _storeInfoChangeSet;
        private string _clientStoreInfoHash;

        public static StoreInfoService Instance => Singleton<StoreInfoService>.Instance;

        public ErrorList GetPollRequest(out List<PollRequest> pollRequestList)
        {
            ErrorList errors = new ErrorList();
            pollRequestList = new List<PollRequest>();
            try
            {
                ClientStoreInfo clientStoreInfo1;
                ErrorList clientStoreInfo2 = this.GetClientStoreInfo(out clientStoreInfo1);
                errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)clientStoreInfo2);
                string json = clientStoreInfo1.ToJson();
                string hash = this.GetHash(clientStoreInfo1);
                if (!string.IsNullOrEmpty(this._clientStoreInfoHash) && this._clientStoreInfoHash != hash)
                    this._syncID = 0;
                this.SetHash(hash);
                if (this._syncID == 0)
                    pollRequestList.Add(new PollRequest()
                    {
                        PollRequestType = PollRequestType.StoreInfo,
                        SyncId = 0,
                        Data = json
                    });
                else
                    pollRequestList.Add(new PollRequest()
                    {
                        PollRequestType = PollRequestType.StoreInfo,
                        SyncId = this._syncID
                    });
            }
            catch (Exception ex)
            {
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SIS0091", "Unhandled exception occurred in StoreInfoModel.GetPollRequest.", ex));
            }
            errors.ToLogHelper();
            return errors;
        }

        public ErrorList ProcessPollReply(List<PollReply> pollReplyList)
        {
            ErrorList errors = new ErrorList();
            try
            {
                if (pollReplyList.ToList<PollReply>().Where<PollReply>((Func<PollReply, bool>)(pr => pr.PollReplyType == PollReplyType.UpdateStoreInfo)).FirstOrDefault<PollReply>() != null)
                {
                    this._syncID = 0;
                    this._clientStoreInfoHash = string.Empty;
                    LogHelper.Instance.Log("Store Info server is clearing the sync id as requested by the server.");
                    return new ErrorList();
                }
                PollReply instance = pollReplyList.ToList<PollReply>().Where<PollReply>((Func<PollReply, bool>)(pr => pr.PollReplyType == PollReplyType.StoreInfoChangeSet)).FirstOrDefault<PollReply>();
                if (instance != null)
                {
                    if (string.IsNullOrEmpty(instance.Data))
                    {
                        errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0092", "Error, A poll reply contained a type of store info changeset without the changeset.", instance.ToJson()));
                    }
                    else
                    {
                        ErrorList collection = this.ProcessChangeSet(instance.Data.ToObject<StoreInfoChangeSetData>());
                        errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)collection);
                    }
                    this._syncID = instance.SyncId;
                }
            }
            catch (Exception ex)
            {
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0091", "Unhandled exception occurred in StoreInfoService.ProcessPollReply.", ex));
            }
            errors.ToLogHelper();
            return errors;
        }

        private ErrorList ProcessChangeSet(StoreInfoChangeSetData changeSet)
        {
            ErrorList errorList = new ErrorList();
            LogHelper.Instance.Log("Processing StoreInfo ChangeSet: " + changeSet.ToJson(), LogEntryType.Debug);
            try
            {
                this._storeInfoChangeSet = changeSet;
                if (this._storeInfoChangeSet.CorrectTimeZone && !TimeZoneFunctions.SetTimeZone(TimeZoneInfo.FromSerializedString(this._storeInfoChangeSet.TimeZoneInfoString)))
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SIS0100", "Setting timezone failed", "Check the log for further details."));
                if (this._storeInfoChangeSet.CorrectDateTime)
                {
                    if (!TimeZoneFunctions.SetTime(this._storeInfoChangeSet.CurrentUTCDateTime))
                        errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SIS0101", "Setting datetime failed", "Check the log for further details."));
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SIS0089", string.Format("Unhandled exception occurred in StoreInfoService.ProcessChangeSet for store info"), ex));
            }
            return errorList;
        }

        private void SetHash(string hash) => this._clientStoreInfoHash = hash;

        private ErrorList GetClientStoreInfo(out ClientStoreInfo clientStoreInfo)
        {
            ErrorList errors = new ErrorList();
            clientStoreInfo = new ClientStoreInfo()
            {
                Info = new Dictionary<string, string>()
            };
            try
            {
                TimeZoneInfo.ClearCachedData();
                clientStoreInfo.TimeZoneInfoString = TimeZoneInfo.Local.ToSerializedString();
                clientStoreInfo.UTCDateTime = DateTime.UtcNow;
                clientStoreInfo.Version = Assembly.GetEntryAssembly().GetName().Version.ToString();
                clientStoreInfo.Info.Add("OS Version", System.Environment.OSVersion.Version.ToString());
                clientStoreInfo.Info.Add("OS VersionString", System.Environment.OSVersion.VersionString);
                clientStoreInfo.Info.Add("OS Platform", System.Environment.OSVersion.Platform.ToString());
                clientStoreInfo.Info.Add("OS ServicePack", System.Environment.OSVersion.ServicePack);
                string str = ((IEnumerable<IPAddress>)Dns.GetHostEntry(Dns.GetHostName()).AddressList).FirstOrDefault<IPAddress>((Func<IPAddress, bool>)(a => a.AddressFamily == AddressFamily.InterNetwork)).ToString();
                clientStoreInfo.Info.Add("IP Address", str);
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                if (((IEnumerable<NetworkInterface>)networkInterfaces).Any<NetworkInterface>())
                {
                    string dnsSuffix = networkInterfaces[0].GetIPProperties().DnsSuffix;
                    clientStoreInfo.Info.Add("DNS Suffix", dnsSuffix);
                }
            }
            catch (Exception ex)
            {
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SIS0092", "Unhandled exception occurred in StoreInfoServieGetClientStoreInfo.", ex));
            }
            errors.ToLogHelper();
            return errors;
        }

        private string GetHash(ClientStoreInfo clientStoreInfo)
        {
            var obj = new
            {
                TimeZoneInfoString = clientStoreInfo.TimeZoneInfoString,
                Version = clientStoreInfo.Version,
                Info = new Dictionary<string, string>()
            };
            clientStoreInfo.Info.ForEach<KeyValuePair<string, string>>((Action<KeyValuePair<string, string>>)(info => obj.Info.Add(info.Key, info.Value)));
            return Encoding.ASCII.GetBytes(obj.ToJson()).ToASCIISHA1Hash();
        }

        private StoreInfoService()
        {
            if (ServiceLocator.Instance.GetService<IStoreInfoService>() != null)
                return;
            ServiceLocator.Instance.AddService(typeof(IStoreInfoService), (object)this);
        }
    }
}
