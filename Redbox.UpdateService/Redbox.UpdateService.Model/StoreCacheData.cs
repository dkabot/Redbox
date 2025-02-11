using System;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.UpdateService.Model
{
  public class StoreCacheData
  {
    private readonly List<long> _downloadFileList = new List<long>();
    private readonly object _lock = new object();

    public StoreCacheData() => this.ClientConfigFiles = new List<ClientConfigFile>();

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

    public List<Redbox.UpdateService.Model.ClientRepositoryData> ClientRepositoryData { get; set; }

    public int ClientRepositoryDataSyncID { get; set; }

    public List<ClientConfigFile> ClientConfigFiles { get; set; }

    public int ClientConfigFileSyncID { get; set; }

    public ClientStoreInfo ClientStoreInfo { get; set; }

    public int ClientStoreInfoSyncID { get; set; }

    public List<long> DownloadFileList
    {
      get
      {
        lock (this._lock)
        {
          List<long> downloadFileList = new List<long>();
          this._downloadFileList.ForEach(new Action<long>(downloadFileList.Add));
          return downloadFileList;
        }
      }
    }

    public void UpdateClientConfigFile(List<ClientConfigFile> clientConfigFile)
    {
      lock (this._lock)
      {
        this.ClientConfigFiles.Clear();
        clientConfigFile.ForEach((Action<ClientConfigFile>) (data => this.ClientConfigFiles.Add(data)));
        ++this.ClientConfigFileSyncID;
      }
    }

    public void UpdateClientConfigFile(ConfigFileArgs args)
    {
      lock (this._lock)
      {
        this.ClientConfigFiles.Clear();
        args.ClientConfigFilesData.ForEach((Action<ClientConfigFile>) (data => this.ClientConfigFiles.Add(data)));
        this.ClientConfigFileSyncID = args.ClientConfigFileSyncID;
      }
    }

    public void UpdateClientStoreInfo(ClientStoreInfo clientStoreInfo)
    {
      lock (this._lock)
      {
        this.ClientStoreInfo = clientStoreInfo;
        ++this.ClientStoreInfoSyncID;
      }
    }

    public void UpdateClientStoreInfo(StoreInfoArgs args)
    {
      lock (this._lock)
      {
        this.ClientStoreInfo = args.ClientStoreInfo;
        this.ClientStoreInfoSyncID = args.ClientStoreInfoSyncID;
      }
    }

    public void UpdateDownloadFileList(List<long> downloadFileList)
    {
      lock (this._lock)
      {
        this._downloadFileList.Clear();
        downloadFileList.ForEach((Action<long>) (data => this._downloadFileList.Add(data)));
      }
    }

    public void UpdateDownloadFileList(DownloadFileArgs args)
    {
      lock (this._lock)
      {
        this._downloadFileList.Clear();
        args.DownloadFileList.ForEach((Action<long>) (data => this._downloadFileList.Add(data)));
      }
    }

    public void UpdateStoreData(StoreData sd)
    {
      lock (this._lock)
      {
        if (this.Store == null)
        {
          this.Store = new StoreData()
          {
            Number = sd.Number,
            ID = sd.ID,
            LastCheckIn = sd.LastCheckIn
          };
        }
        else
        {
          this.Store.Number = sd.Number;
          this.Store.ID = sd.ID;
          this.Store.LastCheckIn = sd.LastCheckIn;
        }
      }
    }

    public void UpdateKioskClientData(KioskClientData kc)
    {
      lock (this._lock)
      {
        this.KioskClient_ID = kc.KioskClientID;
        this.City = kc.City;
        this.County = kc.County;
        this.State = kc.State;
        this.Zip = kc.Zip;
        int? kioskStatusId = kc.KioskStatusId;
        Identifier identifier1;
        if (!kioskStatusId.HasValue)
        {
          identifier1 = (Identifier) null;
        }
        else
        {
          identifier1 = new Identifier();
          kioskStatusId = kc.KioskStatusId;
          identifier1.ID = (long) kioskStatusId.Value;
          identifier1.Reference = kc.KioskStatusName;
        }
        this.KioskStatus = identifier1;
        Identifier identifier2;
        if (!kc.BannerId.HasValue)
        {
          identifier2 = (Identifier) null;
        }
        else
        {
          identifier2 = new Identifier();
          identifier2.ID = kc.BannerId.Value;
          identifier2.Reference = kc.BannerName;
        }
        this.Banner = identifier2;
        Identifier identifier3;
        if (!kc.MarketId.HasValue)
        {
          identifier3 = (Identifier) null;
        }
        else
        {
          identifier3 = new Identifier();
          identifier3.ID = kc.MarketId.Value;
          identifier3.Reference = kc.MarketName;
        }
        this.Market = identifier3;
        Identifier identifier4;
        if (!kc.VendorId.HasValue)
        {
          identifier4 = (Identifier) null;
        }
        else
        {
          identifier4 = new Identifier();
          identifier4.ID = kc.VendorId.Value;
          identifier4.Reference = kc.VendorName;
        }
        this.Vendor = identifier4;
        if (!string.Equals(this.WindowsTimeZoneID, kc.WindowsTimeZoneID, StringComparison.CurrentCultureIgnoreCase))
          this.ClientStoreInfoSyncID = 0;
        this.WindowsTimeZoneID = kc.WindowsTimeZoneID;
      }
    }

    public void UpdateClientRepositoryData(
      List<Redbox.UpdateService.Model.ClientRepositoryData> clientRepositoryData,
      int syncId)
    {
      lock (this._lock)
      {
        if (this.ClientRepositoryData == null)
          this.ClientRepositoryData = new List<Redbox.UpdateService.Model.ClientRepositoryData>();
        foreach (Redbox.UpdateService.Model.ClientRepositoryData clientRepositoryData1 in clientRepositoryData)
        {
          Redbox.UpdateService.Model.ClientRepositoryData data = clientRepositoryData1;
          Redbox.UpdateService.Model.ClientRepositoryData clientRepositoryData2 = this.ClientRepositoryData.FirstOrDefault<Redbox.UpdateService.Model.ClientRepositoryData>((Func<Redbox.UpdateService.Model.ClientRepositoryData, bool>) (c => string.Equals(data.RepositoryName, c.RepositoryName, StringComparison.CurrentCultureIgnoreCase)));
          if (clientRepositoryData2 == null)
          {
            this.ClientRepositoryData.Add(clientRepositoryData1);
          }
          else
          {
            clientRepositoryData2.InTransit = clientRepositoryData1.InTransit;
            clientRepositoryData2.Staged = clientRepositoryData1.Staged;
            clientRepositoryData2.Active = clientRepositoryData1.Active;
            clientRepositoryData2.Head = clientRepositoryData1.Head;
          }
        }
        foreach (Redbox.UpdateService.Model.ClientRepositoryData clientRepositoryData3 in this.ClientRepositoryData.Where<Redbox.UpdateService.Model.ClientRepositoryData>((Func<Redbox.UpdateService.Model.ClientRepositoryData, bool>) (crd1 => !clientRepositoryData.Any<Redbox.UpdateService.Model.ClientRepositoryData>((Func<Redbox.UpdateService.Model.ClientRepositoryData, bool>) (crd2 => string.Equals(crd1.RepositoryName, crd2.RepositoryName, StringComparison.CurrentCultureIgnoreCase))))))
        {
          clientRepositoryData3.InTransit = new Redbox.UpdateService.Model.ClientRepositoryData.Revlog();
          clientRepositoryData3.Staged = new Redbox.UpdateService.Model.ClientRepositoryData.Revlog();
          clientRepositoryData3.Active = new Redbox.UpdateService.Model.ClientRepositoryData.Revlog();
          clientRepositoryData3.Head = new Redbox.UpdateService.Model.ClientRepositoryData.Revlog();
        }
        this.ClientRepositoryDataSyncID = syncId;
      }
    }

    public void UpdateClientRepositoryData(ClientRepositoryArgs args)
    {
      this.UpdateClientRepositoryData(args.ClientRepositoryData, args.ClientRepositoryDataSyncID);
    }

    public void UpdateClientRepositoryData(
      Dictionary<Redbox.UpdateService.Model.ClientRepositoryData.Repository, Redbox.UpdateService.Model.ClientRepositoryData.Revlog> assigned)
    {
      lock (this._lock)
      {
        if (this.ClientRepositoryData == null)
          this.ClientRepositoryData = new List<Redbox.UpdateService.Model.ClientRepositoryData>();
        foreach (KeyValuePair<Redbox.UpdateService.Model.ClientRepositoryData.Repository, Redbox.UpdateService.Model.ClientRepositoryData.Revlog> keyValuePair in assigned)
        {
          KeyValuePair<Redbox.UpdateService.Model.ClientRepositoryData.Repository, Redbox.UpdateService.Model.ClientRepositoryData.Revlog> repository1 = keyValuePair;
          Redbox.UpdateService.Model.ClientRepositoryData clientRepositoryData = this.ClientRepositoryData.FirstOrDefault<Redbox.UpdateService.Model.ClientRepositoryData>((Func<Redbox.UpdateService.Model.ClientRepositoryData, bool>) (c => string.Equals(repository1.Key.Name, c.RepositoryName, StringComparison.CurrentCultureIgnoreCase)));
          if (clientRepositoryData != null)
            clientRepositoryData.Assigned = keyValuePair.Value;
          else
            this.ClientRepositoryData.Add(new Redbox.UpdateService.Model.ClientRepositoryData()
            {
              RepositoryName = keyValuePair.Key.Name,
              RepositoryOID = keyValuePair.Key.OID,
              Assigned = keyValuePair.Value,
              InTransit = new Redbox.UpdateService.Model.ClientRepositoryData.Revlog(),
              Staged = new Redbox.UpdateService.Model.ClientRepositoryData.Revlog(),
              Active = new Redbox.UpdateService.Model.ClientRepositoryData.Revlog(),
              Head = new Redbox.UpdateService.Model.ClientRepositoryData.Revlog()
            });
        }
        this.ClientRepositoryData.RemoveAll((Predicate<Redbox.UpdateService.Model.ClientRepositoryData>) (crd => !assigned.Any<KeyValuePair<Redbox.UpdateService.Model.ClientRepositoryData.Repository, Redbox.UpdateService.Model.ClientRepositoryData.Revlog>>((Func<KeyValuePair<Redbox.UpdateService.Model.ClientRepositoryData.Repository, Redbox.UpdateService.Model.ClientRepositoryData.Revlog>, bool>) (a => string.Equals(crd.RepositoryName, a.Key.Name, StringComparison.CurrentCultureIgnoreCase)))));
      }
    }

    public void ClearClientConfigFileSyncId()
    {
      lock (this._lock)
        this.ClientConfigFileSyncID = 0;
    }

    public void ClearClientStoreInfoSyncId()
    {
      lock (this._lock)
        this.ClientStoreInfoSyncID = 0;
    }

    public void ClearClearClientRepositoryDataSyncId()
    {
      lock (this._lock)
        this.ClientRepositoryDataSyncID = 0;
    }
  }
}
