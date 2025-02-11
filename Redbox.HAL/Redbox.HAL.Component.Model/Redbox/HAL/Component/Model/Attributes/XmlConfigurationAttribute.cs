using System;

namespace Redbox.HAL.Component.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class XmlConfigurationAttribute : Attribute
    {
        public string DefaultValue { get; set; }
    }
}