using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace Redbox.Core
{
    public class XmlFileEditor
    {
        private List<XmlEdit> m_edits;
        private List<XmlNamespace> m_namespaces;

        public XmlFileEditor(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; internal set; }

        public bool RaiseErrors { get; set; }

        public List<XmlEdit> Edits
        {
            get
            {
                if (m_edits == null)
                    m_edits = new List<XmlEdit>();
                return m_edits;
            }
        }

        public bool EditAllMatches { get; set; }

        public List<XmlNamespace> Namespaces
        {
            get
            {
                if (m_namespaces == null)
                    m_namespaces = new List<XmlNamespace>();
                return m_namespaces;
            }
        }

        public ReadOnlyCollection<XmlEditorError> Apply()
        {
            var xmlEditorErrorList = new List<XmlEditorError>();
            try
            {
                if (!File.Exists(FileName))
                {
                    xmlEditorErrorList.Add(new XmlEditorError
                    {
                        Code = "I006",
                        Description = "Unable to edit XML file '" + FileName + "'; it does not exist.",
                        Details = "Correct the setup script to specify a valid file path."
                    });
                    return xmlEditorErrorList.AsReadOnly();
                }

                var nameTable = new NameTable();
                var namespaceManager = new XmlNamespaceManager(nameTable);
                foreach (var xmlNamespace in Namespaces)
                    namespaceManager.AddNamespace(xmlNamespace.Prefix, xmlNamespace.Uri);
                var xmlDocument = new XmlDocument(nameTable);
                try
                {
                    xmlDocument.Load(FileName);
                }
                catch (Exception ex)
                {
                    xmlEditorErrorList.Add(new XmlEditorError
                    {
                        Code = "I015",
                        Description = "Unable to parse XML file '" + FileName + "'; see details for more information.",
                        Details = ex.ToString()
                    });
                    return xmlEditorErrorList.AsReadOnly();
                }

                if (xmlDocument.DocumentElement == null)
                    return xmlEditorErrorList.AsReadOnly();
                foreach (var edit in Edits)
                {
                    var xmlNodeList1 = xmlDocument.DocumentElement.SelectNodes(edit.XPath);
                    if (xmlNodeList1 != null && xmlNodeList1.Count > 0)
                    {
                        var xmlNodeList2 = new List<XmlNode>();
                        if (EditAllMatches)
                            foreach (XmlNode xmlNode in xmlNodeList1)
                                xmlNodeList2.Add(xmlNode);
                        else
                            xmlNodeList2.Add(xmlNodeList1[0]);
                        foreach (var xmlNode in xmlNodeList2)
                            if (xmlNode != null)
                                switch (xmlNode.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        xmlNode.InnerText = edit.Value;
                                        continue;
                                    case XmlNodeType.Attribute:
                                        xmlNode.Value = edit.Value;
                                        continue;
                                    default:
                                        xmlEditorErrorList.Add(new XmlEditorError
                                        {
                                            Code = "I015",
                                            Description =
                                                string.Format(
                                                    "XPath expression '{0}' yielded an XML node type RIX cannot process.",
                                                    edit.XPath),
                                            Details =
                                                "Review your XPath expression, correct, and recompile your setup.  RIX supports editing XML attributes and element nodes."
                                        });
                                        continue;
                                }
                    }
                    else if (RaiseErrors)
                    {
                        xmlEditorErrorList.Add(new XmlEditorError
                        {
                            Code = "I015",
                            Description = string.Format("XPath expression '{0}' did not yield a valid node(s).",
                                edit.XPath),
                            Details = "Review your XPath expression, correct, and recompile your setup."
                        });
                    }
                }

                var w = new XmlTextWriter(FileName, null)
                {
                    Formatting = Formatting.Indented
                };
                xmlDocument.Save(w);
            }
            catch (Exception ex)
            {
                xmlEditorErrorList.Add(new XmlEditorError
                {
                    Code = "I015",
                    Description = string.Format("Unable to edit XML file {0}; see details for exception.", FileName),
                    Details = ex.ToString()
                });
            }

            return xmlEditorErrorList.AsReadOnly();
        }
    }
}