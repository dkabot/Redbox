using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "test-retrofit-deck")]
    internal sealed class VMZRetrofitTest : NativeJobAdapter
    {
        private readonly int[] TestSlots = new int[5]
        {
            16,
            31,
            46,
            61,
            76
        };

        internal VMZRetrofitTest(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            try
            {
                var applicationLog = ApplicationLog.ConfigureLog(Context, true, "KioskTest", false, "RetrofitDeckTest");
                if (!ControllerConfiguration.Instance.IsVMZMachine)
                {
                    applicationLog.Write("The kiosk is still configured for a QLM - bail test.");
                    AddError("Kiosk Misconfiguration.");
                }
                else
                {
                    var byNumber = DecksService.GetByNumber(DumpbinService.PutLocation.Deck);
                    if (!MoveAndGet(byNumber.Number, 1))
                    {
                        AddError("Failure to retrieve disk from start position.");
                    }
                    else
                    {
                        foreach (var testSlot in TestSlots)
                        {
                            if (!MoveAndPut(byNumber.Number, testSlot, "UNKNOWN"))
                            {
                                AddError("Unable to place the disk at a test location.");
                                return;
                            }

                            if (!MoveAndGet(byNumber.Number, testSlot))
                            {
                                AddError("Unable to get the disk from test location.");
                                return;
                            }
                        }

                        if (!MoveAndPut(byNumber.Number, 81, "UNKNOWN"))
                        {
                            AddError("Unable to put disk into adjacent slot.");
                        }
                        else
                        {
                            if (!TestSlotForDisk(byNumber.Number, 81))
                                return;
                            if (!PutToAdjacentAndTest(byNumber.Number, 2, 85))
                            {
                                AddError("Test failure!");
                            }
                            else if (!MoveAndGet(byNumber.Number, 3))
                            {
                                AddError("Test failure!");
                            }
                            else if (!TestAdjacentWithDisk(byNumber.Number, 81))
                            {
                                AddError("Test failure!");
                            }
                            else if (!TestAdjacentWithDisk(byNumber.Number, 85))
                            {
                                AddError("Test failure!");
                            }
                            else if (!FileToBin(byNumber.Number))
                            {
                                AddError("Test failure!");
                            }
                            else if (!MoveAndGet(byNumber.Number, 81))
                            {
                                AddError("Test failure!");
                            }
                            else if (!FileToBin(byNumber.Number))
                            {
                                AddError("Test failure!");
                            }
                            else if (!MoveAndGet(byNumber.Number, 85))
                            {
                                AddError("Test failure!");
                            }
                            else if (!FileToBin(byNumber.Number))
                            {
                                AddError("Test failure!");
                            }
                            else
                            {
                                var num = (int)MotionService.MoveVend(MoveMode.Get, AppLog);
                            }
                        }
                    }
                }
            }
            finally
            {
                var byNumber = DecksService.GetByNumber(DumpbinService.PutLocation.Deck);
                if (!byNumber.IsQlm)
                    InventoryService.MarkDeckInventory(byNumber, "EMPTY");
                DumpbinService.ClearItems();
            }
        }

        private bool FileToBin(int deck)
        {
            if (MotionService.MoveTo(DumpbinService.PutLocation, MoveMode.Put, AppLog) != ErrorCodes.Success)
            {
                AddError("Test failure!");
                return false;
            }

            if (!ServiceLocator.Instance.GetService<IControllerService>().Put("UNKNOWN").Success)
            {
                AddError("Test failure.");
                return false;
            }

            var num = (int)MotionService.MoveTo(deck, 1, MoveMode.None, AppLog);
            return true;
        }

        private bool TestAdjacentWithDisk(int deck, int testSlot)
        {
            if (MotionService.MoveTo(deck, testSlot, MoveMode.Put, AppLog) != ErrorCodes.Success)
            {
                AddError("Test failure!");
                return false;
            }

            var num = (int)ControlSystem.TrackCycle();
            return ServiceLocator.Instance.GetService<IControllerService>().Put("UNKNOWN").IsSlotInUse &&
                   TestSlotForDisk(deck, testSlot);
        }

        private bool PutToAdjacentAndTest(int deck, int slot, int targetSlot)
        {
            if (!MoveAndGet(deck, slot))
            {
                AddError("Unable to retrieve source disk.");
                return false;
            }

            if (MoveAndPut(deck, targetSlot, "UNKNOWN"))
                return TestSlotForDisk(deck, targetSlot);
            AddError("Unable to put disk into adjacent slot.");
            return false;
        }

        private bool TestSlotForDisk(int deck, int slot)
        {
            return true;
        }
    }
}