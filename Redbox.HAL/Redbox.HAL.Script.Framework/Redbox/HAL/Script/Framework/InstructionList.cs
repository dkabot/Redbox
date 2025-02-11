using System;
using System.Collections;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public class InstructionList : IEnumerable<Instruction>, IEnumerable, ICloneable<InstructionList>
    {
        private readonly List<Instruction> m_instructions = new List<Instruction>();

        public InstructionList(Instruction parent)
        {
            Parent = parent;
        }

        public Instruction this[int index] => m_instructions[index];

        public int Count => m_instructions.Count;

        internal Instruction Parent { get; set; }

        public InstructionList Clone(params object[] parms)
        {
            var instructionList = new InstructionList((Instruction)parms[0]);
            foreach (Instruction instruction in this)
                instructionList.Add(instruction.Clone());
            return instructionList;
        }

        IEnumerator<Instruction> IEnumerable<Instruction>.GetEnumerator()
        {
            return m_instructions.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<Instruction>)this).GetEnumerator();
        }

        public void Add(Instruction instruction)
        {
            if (Parent != null)
                instruction.ParentContext = Parent;
            m_instructions.Add(instruction);
        }

        public void AddRange(IEnumerable<Instruction> instructions)
        {
            foreach (var instruction in instructions)
                Add(instruction);
        }

        public int FindInstructionIndexAtLabel(string labelName)
        {
            return m_instructions.FindIndex(each => string.Compare(labelName, each.LabelName, true) == 0);
        }

        public Instruction FindInstructionAtLabel(string labelName)
        {
            return m_instructions.Find(each => string.Compare(labelName, each.LabelName, true) == 0);
        }
    }
}