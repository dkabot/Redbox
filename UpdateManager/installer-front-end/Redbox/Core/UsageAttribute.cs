using System;

namespace Redbox.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    internal class UsageAttribute : Attribute
    {
        public UsageAttribute(string template) => this.Template = template;

        public string Template { get; private set; }
    }
}
