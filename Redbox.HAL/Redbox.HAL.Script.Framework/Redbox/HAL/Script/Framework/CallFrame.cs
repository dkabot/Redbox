using System.Collections.Generic;
using Microsoft.Win32;

namespace Redbox.HAL.Script.Framework
{
    public class CallFrame
    {
        public int PC { get; set; }

        public Instruction IP => ParentContext != null && ParentContext.Instructions.Count != 0
            ? ParentContext.Instructions[PC]
            : null;

        public int X { get; internal set; }

        public int Y { get; internal set; }

        public string ProgramName { get; set; }

        public Instruction ParentContext { get; set; }

        public void ResetPC()
        {
            PC = 0;
        }

        public bool FetchNextInstruction()
        {
            ++PC;
            return ParentContext != null && PC < ParentContext.Instructions.Count;
        }

        internal static CallFrame Load(
            RegistryKey parentKey,
            IDictionary<string, ProgramInstruction> programCache)
        {
            var key = (string)parentKey.GetValue("ProgramName");
            var lineNumber = (int)parentKey.GetValue("ParentContext");
            return new CallFrame
            {
                X = (int)parentKey.GetValue("X", 0),
                Y = (int)parentKey.GetValue("Y", 0),
                PC = (int)parentKey.GetValue("PC"),
                ProgramName = key,
                ParentContext = lineNumber == -1 || !programCache.ContainsKey(key)
                    ? null
                    : programCache[key].GetByLineNumber(lineNumber)
            };
        }

        internal void Save(RegistryKey parentKey)
        {
            parentKey.SetValue("PC", PC);
            parentKey.SetValue("X", X);
            parentKey.SetValue("Y", Y);
            parentKey.SetValue("ProgramName", ProgramName);
            parentKey.SetValue("ParentContext", ParentContext != null ? ParentContext.LineNumber : -1);
        }

        internal void LoadInstructionPointer(int parentContextLineNumber, ProgramInstruction program)
        {
            ParentContext = parentContextLineNumber == -1 || program == null
                ? null
                : program.GetByLineNumber(parentContextLineNumber);
        }
    }
}