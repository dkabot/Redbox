using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class CounterInstruction : Instruction
    {
        public override string Mnemonic => "COUNTER";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens[0].IsKeyValuePair)
                return;
            result.AddInvalidOperandError(string.Format("The {0} instruction expected at least a key-value pair.",
                Mnemonic));
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var keyValuePairValue =
                GetKeyValuePairValue<string>(Operands.GetKeyValuePair("NAME"), result.Errors, context);
            var service1 = ServiceLocator.Instance.GetService<IPersistentCounterService>();
            var service2 = ServiceLocator.Instance.GetService<IDumpbinService>();
            var str = Operands[1].Value;
            switch (Enum<Ops>.ParseIgnoringCase(str, Ops.None))
            {
                case Ops.None:
                    AddError("E001", string.Format("Unrecognized operation: {0}", str), result.Errors);
                    break;
                case Ops.Increment:
                    var persistentCounter1 = service1.Find(keyValuePairValue);
                    if (persistentCounter1 == null)
                    {
                        context.PushTop("FAILURE");
                        break;
                    }

                    context.PushTop(persistentCounter1.Value);
                    context.PushTop(SuccessMessage);
                    break;
                case Ops.Decrement:
                    var persistentCounter2 = service1.Decrement(keyValuePairValue);
                    if (persistentCounter2 == null)
                    {
                        context.PushTop("FAILURE");
                        break;
                    }

                    context.PushTop(persistentCounter2.Value);
                    context.PushTop(SuccessMessage);
                    break;
                case Ops.Get:
                    if (keyValuePairValue.Equals("DUMPBIN", StringComparison.CurrentCultureIgnoreCase))
                    {
                        context.PushTop(service2.CurrentCount());
                        context.PushTop(SuccessMessage);
                        break;
                    }

                    var persistentCounter3 = service1.Find(keyValuePairValue);
                    if (persistentCounter3 == null)
                    {
                        context.PushTop("FAILURE");
                        break;
                    }

                    context.PushTop(persistentCounter3.Value);
                    context.PushTop(SuccessMessage);
                    break;
                case Ops.Create:
                    context.PushTop(service1.Find(keyValuePairValue) != null ? SuccessMessage : (object)"FAILURE");
                    break;
                case Ops.Reset:
                    context.PushTop(service1.Reset(keyValuePairValue) ? SuccessMessage : (object)"FAILURE");
                    break;
            }
        }

        private enum Ops
        {
            None,
            Increment,
            Decrement,
            Get,
            Create,
            Reset
        }
    }
}