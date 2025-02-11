using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class GetInstruction : Instruction
    {
        public override string Mnemonic => "GET";

        protected override ExpectedOperands Expected => ExpectedOperands.Optional;

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count <= 0 || operandTokens[0].IsKeyValuePair)
                return;
            result.AddInvalidOperandError(string.Format("The {0} instruction expects an optional key-value pair.",
                Mnemonic));
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var keyValuePair1 = Operands.GetKeyValuePair("PEEK");
            if (keyValuePair1 != null)
                bool.TryParse(keyValuePair1.Value, out _);
            var flag1 = false;
            var keyValuePair2 = Operands.GetKeyValuePair("CENTER");
            bool result1;
            if (keyValuePair2 != null && bool.TryParse(keyValuePair2.Value, out result1))
                flag1 = result1;
            var test = false;
            var keyValuePair3 = Operands.GetKeyValuePair("TESTONEMPTY");
            bool result2;
            if (keyValuePair3 != null && bool.TryParse(keyValuePair3.Value, out result2))
                test = result2;
            var flag2 = false;
            var keyValuePair4 = Operands.GetKeyValuePair("GET-RT");
            bool result3;
            if (keyValuePair4 != null && bool.TryParse(keyValuePair4.Value, out result3))
                flag2 = result3;
            if (context.IsImmediate)
                test = true;
            var observer = new GetInstructionObserver(test);
            var getResult = ServiceLocator.Instance.GetService<IControllerService>().Get(observer);
            if (getResult.Success & flag1)
            {
                var num = (int)ServiceLocator.Instance.GetService<IControlSystem>()
                    .Center(CenterDiskMethod.VendDoorAndBack);
            }

            if (flag2)
                context.PushTop(GetDateRepresentation(getResult.ReturnTime));
            context.PushTop(getResult.Previous);
            context.PushTop(getResult.ToString().ToUpper());
        }

        private sealed class GetInstructionObserver : IGetObserver
        {
            private readonly bool TestSlot;

            internal GetInstructionObserver(bool test)
            {
                TestSlot = test && ControllerConfiguration.Instance.TestSlotOnEmpty;
            }

            public bool OnEmpty(IGetResult gr)
            {
                if (!TestSlot)
                    return true;
                using (var peekOperation = new PeekOperation())
                {
                    var peekResult = peekOperation.Execute();
                    if (peekResult.TestOk && !peekResult.IsFull)
                        return true;
                    gr.Update(ErrorCodes.ItemStuck);
                    ServiceLocator.Instance.GetService<IInventoryService>().UpdateEmptyStuck(gr.Location);
                    return false;
                }
            }

            public void OnStuck(IGetResult gr)
            {
                if (!TestSlot)
                    return;
                ServiceLocator.Instance.GetService<IInventoryService>().UpdateEmptyStuck(gr.Location);
            }
        }
    }
}