using Redbox.KioskEngine.ComponentModel;
using System;

namespace Redbox.KioskEngine.Environment
{
  [AttributeUsage(AttributeTargets.Method)]
  public class TweenFunctionAttribute : Attribute
  {
    public TweenType Type { get; set; }
  }
}
