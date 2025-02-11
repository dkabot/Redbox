using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    internal class GetAndCenterOperation
    {
        private readonly ExecutionContext Context;

        internal GetAndCenterOperation(ExecutionContext context)
        {
            Context = context;
        }

        internal bool Run()
        {
            var service = ServiceLocator.Instance.GetService<IControlSystem>();
            var sensorReadResult = service.ReadPickerSensors();
            if (!sensorReadResult.Success)
            {
                Context.CreateResult("ErrorMessage",
                    string.Format("Read picker sensors returned error {0}", sensorReadResult.Error), null);
                return false;
            }

            if (sensorReadResult.IsFull)
            {
                Context.CreateResult("ErrorMessage", "The picker is full.", new int?(), new int?(), null,
                    new DateTime?(), null);
                return false;
            }

            var slot = Context.PopTop<int>();
            var deck = Context.PopTop<int>();
            var flag = Context.PopTop<bool>();
            var errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>()
                .MoveTo(deck, slot, MoveMode.Get, Context.AppLog);
            var message = string.Format("MOVE DECK={0} SLOT={1} MODE=GET {2}", deck, slot,
                errorCodes.ToString().ToUpper());
            Context.CreateResult(errorCodes == ErrorCodes.Success ? "SuccessMessage" : "ErrorMessage", message, deck,
                slot, null, new DateTime?(), null);
            if (errorCodes != ErrorCodes.Success)
                return false;
            var getResult = ServiceLocator.Instance.GetService<IControllerService>().Get();
            if (!getResult.Success)
            {
                Context.CreateResult("ErrorMessage",
                    string.Format("GET returned error status {0}", getResult), deck, slot, null,
                    new DateTime?(), null);
                return false;
            }

            if (flag)
            {
                var num = (int)service.Center(CenterDiskMethod.VendDoorAndBack);
            }

            Context.CreateInfoResult("SuccessMessage", "GET SUCCESS");
            return true;
        }
    }
}