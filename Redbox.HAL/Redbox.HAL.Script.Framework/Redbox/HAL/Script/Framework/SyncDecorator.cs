using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    internal abstract class SyncDecorator
    {
        protected readonly ExecutionContext Context;
        protected readonly ExecutionResult Result;
        protected readonly ILocation SyncLocation;

        protected SyncDecorator(ExecutionContext ctx, ExecutionResult result, ILocation loc)
        {
            SyncLocation = loc;
            Result = result;
            Context = ctx;
        }

        protected void UpdateSymbol(string name, object value)
        {
            Context.SetSymbolValue(name, value);
        }

        protected virtual void OnMoveErrorInner(ErrorCodes result)
        {
        }

        protected virtual void OnReadDisk(ScanResult result)
        {
        }

        internal static SyncDecorator Get(
            SyncType type,
            ExecutionContext ctx,
            ExecutionResult r,
            ILocation syncLoc)
        {
            if (type == SyncType.None)
                throw new ArgumentException(nameof(type));
            var syncDecorator = (SyncDecorator)null;
            if (type != SyncType.Standard)
            {
                if (type == SyncType.Location)
                    syncDecorator = new SyncLocationDecorator(ctx, r, syncLoc);
            }
            else
            {
                syncDecorator = new StandardSyncDecorator(ctx, r, syncLoc);
            }

            return syncDecorator;
        }

        internal virtual SyncResult AfterDiskRead(ScanResult sr)
        {
            return SyncResult.Success;
        }

        internal virtual IGetResult GetDisk(IFormattedLog applog)
        {
            return ServiceLocator.Instance.GetService<IControllerService>().Get();
        }

        internal virtual void OnGetFailure(IGetResult gr)
        {
        }

        internal virtual void OnPutError(string id, IPutResult putResult)
        {
        }

        internal virtual void OnExcludedLocation()
        {
        }

        internal ScanResult ReadDisk()
        {
            var result = Scanner.SmartReadDisk();
            OnReadDisk(result);
            return result;
        }

        internal void AddError(string msg)
        {
            Result.Errors.Add(Error.NewError("E999", "Execution context error.", msg));
        }

        internal void OnMoveError(ErrorCodes err)
        {
            Context.CreateMoveErrorResult(SyncLocation.Deck, SyncLocation.Slot);
            AddError("There was a MOVE error.");
            OnMoveErrorInner(err);
        }
    }
}