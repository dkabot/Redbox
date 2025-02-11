using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives
{
    [NativeJob(ProgramName = "tester-move-to-slot")]
    internal sealed class MSMoveToSlot : NativeJobAdapter
    {
        internal MSMoveToSlot(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var deck = Context.PopTop<int>();
            var slot = Context.PopTop<int>();
            var failures = 0;
            var service1 = ServiceLocator.Instance.GetService<IControlSystem>();
            if (!service1.GetBoardVersion(ControlBoards.Aux).ReadSuccess)
            {
                Context.CreateInfoResult("BoardVersionTimeout", "Move: AUX board not responsive.");
                ++failures;
            }

            if (!service1.GetBoardVersion(ControlBoards.Picker).ReadSuccess)
            {
                Context.CreateInfoResult("BoardVersionTimeout", "Move: PCB not responsive.");
                ++failures;
            }

            var service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
            var controlLimitResponse = service2.ReadLimits();
            if (!controlLimitResponse.ReadOk)
            {
                Context.CreateInfoResult(ErrorCodes.ArcusNotResponsive.ToString().ToUpper(), "Unable to read limits.");
                ++failures;
            }
            else
            {
                Array.ForEach(controlLimitResponse.Limits, limit =>
                {
                    if (!limit.Blocked)
                        return;
                    Context.CreateInfoResult("LimitBlocked",
                        string.Format("MOVE: {0} LIMIT BLOCKED", limit.Limit.ToString().ToUpper()));
                    ++failures;
                });
            }

            if (failures > 0)
            {
                AddError("Job failure");
            }
            else
            {
                var service3 = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
                var errorCodes = service2.MoveTo(deck, slot, MoveMode.None, service3.NilLog);
                Context.CreateInfoResult("MoveResult",
                    string.Format("MOVE DECK={0} SLOT={1} {2}", deck, slot, errorCodes.ToString().ToUpper()));
            }
        }
    }
}