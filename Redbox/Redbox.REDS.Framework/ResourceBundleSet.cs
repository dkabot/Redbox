using System.Collections.Generic;
using System.Collections.ObjectModel;
using Wintellect.PowerCollections;

namespace Redbox.REDS.Framework
{
    public class ResourceBundleSet : IResourceBundleSet
    {
        private readonly IResourceBundle m_applicationBundle;
        private readonly string m_name;
        private BundleCollection m_bundles;
        private MultiDictionary<string, IResource> m_resources;

        public ResourceBundleSet(
            string name,
            IResourceBundle applicationBundle,
            IEnumerable<IResourceBundle> otherBundles)
        {
            m_name = name;
            m_applicationBundle = applicationBundle;
            var resourceBundleList = new List<IResourceBundle>();
            resourceBundleList.Add(applicationBundle);
            resourceBundleList.AddRange(otherBundles);
            foreach (var bundle in resourceBundleList)
            {
                Bundles.AddBundleVersion(bundle);
                foreach (var resource in (CollectionBase<KeyValuePair<string, ICollection<IResource>>>)
                         ((ResourceBundle)bundle).Resources)
                    Resources.Add(resource);
            }
        }

        internal BundleCollection Bundles
        {
            get
            {
                if (m_bundles == null)
                    m_bundles = new BundleCollection();
                return m_bundles;
            }
        }

        internal MultiDictionary<string, IResource> Resources
        {
            get
            {
                if (m_resources == null)
                    m_resources = new MultiDictionary<string, IResource>(true);
                return m_resources;
            }
        }

        public ReadOnlyCollection<IResource> GetResources(
            string typeName,
            IResourceBundleFilter filter)
        {
            var resourceList = new List<IResource>();
            foreach (var bundle in Bundles.GetBundles())
            foreach (var bundleVersion in Bundles.GetBundleVersions(bundle))
                resourceList.AddRange(bundleVersion.Instance.GetResources(typeName, filter));
            return resourceList.AsReadOnly();
        }

        public ReadOnlyCollection<IBundleSpecifier> GetBundles()
        {
            var bundleSpecifierList = new List<IBundleSpecifier>();
            foreach (var allBundleVersion in Bundles.GetAllBundleVersions())
                bundleSpecifierList.Add(allBundleVersion);
            return bundleSpecifierList.AsReadOnly();
        }

        public ReadOnlyCollection<IResource> GetResourcesUnfiltered(
            string typeName)
        {
            var resourceList = new List<IResource>();
            foreach (var bundle in Bundles.GetBundles())
            foreach (var bundleVersion in Bundles.GetBundleVersions(bundle))
                resourceList.AddRange(bundleVersion.Instance.GetAllResources(typeName));
            return resourceList.AsReadOnly();
        }

        public IResource GetResource(string name, IResourceBundleFilter filter)
        {
            return ResourceLookup.GetResource2(Resources, name, filter);
        }

        public IDictionary<string, IResourceType> GetResourceTypes()
        {
            var resourceTypes = new Dictionary<string, IResourceType>();
            foreach (var bundle in Bundles.GetBundles())
            foreach (var bundleVersion in Bundles.GetBundleVersions(bundle))
            foreach (var resourceType in bundleVersion.Instance.ResourceTypes)
                resourceTypes[resourceType.Key] = resourceType.Value;

            return resourceTypes;
        }

        public IResourceBundleFilter CreateFilter()
        {
            return m_applicationBundle != null ? m_applicationBundle.CreateFilter() : null;
        }

        public override string ToString()
        {
            return m_name;
        }
    }
}