using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Redbox.KioskEngine.Environment.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (Redbox.KioskEngine.Environment.Properties.Resources.resourceMan == null)
          Redbox.KioskEngine.Environment.Properties.Resources.resourceMan = new ResourceManager("Redbox.KioskEngine.Environment.Properties.Resources", typeof (Redbox.KioskEngine.Environment.Properties.Resources).Assembly);
        return Redbox.KioskEngine.Environment.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get => Redbox.KioskEngine.Environment.Properties.Resources.resourceCulture;
      set => Redbox.KioskEngine.Environment.Properties.Resources.resourceCulture = value;
    }

    internal static Bitmap Authentication
    {
      get
      {
        return (Bitmap) Redbox.KioskEngine.Environment.Properties.Resources.ResourceManager.GetObject(nameof (Authentication), Redbox.KioskEngine.Environment.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap DatabaseSchema
    {
      get
      {
        return (Bitmap) Redbox.KioskEngine.Environment.Properties.Resources.ResourceManager.GetObject(nameof (DatabaseSchema), Redbox.KioskEngine.Environment.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap Health
    {
      get
      {
        return (Bitmap) Redbox.KioskEngine.Environment.Properties.Resources.ResourceManager.GetObject(nameof (Health), Redbox.KioskEngine.Environment.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap Inventory
    {
      get
      {
        return (Bitmap) Redbox.KioskEngine.Environment.Properties.Resources.ResourceManager.GetObject(nameof (Inventory), Redbox.KioskEngine.Environment.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap MerchantService
    {
      get
      {
        return (Bitmap) Redbox.KioskEngine.Environment.Properties.Resources.ResourceManager.GetObject(nameof (MerchantService), Redbox.KioskEngine.Environment.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap Promotion
    {
      get
      {
        return (Bitmap) Redbox.KioskEngine.Environment.Properties.Resources.ResourceManager.GetObject(nameof (Promotion), Redbox.KioskEngine.Environment.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap QueueService
    {
      get
      {
        return (Bitmap) Redbox.KioskEngine.Environment.Properties.Resources.ResourceManager.GetObject(nameof (QueueService), Redbox.KioskEngine.Environment.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap Reservation
    {
      get
      {
        return (Bitmap) Redbox.KioskEngine.Environment.Properties.Resources.ResourceManager.GetObject(nameof (Reservation), Redbox.KioskEngine.Environment.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap ShoppingSession
    {
      get
      {
        return (Bitmap) Redbox.KioskEngine.Environment.Properties.Resources.ResourceManager.GetObject(nameof (ShoppingSession), Redbox.KioskEngine.Environment.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap Store
    {
      get
      {
        return (Bitmap) Redbox.KioskEngine.Environment.Properties.Resources.ResourceManager.GetObject(nameof (Store), Redbox.KioskEngine.Environment.Properties.Resources.resourceCulture);
      }
    }
  }
}
