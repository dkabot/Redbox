using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class TransferOperation : IDisposable, IPutObserver
    {
        private readonly IControllerService ControllerService;
        private readonly IControlSystem ControlSystem;
        private readonly IInventoryService InventoryService;
        private readonly IMotionControlService MotionService;
        private readonly ILocation Source;
        private IGetResult GetResult;
        private bool m_disposed;

        internal TransferOperation(IControllerService service, ILocation source)
        {
            InventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
            MotionService = ServiceLocator.Instance.GetService<IMotionControlService>();
            ControlSystem = ServiceLocator.Instance.GetService<IControlSystem>();
            ControllerService = service;
            Source = source;
        }

        internal bool PreserveFlagsOnPut { get; set; }

        public void Dispose()
        {
            DisposeInner(true);
        }

        public void OnSuccessfulPut(IPutResult result, IFormattedLog log)
        {
            OnRestoreState(result.PutLocation, PreserveFlagsOnPut);
        }

        public void OnFailedPut(IPutResult result, IFormattedLog log)
        {
        }

        internal ITransferResult Transfer(IList<ILocation> destinations)
        {
            var result = new TransferResult();
            FetchSource(result);
            if (result.TransferError != ErrorCodes.Success)
                return result;
            foreach (var destination in destinations)
            {
                MoveAndPut(result, destination);
                if (result.TransferError == ErrorCodes.Success)
                    return result;
            }

            ReturnToSource(result);
            return result;
        }

        internal ITransferResult Transfer(IList<ILocation> destinations, IGetObserver observer)
        {
            var result = new TransferResult();
            FetchSource(result, observer);
            if (result.TransferError != ErrorCodes.Success)
                return result;
            foreach (var destination in destinations)
            {
                MoveAndPut(result, destination);
                if (result.TransferError == ErrorCodes.Success)
                    return result;
            }

            ReturnToSource(result);
            return result;
        }

        internal ITransferResult Transfer(ILocation destination)
        {
            var result = new TransferResult();
            FetchSource(result);
            if (result.TransferError != ErrorCodes.Success)
                return result;
            MoveAndPut(result, destination);
            if (!result.Transferred)
                ReturnToSource(result);
            return result;
        }

        private void MoveAndPut(TransferResult result, ILocation destination)
        {
            var errorCodes = MotionService.MoveTo(destination, MoveMode.Put);
            if (errorCodes != ErrorCodes.Success)
            {
                result.TransferError = errorCodes;
            }
            else
            {
                var num = (int)ControlSystem.TrackCycle();
                var putResult = ControllerService.Put(this, GetResult.Previous);
                result.TransferError = putResult.Code;
                if (!putResult.Success)
                    return;
                result.Destination = destination;
            }
        }

        private void ReturnToSource(TransferResult result)
        {
            var errorCodes = MotionService.MoveTo(Source, MoveMode.Put);
            if (errorCodes != ErrorCodes.Success)
            {
                result.TransferError = errorCodes;
            }
            else
            {
                var num = (int)ControlSystem.TrackCycle();
                if (!ControllerService.Put(GetResult.Previous).Success)
                    return;
                result.ReturnedToSource = true;
                OnRestoreState(Source, true);
            }
        }

        private void OnRestoreState(ILocation location, bool preserve)
        {
            if (preserve)
                location.Flags = GetResult.Flags;
            location.ReturnDate = GetResult.ReturnTime;
            InventoryService.Save(location);
        }

        private void FetchSource(TransferResult result, IGetObserver observer)
        {
            var errorCodes = MotionService.MoveTo(Source, MoveMode.Get);
            if (errorCodes != ErrorCodes.Success)
            {
                result.TransferError = errorCodes;
            }
            else
            {
                GetResult = ControllerService.Get(observer);
                result.TransferError = GetResult.Success ? ErrorCodes.Success : GetResult.HardwareError;
                if (!GetResult.Success)
                    return;
                result.Source = Source;
            }
        }

        private void FetchSource(TransferResult result)
        {
            var errorCodes = MotionService.MoveTo(Source, MoveMode.Get);
            if (errorCodes != ErrorCodes.Success)
            {
                result.TransferError = errorCodes;
            }
            else
            {
                GetResult = ControllerService.Get();
                result.TransferError = GetResult.Success ? ErrorCodes.Success : GetResult.HardwareError;
                if (!GetResult.Success)
                    return;
                result.Source = Source;
            }
        }

        private void DisposeInner(bool fromDispose)
        {
            if (m_disposed)
                return;
            m_disposed = true;
        }
    }
}