namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    internal sealed class CortexRegister
    {
        internal CortexRegister(string string_0, string string_1)
        {
            Register = string_0;
            Value = string_1;
        }

        internal string Register { get; private set; }

        internal string Value { get; private set; }
    }
}