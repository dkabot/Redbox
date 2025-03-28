using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Redbox.KioskEngine.Environment
{
  public class ThemeService : IThemeService
  {
    private List<ITheme> _themes = new List<ITheme>();
    private ITheme _currentTheme;

    public static IThemeService Instance => (IThemeService) Singleton<ThemeService>.Instance;

    public ITheme CurrentTheme
    {
      get => this._currentTheme;
      set
      {
        if (this._currentTheme == value)
          return;
        ITheme currentTheme = this._currentTheme;
        this._currentTheme = value;
        if (this.OnCurrentThemeChanged == null)
          return;
        this.OnCurrentThemeChanged(currentTheme, this._currentTheme);
      }
    }

    public ITheme DefaultTheme
    {
      get
      {
        List<ITheme> themes = this._themes;
        return themes == null ? (ITheme) null : themes.FirstOrDefault<ITheme>((Func<ITheme, bool>) (x => x.IsDefault));
      }
    }

    public List<ITheme> Themes
    {
      get => this._themes;
      set => this._themes = value;
    }

    public event ThemeChanged OnCurrentThemeChanged;

    public void RegisterThemedControls(string assemblyName)
    {
      string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assemblyName);
      LogHelper.Instance.Log(string.Format("Registering themed controls in assembly {0}", (object) path), LogEntryType.Info);
      if (!File.Exists(path))
        return;
      foreach (Type themedControlType in ((IEnumerable<Type>) Assembly.LoadFile(path).GetTypes()).Where<Type>((Func<Type, bool>) (t => t.IsDefined(typeof (ThemedControlAttribute)))).ToList<Type>())
      {
        foreach (object customAttribute in themedControlType.GetCustomAttributes(true))
        {
          ThemedControlAttribute themedControlAttribute = customAttribute as ThemedControlAttribute;
          if (themedControlAttribute != null)
          {
            ITheme theme = this.Themes.FirstOrDefault<ITheme>((Func<ITheme, bool>) (x => x.Name == themedControlAttribute.ThemeName));
            if (theme == null)
            {
              theme = (ITheme) new Theme()
              {
                Name = themedControlAttribute.ThemeName
              };
              this.Themes.Add(theme);
            }
            theme.AddThemedControl(themedControlAttribute.ControlName, themedControlType);
          }
        }
      }
    }

    public FrameworkElement GetNewThemedControlInstance(string controlName)
    {
      return this.CurrentTheme?.GetNewThemedControlInstance(controlName) ?? this.DefaultTheme?.GetNewThemedControlInstance(controlName);
    }

    private ThemeService()
    {
    }
  }
}
