using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Redbox.HAL.Common.GUI.Functions.Properties
{
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [DebuggerNonUserCode]
    [CompilerGenerated]
    internal class Resources
    {
        private static ResourceManager resourceMan;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                if (resourceMan == null)
                    resourceMan = new ResourceManager("Redbox.HAL.Common.GUI.Functions.Properties.Resources",
                        typeof(Resources).Assembly);
                return resourceMan;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture { get; set; }

        internal static Bitmap sortAscending2 =>
            (Bitmap)ResourceManager.GetObject(nameof(sortAscending2), Culture);

        internal static Bitmap sortDescending2 =>
            (Bitmap)ResourceManager.GetObject(nameof(sortDescending2), Culture);

        internal static Bitmap undo => (Bitmap)ResourceManager.GetObject(nameof(undo), Culture);
    }
}