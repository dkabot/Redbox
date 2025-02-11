using System;
using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public class Instruction : ICloneable<Instruction>
    {
        public enum Branch
        {
            None,
            Unconditional,
            Conditional
        }

        protected const string FailureMessage = "FAILURE";
        private const string ErrorMessageFormat = "The {0} instruction failed (line {1} of script {2}): {3}";
        private int m_falseBranch = ProgramInstruction.InvalidLineNumber;
        private InstructionList m_instructions;
        private TokenList m_operands;
        private int m_trueBranch = ProgramInstruction.EndLineNumber;

        public Instruction FirstInstruction => Instructions[0];

        public virtual string EndContextMnemonic => null;

        public virtual bool BeginsContext => false;

        public virtual bool IsCooperativeMultitask => false;

        public Instruction ParentContext { get; set; }

        public virtual string LabelName { get; set; }

        public virtual bool EndsContext => false;

        public virtual string Mnemonic => string.Empty;

        public virtual bool ExitsContext => false;

        public TokenList Operands
        {
            get
            {
                if (m_operands == null)
                    m_operands = new TokenList();
                return m_operands;
            }
            private set => m_operands = value;
        }

        public int LineNumber { get; set; }

        public int X { get; internal set; }

        public int Y { get; internal set; }

        protected virtual ExpectedOperands Expected => ExpectedOperands.AtLeastOne;

        protected string SuccessMessage => ErrorCodes.Success.ToString().ToUpper();

        protected string TimeoutMessage => ErrorCodes.Timeout.ToString().ToUpper();

        internal virtual Branch BranchType => Branch.None;

        internal virtual bool IsSynthetic => false;

        internal InstructionList Instructions
        {
            get
            {
                if (m_instructions == null)
                    m_instructions = new InstructionList(this);
                return m_instructions;
            }
            set => m_instructions = value;
        }

        internal bool IsLabel { get; set; }

        internal virtual bool CreatesVariableDef => false;

        internal virtual int TrueBranchTarget
        {
            get => m_trueBranch;
            set => m_trueBranch = value;
        }

        internal virtual int FalseBranchTarget
        {
            get => m_falseBranch;
            set => m_falseBranch = value;
        }

        internal virtual string CounterSymbol { get; set; }

        internal virtual string TargetLabel => string.Empty;

        public Instruction Clone(params object[] parms)
        {
            var instance = (Instruction)Activator.CreateInstance(GetType());
            instance.LabelName = LabelName;
            instance.LineNumber = LineNumber;
            instance.Operands = Operands.Clone();
            instance.Instructions = Instructions.Clone(instance);
            instance.TrueBranchTarget = TrueBranchTarget;
            instance.FalseBranchTarget = FalseBranchTarget;
            instance.CounterSymbol = CounterSymbol;
            return instance;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is Instruction instruction && instruction.Mnemonic.ToUpper().Equals(Mnemonic) &&
                   instruction.LineNumber == LineNumber;
        }

        public static bool MnemonicImpliesConstSymbol(string mnemonic)
        {
            return mnemonic.ToUpper().Equals("CONST");
        }

        public static Instruction Parse(TokenList tokens, int lineNumber, ExecutionResult result)
        {
            if (tokens.Count == 0)
                return null;
            foreach (var type in typeof(Instruction).Assembly.GetTypes())
                if (type.IsSubclassOf(typeof(Instruction)) && Activator.CreateInstance(type) is Instruction instance &&
                    string.Compare(instance.Mnemonic, tokens.GetMnemonic().Value, true) == 0)
                {
                    instance.LineNumber = lineNumber;
                    var label = tokens.GetLabel();
                    if (label != null)
                        instance.LabelName = label.Value;
                    instance.OnParse(tokens, result);
                    return instance;
                }

            return null;
        }

        public Instruction GetByLineNumber(int lineNumber)
        {
            return GetByLineNumber(this, lineNumber);
        }

        public void Execute(ExecutionResult result, ExecutionContext context)
        {
            try
            {
                OnExecute(result, context);
            }
            catch (Exception ex)
            {
                var str = string.Format(
                    "Program '{0}', Line {1}: An unhandled exception was raised by the execution engine.",
                    context != null ? context.Registers.ActiveFrame.ProgramName : (object)"Idle", LineNumber);
                result.Errors.Add(Error.NewError("E001", str, ex.ToString()));
                LogHelper.Instance.Log(str, ex);
            }
        }

        public ProgramInstruction GetProgram()
        {
            var program = ParentContext ?? this;
            while (true)
                switch (program)
                {
                    case null:
                    case ProgramInstruction _:
                        goto label_3;
                    default:
                        program = program.ParentContext;
                        continue;
                }

            label_3:
            return program as ProgramInstruction;
        }

        public override string ToString()
        {
            var stringBuilder1 = new StringBuilder();
            if (LabelName != null)
                stringBuilder1.Append(LabelName + ":");
            stringBuilder1.AppendFormat(" {0,-16}", Mnemonic);
            var stringBuilder2 = new StringBuilder();
            foreach (var operand in Operands)
            {
                if (stringBuilder2.Length > 0)
                    stringBuilder2.Append(" ");
                if (operand.Type == TokenType.StringLiteral)
                    stringBuilder2.AppendFormat("\"{0}\"", operand.Value.ToUpper());
                else
                    stringBuilder2.Append(operand.Value.ToUpper());
            }

            if (stringBuilder2.Length > 0)
                stringBuilder1.Append(stringBuilder2);
            return stringBuilder1.ToString();
        }

        internal bool OperandRecognized(string operand)
        {
            return operand.Equals("TRUE") || operand.Equals("FALSE") || IsOperandRecognized(operand);
        }

        protected void OnParse(TokenList tokens, ExecutionResult result)
        {
            var tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
            if (Expected == ExpectedOperands.None)
            {
                if (tokensAfterMnemonic.Count <= 0)
                    return;
                result.AddInvalidOperandError(string.Format("The {0} instruction doesn't expect operands.", Mnemonic));
            }
            else if (ExpectedOperands.AtLeastOne == Expected && tokensAfterMnemonic.Count == 0)
            {
                result.AddMissingOperandError(string.Format("The {0} instruction expects at least one operand.",
                    Mnemonic));
            }
            else
            {
                if (ExpectedOperands.Optional == Expected && tokensAfterMnemonic.Count == 0)
                    return;
                ParseOperands(tokensAfterMnemonic, result);
                if (result.Errors.ContainsError())
                    return;
                Operands.AddRange(tokensAfterMnemonic);
            }
        }

        protected virtual void OnExecute(ExecutionResult result, ExecutionContext context)
        {
        }

        protected virtual bool IsOperandRecognized(string op)
        {
            return false;
        }

        protected virtual void ParseOperands(TokenList operands, ExecutionResult result)
        {
        }

        protected void LogIncorrectBranch(string targetMnemonic, ExecutionResult result)
        {
        }

        protected bool CheckAssignment(TokenType type, ExecutionResult result)
        {
            if (type != TokenType.ConstSymbol)
                return true;
            result.AddError(ExecutionErrors.InvalidAssignment,
                "Illegal attempt to " + Mnemonic + " value to CONST symbol.");
            return false;
        }

        protected bool GetLocation(
            ExecutionResult result,
            ExecutionContext context,
            out ILocation location)
        {
            var deck = -1;
            var slot = -1;
            location = null;
            if (!GetLocation(result, context, out deck, out slot))
                return false;
            var service = ServiceLocator.Instance.GetService<IInventoryService>();
            location = service.Get(deck, slot);
            return true;
        }

        protected bool GetLocation(
            ExecutionResult result,
            ExecutionContext context,
            out int deck,
            out int slot)
        {
            deck = -1;
            slot = -1;
            var deckOperand = GetDeckOperand(result.Errors, context);
            if (!deckOperand.HasValue)
                return false;
            deck = deckOperand.Value;
            var keyValuePairValue =
                GetKeyValuePairValue<int?>(Operands.GetKeyValuePair("SLOT"), result.Errors, context);
            if (result.Errors.Count > 0)
                return false;
            if (keyValuePairValue.HasValue)
                slot = keyValuePairValue.Value;
            return true;
        }

        protected string GetID(ExecutionResult result, ExecutionContext context)
        {
            var keyValuePair = Operands.GetKeyValuePair("ID");
            if (keyValuePair == null)
            {
                result.Errors.Add(Error.NewError("E099", "Missing operand.", "The ID operand is missing."));
                return null;
            }

            var keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair, result.Errors, context);
            return result.Errors.Count != 0 ? string.Empty : keyValuePairValue;
        }

        protected void HandleError(IControlResponse response, ExecutionContext context)
        {
            if (response.CommError)
                context.PushTop(response.Diagnostic);
            else
                context.PushTop(response.TimedOut ? TimeoutMessage : (object)SuccessMessage);
        }

        protected int? GetDeckOperand(ErrorList errors, ExecutionContext context)
        {
            var keyValuePairValue = GetKeyValuePairValue<int?>(Operands.GetKeyValuePair("DECK"), errors, context);
            return errors.Count > 0 ? new int?() : keyValuePairValue;
        }

        internal object[] ConvertSymbolsToParameterArray(ErrorList errors, ExecutionContext context)
        {
            var objectList = new List<object>();
            for (var index = 1; index < Operands.Count; ++index)
            {
                var operand = Operands[index];
                if (!operand.IsKeyValuePair)
                {
                    var obj = operand.ConvertValue();
                    if ((operand.Type == TokenType.Symbol || operand.Type == TokenType.ConstSymbol) &&
                        operand.Value.Equals(obj))
                        obj = context.GetSymbolValue(operand.Value, errors);
                    objectList.Add(obj);
                }
            }

            return objectList.ToArray();
        }

        internal bool GetKeyValuePairValue<T>(
            string key,
            out T val,
            ErrorList errors,
            ExecutionContext context)
        {
            var keyValuePair = Operands.GetKeyValuePair(key);
            if (keyValuePair == null)
            {
                val = default;
                return false;
            }

            val = GetKeyValuePairValue<T>(keyValuePair, errors, context);
            return true;
        }

        internal T GetKeyValuePairValue<T>(
            KeyValuePair keyValuePair,
            ErrorList errors,
            ExecutionContext context)
        {
            if (keyValuePair == null)
                return default;
            var obj = (object)null;
            if (keyValuePair.Type == TokenType.NumericLiteral)
            {
                obj = int.Parse(keyValuePair.Value);
            }
            else if (keyValuePair.Type == TokenType.Symbol || keyValuePair.Type == TokenType.ConstSymbol)
            {
                var symbolValue = context.GetSymbolValue(keyValuePair.Value, errors);
                if (symbolValue != null)
                    obj = symbolValue;
            }
            else
            {
                obj = keyValuePair.Value;
            }

            var keyValuePairValue = default(T);
            try
            {
                keyValuePairValue = ConversionHelper.ChangeType<T>(obj);
            }
            catch (Exception ex)
            {
                AddError("E002", "Type conversion error: " + ex, errors);
            }

            return keyValuePairValue;
        }

        internal int? GetNullableNumericOperand(
            ExecutionResult result,
            string name,
            ExecutionContext context)
        {
            var keyValuePairValue = GetKeyValuePairValue<int?>(Operands.GetKeyValuePair(name), result.Errors, context);
            return result.Errors.Count > 0 ? new int?() : keyValuePairValue;
        }

        internal string GetDateRepresentation(DateTime? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.ToString() : "NONE";
        }

        internal int? GetTimeOutValue(ErrorList errors, ExecutionContext context)
        {
            var timeOutValue = new int?();
            var keyValuePair = Operands.GetKeyValuePair(TimeoutMessage);
            if (keyValuePair == null)
                return new int?();
            if (keyValuePair.Type == TokenType.Symbol || keyValuePair.Type == TokenType.ConstSymbol)
            {
                timeOutValue = (int?)context.GetSymbolValue(keyValuePair.Value, errors);
            }
            else
            {
                int result;
                if (keyValuePair.Type == TokenType.NumericLiteral && int.TryParse(keyValuePair.Value, out result))
                    timeOutValue = result;
            }

            return timeOutValue;
        }

        internal bool GetWaitFlag()
        {
            var waitFlag = false;
            var keyValuePair = Operands.GetKeyValuePair("WAIT");
            bool result;
            if (keyValuePair != null && bool.TryParse(keyValuePair.Value, out result))
                waitFlag = result;
            return waitFlag;
        }

        internal void AddError(string code, string message, ErrorList errors)
        {
            errors.Add(Error.NewError(code, message, string.Format("Line {0}: {1}", LineNumber, this)));
        }

        internal void AddWarning(string code, string message, ErrorList errors)
        {
            errors.Add(Error.NewWarning(code, message, string.Format("Line {0}: {1}", LineNumber, this)));
        }

        internal Instruction GetByLineNumber(Instruction instruction, int lineNumber)
        {
            if (instruction.LineNumber == lineNumber)
                return instruction;
            foreach (Instruction instruction1 in instruction.Instructions)
            {
                var byLineNumber = GetByLineNumber(instruction1, lineNumber);
                if (byLineNumber != null)
                    return byLineNumber;
            }

            return null;
        }

        internal bool IsSymbolic(Token tok)
        {
            return tok.Type == TokenType.ConstSymbol || tok.Type == TokenType.Symbol;
        }

        internal bool IsBranch()
        {
            return BranchType == Branch.Unconditional || BranchType == Branch.Conditional;
        }

        internal bool IsConditional()
        {
            return BranchType == Branch.Conditional;
        }

        internal virtual bool ValidateConditionalBranch(
            Instruction branchTarget,
            ExecutionResult result)
        {
            return true;
        }

        protected enum ExpectedOperands
        {
            None,
            Optional,
            AtLeastOne
        }
    }
}