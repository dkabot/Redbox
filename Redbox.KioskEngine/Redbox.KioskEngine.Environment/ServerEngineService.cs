using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment
{
  public class ServerEngineService : IServerEngineService
  {
    internal const int DefaultServerEngineServiceTimeout = 30000;
    internal const string DefaultServerEngineServiceUrl = "rcp://localhost:7008";

    public static ServerEngineService Instance => Singleton<ServerEngineService>.Instance;

    public ErrorList Initialize()
    {
      ErrorList errorList = new ErrorList();
      ServiceLocator.Instance.AddService(typeof (IServerEngineService), (object) ServerEngineService.Instance);
      return errorList;
    }

    public void Reset()
    {
    }

    public string Url
    {
      get
      {
        return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue<string>("Local Services\\Server Engine", "ServerEngineServiceURL", "rcp://localhost:7008");
      }
      set
      {
        ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue<string>("Local Services\\Server Engine", "ServerEngineServiceURL", value);
        this.Reset();
      }
    }

    public int Timeout
    {
      get
      {
        return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue<int>("Local Services\\Server Engine", "ServerEngineServiceTimeout", 30000);
      }
      set
      {
        ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue<int>("Local Services\\Server Engine", "ServerEngineServiceTimeout", value);
        this.Reset();
      }
    }

    private ServerEngineService()
    {
    }
  }
}
