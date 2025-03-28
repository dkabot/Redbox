using Redbox.KioskEngine.ComponentModel;
using System.Collections.Generic;

namespace Redbox.KioskEngine.Environment
{
  public class StyleSheet : IStyleSheet
  {
    private readonly IDictionary<string, IStyleSheetStyle> m_styles = (IDictionary<string, IStyleSheetStyle>) new Dictionary<string, IStyleSheetStyle>();

    public void RemoveStyle(string name) => this.m_styles.Remove(name);

    public IStyleSheetStyle AddStyle(string name)
    {
      if (this.m_styles.ContainsKey(name))
        return this.m_styles[name];
      StyleSheetStyle styleSheetStyle = new StyleSheetStyle()
      {
        Name = name
      };
      this.m_styles[name] = (IStyleSheetStyle) styleSheetStyle;
      return (IStyleSheetStyle) styleSheetStyle;
    }

    public IStyleSheetStyle GetStyle(string name)
    {
      return !this.m_styles.ContainsKey(name) ? (IStyleSheetStyle) null : this.m_styles[name];
    }

    public string Name { get; internal set; }
  }
}
