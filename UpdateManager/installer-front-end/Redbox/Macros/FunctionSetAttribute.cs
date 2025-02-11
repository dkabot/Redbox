using System;

namespace Redbox.Macros
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    internal sealed class FunctionSetAttribute : Attribute
    {
        public FunctionSetAttribute(string prefix, string category)
        {
            if (prefix == null)
                throw new ArgumentNullException(nameof(prefix));
            if (category == null)
                throw new ArgumentNullException(nameof(category));
            if (prefix.Trim().Length == 0)
                throw new ArgumentOutOfRangeException(nameof(prefix), (object)prefix, "A zero-length string is not an allowed value.");
            if (category.Trim().Length == 0)
                throw new ArgumentOutOfRangeException(nameof(category), (object)category, "A zero-length string is not an allowed value.");
            this.Prefix = prefix;
            this.Category = category;
        }

        public string Category { get; set; }

        public string Prefix { get; set; }
    }
}
