using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Redbox.Core;

namespace Redbox.REDS.Framework
{
    public class ManifestAspectType : AspectType
    {
        protected override void OnParseAspect(
            IResourceBundle bundle,
            IAspect aspect,
            XmlNode aspectNode,
            ErrorList errors)
        {
            if (aspect.Name != "schema")
                return;
            var xmlNodeList1 = aspectNode.SelectNodes("resourceType");
            if (xmlNodeList1 == null)
                return;
            foreach (XmlNode node1 in xmlNodeList1)
            {
                var attributeValue1 = node1.GetAttributeValue<string>("name");
                if (string.IsNullOrEmpty(attributeValue1))
                {
                    errors.Add(Error.NewError("R005", "The name attribute must be defined on the aspect element.",
                        "Add a valid name attribute to the aspect element."));
                    break;
                }

                var resourceType1 = (IResourceType)null;
                var attributeValue2 = node1.GetAttributeValue<string>("superType");
                if (!string.IsNullOrEmpty(attributeValue2))
                {
                    if (!bundle.ResourceTypes.ContainsKey(attributeValue2))
                    {
                        errors.Add(Error.NewError("R003",
                            string.Format("The type '{0}' is not defined.", attributeValue2),
                            "Specify a valid resource type in the resource XML and recompile."));
                        break;
                    }

                    resourceType1 = bundle.ResourceTypes[attributeValue2];
                }

                var attributeValue3 = node1.GetAttributeValue<string>("runtimeType");
                var runtimeType = (Type)null;
                if (!string.IsNullOrEmpty(attributeValue3))
                {
                    runtimeType = GetType(attributeValue3);
                    if (runtimeType == null)
                        errors.Add(Error.NewWarning("R004",
                            string.Format("The runtime type '{0}' is not defined in any loaded assembly.",
                                attributeValue3),
                            "Specify a valid runtime type and ensure the appropriate assemblies are loaded."));
                }

                var resourceType2 = resourceType1 == null
                    ? bundle.ResourceTypes["root"]
                    : resourceType1.NewType(attributeValue1, runtimeType);
                var xmlNodeList2 = node1.SelectNodes("propertyType");
                if (xmlNodeList2 != null)
                    foreach (XmlNode node2 in xmlNodeList2)
                    {
                        var type1 = GetType(node2.GetAttributeValue<string>("type"));
                        if ((object)type1 == null)
                            type1 = typeof(string);
                        var type2 = type1;
                        resourceType2.AddPropertyType(node2.GetAttributeValue<string>("name"), type2,
                                node2.GetAttributeValue<string>("description"),
                                node2.GetAttributeValue<bool>("isRequired"),
                                node2.GetAttributeValue<bool>("isFilter"), node2.GetAttributeValue<int?>("score"),
                                node2.GetAttributeValue<string>("label"), node2.GetAttributeValue<string>("category"))
                            .SetDefaultValue(node2.GetAttributeValue<string>("defaultValue"));
                    }

                var xmlNodeList3 = node1.SelectNodes("aspectType");
                if (xmlNodeList3 != null)
                    foreach (XmlNode node3 in xmlNodeList3)
                    {
                        var aspectType = resourceType2.AddAspectType(node3.GetAttributeValue<string>("name"),
                            GetType(node3.GetAttributeValue<string>("type")));
                        var xmlNodeList4 = node3.SelectNodes("property");
                        if (xmlNodeList4 != null)
                            foreach (XmlNode node4 in xmlNodeList4)
                                aspectType[node4.GetAttributeValue<string>("name")] =
                                    node4.GetAttributeValue<string>("value");
                    }

                if (resourceType2.Name != "root")
                    bundle.ResourceTypes[attributeValue1] = resourceType2;
            }
        }

        private static Type GetType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;
            if (typeName.StartsWith("System.Windows.Forms"))
                return typeof(Control).Assembly.GetType(typeName);
            if (typeName.StartsWith("System.Drawing"))
                return typeof(FontStyle).Assembly.GetType(typeName);
            return typeName.StartsWith("Redbox.REDS.Framework")
                ? typeof(ManifestAspectType).Assembly.GetType(typeName)
                : typeName.ToType();
        }
    }
}