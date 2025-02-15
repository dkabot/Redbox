using System;

namespace Redbox.REDS.Framework
{
    public class BundleSpecifier : IBundleSpecifier
    {
        public BundleSpecifier(string specifier)
        {
            var strArray = !string.IsNullOrEmpty(specifier)
                ? specifier.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                : throw new ArgumentException("specifier cannot be null.");
            Name = strArray.Length >= 2
                ? strArray[0]
                : throw new ArgumentException("specifier must be in the format: product,version");
            Version = new Version(strArray[1] == "*" ? "999999.999999" : strArray[1].Replace("*", "999999"));
        }

        public BundleSpecifier(
            string productName,
            string productVersion,
            BundleType type,
            IResourceBundle bundle)
            : this(string.Format("{0},{1}", productName, productVersion))
        {
            Instance = bundle;
            Type = type;
        }

        public string Name { get; internal set; }

        public Version Version { get; internal set; }

        public BundleType Type { get; internal set; }

        public IResourceBundle Instance { get; internal set; }

        public override string ToString()
        {
            return string.Format("Bundle Specifier: Product={0}, Version={1}", Name, Version);
        }
    }
}