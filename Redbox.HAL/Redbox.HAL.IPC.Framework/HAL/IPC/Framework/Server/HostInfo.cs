using System;
using System.Reflection;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework.Server
{
    internal sealed class HostInfo : IHostInfo
    {
        internal HostInfo(Assembly a)
        {
            Product = GetProductName(a);
            Version = GetVersion(a);
            Copyright = GetProductName(a);
        }

        public string Product { get; }

        public string Version { get; }

        public string Copyright { get; }

        private string GetProductName(Assembly assembly)
        {
            return ((AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute)))
                ?.Product;
        }

        private string GetCopyright(Assembly assembly)
        {
            return ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly,
                typeof(AssemblyCopyrightAttribute)))?.Copyright;
        }

        private string GetVersion(Assembly assembly)
        {
            return assembly.GetName().Version.ToString();
        }
    }
}