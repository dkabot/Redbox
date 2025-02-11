using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32;
using Redbox.DirectShow.Interop;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.DirectShow;

public class FilterInfoCollection : CollectionBase
{
    private static bool? m_useOldOption;

    public FilterInfoCollection(Guid category)
    {
        CollectFilters(category);
    }

    public FilterInfo this[int index] => (FilterInfo)InnerList[index];

    private static bool UseOld
    {
        get
        {
            if (m_useOldOption.HasValue)
                return m_useOldOption.Value;
            var registryKey = (RegistryKey)null;
            try
            {
                registryKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Redbox\\HAL");
                if (registryKey == null)
                {
                    m_useOldOption = false;
                    return m_useOldOption.Value;
                }

                var obj = registryKey.GetValue("UseOldEnumerator");
                if (obj == null)
                {
                    m_useOldOption = false;
                    return m_useOldOption.Value;
                }

                m_useOldOption = ConversionHelper.ChangeType<bool>(obj);
                return m_useOldOption.Value;
            }
            catch (Exception ex)
            {
                m_useOldOption = false;
                return m_useOldOption.Value;
            }
            finally
            {
                registryKey?.Close();
            }
        }
    }

    private void CollectFilters(Guid category)
    {
        var o = (object)null;
        var enumMoniker = (IEnumMoniker)null;
        var rgelt = new IMoniker[1];
        try
        {
            o = Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.SystemDeviceEnum) ??
                                         throw new ApplicationException("Failed creating device enumerator"));
            if (((ICreateDevEnum)o).CreateClassEnumerator(ref category, out enumMoniker, 0) != 0)
                throw new ApplicationException("No devices of the category");
            var zero = IntPtr.Zero;
            while (enumMoniker.Next(1, rgelt, zero) == 0)
                if (rgelt[0] == null)
                {
                    if (UseOld)
                        break;
                }
                else
                {
                    InnerList.Add(new FilterInfo(rgelt[0]));
                    Marshal.ReleaseComObject(rgelt[0]);
                    rgelt[0] = null;
                }

            InnerList.Sort();
        }
        catch
        {
        }
        finally
        {
            if (o != null)
                Marshal.ReleaseComObject(o);
            if (enumMoniker != null)
                Marshal.ReleaseComObject(enumMoniker);
            if (rgelt[0] != null)
            {
                Marshal.ReleaseComObject(rgelt[0]);
                rgelt[0] = null;
            }
        }
    }
}