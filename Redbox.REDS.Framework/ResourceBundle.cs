using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using System.Xml;
using Wintellect.PowerCollections;

namespace Redbox.REDS.Framework
{
    public class ResourceBundle : IResourceBundle, IDisposable
    {
        private MultiDictionary<string, IResource> m_resources;
        private IDictionary<string, IResourceType> m_resourceTypes;

        public ResourceBundle()
        {
        }

        public ResourceBundle(string fileName, ResourceBundleStorageType storageType)
        {
            switch (storageType)
            {
                case ResourceBundleStorageType.Zipped:
                    Storage = new ZippedResourceBundleStorage();
                    break;
                case ResourceBundleStorageType.FileSystem:
                    Storage = new FileSystemResourceBundleStorage();
                    break;
            }

            Storage.Open(fileName);
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

        public void Dispose()
        {
            Storage.Dispose();
        }

        public ErrorList Activate(IResourceBundleFilter filter, out IManifestInfo manifestInfo)
        {
            var errorList = new ErrorList();
            if (GetManifest(filter, out manifestInfo) == null)
            {
                errorList.Add(Error.NewError("E003",
                    string.Format("Resource bundle '{0}' does not contain: manifest.resource.", Storage.BundlePath),
                    "The Redbox Engine for Distributed Stores requires one valid bundle to execute."));
                return errorList;
            }

            if (manifestInfo.BundleType == BundleType.Application)
            {
                if (string.IsNullOrEmpty(manifestInfo.StartupScript))
                {
                    errorList.Add(Error.NewError("E004",
                        string.Format(
                            "Application resource bundle '{0}' must specify a valid startup_script in the manifest.resource.",
                            Storage.BundlePath),
                        "The Redbox Engine for Distributed Stores requires one valid startup script to execute."));
                    return errorList;
                }

                if (GetResource(manifestInfo.StartupScript, filter) == null)
                    errorList.Add(Error.NewError("E004",
                        string.Format("Resource bundle '{0}' does not contain: {1}.resource.", Storage.BundlePath,
                            manifestInfo.StartupScript),
                        "The resource specified in the startup_script property does not exist within the bundle.  Specify a valid script resource."));
                return errorList;
            }

            if (manifestInfo.BundleType != BundleType.HTMLApplication)
                return errorList;
            if (string.IsNullOrEmpty(manifestInfo.ControlAssembly))
            {
                errorList.Add(Error.NewError("E005",
                    string.Format(
                        "HTML Application resource bundle '{0}' must specify a valid control_assembly in the manifest.resource.",
                        Storage.BundlePath),
                    "The Redbox Engine for Distributed Stores requires a controller assembly to execute."));
                return errorList;
            }

            if (GetResource(manifestInfo.ControlAssembly, filter) == null)
                errorList.Add(Error.NewError("E005",
                    string.Format("Resource bundle '{0}' does not contain: {1}.resource.", Storage.BundlePath,
                        manifestInfo.ControlAssembly),
                    "The resource specified in the control_assembly property does not exist within the bundle.  Specify a valid control assembly resource."));
            return errorList;
        }

        public IResourceBundleStorageEntry GetEntry(string path)
        {
            return Storage.GetEntry(path);
        }

        public IResource GetManifest(IResourceBundleFilter filter, out IManifestInfo manifestInfo)
        {
            manifestInfo = null;
            var resource = GetResource("manifest", filter);
            if (resource == null)
                return null;
            manifestInfo = new ManifestInfo(resource);
            return resource;
        }

        public IResource GetResource(string name, IResourceBundleFilter filter)
        {
            return ResourceLookup.GetResource2(Resources, name, filter);
        }

        public ICollection<IResource> GetResources(string name)
        {
            return Resources[name];
        }

        public List<IResource> GetResources(string typeName, IResourceBundleFilter filter)
        {
            var resources = new List<IResource>();
            if (string.IsNullOrEmpty(typeName) || !ResourceTypes.ContainsKey(typeName))
                return resources;
            var resourceType = ResourceTypes[typeName];
            foreach (var key in Resources.Keys)
            {
                var resource = GetResource(key, filter);
                if (resource != null && resource.Type == resourceType)
                    resources.Add(resource);
            }

            return resources;
        }

        public List<IResource> GetAllResources()
        {
            var allResources = new List<IResource>();
            foreach (var key in Resources.Keys)
                allResources.AddRange(Resources[key]);
            return allResources;
        }

        public List<IResource> GetAllResources(string typeName)
        {
            var allResources = new List<IResource>();
            if (string.IsNullOrEmpty(typeName) || !ResourceTypes.ContainsKey(typeName))
                return allResources;
            var resourceType = ResourceTypes[typeName];
            foreach (var key in Resources.Keys)
            foreach (var resource in Resources[key])
                if (resource.Type == resourceType)
                    allResources.Add(resource);

            return allResources;
        }

        public ErrorList StoreResource(string name, string data)
        {
            var errorList = new ErrorList();
            if (Storage.SupportsSave)
                return errorList;
            errorList.Add(Error.NewError("B003", "The current bundle storage provider does not support saving.",
                "Access the bundle using a storage provider that supports saving."));
            return errorList;
        }

        public IResourceBundleFilter CreateFilter()
        {
            return new ResourceBundleFilter();
        }

        public ICollection<string> GetLoadedResources()
        {
            return Resources.Keys;
        }

        public IDictionary<string, IResourceType> ResourceTypes
        {
            get
            {
                if (m_resourceTypes == null)
                    m_resourceTypes = new Dictionary<string, IResourceType>();
                return m_resourceTypes;
            }
        }

        public IResourceBundleStorage Storage { get; internal set; }

        public static ResourceBundle Create(
            string fileName,
            string trimPrefix,
            ResourceBundleStorageType storageType)
        {
            try
            {
                var resourceBundle = new ResourceBundle();
                resourceBundle.Storage = new ZippedResourceBundleStorage();
                resourceBundle.Storage.Create(fileName, trimPrefix);
                return resourceBundle;
            }
            catch
            {
                return null;
            }
        }

        public void AddEntry(string path)
        {
            Storage.AddEntry(path, null);
        }

        public void AddEntry(string path, string trimPrefix)
        {
            Storage.AddEntry(path, trimPrefix);
        }

        public void BeginUpdate()
        {
            Storage.BeginUpdate();
        }

        public void CommitUpdate()
        {
            Storage.CommitUpdate();
        }

        public ErrorList LoadResources()
        {
            var errorList = new ErrorList();
            InitializeTypes();
            errorList.AddRange(ParseResource(Storage.GetEntry("manifest.resource") ??
                                             throw new MissingManifestResourceException(
                                                 "The manifest.resource is required.")));
            foreach (var storageEntry in Storage.GetResourceManifest())
                if (storageEntry.Name.EndsWith(".resource", StringComparison.InvariantCulture) &&
                    string.Compare("manifest.resource", storageEntry.Name, true) != 0)
                    errorList.AddRange(ParseResource(storageEntry));
            return errorList;
        }

        internal ErrorList ParseResource(IResourceBundleStorageEntry storageEntry)
        {
            var resource1 = new ErrorList();
            try
            {
                using (var inStream = new MemoryStream(Storage.GetEntryContent(storageEntry)))
                {
                    var document = new XmlDocument();
                    document.Load(inStream);
                    IResource resource2;
                    resource1.AddRange(Resource.FromXml(this, document, out resource2));
                    if (!resource1.ContainsError())
                    {
                        ((Resource)resource2).StorageEntry = storageEntry;
                        Resources.Add(resource2.Name, resource2);
                    }

                    return resource1;
                }
            }
            catch (Exception ex)
            {
                resource1.Add(Error.NewError("R999",
                    "An unhandled exception was raised in ResourceBundle.ParseResource.", ex));
            }

            return resource1;
        }

        private void InitializeTypes()
        {
            var resourceType1 = new ResourceType
            {
                Name = "root"
            };
            resourceType1.InnerAspects.Add(new AspectType
            {
                Name = "content"
            });
            ResourceTypes["root"] = resourceType1;
            var resourceType2 = new ResourceType
            {
                Name = "manifest"
            };
            resourceType2.InnerProperties.Add(new PropertyType
            {
                Name = "requires",
                Type = typeof(string)
            });
            resourceType2.InnerProperties.Add(new PropertyType
            {
                Name = "public_key",
                Type = typeof(string)
            });
            resourceType2.InnerProperties.Add(new PropertyType
            {
                Name = "startup_script",
                Type = typeof(string),
                IsRequired = false
            });
            resourceType2.InnerProperties.Add(new PropertyType
            {
                Name = "name",
                Type = typeof(string),
                IsRequired = true
            });
            resourceType2.InnerProperties.Add(new PropertyType
            {
                Name = "product_name",
                Type = typeof(string),
                IsRequired = true
            });
            resourceType2.InnerProperties.Add(new PropertyType
            {
                Name = "product_version",
                Type = typeof(string),
                IsRequired = true
            });
            resourceType2.InnerProperties.Add(new PropertyType
            {
                Name = "bundle_type",
                Type = typeof(BundleType?),
                DefaultValue = BundleType.Application
            });
            resourceType2.InnerProperties.Add(new PropertyType
            {
                Name = "test_control",
                Type = typeof(string),
                IsRequired = false
            });
            resourceType2.InnerProperties.Add(new PropertyType
            {
                Name = "control_assembly",
                Type = typeof(string),
                IsRequired = false
            });
            var innerAspects = resourceType2.InnerAspects;
            var manifestAspectType = new ManifestAspectType();
            manifestAspectType.Name = "schema";
            innerAspects.Add(manifestAspectType);
            ResourceTypes["manifest"] = resourceType2;
            Resources.Clear();
        }
    }
}