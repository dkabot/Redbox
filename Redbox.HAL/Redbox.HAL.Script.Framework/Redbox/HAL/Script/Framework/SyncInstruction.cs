using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class SyncInstruction : Instruction
    {
        public override string Mnemonic => "SYNC";

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var keyValuePair = Operands.GetKeyValuePair("TYPE");
            var type = SyncType.Standard;
            if (keyValuePair != null)
            {
                type = Enum<SyncType>.ParseIgnoringCase(keyValuePair.Value, SyncType.None);
                if (type == SyncType.None)
                {
                    result.Errors.Add(Error.NewError("E777", "Unrecognized sync type.",
                        string.Format("Unrecognized sync type {0}", keyValuePair)));
                    return;
                }
            }

            ILocation location;
            if (!GetLocation(result, context, out location))
            {
                result.Errors.Add(Error.NewError("E778", "Missing required operands.",
                    "The DECK and SLOT operands are missing. Unrecognized sync type {0}"));
            }
            else
            {
                var decorator = SyncDecorator.Get(type, context, result, location);
                var syncResult = SyncHelper.SyncSlot(result, context, location, decorator);
                context.PushTop(syncResult.ToString().ToUpper());
            }
        }
    }
}