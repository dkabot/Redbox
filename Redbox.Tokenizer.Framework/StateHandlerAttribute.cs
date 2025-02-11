using System;

namespace Redbox.Tokenizer.Framework
{
    [AttributeUsage(AttributeTargets.Method)]
    public class StateHandlerAttribute : Attribute
    {
        public object State { get; set; }
    }
}