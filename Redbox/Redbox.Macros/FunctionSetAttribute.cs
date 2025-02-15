using System;

namespace Redbox.Macros
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class FunctionSetAttribute : Attribute
    {
        public FunctionSetAttribute(string prefix, string category)
        {
            if (prefix == null)
                throw new ArgumentNullException(nameof(prefix));
            if (category == null)
                throw new ArgumentNullException(nameof(category));
            if (prefix.Trim().Length == 0)
                throw new ArgumentOutOfRangeException(nameof(prefix), prefix,
                    "A zero-length string is not an allowed value.");
            if (category.Trim().Length == 0)
                throw new ArgumentOutOfRangeException(nameof(category), category,
                    "A zero-length string is not an allowed value.");
            Prefix = prefix;
            Category = category;
        }

        public string Category { get; set; }

        public string Prefix { get; set; }
    }
}