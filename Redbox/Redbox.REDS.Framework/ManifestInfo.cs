using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Redbox.REDS.Framework
{
    internal class ManifestInfo : IManifestInfo
    {
        public ManifestInfo(IResource resource)
        {
            InnerRequires = new List<IBundleSpecifier>();
            BundleType = (BundleType)resource["bundle_type"];
            ProductName = (string)resource["product_name"];
            ProductVersion = (string)resource["product_version"];
            StartupScript = (string)resource["startup_script"];
            TestControl = (string)resource["test_control"];
            ControlAssembly = (string)resource["control_assembly"];
            if (resource["requires"] is string str)
            {
                foreach (var specifier in str.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    InnerRequires.Add(new BundleSpecifier(specifier));
            }
            else
            {
                if (!(resource["requires"] is List<object> objectList))
                    return;
                foreach (var specifier in objectList)
                    InnerRequires.Add(new BundleSpecifier(specifier as string));
            }
        }

        private List<IBundleSpecifier> InnerRequires { get; }

        public string ProductName { get; internal set; }

        public string ProductVersion { get; internal set; }

        public string StartupScript { get; internal set; }

        public string TestControl { get; internal set; }

        public string ControlAssembly { get; internal set; }

        public BundleType BundleType { get; internal set; }

        public ReadOnlyCollection<IBundleSpecifier> Requires => InnerRequires.AsReadOnly();
    }
}