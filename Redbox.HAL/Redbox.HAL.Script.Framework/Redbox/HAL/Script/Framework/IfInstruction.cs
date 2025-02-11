using System;
using System.Text.RegularExpressions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public class IfInstruction : Instruction
    {
        public override string EndContextMnemonic => "ENDIF";

        public override bool BeginsContext => true;

        public override string Mnemonic => "IF";

        internal override Branch BranchType => Branch.Conditional;

        internal override bool ValidateConditionalBranch(Instruction target, ExecutionResult result)
        {
            if (target.Mnemonic.Equals("ELSEIF") || target.Mnemonic.Equals("ELSE") || target.Mnemonic.Equals("ENDIF"))
            {
                FalseBranchTarget = target.LineNumber;
                return true;
            }

            LogIncorrectBranch(target.Mnemonic, result);
            return false;
        }

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count != 3)
            {
                result.AddError(ExecutionErrors.InvalidOperand,
                    "The IF mnemonic requires a symbol, followed by a relational or equality operator and a comparand.");
            }
            else
            {
                if (operandTokens[0].Type == TokenType.Symbol || operandTokens[1].Type == TokenType.Operator)
                    return;
                result.AddError(ExecutionErrors.InvalidOperand,
                    "Expected a symbol and relational or equality operator.");
            }
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var objA = (IComparable)Operands[0].ConvertValue();
            if ((Operands[0].Type == TokenType.Symbol || Operands[0].Type == TokenType.ConstSymbol) &&
                Operands[0].Value.Equals(objA))
                objA = (IComparable)context.GetSymbolValue(Operands[0].Value, result.Errors);
            var objB = (IComparable)Operands[2].ConvertValue();
            if ((Operands[2].Type == TokenType.Symbol || Operands[2].Type == TokenType.ConstSymbol) &&
                Operands[2].Value.Equals(objB))
                objB = (IComparable)context.GetSymbolValue(Operands[2].Value, result.Errors);
            if (objA != null && objB != null && objA.GetType() != objB.GetType())
                objB = (IComparable)Convert.ChangeType(objB, objA.GetType());
            var flag = false;
            switch (Operands[1].Value.Trim())
            {
                case "==":
                    flag = Equals(objA, objB);
                    break;
                case "~=":
                    if (objA != null && objB != null)
                    {
                        var match = Regex.Match(objA.ToString(), objB.ToString());
                        flag = match != null && match.Success;
                    }

                    break;
                case "!=":
                    flag = !Equals(objA, objB);
                    break;
                case ">":
                    flag = objA != null && objA.CompareTo(objB) > 0;
                    break;
                case "<":
                    flag = objA != null && objA.CompareTo(objB) < 0;
                    break;
                case ">=":
                    flag = objA != null && objA.CompareTo(objB) >= 0;
                    break;
                case "<=":
                    flag = objA != null && objA.CompareTo(objB) <= 0;
                    break;
            }

            if (!flag)
                return;
            result.SwitchToContext = this;
        }

        private static class Constants
        {
            internal static class Operands
            {
                public const string PatternMatch = "~=";
                public const string EqualTo = "==";
                public const string NotEqualTo = "!=";
                public const string GreaterThan = ">";
                public const string LessThan = "<";
                public const string GreaterThanEqualTo = ">=";
                public const string LessThanEqualTo = "<=";
            }
        }
    }
}