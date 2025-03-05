using System.Net;

namespace Redbox.KioskEngine.ComponentModel.ApiService.EngineCore
{
  public class GetStoreInfoResponse : ApiServiceBaseResponse<StoreInfo>
  {
    public GetStoreInfoResponse()
    {
      this.StatusCode = HttpStatusCode.OK;
      this.Success = true;
      this.Content = new StoreInfo();
    }
  }
}
