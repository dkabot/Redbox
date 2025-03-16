using System.Net;

namespace Redbox.KioskEngine.ComponentModel.ApiService.EngineCore
{
    public class EngineCoreKioskEngineVersionResponse : ApiServiceBaseResponse<KioskEngineVersions>
    {
        public EngineCoreKioskEngineVersionResponse()
        {
            StatusCode = HttpStatusCode.OK;
            Success = true;
            Content = new KioskEngineVersions();
        }
    }
}