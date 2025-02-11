using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Redbox.UpdateManager.UI.Properties
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
                if (Redbox.UpdateManager.UI.Properties.Resources.resourceMan == null)
                    Redbox.UpdateManager.UI.Properties.Resources.resourceMan = new ResourceManager("Redbox.UpdateManager.UI.Properties.Resources", typeof(Redbox.UpdateManager.UI.Properties.Resources).Assembly);
                return Redbox.UpdateManager.UI.Properties.Resources.resourceMan;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get => Redbox.UpdateManager.UI.Properties.Resources.resourceCulture;
            set => Redbox.UpdateManager.UI.Properties.Resources.resourceCulture = value;
        }
    }
}
