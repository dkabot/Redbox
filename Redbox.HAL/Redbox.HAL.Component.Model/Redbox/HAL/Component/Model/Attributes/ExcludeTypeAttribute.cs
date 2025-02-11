using System;

namespace Redbox.HAL.Component.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ExcludeTypeAttribute : Attribute
    {
    }
}