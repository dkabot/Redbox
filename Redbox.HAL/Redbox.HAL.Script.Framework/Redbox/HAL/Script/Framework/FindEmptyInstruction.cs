using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public class FindEmptyInstruction : Instruction
    {
        public override string Mnemonic => "FINDEMPTY";

        protected override ExpectedOperands Expected => ExpectedOperands.None;

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            using (var emptyLocations =
                   ServiceLocator.Instance.GetService<IEmptySearchPatternService>().FindEmptyLocations())
            {
                if (emptyLocations.FoundEmpty == 0)
                {
                    context.PushTop("MACHINE FULL");
                }
                else
                {
                    var emptyLocation = emptyLocations.EmptyLocations[0];
                    context.PushTop(emptyLocation.Slot);
                    context.PushTop(emptyLocation.Deck);
                    context.PushTop("FOUND");
                }
            }
        }
    }
}