using Redbox.Controls.Themes;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Redbox.KioskEngine.Environment
{
  public class Theme : ITheme
  {
    private Dictionary<string, string> _styleConversions = new Dictionary<string, string>();
    private Dictionary<string, Type> _themedControlTypes = new Dictionary<string, Type>();

    public string Name { get; set; }

    public bool IsDefault { get; set; }

    public bool IsAnimated { get; set; } = true;

    public FrameworkElement GetNewThemedControlInstance(string controlName)
    {
      FrameworkElement themedControlInstance = (FrameworkElement) null;
      Type themedControlType = this.GetThemedControlType(controlName);
      if (themedControlType != (Type) null)
      {
        object instance = Activator.CreateInstance(themedControlType);
        if (instance != null)
          themedControlInstance = instance as FrameworkElement;
      }
      return themedControlInstance;
    }

    public Type GetThemedControlType(string controlName)
    {
      Type themedControlType = (Type) null;
      this._themedControlTypes.TryGetValue(controlName, out themedControlType);
      return themedControlType;
    }

    public void AddThemedControl(string controlName, Type themedControlType)
    {
      this._themedControlTypes.Add(controlName, themedControlType);
    }

    public void RegisterStyleConversion(string originalStyleName, string newStyleName)
    {
      this._styleConversions.Add(originalStyleName, newStyleName);
    }

    public void SetStyle(FrameworkElement frameworkElement)
    {
      Style style = this.GetStyle(frameworkElement.Style, frameworkElement);
      if (style != frameworkElement.Style)
        frameworkElement.Style = style;
      if (!(frameworkElement is IThemedControl themedControl))
        return;
      themedControl.IsAnimated = this.IsAnimated;
    }

    public Style GetStyle(Style currentStyle, FrameworkElement frameworkElement)
    {
        Style style = currentStyle;
        DependencyObject reference = VisualTreeHelper.GetParent((DependencyObject)frameworkElement);

        while (reference != null)
        {
            if (reference is Window || reference is UserControl)
            {
                break;
            }
            reference = VisualTreeHelper.GetParent(reference);
        }

        if (reference == null)
        {
            FrameworkElement parent = frameworkElement.Parent as FrameworkElement;
            while (parent != null)
            {
                if (parent is UserControl)
                {
                    break;
                }
                parent = parent.Parent as FrameworkElement;
            }
            reference = (DependencyObject)parent;
        }

        if (reference is Control control)
        {
            string nameFromResource = Theme.FindNameFromResource((object)currentStyle, control);
            if (nameFromResource != null)
            {
                string resourceKey = null;
                if (this._styleConversions.TryGetValue(nameFromResource, out resourceKey))
                {
                    var resource = control.FindResource((object)resourceKey);
                    if (resource is Style newStyle)
                    {
                        style = newStyle;
                    }
                }
            }
        }

        return style;
    }


        private static string FindNameFromResource(object resourceItem, Control control)
    {
      foreach (ResourceDictionary mergedDictionary in Application.Current.Resources.MergedDictionaries)
      {
        foreach (object key in (IEnumerable) mergedDictionary.Keys)
        {
          if (mergedDictionary[key] == resourceItem)
            return key.ToString();
        }
      }
      return (string) null;
    }
  }
}
