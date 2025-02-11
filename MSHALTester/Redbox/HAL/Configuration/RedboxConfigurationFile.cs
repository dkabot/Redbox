using System;
using System.IO;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Configuration;

internal sealed class RedboxConfigurationFile : IConfigurationFile
{
    internal RedboxConfigurationFile()
    {
        FullSourcePath = System.IO.Path.Combine(Path, FileName);
    }

    public SystemConfigurations Type => SystemConfigurations.Redbox;

    public string Path => "c:\\program files\\Redbox\\Halservice\\bin";

    public string FileName => "hal.xml";

    public string FullSourcePath { get; }

    public void ImportFrom(IConfigurationFile config, ErrorList errors)
    {
        throw new NotImplementedException();
    }

    public ConversionResult ConvertTo(KioskConfiguration newconfig, ErrorList errors)
    {
        if (!File.Exists(FullSourcePath))
            return ConversionResult.InvalidFile;
        var xmlDocument = new XmlDocument();
        try
        {
            xmlDocument.Load(FullSourcePath);
        }
        catch (Exception ex)
        {
            errors.Add(Error.NewError("C001", "Invalid XML file", ex.Message));
            return ConversionResult.InvalidFile;
        }

        var xmlNode1 = xmlDocument.DocumentElement.SelectSingleNode("Controller/Decks");
        var lastChild = xmlNode1.LastChild;
        if (lastChild.GetAttributeValue<bool>("IsQlm"))
        {
            var xmlNode2 = (XmlNode)null;
            foreach (XmlNode childNode in xmlNode1.ChildNodes)
                if (7 == childNode.GetAttributeValue<int>("Number"))
                {
                    xmlNode2 = childNode;
                    break;
                }

            var xmlNode3 = xmlNode2.Clone();
            xmlNode3.SetAttributeValue("Number", 8);
            xmlNode3.SetAttributeValue("Offset", lastChild.GetAttributeValue<int>("Offset"));
            xmlNode1.ReplaceChild(xmlNode3, lastChild);
            xmlDocument.Save(FullSourcePath);
        }

        return ConversionResult.Success;
    }
}