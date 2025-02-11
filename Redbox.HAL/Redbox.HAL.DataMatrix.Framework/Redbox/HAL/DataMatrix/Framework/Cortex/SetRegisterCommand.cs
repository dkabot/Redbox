namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    internal sealed class SetRegisterCommand : TextCommand
    {
        internal SetRegisterCommand(CortexRegister register, string name)
        {
            Register = register;
            Name = name;
        }

        protected override string Prefix => "C";

        protected override string Data => string.Format("({0}){1}", Register.Register, Register.Value);

        internal string Name { get; private set; }

        internal CortexRegister Register { get; }
    }
}