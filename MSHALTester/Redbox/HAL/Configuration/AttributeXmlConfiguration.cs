using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Attributes;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.Configuration;

public abstract class AttributeXmlConfiguration :
    IAttributeXmlConfiguration,
    IConfigurationObserver
{
    private const BindingFlags PropertyFlags = BindingFlags.Instance | BindingFlags.Public;
    private readonly List<IConfigurationObserver> Observers = new();
    private readonly Type ThisType;

    protected AttributeXmlConfiguration(string xmlRoot, Type type)
    {
        RootName = xmlRoot;
        ThisType = type;
        using (var executionTimer = new ExecutionTimer())
        {
            foreach (var property in ThisType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                if (Attribute.GetCustomAttribute(property, typeof(XmlConfigurationAttribute)) is
                    XmlConfigurationAttribute customAttribute)
                    try
                    {
                        var obj = ConversionHelper.ChangeType(customAttribute.DefaultValue, property.PropertyType);
                        property.SetValue(this, obj, null);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log(
                            string.Format(
                                "[AttributeXmlConfiguration] Ctor caught an exception processing property {0}.",
                                property.Name), ex);
                    }

            executionTimer.Stop();
            LogHelper.Instance.Log("[AttributeXmlConfiguration] Time to set defaults on type {0}: {1}ms", type.Name,
                executionTimer.ElapsedMilliseconds);
        }
    }

    protected abstract string FileNodeName { get; }

    public void NotifyConfigurationLoaded()
    {
        foreach (var observer in Observers)
            observer.NotifyConfigurationLoaded();
    }

    public void NotifyConfigurationChangeStart()
    {
        foreach (var observer in Observers)
            observer.NotifyConfigurationChangeStart();
    }

    public void NotifyConfigurationChangeEnd()
    {
        foreach (var observer in Observers)
            observer.NotifyConfigurationChangeEnd();
    }

    public void AddObserver(IConfigurationObserver observer)
    {
        Observers.Add(observer);
    }

    public void RemoveObserver(IConfigurationObserver observer)
    {
        Observers.Remove(observer);
    }

    public void LoadProperties(XmlDocument document, ErrorList errors)
    {
        using (var executionTimer = new ExecutionTimer())
        {
            foreach (XmlNode childNode in document.DocumentElement.SelectSingleNode(FileNodeName).ChildNodes)
            {
                var name = childNode.Name;
                try
                {
                    var property = ThisType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
                    if (property == null)
                    {
                        if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
                            LogHelper.Instance.Log("[AttributeXmlConfiguration] Unable to find property '{0}'", name);
                    }
                    else if (Attribute.GetCustomAttribute(property, typeof(XmlConfigurationAttribute)) is
                             XmlConfigurationAttribute)
                    {
                        var obj = ConversionHelper.ChangeType(childNode.InnerText, property.PropertyType);
                        property.SetValue(this, obj, null);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("[AbstractXmlConfiguration] LoadProperties caught an exception.", ex);
                }
            }

            LogHelper.Instance.Log("[AbstractXmlConfiguration] Time to load defaults on type {0}: {1}ms", ThisType.Name,
                executionTimer.ElapsedMilliseconds);
            executionTimer.Stop();
        }

        LoadPropertiesInner(document, errors);
    }

    public void StoreProperties(XmlDocument document, ErrorList errors)
    {
        using (var executionTimer = new ExecutionTimer())
        {
            foreach (var property in ThisType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                if (Attribute.GetCustomAttribute(property, typeof(XmlConfigurationAttribute)) is
                    XmlConfigurationAttribute)
                    try
                    {
                        var str = property.GetValue(this, null).ToString();
                        SetSingleNodeValue(document.DocumentElement,
                            string.Format("{0}/{1}", FileNodeName, property.Name), str);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log(
                            string.Format(
                                "[AbstractXmlConfiguration] StoreProperties caught an exception processing property {0}",
                                property.Name), ex);
                    }

            executionTimer.Stop();
            LogHelper.Instance.Log("[AbstractXmlConfiguration] Time to save defaults on type {0}: {1}ms", ThisType.Name,
                executionTimer.ElapsedMilliseconds);
        }

        StorePropertiesInner(document, errors);
    }

    public void Import(ErrorList errors)
    {
        ImportInner(errors);
    }

    public void Upgrade(XmlDocument document, ErrorList errors)
    {
        UpgradeInner(document, errors);
    }

    [Browsable(false)] public string RootName { get; }

    protected abstract void ImportInner(ErrorList errors);

    protected abstract void UpgradeInner(XmlDocument document, ErrorList errors);

    protected abstract void LoadPropertiesInner(XmlDocument document, ErrorList errors);

    protected abstract void StorePropertiesInner(XmlDocument document, ErrorList errors);

    private void SetSingleNodeValue(XmlNode node, string path, object value)
    {
        GetTargetNode(node, path).InnerText = value.ToString();
    }

    private T GetNodeValue<T>(XmlNode parent, string singleNode, T defaultValue)
    {
        var xmlNode = parent.SelectSingleNode(singleNode);
        try
        {
            var service = ServiceLocator.Instance.GetService<IRuntimeService>();
            if (xmlNode != null)
                if (!string.IsNullOrEmpty(xmlNode.InnerText))
                    return (T)ConversionHelper.ChangeType(service.ExpandConstantMacros(xmlNode.InnerText), typeof(T));
        }
        catch (ArgumentException ex)
        {
        }

        return defaultValue;
    }

    private XmlNode GetTargetNode(XmlNode node, string path)
    {
        var strArray = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        var targetNode = node;
        foreach (var str in strArray)
        {
            var newChild = targetNode.SelectSingleNode(str);
            if (newChild == null)
            {
                newChild = targetNode.OwnerDocument.CreateElement(str);
                targetNode.AppendChild(newChild);
            }

            targetNode = newChild;
        }

        return targetNode;
    }
}