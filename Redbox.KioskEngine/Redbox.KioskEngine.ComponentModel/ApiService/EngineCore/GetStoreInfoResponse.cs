using System.Net;

namespace Redbox.KioskEngine.ComponentModel.ApiService.EngineCore
{
    public class GetStoreInfoResponse : ApiServiceBaseResponse<StoreInfo>
    {
        public GetStoreInfoResponse()
        {
            StatusCode = HttpStatusCode.OK;
            Success = true;
            Content = new StoreInfo();
        }
    }
}