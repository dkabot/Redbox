using System;

namespace Redbox.HAL.Script.Framework
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NativeJobAttribute : Attribute
    {
        public string ProgramName { get; set; }

        public string Operand { get; set; }

        public bool HideFromList { get; set; }
    }
}