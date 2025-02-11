using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;

namespace Redbox.Core
{
    internal class XmlFileEditor
    {
        private List<XmlEdit> m_edits;
        private List<XmlNamespace> m_namespaces;

        public XmlFileEditor(string fileName) => this.FileName = fileName;

        public ReadOnlyCollection<XmlEditorError> Apply()
        {
            List<XmlEditorError> xmlEditorErrorList = new List<XmlEditorError>();
            try
            {
                if (!File.Exists(this.FileName))
                {
                    xmlEditorErrorList.Add(new XmlEditorError()
                    {
                        Code = "I006",
                        Description = "Unable to edit XML file '" + this.FileName + "'; it does not exist.",
                        Details = "Correct the setup script to specify a valid file path."
                    });
                    return xmlEditorErrorList.AsReadOnly();
                }
                NameTable nameTable = new NameTable();
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager((XmlNameTable)nameTable);
                foreach (XmlNamespace xmlNamespace in this.Namespaces)
                    namespaceManager.AddNamespace(xmlNamespace.Prefix, xmlNamespace.Uri);
                XmlDocument xmlDocument = new XmlDocument((XmlNameTable)nameTable);
                try
                {
                    xmlDocument.Load(this.FileName);
                }
                catch (Exception ex)
                {
                    xmlEditorErrorList.Add(new XmlEditorError()
                    {
                        Code = "I015",
                        Description = "Unable to parse XML file '" + this.FileName + "'; see details for more information.",
                        Details = ex.ToString()
                    });
                    return xmlEditorErrorList.AsReadOnly();
                }
                if (xmlDocument.DocumentElement == null)
                    return xmlEditorErrorList.AsReadOnly();
                foreach (XmlEdit edit in this.Edits)
                {
                    XmlNodeList xmlNodeList1 = xmlDocument.DocumentElement.SelectNodes(edit.XPath);
                    if (xmlNodeList1 != null && xmlNodeList1.Count > 0)
                    {
                        List<XmlNode> xmlNodeList2 = new List<XmlNode>();
                        if (this.EditAllMatches)
                        {
                            foreach (XmlNode xmlNode in xmlNodeList1)
                                xmlNodeList2.Add(xmlNode);
                        }
                        else
                            xmlNodeList2.Add(xmlNodeList1[0]);
                        foreach (XmlNode xmlNode in xmlNodeList2)
                        {
                            if (xmlNode != null)
                            {
                                switch (xmlNode.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        xmlNode.InnerText = edit.Value;
                                        continue;
                                    case XmlNodeType.Attribute:
                                        xmlNode.Value = edit.Value;
                                        continue;
                                    default:
                                        xmlEditorErrorList.Add(new XmlEditorError()
                                        {
                                            Code = "I015",
                                            Description = string.Format("XPath expression '{0}' yielded an XML node type RIX cannot process.", (object)edit.XPath),
                                            Details = "Review your XPath expression, correct, and recompile your setup.  RIX supports editing XML attributes and element nodes."
                                        });
                                        continue;
                                }
                            }
                        }
                    }
                    else if (this.RaiseErrors)
                        xmlEditorErrorList.Add(new XmlEditorError()
                        {
                            Code = "I015",
                            Description = string.Format("XPath expression '{0}' did not yield a valid node(s).", (object)edit.XPath),
                            Details = "Review your XPath expression, correct, and recompile your setup."
                        });
                }
                XmlTextWriter w = new XmlTextWriter(this.FileName, (Encoding)null)
                {
                    Formatting = Formatting.Indented
                };
                xmlDocument.Save((XmlWriter)w);
            }
            catch (Exception ex)
            {
                xmlEditorErrorList.Add(new XmlEditorError()
                {
                    Code = "I015",
                    Description = string.Format("Unable to edit XML file {0}; see details for exception.", (object)this.FileName),
                    Details = ex.ToString()
                });
            }
            return xmlEditorErrorList.AsReadOnly();
        }

        public string FileName { get; internal set; }

        public bool RaiseErrors { get; set; }

        public List<XmlEdit> Edits
        {
            get
            {
                if (this.m_edits == null)
                    this.m_edits = new List<XmlEdit>();
                return this.m_edits;
            }
        }

        public bool EditAllMatches { get; set; }

        public List<XmlNamespace> Namespaces
        {
            get
            {
                if (this.m_namespaces == null)
                    this.m_namespaces = new List<XmlNamespace>();
                return this.m_namespaces;
            }
        }
    }
}
