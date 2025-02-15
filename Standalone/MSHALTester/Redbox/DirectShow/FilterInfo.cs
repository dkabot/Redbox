using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Redbox.DirectShow.Interop;

namespace Redbox.DirectShow;

public class FilterInfo : IComparable
{
    public FilterInfo(string monikerString)
    {
        MonikerString = monikerString;
        Name = GetName(monikerString);
    }

    internal FilterInfo(IMoniker moniker)
    {
        MonikerString = GetMonikerString(moniker);
        Name = GetName(moniker);
    }

    public string Name { get; }

    public string MonikerString { get; private set; }

    public int CompareTo(object value)
    {
        var filterInfo = (FilterInfo)value;
        return filterInfo == null ? 1 : Name.CompareTo(filterInfo.Name);
    }

    public static object CreateFilter(string filterMoniker)
    {
        var ppvResult = (object)null;
        var ppbc = (IBindCtx)null;
        var ppmk = (IMoniker)null;
        var pchEaten = 0;
        if (Win32.CreateBindCtx(0, out ppbc) == 0)
        {
            if (Win32.MkParseDisplayName(ppbc, filterMoniker, ref pchEaten, out ppmk) == 0)
            {
                var guid = typeof(IBaseFilter).GUID;
                ppmk.BindToObject(null, null, ref guid, out ppvResult);
                Marshal.ReleaseComObject(ppmk);
            }

            Marshal.ReleaseComObject(ppbc);
        }

        return ppvResult;
    }

    private string GetMonikerString(IMoniker moniker)
    {
        string ppszDisplayName;
        moniker.GetDisplayName(null, null, out ppszDisplayName);
        return ppszDisplayName;
    }

    private string GetName(IMoniker moniker)
    {
        var ppvObj = (object)null;
        try
        {
            var guid = typeof(IPropertyBag).GUID;
            moniker.BindToStorage(null, null, ref guid, out ppvObj);
            var propertyBag = (IPropertyBag)ppvObj;
            var obj = (object)"";
            ref var local = ref obj;
            var zero = IntPtr.Zero;
            var errorCode = propertyBag.Read("FriendlyName", ref local, zero);
            if (errorCode != 0)
                Marshal.ThrowExceptionForHR(errorCode);
            var str = (string)obj;
            return str != null && str.Length >= 1 ? str : throw new ApplicationException();
        }
        catch (Exception ex)
        {
            return "";
        }
        finally
        {
            if (ppvObj != null)
                Marshal.ReleaseComObject(ppvObj);
        }
    }

    private string GetName(string monikerString)
    {
        var ppbc = (IBindCtx)null;
        var ppmk = (IMoniker)null;
        var name = "";
        var pchEaten = 0;
        if (Win32.CreateBindCtx(0, out ppbc) == 0)
        {
            if (Win32.MkParseDisplayName(ppbc, monikerString, ref pchEaten, out ppmk) == 0)
            {
                name = GetName(ppmk);
                Marshal.ReleaseComObject(ppmk);
                ppmk = null;
            }

            Marshal.ReleaseComObject(ppbc);
        }

        return name;
    }
}