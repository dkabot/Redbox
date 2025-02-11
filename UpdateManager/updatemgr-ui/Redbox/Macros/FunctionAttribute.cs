using System;

namespace Redbox.Macros
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal sealed class FunctionAttribute : Attribute
    {
        public FunctionAttribute(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            this.Name = name.Trim().Length != 0 ? name : throw new ArgumentOutOfRangeException(nameof(name), (object)name, "A zero-length string is not an allowed value.");
        }

        public string Name { get; set; }
    }
}
