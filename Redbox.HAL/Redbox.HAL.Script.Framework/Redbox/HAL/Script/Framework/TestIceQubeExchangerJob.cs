using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "check-iceqube-board", Operand = "CHECK-ICEQUBE-BOARD")]
    internal sealed class TestIceQubeExchangerJob : NativeJobAdapter
    {
        internal TestIceQubeExchangerJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var service = ServiceLocator.Instance.GetService<IAirExchangerService>();
            var applicationLog = ApplicationLog.ConfigureLog(Context, true, "Service", true, "IceQubeBoard");
            if (!service.ShouldRetry())
            {
                OnPersistentError();
            }
            else
            {
                if (AirExchangerStatus.Error != service.CheckStatus())
                    return;
                LogHelper.Instance.WithContext("Air exchanger board status in error; attempt reset.");
                var airExchangerStatus = AirExchangerStatus.NotConfigured;
                for (var index = 0; index < 3; ++index)
                {
                    airExchangerStatus = service.Reset();
                    applicationLog.Write(string.Format("Reset status is {0}", airExchangerStatus.ToString().ToUpper()));
                    if (AirExchangerStatus.Ok == airExchangerStatus)
                    {
                        service.ResetFailureCount();
                        return;
                    }

                    ServiceLocator.Instance.GetService<IRuntimeService>().Wait(2000);
                }

                if (AirExchangerStatus.Error != airExchangerStatus)
                    return;
                service.IncrementResetFailureCount();
                if (service.ShouldRetry())
                    return;
                OnPersistentError();
            }
        }

        private void OnPersistentError()
        {
            var str = "The board is in persistent error; no further queries.";
            Context.AppLog.Write(str);
            LogHelper.Instance.WithContext(str);
            AddError("IceQube board in persistent error state.");
        }
    }
}