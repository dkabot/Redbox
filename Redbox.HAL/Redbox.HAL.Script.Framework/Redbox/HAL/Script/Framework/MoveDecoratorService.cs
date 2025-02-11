using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    internal struct MoveDecoratorService : IDisposable
    {
        private readonly ExecutionContext Context;
        private readonly int DefaultOffset;

        public void Dispose()
        {
        }

        internal ErrorCodes ShowEmptyStuck(ILocation location)
        {
            return ShowEmptyStuck(location, DefaultOffset);
        }

        internal ErrorCodes ShowEmptyStuck(ILocation location, int offset)
        {
            var errorCodes = MoveOffset(location, offset);
            if (errorCodes != ErrorCodes.Success)
            {
                if (!Context.IsImmediate)
                    Context.CreateInfoResult("MoveOffsetError", "There was an error moving the offset.");
            }
            else
            {
                var service = ServiceLocator.Instance.GetService<IControlSystem>();
                service.SetFinger(GripperFingerState.Rent);
                service.TimedExtend();
                service.SetSensors(true);
                if (!Context.IsImmediate)
                    Context.CreateInfoResult("MoveOffsetOk",
                        "The damaged disk is located 5 slots left, starting at the disk next to the extended gripper fingers.");
            }

            return errorCodes;
        }

        internal ErrorCodes MoveOffset(ILocation location, int offset)
        {
            var from = ServiceLocator.Instance.GetService<IDecksService>().GetFrom(location);
            var slot = location.Slot - offset;
            if (slot <= 0)
                slot = from.NumberOfSlots - Math.Abs(slot);
            var errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>()
                .MoveTo(location.Deck, slot, MoveMode.None, Context.AppLog);
            Context.AppLog.Write(string.Format("MOVE-OFFSET Deck = {0} Slot = {1} returned {2}", from.Number, slot,
                errorCodes.ToString().ToUpper()));
            return errorCodes;
        }

        internal MoveDecoratorService(ExecutionContext context)
            : this()
        {
            Context = context;
            DefaultOffset = 5;
        }
    }
}