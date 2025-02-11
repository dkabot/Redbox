using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives
{
    [NativeJob(ProgramName = "adjacent-bin-test-kent")]
    internal sealed class AdjacentBinTestKentJob : NativeJobAdapter
    {
        private readonly int[] SourceDiskSlots = new int[2]
        {
            30,
            60
        };

        internal AdjacentBinTestKentJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var applicationLog = ApplicationLog.ConfigureLog(Context, true, "KioskTest", false, "AdjacentDumpSlotTest");
            if (ServiceLocator.Instance.GetService<IDecksService>().QlmDeck != null)
            {
                applicationLog.Write("The kiosk shows a QLM configuration - go no further.");
            }
            else
            {
                var byNumber =
                    DecksService.GetByNumber(ServiceLocator.Instance.GetService<IDumpbinService>().PutLocation.Deck);
                if (!PutToAdjacentAndTest(byNumber.Number, 2, 81))
                {
                    AddError("Test failure!");
                }
                else if (!PutToAdjacentAndTest(byNumber.Number, 3, 85))
                {
                    AddError("Test failure!");
                }
                else if (!MoveAndGet(byNumber.Number, 4))
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
                else if (!FileDiskInPickerToBin())
                {
                    AddError("Test failure!");
                }
                else if (!MoveAndGet(byNumber.Number, 81))
                {
                    AddError("Test failure!");
                }
                else if (!FileDiskInPickerToBin())
                {
                    AddError("Test failure!");
                }
                else if (!MoveAndGet(byNumber.Number, 85))
                {
                    AddError("Test failure!");
                }
                else
                {
                    if (FileDiskInPickerToBin())
                        return;
                    AddError("Test failure!");
                }
            }
        }

        private bool FileDiskInPickerToBin()
        {
            if (MotionService.MoveTo(ServiceLocator.Instance.GetService<IDumpbinService>().PutLocation, MoveMode.Put,
                    AppLog) != ErrorCodes.Success)
            {
                AddError("Test failure!");
                return false;
            }

            var num = (int)ControlSystem.TrackCycle();
            if (ServiceLocator.Instance.GetService<IControllerService>().Put("UNKNOWN").Success)
                return true;
            AddError("Test failure.");
            return false;
        }

        private bool TestAdjacentWithDisk(int deck, int testSlot)
        {
            if (ServiceLocator.Instance.GetService<IMotionControlService>()
                    .MoveTo(deck, testSlot, MoveMode.Put, AppLog) != ErrorCodes.Success)
            {
                AddError("Test failure!");
                return false;
            }

            var num = (int)ControlSystem.TrackCycle();
            return ServiceLocator.Instance.GetService<IControllerService>().Put("UNKNOWN").IsSlotInUse;
        }

        private bool PutToAdjacentAndTest(int deck, int slot, int targetSlot)
        {
            if (!MoveAndGet(deck, slot))
            {
                AddError("Unable to retrieve source disk.");
                return false;
            }

            if (MoveAndPut(deck, targetSlot, "UNKNOWN"))
                return true;
            AddError("Unable to put disk into adjacent slot.");
            return false;
        }
    }
}