using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class NativeJobInstruction : Instruction
    {
        public override string Mnemonic => "NATIVEJOB";

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var nativeJobAdapter = ExecutionEngine.Instance.MakeJobFromOperand(context, result, Operands[0].Value);
            if (nativeJobAdapter == null)
                result.Errors.Add(Error.NewError("E777", "Unrecognized job.",
                    string.Format("The job \"{0}\" is not in the native job cache.", Operands[0].Value)));
            else
                nativeJobAdapter.Execute();
        }
    }
}