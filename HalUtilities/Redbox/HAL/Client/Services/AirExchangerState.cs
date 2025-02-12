using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Services
{
    public sealed class AirExchangerState
    {
        private const string ExchangerMnemonic = "AIRXCHGR";
        private readonly HardwareService Service;

        public AirExchangerState(HardwareService s)
        {
            Service = s;
        }

        public ExchangerFanStatus FanStatus { get; private set; }

        public bool Configured { get; private set; }

        public ExchangerFanStatus Configure(
            AirExchangerStatus exchgrStatus,
            ExchangerFanStatus fanStatus)
        {
            Configured = exchgrStatus != 0;
            FanStatus = fanStatus;
            return FanStatus;
        }

        public ExchangerFanStatus ToggleFan()
        {
            if (Service.ExecuteImmediate(
                    string.Format("{0} {1}", "AIRXCHGR",
                        ExchangerFanStatus.On == FanStatus ? "FANOFF" : (object)"FANON"), out var _).Success)
            {
                if (ExchangerFanStatus.On == FanStatus)
                    FanStatus = ExchangerFanStatus.Off;
                else if (ExchangerFanStatus.Off == FanStatus)
                    FanStatus = ExchangerFanStatus.On;
            }

            return FanStatus;
        }

        public bool ToggleConfiguration()
        {
            var flag = !Configured;
            if (!Service.ExecuteImmediate(
                    string.Format("SETCFG \"EnableIceQubePolling\" \"{0}\" TYPE=CONTROLLER", flag.ToString()),
                    out var _).Success)
                return false;
            Configured = flag;
            return true;
        }
    }
}