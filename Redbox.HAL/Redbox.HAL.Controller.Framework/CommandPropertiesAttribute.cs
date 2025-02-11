using System;
using System.Reflection;

namespace Redbox.HAL.Controller.Framework
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class CommandPropertiesAttribute : Attribute
    {
        public CommandPropertiesAttribute()
        {
            StatusBit = WaitPauseTime = CommandWait = -1;
            Address = AddressSelector.None;
        }

        public int StatusBit { get; set; }

        public string Command { get; set; }

        public int CommandWait { get; set; }

        public string ResetCommand { get; set; }

        public AddressSelector Address { get; set; }

        public int WaitPauseTime { get; set; }

        public static CommandPropertiesAttribute GetProperties(FieldInfo fieldInfo)
        {
            var customAttributes =
                (CommandPropertiesAttribute[])GetCustomAttributes(fieldInfo, typeof(CommandPropertiesAttribute));
            return customAttributes != null && customAttributes.Length != 0 ? customAttributes[0] : null;
        }
    }
}