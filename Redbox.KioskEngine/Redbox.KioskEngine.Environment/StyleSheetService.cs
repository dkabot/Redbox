using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

namespace Redbox.KioskEngine.Environment
{
  public class StyleSheetService : IStyleSheetService
  {
    private readonly IDictionary<string, IStyleSheet> m_stylesheets = (IDictionary<string, IStyleSheet>) new Dictionary<string, IStyleSheet>();

    public static IStyleSheetService Instance
    {
      get => (IStyleSheetService) Singleton<StyleSheetService>.Instance;
    }

    public void Reset()
    {
      LogHelper.Instance.Log("Style sheet service reset.");
      this.m_stylesheets.Clear();
    }

    public IStyleSheet New(string name)
    {
      IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
      if (service == null)
        return (IStyleSheet) null;
      if (this.m_stylesheets.ContainsKey(name))
      {
        LogHelper.Instance.Log("Stylesheet resource '{0}' is in cache.", (object) name);
        return this.m_stylesheets[name];
      }
      LogHelper.Instance.Log("Stylesheet resource '{0}' is not in cache; build cache entry.", (object) name);
      XmlNode xml = service.GetXml(name);
      if (xml == null)
        return (IStyleSheet) null;
      StyleSheet styleSheet = new StyleSheet()
      {
        Name = name
      };
      this.m_stylesheets[name] = (IStyleSheet) styleSheet;
      XmlNodeList xmlNodeList1 = xml.SelectNodes("style");
      if (xmlNodeList1 != null)
      {
        foreach (XmlNode node1 in xmlNodeList1)
        {
          string attributeValue1 = node1.GetAttributeValue<string>(nameof (name));
          if (!string.IsNullOrEmpty(attributeValue1))
          {
            if (styleSheet.GetStyle(attributeValue1) != null)
            {
              LogHelper.Instance.Log("WARNING: Duplicate style '{0}' defined in stylesheet resource: {1}.", (object) attributeValue1, (object) name);
            }
            else
            {
              XmlNodeList xmlNodeList2 = node1.SelectNodes("state");
              if (xmlNodeList2 != null)
              {
                IStyleSheetStyle styleSheetStyle = styleSheet.AddStyle(attributeValue1);
                foreach (XmlNode node2 in xmlNodeList2)
                {
                  string attributeValue2 = node2.GetAttributeValue<string>(nameof (name));
                  if (!string.IsNullOrEmpty(attributeValue2))
                  {
                    if (styleSheetStyle.GetState(attributeValue2) != null)
                    {
                      LogHelper.Instance.Log("WARNING: Duplicate style state '{0}' defined in style '{1}' of resource: {2}.", (object) attributeValue2, (object) attributeValue1, (object) name);
                    }
                    else
                    {
                      XmlNodeList xmlNodeList3 = node2.SelectNodes("property");
                      if (xmlNodeList3 != null)
                      {
                        IStyleSheetState styleSheetState = styleSheetStyle.AddState(attributeValue2);
                        foreach (XmlNode node3 in xmlNodeList3)
                        {
                          string attributeValue3 = node3.GetAttributeValue<string>(nameof (name));
                          if (!string.IsNullOrEmpty(attributeValue3))
                            ((StyleSheetState) styleSheetState)[attributeValue3] = node3.GetAttributeValue<object>("value");
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
      return (IStyleSheet) styleSheet;
    }

    public IStyleSheet GetStyleSheet(string name)
    {
      return !this.m_stylesheets.ContainsKey(name) ? (IStyleSheet) null : this.m_stylesheets[name];
    }

    public ReadOnlyCollection<IStyleSheet> StyleSheets
    {
      get
      {
        return new List<IStyleSheet>((IEnumerable<IStyleSheet>) this.m_stylesheets.Values).AsReadOnly();
      }
    }

    private StyleSheetService()
    {
    }
  }
}
