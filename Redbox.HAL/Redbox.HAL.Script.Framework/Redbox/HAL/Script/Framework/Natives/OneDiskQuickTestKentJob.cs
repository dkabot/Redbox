using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives
{
    [NativeJob(ProgramName = "one-disk-quick-deck-test-kent")]
    internal sealed class OneDiskQuickTestKentJob : NativeJobAdapter
    {
        private readonly int[] TestSlots = new int[2] { 30, 60 };

        internal OneDiskQuickTestKentJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            var appLog = ApplicationLog.ConfigureLog(Context, true, "KioskTest", false, "OneDiskQuickTest");
            var errors = 0;
            var decksService = ServiceLocator.Instance.GetService<IDecksService>();
            decksService.ForAllDecksDo(deck =>
            {
                if (deck.IsQlm)
                {
                    appLog.Write("The deck is a qlm deck - finish test.");
                    return true;
                }

                if (!TestDeck(deck))
                {
                    ++errors;
                    return false;
                }

                var byNumber = decksService.GetByNumber(deck.Number + 1);
                if (byNumber == null || byNumber.IsQlm || MoveAndPut(byNumber.Number, 1, "UNKNOWN"))
                    return true;
                AddError("Unable to place the into the slot.");
                ++errors;
                return false;
            });
            if (errors > 0)
                return;
            if (!MoveAndPut(1, 1, "UNKNOWN"))
                AddError("Unable to place the disk back into the source slot.");
            var num = (int)ServiceLocator.Instance.GetService<IMotionControlService>().MoveVend(MoveMode.Get, appLog);
        }

        private bool TestDeck(IDeck deck)
        {
            if (!MoveAndGet(deck.Number, 1))
            {
                AddError("Failure to retrieve disk from start position.");
                return false;
            }

            foreach (var testSlot in TestSlots)
            {
                if (!MoveAndPut(deck.Number, testSlot, "UNKNOWN"))
                {
                    AddError("Unable to place the disk at a test location.");
                    return false;
                }

                if (!MoveAndGet(deck.Number, testSlot))
                {
                    AddError("Unable to get the disk from test location.");
                    return false;
                }
            }

            return true;
        }
    }
}