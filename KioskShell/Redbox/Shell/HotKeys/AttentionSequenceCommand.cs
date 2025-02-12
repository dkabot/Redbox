using Outerwall.Shell.HotKeys;
using Redbox.Core;
using Redbox.Shell.ComponentModel;

namespace Redbox.Shell.HotKeys
{
    public class AttentionSequenceCommand : IHotKeyCommand
    {
        public void Execute()
        {
            LogHelper.Instance.Log("AttentionSequenceCommand.Execute() - start");
            var service = ServiceLocator.Instance.GetService<IKioskEngineService>();
            if (service == null)
                return;
            bool isRunning;
            var errorList1 = service.IsEngineRunning(out isRunning);
            if (errorList1.ContainsError())
            {
                errorList1.ForEach(e => LogHelper.Instance.Log("{0} Details: {1}", e, e.Details));
            }
            else
            {
                LogHelper.Instance.Log("AttentionSequenceCommand.Execute() - running: " + isRunning);
                var errorList2 = isRunning ? service.ActivateControlPanel() : service.StartEngineWithControlPanel();
                if (errorList2.ContainsError())
                    errorList2.ForEach(e => LogHelper.Instance.Log("{0} Details: {1}", e, e.Details));
                else
                    LogHelper.Instance.Log("Launch control panel successful.");
            }
        }
    }
}