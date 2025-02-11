using System.Collections.Generic;

namespace Redbox.UpdateService.Model
{
  public class ClientRepositoryArgs
  {
    public string StoreNumber { get; set; }

    public List<Redbox.UpdateService.Model.ClientRepositoryData> ClientRepositoryData { get; set; }

    public int ClientRepositoryDataSyncID { get; set; }
  }
}
