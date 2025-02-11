using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class PutInstruction : Instruction
    {
        public override string Mnemonic => "PUT";

        protected override ExpectedOperands Expected => ExpectedOperands.Optional;

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            if (context.StackDepth == 0)
            {
                AddError("E004", "The PUT instruction expects a valid string literal ID on the stack.", result.Errors);
            }
            else
            {
                var keyValuePair1 = Operands.GetKeyValuePair("HOMEONFAILURE");
                if (keyValuePair1 != null)
                    bool.TryParse(keyValuePair1.Value, out _);
                var keyValuePair2 = Operands.GetKeyValuePair("PEEK");
                if (keyValuePair2 != null)
                    bool.TryParse(keyValuePair2.Value, out _);
                if (!(context.PopTop() is string id))
                {
                    AddError("E004", "The PUT instruction expects a valid string literal ID on the stack.",
                        result.Errors);
                }
                else
                {
                    var num = (int)ServiceLocator.Instance.GetService<IControlSystem>().TrackCycle();
                    var putResult = ServiceLocator.Instance.GetService<IControllerService>().Put(id);
                    context.PushTop(putResult.ToString().ToUpper());
                    context.PushTop(1);
                }
            }
        }

        private static class KeyNames
        {
            public const string Peek = "PEEK";
            public const string HomeOnFailure = "HOMEONFAILURE";
            public const string MarkReturnTime = "MARKRETURNTIME";
        }
    }
}