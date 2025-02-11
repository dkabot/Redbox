using System;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "test-boards-job")]
    internal class TestBoardsJob : NativeJobAdapter
    {
        internal TestBoardsJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var revision = ControlSystem.GetRevision();
            Array.ForEach(revision.Responses, each =>
            {
                var str = each.ReadSuccess ? "responsive" : "not responsive";
                Context.CreateInfoResult("VersionResult",
                    string.Format("{0} board {1}", each.BoardName.ToString().ToUpper(), str));
            });
            if (revision.Success)
                return;
            AddError("One or more comm failures.");
        }
    }
}