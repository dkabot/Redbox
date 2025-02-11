using System;

namespace Redbox.Core
{
    internal class ValidValueListProviderAttribute : Attribute
    {
        public ValidValueListProviderAttribute(string methodName) => this.MethodName = methodName;

        public string MethodName { get; private set; }
    }
}
