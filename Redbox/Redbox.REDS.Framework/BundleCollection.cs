using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;

namespace Redbox.REDS.Framework
{
    public sealed class BundleCollection
    {
        private readonly OrderedMultiDictionary<string, IBundleSpecifier> m_bundles =
            new OrderedMultiDictionary<string, IBundleSpecifier>(false, (lhs, rhs) => string.Compare(lhs, rhs, true),
                (lhs, rhs) => lhs.Version.CompareTo(rhs.Version));

        public int Count => m_bundles.Count;

        public void Clear()
        {
            foreach (var bundle in (CollectionBase<KeyValuePair<string, ICollection<IBundleSpecifier>>>)m_bundles)
            foreach (var bundleSpecifier in bundle.Value)
                if (bundleSpecifier.Instance != null)
                    bundleSpecifier.Instance.Dispose();

            m_bundles.Clear();
        }

        public ErrorList AddBundleVersion(IResourceBundle bundle)
        {
            var errorList = new ErrorList();
            if (bundle == null)
            {
                errorList.Add(Error.NewError("R901", "The bundle parameter cannot be null.",
                    "Ensure a valid bundle instance is passed to the BundleCollection.AddBundle method."));
                return errorList;
            }

            IManifestInfo manifestInfo;
            if (bundle.GetManifest(bundle.CreateFilter(), out manifestInfo) != null)
                if (manifestInfo != null)
                    try
                    {
                        m_bundles.Add(manifestInfo.ProductName,
                            new BundleSpecifier(manifestInfo.ProductName, manifestInfo.ProductVersion,
                                manifestInfo.BundleType, bundle));
                    }
                    catch (Exception ex)
                    {
                        errorList.Add(Error.NewError("R999",
                            "An unhandled exception was raised in BundleCollection.AddBundle.", ex));
                    }

            return errorList;
        }

        public ErrorList RemoveBundleVersion(IResourceBundle bundle)
        {
            return new ErrorList();
        }

        public void RemoveAllVersions(string productName)
        {
            m_bundles.Remove(productName);
        }

        public ErrorList GetBundle(
            string productName,
            string productVersion,
            out IResourceBundle bundle,
            out IManifestInfo manifestInfo)
        {
            Console.WriteLine("REDS.BundleCollection.GetBundle(" + productName + ", " + productVersion + ")");
            bundle = null;
            manifestInfo = null;
            var bundle1 = new ErrorList();
            var bundle2 = m_bundles[productName];
            if (bundle2 == null || bundle2.Count == 0)
            {
                Console.WriteLine("> found NO bundles named " + productName);
                bundle1.Add(Error.NewError("B125", string.Format("Found NO bundles named: {0}", productName),
                    "Check the bundles folder under the Kiosk Engine folder and ensure the bundle is actually installed."));
                return bundle1;
            }

            var bundleSpecifierList = new List<IBundleSpecifier>(bundle2);
            Console.WriteLine("> found [" + bundleSpecifierList.Count + "] bundles named " + productName);
            var targetVersion = new BundleSpecifier(string.Format("{0},{1}", productName, productVersion));
            var bundleSpecifier =
                bundleSpecifierList.Find(wv => wv.Name == targetVersion.Name && wv.Version == targetVersion.Version);
            if (bundleSpecifier != null)
            {
                bundle = bundleSpecifier.Instance;
            }
            else
            {
                bundleSpecifierList.Add(targetVersion);
                bundleSpecifierList.Sort((lhs, rhs) => lhs.Version.CompareTo(rhs.Version));
                var num = bundleSpecifierList.IndexOf(targetVersion);
                bundle = num == 0 ? null : bundleSpecifierList[num - 1].Instance;
            }

            if (bundle != null)
            {
                if (bundle.GetManifest(bundle.CreateFilter(), out manifestInfo) == null || manifestInfo == null)
                    bundle1.Add(Error.NewError("B120",
                        string.Format("No manifest resource was found for the bundle: {0}.", bundle.Storage.BundlePath),
                        "The bundle may not be compiled correctly or the manifest.resource file was moved or deleted after the bundle was scanned."));
            }
            else
            {
                bundle1.Add(Error.NewError("B130",
                    string.Format("The requested bundle was not found: {0},{1}", productName, productVersion),
                    "Check the bundles folder under the Kiosk Engine folder and ensure the bundle is actually installed."));
            }

            return bundle1;
        }

        public IEnumerable<string> GetBundles()
        {
            return m_bundles.Keys;
        }

        public IEnumerable<IBundleSpecifier> GetAllBundleVersions()
        {
            return m_bundles.Values;
        }

        public IEnumerable<IBundleSpecifier> GetBundleVersions(string productName)
        {
            return m_bundles[productName];
        }
    }
}