using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Redbox.UpdateManager.StoreInstallerFrontEnd.Properties
{
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
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
                if (Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.resourceMan == null)
                    Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.resourceMan = new ResourceManager("Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources", typeof(Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources).Assembly);
                return Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.resourceMan;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get => Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.resourceCulture;
            set => Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.resourceCulture = value;
        }

        internal static Bitmap error
        {
            get
            {
                return (Bitmap)Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.ResourceManager.GetObject(nameof(error), Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.resourceCulture);
            }
        }

        internal static Bitmap error2
        {
            get
            {
                return (Bitmap)Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.ResourceManager.GetObject(nameof(error2), Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.resourceCulture);
            }
        }

        internal static Bitmap pending
        {
            get
            {
                return (Bitmap)Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.ResourceManager.GetObject(nameof(pending), Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.resourceCulture);
            }
        }

        internal static Bitmap running
        {
            get
            {
                return (Bitmap)Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.ResourceManager.GetObject(nameof(running), Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.resourceCulture);
            }
        }

        internal static Bitmap running1
        {
            get
            {
                return (Bitmap)Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.ResourceManager.GetObject(nameof(running1), Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.resourceCulture);
            }
        }

        internal static Bitmap Spinner32x32
        {
            get
            {
                return (Bitmap)Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.ResourceManager.GetObject(nameof(Spinner32x32), Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.resourceCulture);
            }
        }

        internal static Bitmap sucess
        {
            get
            {
                return (Bitmap)Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.ResourceManager.GetObject(nameof(sucess), Redbox.UpdateManager.StoreInstallerFrontEnd.Properties.Resources.resourceCulture);
            }
        }
    }
}
