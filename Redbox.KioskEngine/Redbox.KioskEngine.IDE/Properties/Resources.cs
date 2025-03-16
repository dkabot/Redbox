using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Redbox.KioskEngine.IDE.Properties
{
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
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
                if (resourceMan == null)
                    resourceMan = new ResourceManager("Redbox.KioskEngine.IDE.Properties.Resources",
                        typeof(Resources).Assembly);
                return resourceMan;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get => resourceCulture;
            set => resourceCulture = value;
        }

        internal static Bitmap Bundle => (Bitmap)ResourceManager.GetObject(nameof(Bundle), resourceCulture);

        internal static Bitmap CallStack => (Bitmap)ResourceManager.GetObject(nameof(CallStack), resourceCulture);

        internal static Bitmap Clean => (Bitmap)ResourceManager.GetObject(nameof(Clean), resourceCulture);

        internal static Bitmap Copy => (Bitmap)ResourceManager.GetObject(nameof(Copy), resourceCulture);

        internal static Bitmap Critical => (Bitmap)ResourceManager.GetObject(nameof(Critical), resourceCulture);

        internal static Bitmap Cross => (Bitmap)ResourceManager.GetObject(nameof(Cross), resourceCulture);

        internal static Bitmap Cube => (Bitmap)ResourceManager.GetObject(nameof(Cube), resourceCulture);

        internal static Bitmap Cut => (Bitmap)ResourceManager.GetObject(nameof(Cut), resourceCulture);

        internal static Bitmap DebugBreakpoint =>
            (Bitmap)ResourceManager.GetObject(nameof(DebugBreakpoint), resourceCulture);

        internal static Bitmap DebugBreakpointClear =>
            (Bitmap)ResourceManager.GetObject(nameof(DebugBreakpointClear), resourceCulture);

        internal static Bitmap Debugger => (Bitmap)ResourceManager.GetObject(nameof(Debugger), resourceCulture);

        internal static Bitmap DebugPause => (Bitmap)ResourceManager.GetObject(nameof(DebugPause), resourceCulture);

        internal static Bitmap DebugPlay => (Bitmap)ResourceManager.GetObject(nameof(DebugPlay), resourceCulture);

        internal static Bitmap DebugStepInto =>
            (Bitmap)ResourceManager.GetObject(nameof(DebugStepInto), resourceCulture);

        internal static Bitmap DebugStepOut => (Bitmap)ResourceManager.GetObject(nameof(DebugStepOut), resourceCulture);

        internal static Bitmap DebugStepOver =>
            (Bitmap)ResourceManager.GetObject(nameof(DebugStepOver), resourceCulture);

        internal static Bitmap DebugStop => (Bitmap)ResourceManager.GetObject(nameof(DebugStop), resourceCulture);

        internal static Bitmap Delete => (Bitmap)ResourceManager.GetObject(nameof(Delete), resourceCulture);

        internal static Bitmap Dependencies => (Bitmap)ResourceManager.GetObject(nameof(Dependencies), resourceCulture);

        internal static Bitmap ErrorList => (Bitmap)ResourceManager.GetObject(nameof(ErrorList), resourceCulture);

        internal static Bitmap Execute => (Bitmap)ResourceManager.GetObject(nameof(Execute), resourceCulture);

        internal static Bitmap Filter => (Bitmap)ResourceManager.GetObject(nameof(Filter), resourceCulture);

        internal static Bitmap Folder => (Bitmap)ResourceManager.GetObject(nameof(Folder), resourceCulture);

        internal static Bitmap FolderClosed => (Bitmap)ResourceManager.GetObject(nameof(FolderClosed), resourceCulture);

        internal static Bitmap ImmediateWindow =>
            (Bitmap)ResourceManager.GetObject(nameof(ImmediateWindow), resourceCulture);

        internal static Bitmap Information => (Bitmap)ResourceManager.GetObject(nameof(Information), resourceCulture);

        internal static Bitmap Minus => (Bitmap)ResourceManager.GetObject(nameof(Minus), resourceCulture);

        internal static Bitmap NewDocument => (Bitmap)ResourceManager.GetObject(nameof(NewDocument), resourceCulture);

        internal static Bitmap Open => (Bitmap)ResourceManager.GetObject(nameof(Open), resourceCulture);

        internal static Bitmap Output => (Bitmap)ResourceManager.GetObject(nameof(Output), resourceCulture);

        internal static Bitmap PageSetup => (Bitmap)ResourceManager.GetObject(nameof(PageSetup), resourceCulture);

        internal static Bitmap Paste => (Bitmap)ResourceManager.GetObject(nameof(Paste), resourceCulture);

        internal static Bitmap Plus => (Bitmap)ResourceManager.GetObject(nameof(Plus), resourceCulture);

        internal static Bitmap Print => (Bitmap)ResourceManager.GetObject(nameof(Print), resourceCulture);

        internal static Bitmap Properties => (Bitmap)ResourceManager.GetObject(nameof(Properties), resourceCulture);

        internal static Bitmap Redo => (Bitmap)ResourceManager.GetObject(nameof(Redo), resourceCulture);

        internal static Bitmap Refresh => (Bitmap)ResourceManager.GetObject(nameof(Refresh), resourceCulture);

        internal static Bitmap Save => (Bitmap)ResourceManager.GetObject(nameof(Save), resourceCulture);

        internal static Bitmap SaveAll => (Bitmap)ResourceManager.GetObject(nameof(SaveAll), resourceCulture);

        internal static Bitmap SaveAs => (Bitmap)ResourceManager.GetObject(nameof(SaveAs), resourceCulture);

        internal static Bitmap Sitemap => (Bitmap)ResourceManager.GetObject(nameof(Sitemap), resourceCulture);

        internal static Bitmap Timers => (Bitmap)ResourceManager.GetObject(nameof(Timers), resourceCulture);

        internal static Bitmap Tools => (Bitmap)ResourceManager.GetObject(nameof(Tools), resourceCulture);

        internal static Bitmap Undo => (Bitmap)ResourceManager.GetObject(nameof(Undo), resourceCulture);

        internal static Bitmap Views => (Bitmap)ResourceManager.GetObject(nameof(Views), resourceCulture);

        internal static Bitmap Warning => (Bitmap)ResourceManager.GetObject(nameof(Warning), resourceCulture);
    }
}