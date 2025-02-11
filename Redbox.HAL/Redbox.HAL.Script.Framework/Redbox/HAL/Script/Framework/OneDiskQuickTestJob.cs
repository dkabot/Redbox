using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "one-disk-quick-deck-test")]
    internal sealed class OneDiskQuickTestJob : NativeJobAdapter
    {
        internal OneDiskQuickTestJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            var log = ApplicationLog.ConfigureLog(Context, true, "KioskTest", false, "OneDiskQuickTest");
            if (!ControllerConfiguration.Instance.IsVMZMachine)
            {
                log.WriteFormatted("** the machine is not configured for a VMZ - bailing **");
                AddError("Misconfigured test");
            }
            else
            {
                var deck1 = Context.PopTop<int>();
                var slot1 = Context.PopTop<int>();
                if (!MoveAndGet(deck1, slot1))
                {
                    AddError("Failure to retrieve disk from start position.");
                }
                else
                {
                    var num1 = Context.PopTop<int>();
                    var flag = false;
                    var numArray1 = new int[2] { 30, 60 };
                    var numArray2 = new int[3] { 1, 30, 60 };
                    for (var deck2 = 1; deck2 <= num1 && !flag; ++deck2)
                        foreach (var slot2 in deck2 == deck1 ? numArray1 : numArray2)
                        {
                            if (!MoveAndPut(deck2, slot2, "UNKNOWN"))
                            {
                                AddError("Unable to place the disk at a test location.");
                                flag = true;
                                break;
                            }

                            if (!MoveAndGet(deck2, slot2))
                            {
                                AddError("Unable to get the disk from test location.");
                                flag = true;
                                break;
                            }
                        }

                    if (flag)
                        return;
                    if (!MoveAndPut(deck1, slot1, "UNKNOWN"))
                    {
                        AddError("Unable to place the disk at a test location.");
                    }
                    else
                    {
                        var num2 = (int)MotionService.MoveVend(MoveMode.Get, log);
                    }
                }
            }
        }
    }
}