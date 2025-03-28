using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;

namespace Redbox.KioskEngine.Environment
{
  public class FontCacheService : IFontCacheService, IDisposable
  {
    private IDictionary<string, Font> m_fonts;
    private PrivateFontCollection m_privateFontCollection;

    public static FontCacheService Instance => Singleton<FontCacheService>.Instance;

    public Font GetFont(string name)
    {
      return !this.Fonts.ContainsKey(name) ? (Font) null : this.Fonts[name];
    }

    public Font RegisterFont(string name, string familyName, float size, FontStyle style)
    {
      Font font = this.GetFont(name) ?? new Font(familyName, size, style);
      this.Fonts[name] = font;
      return font;
    }

    public Font RegisterFont(
      string name,
      string familyName,
      float size,
      FontStyle style,
      byte[] data)
    {
      Font font1 = this.GetFont(name);
      if (font1 != null)
        return font1;
      FontFamily family = (FontFamily) null;
      StringComparison ccIgnoreCase = StringComparison.CurrentCultureIgnoreCase;
      if (this.PrivateFonts.Families.Length != 0)
        family = ((IEnumerable<FontFamily>) this.PrivateFonts.Families).ToList<FontFamily>().Find((Predicate<FontFamily>) (f => f.Name.Equals(familyName, ccIgnoreCase)));
      if (family == null)
      {
        IntPtr num = Marshal.AllocCoTaskMem(data.Length);
        FontCacheService.AddFontMemResourceEx(data, data.Length, IntPtr.Zero, out uint _);
        Marshal.Copy(data, 0, num, data.Length);
        this.PrivateFonts.AddMemoryFont(num, data.Length);
        Marshal.FreeCoTaskMem(num);
        family = ((IEnumerable<FontFamily>) this.PrivateFonts.Families).ToList<FontFamily>().Find((Predicate<FontFamily>) (f => f.Name.Equals(familyName, ccIgnoreCase)));
        if (family == null && familyName == "Target Alt Regular")
          family = ((IEnumerable<FontFamily>) this.PrivateFonts.Families).ToList<FontFamily>().Find((Predicate<FontFamily>) (f => f.Name.Equals("Target Alt", ccIgnoreCase)));
        if (family == null && familyName == "VAG Rounded Std Light")
          family = ((IEnumerable<FontFamily>) this.PrivateFonts.Families).ToList<FontFamily>().Find((Predicate<FontFamily>) (f => f.Name.Equals("VAG Rounded Std", ccIgnoreCase)));
      }
      if (family == null)
        family = ((IEnumerable<FontFamily>) this.PrivateFonts.Families).FirstOrDefault<FontFamily>();
      Font font2 = new Font(family, size, style);
      this.Fonts[name] = font2;
      return font2;
    }

    public ErrorList Initialize()
    {
      LogHelper.Instance.Log("Initialize font cache service.");
      ServiceLocator.Instance.AddService(typeof (IFontCacheService), (object) FontCacheService.Instance);
      return new ErrorList();
    }

    public void DropCache()
    {
      LogHelper.Instance.Log("Drop font cache.");
      foreach (Font font in (IEnumerable<Font>) this.Fonts.Values)
      {
        try
        {
          LogHelper.Instance.Log("...Dipose font '{0}'.", (object) font.Name);
          font.Dispose();
        }
        catch (Exception ex)
        {
          LogHelper.Instance.Log("An unhandled exception was raised in FontCacheService.DropCache.", ex);
        }
      }
      this.Fonts.Clear();
      if (this.m_privateFontCollection != null)
        this.m_privateFontCollection.Dispose();
      this.m_privateFontCollection = (PrivateFontCollection) null;
    }

    public void Dispose() => this.DropCache();

    internal IDictionary<string, Font> Fonts
    {
      get
      {
        if (this.m_fonts == null)
          this.m_fonts = (IDictionary<string, Font>) new Dictionary<string, Font>();
        return this.m_fonts;
      }
    }

    internal PrivateFontCollection PrivateFonts
    {
      get
      {
        if (this.m_privateFontCollection == null)
          this.m_privateFontCollection = new PrivateFontCollection();
        return this.m_privateFontCollection;
      }
    }

    private FontCacheService()
    {
    }

    [DllImport("gdi32.dll")]
    private static extern IntPtr AddFontMemResourceEx(
      byte[] pbFont,
      int cbFont,
      IntPtr pdv,
      out uint pcFonts);

    [DllImport("gdi32.dll")]
    private static extern bool RemoveFontMemResourceEx(IntPtr fh);
  }
}
