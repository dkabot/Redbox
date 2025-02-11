using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public class VersionInstruction : Instruction
    {
        public override string Mnemonic => "VERSION";

        protected override ExpectedOperands Expected => ExpectedOperands.Optional;

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count <= 1)
                return;
            result.AddInvalidOperandError(
                "The VERSION instruction supports an optional operand indicating the target board.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var service = ServiceLocator.Instance.GetService<IControlSystem>();
            if (Operands.Count == 1)
            {
                var operand = Operands[0];
                var ignoringCase = Enum<ControlBoards>.ParseIgnoringCase(operand.Value, ControlBoards.None);
                if (ignoringCase == ControlBoards.None)
                {
                    result.Errors.Add(Error.NewError("E001", "Unrecognized board option.",
                        string.Format("The option {0} is not a board.", operand.Value)));
                }
                else
                {
                    var boardVersion = service.GetBoardVersion(ignoringCase);
                    context.PushTop(boardVersion);
                }
            }
            else
            {
                var revision = service.GetRevision();
                if (!revision.Success)
                {
                    context.PushTop(ErrorCodes.CommunicationError.ToString().ToUpper());
                }
                else
                {
                    Array.ForEach(revision.Responses, each => context.PushTop(each));
                    context.PushTop(string.Format("REVISION {0}", revision.Revision));
                }
            }
        }
    }
}