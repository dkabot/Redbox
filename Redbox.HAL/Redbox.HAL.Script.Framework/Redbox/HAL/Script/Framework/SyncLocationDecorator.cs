using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class SyncLocationDecorator : SyncDecorator, IGetObserver
    {
        internal SyncLocationDecorator(ExecutionContext ctx, ExecutionResult r, ILocation loc)
            : base(ctx, r, loc)
        {
        }

        public void OnStuck(IGetResult result)
        {
            HandleStuck(result);
        }

        public bool OnEmpty(IGetResult result)
        {
            using (var peekOperation = new PeekOperation())
            {
                var peekResult = peekOperation.Execute();
                if (!peekResult.TestOk)
                {
                    UpdateSymbol("MSTESTER-SYMBOL-GET-ERR-MSG",
                        string.Format("Peek Deck={0} Slot={1} returned status {2} after GET returned SLOTEMPTY",
                            SyncLocation.Deck, SyncLocation.Slot, peekResult.Error.ToString().ToUpper()));
                    Context.CreateResult("MachineError", "An error occurred when attempting to peek the slot.",
                        SyncLocation.Deck, SyncLocation.Slot, null, new DateTime?(), null);
                    AddError("There was an error performing a PEEK.");
                    HandleStuck(result);
                    return false;
                }

                var service = ServiceLocator.Instance.GetService<IInventoryService>();
                if (!peekResult.IsFull)
                {
                    UpdateSymbol("MSTESTER-SYMBOL-EMPTY-SLOT-MSG",
                        string.Format("The location Deck={0} Slot={1} is empty.", SyncLocation.Deck,
                            SyncLocation.Slot));
                    SyncLocation.ReturnDate = new DateTime?();
                    service.Save(SyncLocation);
                    return true;
                }

                HandleStuck(result, service);
                return false;
            }
        }

        protected override void OnReadDisk(ScanResult scanResult)
        {
            var scannedMatrix = scanResult.ScannedMatrix;
            var num = scanResult.ReadCount;
            var timeSpan = scanResult.ExecutionTime;
            if (!scanResult.ReadCode)
            {
                num = 0;
                timeSpan = new TimeSpan();
            }

            UpdateSymbol("MSTESTER-SYMBOL-FORMATTED-DETAIL-MSG",
                ServiceLocator.Instance.GetService<IInventoryService>().IsBarcodeDuplicate(scannedMatrix, out _)
                    ? string.Format("SUCCESS: {0} codes ({1}), time = {2} DUPLICATE", scannedMatrix, num,
                        timeSpan.ToString())
                    : (object)string.Format("SUCCESS: {0} codes ({1}), time = {2}", scannedMatrix, num,
                        timeSpan.ToString()));
        }

        protected override void OnMoveErrorInner(ErrorCodes moveRes)
        {
            UpdateSymbol("MSTESTER-SYMBOL-MOVE-FALURE",
                string.Format("MOVE Deck={0} Slot={1} returned an error status {2}", SyncLocation.Deck,
                    SyncLocation.Slot, moveRes.ToString().ToUpper()));
        }

        internal override IGetResult GetDisk(IFormattedLog applog)
        {
            return ServiceLocator.Instance.GetService<IControllerService>().Get(this);
        }

        internal override void OnGetFailure(IGetResult gr)
        {
            UpdateSymbol("MSTESTER-SYMBOL-GET-ERR-MSG",
                string.Format("GET Deck={0} Slot={1} returned error status {2}", SyncLocation.Deck, SyncLocation.Slot,
                    gr));
            AddError("There was an error performing a GET.");
        }

        internal override void OnPutError(string id, IPutResult putResult)
        {
            UpdateSymbol("MSTESTER-SYMBOL-PUT-FAILURE",
                string.Format("PUT Deck={0} Slot={1} ID={2} returned error status {3}", SyncLocation.Deck,
                    SyncLocation.Slot, id, putResult));
            Context.CreateResult("PutFailure", "The PUT instruction failed.", SyncLocation.Deck, SyncLocation.Slot, id,
                new DateTime?(), null);
        }

        internal override void OnExcludedLocation()
        {
            UpdateSymbol("MSTESTER-SYMBOL-EXCLUDED-SLOT-MSG",
                SyncLocation.IsWide
                    ? string.Format("LOCATION DECK={0} SLOT={1} is a wide slot on deck 7, and will not be synced.",
                        SyncLocation.Deck, SyncLocation.Slot)
                    : (object)string.Format("LOCATION DECK={0} SLOT={1} is excluded", SyncLocation.Deck,
                        SyncLocation.Slot));
        }

        private void HandleStuck(IGetResult result)
        {
            var service = ServiceLocator.Instance.GetService<IInventoryService>();
            HandleStuck(result, service);
        }

        private void HandleStuck(IGetResult result, IInventoryService service)
        {
            UpdateSymbol("MSTESTER-SYMBOL-STUCK-DISC-MSG",
                string.Format("The location Deck={0} Slot={1} has a disc stuck in slot.", SyncLocation.Deck,
                    SyncLocation.Slot));
            result.Update(ErrorCodes.ItemStuck);
            service.UpdateEmptyStuck(result.Location);
        }
    }
}