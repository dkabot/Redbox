using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework.Natives
{
    [NativeJob(ProgramName = "random-sync", Operand = "RANDOMSYNC", HideFromList = true)]
    internal sealed class RandomSync : NativeJobAdapter
    {
        internal RandomSync(ExecutionResult result, ExecutionContext context)
            : base(result, context)
        {
        }

        protected override void ExecuteInner()
        {
            var applicationLog = ApplicationLog.ConfigureLog(Context, false, nameof(RandomSync), true, null);
            if (!ControllerConfiguration.Instance.IsVMZMachine)
            {
                applicationLog.Write("The random sync job is only supported on VMZ kiosks. Nice try.");
                AddError("Unsupported configuration.");
            }
            else
            {
                var service = ServiceLocator.Instance.GetService<IInventoryService>();
                var num1 = Context.PopTop<int>();
                var vendtime = Context.PopTop<int>();
                Context.AppLog.WriteFormatted("Vend time is set to {0}s, vend frequency = {1}", vendtime, num1);
                Context.AppLog.Write("Random sync test start.");
                var locationSelector = new RandomLocationSelector(DecksService, service);
                var num2 = 0;
                var num3 = 0;
                var flag = true;
                while (flag)
                {
                    var loc = locationSelector.SelectTargetLocation();
                    if (loc == null)
                        break;
                    Context.AppLog.WriteFormatted("Sync {0}", loc.ToString());
                    ++num3;
                    var vend = false;
                    if (num1 != 0 && num3 % num1 == 0)
                        vend = true;
                    var decorator = new RandomSyncDecorator(Context, Result, loc, vendtime, vend);
                    var syncResult = SyncHelper.SyncSlot(Result, Context, loc, decorator);
                    switch (syncResult)
                    {
                        case SyncResult.HardwareError:
                        case SyncResult.GetError:
                            Context.AppLog.WriteFormatted("Sync returned error {0}", syncResult.ToString());
                            flag = false;
                            AddError("Sync encountered an error.");
                            break;
                    }

                    if (decorator.SecureReads > 0)
                        ++num2;
                    if (ContextSwitchRequested())
                        flag = false;
                }

                Context.AppLog.WriteFormatted("Disk count with secure code finds: {0}", num2);
            }
        }
    }
}