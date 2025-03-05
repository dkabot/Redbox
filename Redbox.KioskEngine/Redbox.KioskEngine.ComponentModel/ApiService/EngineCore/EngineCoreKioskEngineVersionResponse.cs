using System.Net;

namespace Redbox.KioskEngine.ComponentModel.ApiService.EngineCore
{
  public class EngineCoreKioskEngineVersionResponse : ApiServiceBaseResponse<KioskEngineVersions>
  {
    public EngineCoreKioskEngineVersionResponse()
    {
      this.StatusCode = HttpStatusCode.OK;
      this.Success = true;
      this.Content = new KioskEngineVersions();
    }
  }
}
