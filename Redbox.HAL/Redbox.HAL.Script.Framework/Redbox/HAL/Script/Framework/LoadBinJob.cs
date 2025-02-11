using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "load-bin", Operand = "LOAD-BIN")]
    internal sealed class LoadBinJob : NativeJobAdapter
    {
        internal LoadBinJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            var applicationLog = ApplicationLog.ConfigureLog(Context, false, "KioskTest", false, "load-bin-fast");
            if (!ControllerConfiguration.Instance.IsVMZMachine)
            {
                applicationLog.Write("The dump bin is not configured.");
                AddError("The dump bin is not configured.");
            }
            else
            {
                var str1 = Context.PopTop<string>();
                var ignoringCase = Enum<LoadBy>.ParseIgnoringCase(str1, LoadBy.None);
                if (ignoringCase == LoadBy.None)
                {
                    var str2 = string.Format("Unrecognized argument {0}", str1);
                    applicationLog.Write(str2);
                    AddError(str2);
                }
                else
                {
                    var num = Context.PopTop<int>();
                    for (var index = 0; index < num; ++index)
                    {
                        if (DumpbinService.IsFull())
                            return;
                        ILocation location;
                        if (LoadBy.Barcode == ignoringCase)
                        {
                            location = InventoryService.Lookup(Context.PopTop<string>());
                            if (location == null)
                                continue;
                        }
                        else
                        {
                            location = InventoryService.Get(Context.PopTop<int>(), Context.PopTop<int>());
                        }

                        if (MotionService.MoveTo(location, MoveMode.Get, AppLog) != ErrorCodes.Success)
                        {
                            AddError("There was an error during the move.");
                            return;
                        }

                        var service = ServiceLocator.Instance.GetService<IControllerService>();
                        var getResult = service.Get();
                        if (!getResult.Success)
                        {
                            AddError("Unable to fetch disk from drum.");
                            return;
                        }

                        if (MotionService.MoveTo(DumpbinService.PutLocation, MoveMode.Put, AppLog) !=
                            ErrorCodes.Success)
                        {
                            AddError("There was an error during the move.");
                            return;
                        }

                        if (!service.Put(getResult.Previous).Success)
                        {
                            AddError("Unable to place disk into drum.");
                            return;
                        }
                    }

                    if (MotionService.MoveVend(MoveMode.Get, AppLog) == ErrorCodes.Success)
                        return;
                    AddError("Unable to move to vend.");
                }
            }
        }
    }
}