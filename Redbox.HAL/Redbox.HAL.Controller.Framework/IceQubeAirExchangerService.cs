using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal class IceQubeAirExchangerService : IAirExchangerService, IConfigurationObserver
    {
        private const string ResetFailures = "IceQubeResetFailures";

        internal IceQubeAirExchangerService()
        {
            FanStatus = ExchangerFanStatus.NotConfigured;
            ControllerConfiguration.Instance.AddObserver(this);
        }

        public AirExchangerStatus CheckStatus()
        {
            return !Configured
                ? AirExchangerStatus.NotConfigured
                : CheckStatusInner(ServiceLocator.Instance.GetService<IControlSystem>());
        }

        public void TurnOnFan()
        {
            if (!Configured)
                return;
            if (ExchangerFanStatus.On == FanStatus)
            {
                LogHelper.Instance.Log("Fan is already on - bypass.");
            }
            else
            {
                var service = ServiceLocator.Instance.GetService<IControlSystem>();
                TurnOnFanChecked(service);
                Thread.Sleep(500);
                var num = (int)ResetChecked(service);
            }
        }

        public void TurnOffFan()
        {
            if (!Configured)
                return;
            if (ExchangerFanStatus.Off == FanStatus)
            {
                LogHelper.Instance.WithContext("Air exchanger fan is already off - bypass.");
            }
            else
            {
                TurnOffFanChecked(ServiceLocator.Instance.GetService<IControlSystem>());
                Thread.Sleep(10500);
            }
        }

        public AirExchangerStatus Reset()
        {
            return !Configured
                ? AirExchangerStatus.NotConfigured
                : ResetChecked(ServiceLocator.Instance.GetService<IControlSystem>());
        }

        public int PersistentFailureCount()
        {
            var persistentCounter = ServiceLocator.Instance.GetService<IPersistentCounterService>()
                .Find("IceQubeResetFailures");
            return persistentCounter != null ? persistentCounter.Value : 0;
        }

        public bool ShouldRetry()
        {
            var persistentCounter = ServiceLocator.Instance.GetService<IPersistentCounterService>()
                .Find("IceQubeResetFailures");
            return persistentCounter != null && persistentCounter.Value < 3;
        }

        public void ResetFailureCount()
        {
            ServiceLocator.Instance.GetService<IPersistentCounterService>().Reset("IceQubeResetFailures");
        }

        public void IncrementResetFailureCount()
        {
            ServiceLocator.Instance.GetService<IPersistentCounterService>().Increment("IceQubeResetFailures");
        }

        public bool Configured { get; private set; }

        public ExchangerFanStatus FanStatus { get; private set; }

        public void NotifyConfigurationChangeStart()
        {
            LogHelper.Instance.Log("[IceQubeService] Configuration change start.");
            FanStatus = ExchangerFanStatus.Unknown;
        }

        public void NotifyConfigurationChangeEnd()
        {
            LogHelper.Instance.Log("[IceQubeService] Configuration change end.");
            Configured = ControllerConfiguration.Instance.EnableIceQubePolling;
            FanStatus = Configured ? ExchangerFanStatus.On : ExchangerFanStatus.NotConfigured;
            if (Configured)
                return;
            TurnOnFan();
        }

        public void NotifyConfigurationLoaded()
        {
            LogHelper.Instance.Log("[IceQubeService] Configuration loaded.");
            Configured = ControllerConfiguration.Instance.EnableIceQubePolling;
            FanStatus = Configured ? ExchangerFanStatus.On : ExchangerFanStatus.NotConfigured;
        }

        private AirExchangerStatus ResetChecked(IControlSystem cs)
        {
            var millisecondsTimeout = 1500;
            TurnOffFanChecked(cs);
            Thread.Sleep(millisecondsTimeout);
            TurnOnFanChecked(cs);
            Thread.Sleep(millisecondsTimeout);
            return CheckStatusInner(cs);
        }

        private bool TurnOnFanChecked(IControlSystem cs)
        {
            var controlResponse = cs.LockQlmDoor();
            if (controlResponse.Success)
                FanStatus = ExchangerFanStatus.On;
            return controlResponse.Success;
        }

        private bool TurnOffFanChecked(IControlSystem cs)
        {
            var controlResponse = cs.UnlockQlmDoor();
            if (controlResponse.Success)
                FanStatus = ExchangerFanStatus.Off;
            return controlResponse.Success;
        }

        private AirExchangerStatus CheckStatusInner(IControlSystem cs)
        {
            var readInputsResult = cs.ReadAuxInputs();
            return !readInputsResult.Success || !readInputsResult.IsInputActive(AuxInputs.QlmBayDoor)
                ? AirExchangerStatus.Error
                : AirExchangerStatus.Ok;
        }
    }
}