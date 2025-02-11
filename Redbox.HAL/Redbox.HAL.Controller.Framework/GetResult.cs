using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class GetResult : IGetResult
    {
        internal GetResult(ILocation location)
        {
            Location = location;
            Previous = Location.ID;
            Flags = Location.Flags;
            ReturnTime = location.ReturnDate;
            HardwareError = ErrorCodes.Success;
            LogHelper.Instance.Log(LogEntryType.Debug, "[GET Result] Loc = {0} Prev = {1} Flags = {2} r/t = {3}",
                location.ToString(), Previous, Flags.ToString(), ReturnTime.ToString());
        }

        public bool EmptyOrStuck => IsSlotEmpty || ItemStuck;

        public void Update(ErrorCodes newError)
        {
            HardwareError = newError;
        }

        public ErrorCodes HardwareError { get; private set; }

        public bool Success => HardwareError == ErrorCodes.Success;

        public bool IsSlotEmpty => ErrorCodes.SlotEmpty == HardwareError;

        public bool ItemStuck => ErrorCodes.ItemStuck == HardwareError;

        public ILocation Location { get; }

        public string Previous { get; }

        public DateTime? ReturnTime { get; }

        public MerchFlags Flags { get; }

        public override string ToString()
        {
            return HardwareError.ToString();
        }
    }
}