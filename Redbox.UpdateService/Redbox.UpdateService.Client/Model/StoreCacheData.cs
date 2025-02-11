using System;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.UpdateService.Model
{
    public class StoreCacheData
    {
        private readonly List<long> _downloadFileList = new List<long>();
        private readonly object _lock = new object();

        public StoreCacheData()
        {
            ClientConfigFiles = new List<ClientConfigFile>();
        }

        public StoreData Store { get; set; }

        public long KioskClient_ID { get; set; }

        public string City { get; set; }

        public string County { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        public Identifier KioskStatus { get; set; }

        public Identifier Vendor { get; set; }

        public Identifier Market { get; set; }

        public Identifier Banner { get; set; }

        public string WindowsTimeZoneID { get; set; }

        public List<ClientRepositoryData> ClientRepositoryData { get; set; }

        public int ClientRepositoryDataSyncID { get; set; }

        public List<ClientConfigFile> ClientConfigFiles { get; set; }

        public int ClientConfigFileSyncID { get; set; }

        public ClientStoreInfo ClientStoreInfo { get; set; }

        public int ClientStoreInfoSyncID { get; set; }

        public List<long> DownloadFileList
        {
            get
            {
                lock (_lock)
                {
                    var downloadFileList = new List<long>();
                    _downloadFileList.ForEach(downloadFileList.Add);
                    return downloadFileList;
                }
            }
        }

        public void UpdateClientConfigFile(List<ClientConfigFile> clientConfigFile)
        {
            lock (_lock)
            {
                ClientConfigFiles.Clear();
                clientConfigFile.ForEach(data => ClientConfigFiles.Add(data));
                ++ClientConfigFileSyncID;
            }
        }

        public void UpdateClientConfigFile(ConfigFileArgs args)
        {
            lock (_lock)
            {
                ClientConfigFiles.Clear();
                args.ClientConfigFilesData.ForEach(data => ClientConfigFiles.Add(data));
                ClientConfigFileSyncID = args.ClientConfigFileSyncID;
            }
        }

        public void UpdateClientStoreInfo(ClientStoreInfo clientStoreInfo)
        {
            lock (_lock)
            {
                ClientStoreInfo = clientStoreInfo;
                ++ClientStoreInfoSyncID;
            }
        }

        public void UpdateClientStoreInfo(StoreInfoArgs args)
        {
            lock (_lock)
            {
                ClientStoreInfo = args.ClientStoreInfo;
                ClientStoreInfoSyncID = args.ClientStoreInfoSyncID;
            }
        }

        public void UpdateDownloadFileList(List<long> downloadFileList)
        {
            lock (_lock)
            {
                _downloadFileList.Clear();
                downloadFileList.ForEach(data => _downloadFileList.Add(data));
            }
        }

        public void UpdateDownloadFileList(DownloadFileArgs args)
        {
            lock (_lock)
            {
                _downloadFileList.Clear();
                args.DownloadFileList.ForEach(data => _downloadFileList.Add(data));
            }
        }

        public void UpdateStoreData(StoreData sd)
        {
            lock (_lock)
            {
                if (Store == null)
                {
                    Store = new StoreData
                    {
                        Number = sd.Number,
                        ID = sd.ID,
                        LastCheckIn = sd.LastCheckIn
                    };
                }
                else
                {
                    Store.Number = sd.Number;
                    Store.ID = sd.ID;
                    Store.LastCheckIn = sd.LastCheckIn;
                }
            }
        }

        public void UpdateKioskClientData(KioskClientData kc)
        {
            lock (_lock)
            {
                KioskClient_ID = kc.KioskClientID;
                City = kc.City;
                County = kc.County;
                State = kc.State;
                Zip = kc.Zip;
                var kioskStatusId = kc.KioskStatusId;
                Identifier identifier1;
                if (!kioskStatusId.HasValue)
                {
                    identifier1 = null;
                }
                else
                {
                    identifier1 = new Identifier();
                    kioskStatusId = kc.KioskStatusId;
                    identifier1.ID = kioskStatusId.Value;
                    identifier1.Reference = kc.KioskStatusName;
                }

                KioskStatus = identifier1;
                Identifier identifier2;
                if (!kc.BannerId.HasValue)
                {
                    identifier2 = null;
                }
                else
                {
                    identifier2 = new Identifier();
                    identifier2.ID = kc.BannerId.Value;
                    identifier2.Reference = kc.BannerName;
                }

                Banner = identifier2;
                Identifier identifier3;
                if (!kc.MarketId.HasValue)
                {
                    identifier3 = null;
                }
                else
                {
                    identifier3 = new Identifier();
                    identifier3.ID = kc.MarketId.Value;
                    identifier3.Reference = kc.MarketName;
                }

                Market = identifier3;
                Identifier identifier4;
                if (!kc.VendorId.HasValue)
                {
                    identifier4 = null;
                }
                else
                {
                    identifier4 = new Identifier();
                    identifier4.ID = kc.VendorId.Value;
                    identifier4.Reference = kc.VendorName;
                }

                Vendor = identifier4;
                if (!string.Equals(WindowsTimeZoneID, kc.WindowsTimeZoneID, StringComparison.CurrentCultureIgnoreCase))
                    ClientStoreInfoSyncID = 0;
                WindowsTimeZoneID = kc.WindowsTimeZoneID;
            }
        }

        public void UpdateClientRepositoryData(
            List<ClientRepositoryData> clientRepositoryData,
            int syncId)
        {
            lock (_lock)
            {
                if (ClientRepositoryData == null)
                    ClientRepositoryData = new List<ClientRepositoryData>();
                foreach (var clientRepositoryData1 in clientRepositoryData)
                {
                    var data = clientRepositoryData1;
                    var clientRepositoryData2 = ClientRepositoryData.FirstOrDefault(c =>
                        string.Equals(data.RepositoryName, c.RepositoryName,
                            StringComparison.CurrentCultureIgnoreCase));
                    if (clientRepositoryData2 == null)
                    {
                        ClientRepositoryData.Add(clientRepositoryData1);
                    }
                    else
                    {
                        clientRepositoryData2.InTransit = clientRepositoryData1.InTransit;
                        clientRepositoryData2.Staged = clientRepositoryData1.Staged;
                        clientRepositoryData2.Active = clientRepositoryData1.Active;
                        clientRepositoryData2.Head = clientRepositoryData1.Head;
                    }
                }

                foreach (var clientRepositoryData3 in ClientRepositoryData.Where(crd1 =>
                             !clientRepositoryData.Any(crd2 => string.Equals(crd1.RepositoryName, crd2.RepositoryName,
                                 StringComparison.CurrentCultureIgnoreCase))))
                {
                    clientRepositoryData3.InTransit = new ClientRepositoryData.Revlog();
                    clientRepositoryData3.Staged = new ClientRepositoryData.Revlog();
                    clientRepositoryData3.Active = new ClientRepositoryData.Revlog();
                    clientRepositoryData3.Head = new ClientRepositoryData.Revlog();
                }

                ClientRepositoryDataSyncID = syncId;
            }
        }

        public void UpdateClientRepositoryData(ClientRepositoryArgs args)
        {
            UpdateClientRepositoryData(args.ClientRepositoryData, args.ClientRepositoryDataSyncID);
        }

        public void UpdateClientRepositoryData(
            Dictionary<ClientRepositoryData.Repository, ClientRepositoryData.Revlog> assigned)
        {
            lock (_lock)
            {
                if (ClientRepositoryData == null)
                    ClientRepositoryData = new List<ClientRepositoryData>();
                foreach (var keyValuePair in assigned)
                {
                    var repository1 = keyValuePair;
                    var clientRepositoryData = ClientRepositoryData.FirstOrDefault(c =>
                        string.Equals(repository1.Key.Name, c.RepositoryName,
                            StringComparison.CurrentCultureIgnoreCase));
                    if (clientRepositoryData != null)
                        clientRepositoryData.Assigned = keyValuePair.Value;
                    else
                        ClientRepositoryData.Add(new ClientRepositoryData
                        {
                            RepositoryName = keyValuePair.Key.Name,
                            RepositoryOID = keyValuePair.Key.OID,
                            Assigned = keyValuePair.Value,
                            InTransit = new ClientRepositoryData.Revlog(),
                            Staged = new ClientRepositoryData.Revlog(),
                            Active = new ClientRepositoryData.Revlog(),
                            Head = new ClientRepositoryData.Revlog()
                        });
                }

                ClientRepositoryData.RemoveAll(crd => !assigned.Any(a =>
                    string.Equals(crd.RepositoryName, a.Key.Name, StringComparison.CurrentCultureIgnoreCase)));
            }
        }

        public void ClearClientConfigFileSyncId()
        {
            lock (_lock)
            {
                ClientConfigFileSyncID = 0;
            }
        }

        public void ClearClientStoreInfoSyncId()
        {
            lock (_lock)
            {
                ClientStoreInfoSyncID = 0;
            }
        }

        public void ClearClearClientRepositoryDataSyncId()
        {
            lock (_lock)
            {
                ClientRepositoryDataSyncID = 0;
            }
        }
    }
}