using System.Collections.Generic;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class Registers
    {
        internal readonly Stack<CallFrame> CallStack = new Stack<CallFrame>();

        internal CallFrame ActiveFrame => CallStack.Count != 0 ? CallStack.Peek() : null;

        internal Instruction PopCallFrame()
        {
            return CallStack.Count != 0 ? CallStack.Pop().ParentContext : null;
        }

        internal void PushCallFrame(string programName, Instruction parentContext)
        {
            CallStack.Push(new CallFrame
            {
                ProgramName = programName,
                ParentContext = parentContext,
                X = parentContext.X,
                Y = parentContext.Y
            });
        }

        internal bool JumpToLabel(string labelName)
        {
            while (ActiveFrame != null)
            {
                var instructionIndexAtLabel =
                    ActiveFrame.ParentContext.Instructions.FindInstructionIndexAtLabel(labelName);
                if (instructionIndexAtLabel != -1)
                {
                    ActiveFrame.PC = instructionIndexAtLabel - 1;
                    return true;
                }

                PopCallFrame();
            }

            return false;
        }

        internal void Reset()
        {
            CallStack.Clear();
        }
    }
}