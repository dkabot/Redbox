using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class SecretOptionInstruction : Instruction
    {
        public override string Mnemonic => "SECRETOPTION";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            var op = operandTokens[0].Value;
            if (OperationRecognized(result, op))
                return;
            result.AddInvalidOperandError(string.Format("The {0} is unrecognized.", op));
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var operation = ComputeOperation(result, Operands[0].Value);
            string key;
            if (!LocateKey(result, context, out key))
            {
                AddError("E001", "Missing 'NAME' operand.", result.Errors);
            }
            else
            {
                var map = ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap();
                var val = (string)null;
                LocateValue(result, context, out val);
                switch (operation)
                {
                    case Operation.Set:
                        if (string.IsNullOrEmpty(val))
                        {
                            AddError("E001", "A 'VALUE' operand is expected.", result.Errors);
                            break;
                        }

                        map.SetValue(key, val);
                        break;
                    case Operation.SetEncrypted:
                        result.Errors.Add(Error.NewError("E666", "Unsuppported operation.",
                            "The set encrypted operation on SecretOptions is not supported."));
                        break;
                    case Operation.Get:
                        var str = map.GetValue(key, string.Empty);
                        context.PushTop(string.IsNullOrEmpty(str) ? "UNKNOWN" : (object)str);
                        break;
                    case Operation.GetEncrypted:
                        result.Errors.Add(Error.NewError("E666", "Unsuppported operation.",
                            "The get encrypted operation on SecretOptions is not supported."));
                        break;
                    case Operation.Remove:
                        map.Remove(key);
                        break;
                }
            }
        }

        private bool LocateKey(ExecutionResult result, ExecutionContext context, out string key)
        {
            var keyValuePair = Operands.GetKeyValuePair("NAME");
            key = GetKeyValuePairValue<string>(keyValuePair, result.Errors, context);
            return result.Errors.Count <= 0;
        }

        private bool LocateValue(ExecutionResult result, ExecutionContext context, out string val)
        {
            var keyValuePair = Operands.GetKeyValuePair("VALUE");
            val = GetKeyValuePairValue<string>(keyValuePair, result.Errors, context);
            return result.Errors.Count <= 0;
        }

        private bool OperationRecognized(ExecutionResult result, string op)
        {
            return ComputeOperation(result, op) != 0;
        }

        private Operation ComputeOperation(
            ExecutionResult result,
            string operation)
        {
            var ignoringCase = Enum<Operation>.ParseIgnoringCase(operation, Operation.None);
            if (ignoringCase != Operation.None)
                return ignoringCase;
            var message = string.Format("Unrecognized OPERATION argument: {0}", operation);
            LogHelper.Instance.Log(message, LogEntryType.Info);
            AddError("E001", message, result.Errors);
            return Operation.None;
        }

        private enum Operation
        {
            None,
            Set,
            SetEncrypted,
            Get,
            GetEncrypted,
            Remove
        }
    }
}