using System;

namespace Redbox.UpdateManager.Kernel
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class KernelFunctionAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
