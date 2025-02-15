using System;

namespace Redbox.Core
{
    public class ValidValueListProviderAttribute : Attribute
    {
        public ValidValueListProviderAttribute(string methodName)
        {
            MethodName = methodName;
        }

        public string MethodName { get; private set; }
    }
}