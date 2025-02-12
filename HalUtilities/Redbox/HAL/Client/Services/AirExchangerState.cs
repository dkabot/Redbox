using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Services
{
  public sealed class AirExchangerState
  {
    private const string ExchangerMnemonic = "AIRXCHGR";
    private readonly HardwareService Service;

    public ExchangerFanStatus FanStatus { get; private set; }

    public bool Configured { get; private set; }

    public ExchangerFanStatus Configure(
      AirExchangerStatus exchgrStatus,
      ExchangerFanStatus fanStatus)
    {
      this.Configured = exchgrStatus != 0;
      this.FanStatus = fanStatus;
      return this.FanStatus;
    }

    public ExchangerFanStatus ToggleFan()
    {
      if (this.Service.ExecuteImmediate(string.Format("{0} {1}", (object) "AIRXCHGR", ExchangerFanStatus.On == this.FanStatus ? (object) "FANOFF" : (object) "FANON"), out HardwareJob _).Success)
      {
        if (ExchangerFanStatus.On == this.FanStatus)
          this.FanStatus = ExchangerFanStatus.Off;
        else if (ExchangerFanStatus.Off == this.FanStatus)
          this.FanStatus = ExchangerFanStatus.On;
      }
      return this.FanStatus;
    }

    public bool ToggleConfiguration()
    {
      bool flag = !this.Configured;
      if (!this.Service.ExecuteImmediate(string.Format("SETCFG \"EnableIceQubePolling\" \"{0}\" TYPE=CONTROLLER", (object) flag.ToString()), out HardwareJob _).Success)
        return false;
      this.Configured = flag;
      return true;
    }

    public AirExchangerState(HardwareService s) => this.Service = s;
  }
}
