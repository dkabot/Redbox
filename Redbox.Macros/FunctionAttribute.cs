using System;

namespace Redbox.Macros
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class FunctionAttribute : Attribute
    {
        public FunctionAttribute(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            Name = name.Trim().Length != 0
                ? name
                : throw new ArgumentOutOfRangeException(nameof(name), name,
                    "A zero-length string is not an allowed value.");
        }

        public string Name { get; set; }
    }
}