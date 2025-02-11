using System;

namespace Redbox.HAL.Component.Model.Attributes;

public class ValidValueListProviderAttribute : Attribute
{
    public ValidValueListProviderAttribute(string methodName)
    {
        MethodName = methodName;
    }

    public string MethodName { get; private set; }
}