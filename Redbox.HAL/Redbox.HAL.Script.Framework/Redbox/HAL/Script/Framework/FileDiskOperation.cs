using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    internal struct FileDiskOperation : IDisposable
    {
        private readonly ExecutionContext Context;
        private readonly IFileDiskHandler Handler;

        public void Dispose()
        {
        }

        internal ErrorCodes PutDiscAway(ExecutionResult result, string idInPicker)
        {
            LogHelper.Instance.WithContext("Find an empty slot for item {0}.", idInPicker);
            var errorCodes = OnFileDisk(result, idInPicker);
            if (errorCodes != ErrorCodes.Success)
                LogHelper.Instance.WithContext(LogEntryType.Error,
                    "PutDiskAway ( barcode = {0} ) returned error status {1}.", idInPicker,
                    errorCodes.ToString().ToUpper());
            return errorCodes;
        }

        internal FileDiskOperation(ExecutionContext context)
            : this(new DefaultHandler(), context)
        {
        }

        internal FileDiskOperation(IFileDiskHandler handler, ExecutionContext context)
        {
            Handler = handler;
            Context = context;
        }

        private ErrorCodes OnFileDisk(ExecutionResult result, string idInPicker)
        {
            using (var emptyLocations =
                   ServiceLocator.Instance.GetService<IEmptySearchPatternService>().FindEmptyLocations())
            {
                if (emptyLocations.FoundEmpty == 0)
                {
                    Handler.MachineFull(result, idInPicker);
                    return ErrorCodes.MachineFull;
                }

                var service1 = ServiceLocator.Instance.GetService<IControlSystem>();
                ServiceLocator.Instance.GetService<IInventoryService>().IsBarcodeDuplicate(idInPicker, out _);
                var num1 = emptyLocations.FoundEmpty > 1 ? emptyLocations.FoundEmpty - 1 : 0;
                var service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
                var num2 = 0;
                foreach (var emptyLocation in emptyLocations.EmptyLocations)
                {
                    LogHelper.Instance.WithContext("Moving to {0} to put item in picker away.",
                        emptyLocation.ToString());
                    var moveResult = service2.MoveTo(emptyLocation, MoveMode.Put, Context.AppLog);
                    if (moveResult != ErrorCodes.Success)
                    {
                        var discIterationResult = Handler.MoveError(result, idInPicker, moveResult, emptyLocation.Deck,
                            emptyLocation.Slot);
                        if (FileDiscIterationResult.Halt == discIterationResult)
                            return ErrorCodes.DiskNotFiled;
                        if (FileDiscIterationResult.NextLocation == discIterationResult)
                            continue;
                    }

                    LogHelper.Instance.WithContext("Putting item in picker into slot.");
                    var putResult = ServiceLocator.Instance.GetService<IControllerService>().Put(idInPicker);
                    if (putResult.Success)
                    {
                        Handler.DiskFiled(result, putResult);
                        return ErrorCodes.Success;
                    }

                    if (putResult.IsSlotInUse && ++num2 == num1)
                    {
                        var num3 = (int)service2.InitAxes();
                    }

                    if (FileDiscIterationResult.Halt == Handler.OnFailedPut(result, putResult))
                        return ErrorCodes.DiskNotFiled;
                    var num4 = (int)service1.TrackCycle();
                }

                Handler.FailedToFileDisk(result, idInPicker);
                return ErrorCodes.DiskNotFiled;
            }
        }
    }
}