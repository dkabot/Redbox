using System;

namespace Redbox.HAL.Component.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class UsageAttribute : Attribute
    {
        public UsageAttribute(string template)
        {
            Template = template;
        }

        public string Template { get; private set; }
    }
}