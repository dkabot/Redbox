namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "rebuild-inventory-database")]
    internal sealed class RebuildInventoryDatabaseJob : NativeJobAdapter
    {
        internal RebuildInventoryDatabaseJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            InventoryService.Rebuild(Result.Errors);
            if (!Result.Errors.ContainsError())
                return;
            AddError("Failed to rebuild database.");
        }
    }
}