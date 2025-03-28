using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment
{
  public static class DeviceServiceClientHelperFactory
  {
    public static IDeviceServiceClientHelper Create()
    {
      return DeviceServiceClientHelperSimulator.Instance.UseSimulatedCardReader ? (IDeviceServiceClientHelper) DeviceServiceClientHelperSimulator.Instance : (IDeviceServiceClientHelper) DeviceServiceClientHelper.Instance;
    }
  }
}
