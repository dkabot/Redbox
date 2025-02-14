using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Redbox.REDS.Framework
{
    public class ResourceType : IResourceType
    {
        private List<IAspectType> m_aspects;
        private List<IPropertyType> m_properties;
        private List<IResourceType> m_subTypes;

        internal List<IAspectType> InnerAspects
        {
            get
            {
                if (m_aspects == null)
                    m_aspects = new List<IAspectType>();
                return m_aspects;
            }
        }

        internal List<IResourceType> InnerSubTypes
        {
            get
            {
                if (m_subTypes == null)
                    m_subTypes = new List<IResourceType>();
                return m_subTypes;
            }
        }

        internal List<IPropertyType> InnerProperties
        {
            get
            {
                if (m_properties == null)
                    m_properties = new List<IPropertyType>();
                return m_properties;
            }
        }

        public IResourceType NewType(string name, Type runtimeType)
        {
            var type = runtimeType;
            if ((object)type == null)
                type = typeof(ResourceType);
            var instance = (ResourceType)Activator.CreateInstance(type);
            instance.SuperType = this;
            instance.Name = name;
            InnerSubTypes.Add(instance);
            return instance;
        }

        public IAspectType AddAspectType(string name, Type aspectType)
        {
            var type = aspectType;
            if ((object)type == null)
                type = typeof(AspectType);
            var instance = (AspectType)Activator.CreateInstance(type);
            instance.Name = name;
            InnerAspects.Add(instance);
            return instance;
        }

        public IPropertyType AddPropertyType(
            string name,
            Type type,
            string description,
            bool isRequired,
            bool isFilter,
            int? filterScore,
            string label,
            string category)
        {
            var propertyType = new PropertyType
            {
                Name = name,
                Type = type,
                Label = label,
                IsFilter = isFilter,
                Category = category,
                IsRequired = isRequired,
                Description = description,
                FilterScore = filterScore ?? 1
            };
            InnerProperties.Add(propertyType);
            return propertyType;
        }

        public void RemoveAspectType(string name)
        {
            var aspectType = GetAspectType(name);
            if (aspectType == null)
                return;
            InnerAspects.Remove(aspectType);
        }

        public void RemovePropertyType(string name)
        {
            var propertyType = GetPropertyType(name);
            if (propertyType == null)
                return;
            InnerProperties.Remove(propertyType);
        }

        public IResource NewResourceInstance(string name)
        {
            return new Resource
            {
                Type = this,
                Properties =
                {
                    [nameof(name)] = name,
                    ["created_on"] = DateTime.UtcNow,
                    ["modified_on"] = DateTime.UtcNow
                }
            };
        }

        public IAspectType GetAspectType(string name)
        {
            for (var resourceType = this; resourceType != null; resourceType = (ResourceType)resourceType.SuperType)
            {
                var aspectType = resourceType.InnerAspects.Find(each => each.Name == name);
                if (aspectType != null)
                    return aspectType;
            }

            return null;
        }

        public IPropertyType GetPropertyType(string name)
        {
            for (var resourceType = this; resourceType != null; resourceType = (ResourceType)resourceType.SuperType)
            {
                var propertyType = resourceType.InnerProperties.Find(each => each.Name == name);
                if (propertyType != null)
                    return propertyType;
            }

            return null;
        }

        public ReadOnlyCollection<IPropertyType> GetFilterProperties()
        {
            var propertyTypeList = new List<IPropertyType>();
            for (var resourceType = this; resourceType != null; resourceType = (ResourceType)resourceType.SuperType)
                propertyTypeList.AddRange(resourceType.InnerProperties.FindAll(each => each.IsFilter));
            return propertyTypeList.AsReadOnly();
        }

        public string Name { get; internal set; }

        public IResourceType SuperType { get; internal set; }

        public ReadOnlyCollection<IResourceType> SubTypes => InnerSubTypes.AsReadOnly();

        public ReadOnlyCollection<IAspectType> AspectTypes => InnerAspects.AsReadOnly();

        public ReadOnlyCollection<IPropertyType> Properties => InnerProperties.AsReadOnly();
    }
}