using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core.Descriptors;

namespace Redbox.HAL.Core;

public sealed class UsbDeviceService : IUsbDeviceService
{
    private const int CR_SUCCESS = 0;
    private const int CM_PROB_FAILED_START = 10;
    private const int CM_PROB_DISABLED = 22;
    private const int DN_HAS_PROBLEM = 1024;
    private const int DN_STARTED = 8;
    private const int SPDIT_NODRIVER = 0;
    private const int SPDIT_CLASSDRIVER = 1;
    private const int SPDIT_COMPATDRIVER = 2;
    private const int ERROR_INSUFFICIENT_BUFFER = 122;
    private const int DIGCF_ALLCLASSES = 4;
    private const int DIGCF_PRESENT = 2;
    private const int INVALID_HANDLE_VALUE = -1;
    private const int MAX_DEV_LEN = 1000;
    private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;
    private const int DEVICE_NOTIFY_SERVICE_HANDLE = 1;
    private const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;
    private const int DBT_DEVTYP_DEVICEINTERFACE = 5;
    private const int DBT_DEVNODES_CHANGED = 7;
    private const int WM_DEVICECHANGE = 537;
    private const int DIF_PROPERTYCHANGE = 18;
    private const int DICS_FLAG_GLOBAL = 1;
    private const int DICS_FLAG_CONFIGSPECIFIC = 2;
    private const int DICS_ENABLE = 1;
    private const int DICS_DISABLE = 2;
    private const int SPDRP_DEVICEDESC = 0;
    private const int SPDRP_HARDWAREID = 1;
    private const int SPDRP_COMPATIBLEIDS = 2;
    private const int SPDRP_UNUSED0 = 3;
    private const int SPDRP_SERVICE = 4;
    private const int SPDRP_UNUSED1 = 5;
    private const int SPDRP_UNUSED2 = 6;
    private const int SPDRP_CLASS = 7;
    private const int SPDRP_CLASSGUID = 8;
    private const int SPDRP_DRIVER = 9;
    private const int SPDRP_CONFIGFLAGS = 10;
    private const int SPDRP_MFG = 11;
    private const int SPDRP_FRIENDLYNAME = 12;
    private const int SPDRP_LOCATION_INFORMATION = 13;
    private const int SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 14;
    private const int SPDRP_CAPABILITIES = 15;
    private const int SPDRP_UI_NUMBER = 16;
    private const int SPDRP_UPPERFILTERS = 17;
    private const int SPDRP_LOWERFILTERS = 18;
    private const int SPDRP_BUSTYPEGUID = 19;
    private const int SPDRP_LEGACYBUSTYPE = 20;
    private const int SPDRP_BUSNUMBER = 21;
    private const int SPDRP_ENUMERATOR_NAME = 22;
    private const int SPDRP_SECURITY = 23;
    private const int SPDRP_SECURITY_SDS = 24;
    private const int SPDRP_DEVTYPE = 25;
    private const int SPDRP_EXCLUSIVE = 26;
    private const int SPDRP_CHARACTERISTICS = 27;
    private const int SPDRP_ADDRESS = 28;
    private const int SPDRP_UI_NUMBER_DESC_FORMAT = 29;
    private const int SPDRP_DEVICE_POWER_DATA = 30;
    private const int SPDRP_REMOVAL_POLICY = 31;
    private const int SPDRP_REMOVAL_POLICY_HW_DEFAULT = 32;
    private const int SPDRP_REMOVAL_POLICY_OVERRIDE = 33;
    private const int SPDRP_INSTALL_STATE = 34;
    private const int SPDRP_LOCATION_PATHS = 35;
    private const int SPDRP_BASE_CONTAINERID = 36;
    private readonly List<IDeviceDescriptor> Cameras = new();
    private readonly List<IDeviceDescriptor> CreditCardReaders = new();
    private readonly bool Debug;
    private readonly DriverDescriptor Driver_Elo = new(new Version("5.5.1.4"), "Elo Touch Solutions");

    private readonly DriverDescriptor Driver_GeneralTouch_HID1 =
        new(new Version("4.2.2.1"), "General Touch Technology Co.,Ltd.");

    private readonly DriverDescriptor Driver_GeneralTouch_HID2 =
        new(new Version("2.10.1781.0"), "General Touch Technology Co., Ltd.");

    private readonly DriverDescriptor Driver_TouchBase = new(new Version("4.0.2.0"), "Touch-Base Ltd");

    private readonly IDriverDescriptor Gen3DriverDescriptor =
        new DriverDescriptor(new Version("5.7.19104.104"), "Sonix");

    private readonly IDriverDescriptor Gen4DriverDescriptor = new DriverDescriptor(new Version("2.1.0.0"), "AVEO");
    private readonly List<ITouchscreenDescriptor> TouchScreenDescriptors = new();

    public UsbDeviceService(bool debug)
    {
        Debug = debug;
        var desc = new DriverDescriptor(new Version("7.13.14.0"), "3M");
        TouchScreenDescriptors.Add(new MicrotouchDescriptor(this, DeviceClass.HIDClass, desc));
        TouchScreenDescriptors.Add(new MicrotouchDescriptor(this, DeviceClass.Mouse, desc));
        TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("04e7", "0020", "Elo", Driver_Elo, this,
            DeviceClass.Mouse));
        TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("04e7", "0042", "Elo_2", Driver_Elo, this,
            DeviceClass.Mouse));
        TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("0dfc", "0001", "General Touch",
            Driver_GeneralTouch_HID2, this, DeviceClass.HIDClass));
        TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("0dfc", "0001", "General Touch",
            Driver_GeneralTouch_HID2, this, DeviceClass.HIDClass));
        TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("0dfc", "0001", "General Touch",
            Driver_GeneralTouch_HID1, this, DeviceClass.Mouse));
        TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("14c8", "0003", "Zytronic", Driver_TouchBase, this,
            DeviceClass.Mouse));
        var service = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>();
        CreditCardReaders.Add(new IdTechRev1(this, service));
        CreditCardReaders.Add(new IdTechRev2(this, service));
        Cameras.Add(new Gen5DeviceDescriptor(this));
        Cameras.Add(new LegacyDeviceDescriptor("1871", "0d01", "4th Gen (Color)", Gen4DriverDescriptor, this));
        Cameras.Add(new LegacyDeviceDescriptor("1871", "0f01", "4th Gen", Gen4DriverDescriptor, this));
        Cameras.Add(new LegacyDeviceDescriptor("0c45", "627b", "3rd Gen", Gen3DriverDescriptor, this));
    }

    public UsbDeviceService()
        : this(false)
    {
    }

    public IDeviceDescriptor FindActiveCamera(bool matchDriver)
    {
        var activeCamera = Cameras.Find(each => each.Locate());
        if (activeCamera == null)
            return null;
        LogHelper.Instance.Log("[UsbDeviceService] FindActiveCamera: HWID found {0}", activeCamera.ToString());
        if (!matchDriver || activeCamera.MatchDriver())
            return activeCamera;
        LogHelper.Instance.Log("[{0}] unable to match camera driver.", GetType().Name);
        return null;
    }

    public IDeviceDescriptor FromQueryString(string hwid)
    {
        return GenericDeviceDescriptor.Create(hwid, DeviceClass.None);
    }

    public bool SetDeviceState(IDeviceDescriptor descriptor, DeviceState state)
    {
        MakeBuffer();
        var lastError = 0;
        var failures = 0;
        EnumerateDevices(descriptor.SetupClass.Guid, SearchFlags(descriptor),
            (IntPtr devInfo, ref SP_DEVINFO_DATA did) =>
            {
                var hwid = QueryHardwareId(devInfo, ref did, out lastError);
                if (hwid == null)
                    return ProcessDeviceInfoResult.Continue;
                var left = FromQueryString(hwid);
                if (left != null && Match(left, descriptor))
                {
                    var flag = ChangeState(devInfo, ref did, state);
                    LogHelper.Instance.Log("[{0}] Found HWID {1}; change state to {2} returned {3}", GetType().Name,
                        hwid, state.ToString(), flag ? "OK" : (object)"FAIL");
                    if (!flag)
                        ++failures;
                }

                return ProcessDeviceInfoResult.Continue;
            });
        return failures == 0;
    }

    public bool ChangeByHWID(IDeviceDescriptor descriptor, DeviceState state)
    {
        var error = 0;
        return EnumerateDevices(descriptor.SetupClass.Guid, SearchFlags(descriptor),
            (IntPtr devInfo, ref SP_DEVINFO_DATA did) =>
            {
                var hwid = QueryHardwareId(devInfo, ref did, out error);
                if (hwid == null)
                    return ProcessDeviceInfoResult.Continue;
                var left = FromQueryString(hwid);
                var deviceInfoResult = ProcessDeviceInfoResult.Continue;
                if (left != null && Match(left, descriptor))
                {
                    if (Debug)
                        LogHelper.Instance.Log("[{0}] Change state of {1} to {2}", GetType().Name, hwid,
                            state.ToString());
                    deviceInfoResult = ChangeState(devInfo, ref did, state)
                        ? ProcessDeviceInfoResult.Success
                        : ProcessDeviceInfoResult.Error;
                }

                return deviceInfoResult;
            });
    }

    public DeviceStatus FindDeviceStatus(IDeviceDescriptor deviceInfo)
    {
        var rv = DeviceStatus.None;
        EnumerateDevices(deviceInfo.SetupClass.Guid, SearchFlags(deviceInfo),
            (IntPtr devInfo, ref SP_DEVINFO_DATA did) =>
            {
                var hwid = QueryHardwareId(devInfo, ref did, out _);
                if (hwid == null)
                    return ProcessDeviceInfoResult.Continue;
                var left = FromQueryString(hwid);
                LogHelper.Instance.Log("[USBService] Find device status hwid = {0}", hwid);
                rv = DeviceStatus.None;
                if (left != null && Match(left, deviceInfo))
                {
                    rv |= DeviceStatus.Found;
                    uint status;
                    uint probNum;
                    if (CM_Get_DevNode_Status(out status, out probNum, did.devInst, 0) == 0)
                    {
                        LogHelper.Instance.Log("[USBService] CM_Get_DevNode {0} ( {1} ) status = {2} problem = {3}",
                            deviceInfo.ToString(), deviceInfo.Friendlyname, status, probNum);
                        if (((int)status & 1024) != 0)
                        {
                            if (22U == probNum)
                                rv |= DeviceStatus.Disabled;
                            else if (10U == probNum)
                                rv |= DeviceStatus.NotStarted;
                        }
                        else if (((int)status & 8) != 0)
                        {
                            rv |= DeviceStatus.Enabled;
                        }

                        return ProcessDeviceInfoResult.Success;
                    }
                }

                return ProcessDeviceInfoResult.Continue;
            });
        return rv;
    }

    public bool MatchDriverByVendor(IDeviceDescriptor desc, IDriverDescriptor driverInfo)
    {
        return EnumerateDevices(desc.SetupClass.Guid, SearchFlags(desc), (IntPtr hDevInfo, ref SP_DEVINFO_DATA did) =>
        {
            var str = QueryHardwareId(hDevInfo, ref did, out _);
            if (str == null)
                return ProcessDeviceInfoResult.Continue;
            var deviceInfoResult = ProcessDeviceInfoResult.Continue;
            if (!string.IsNullOrEmpty(str) && str.Equals(desc.Vendor, StringComparison.CurrentCultureIgnoreCase))
                deviceInfoResult = MatchDriverInner(hDevInfo, ref did, driverInfo, str)
                    ? ProcessDeviceInfoResult.Success
                    : ProcessDeviceInfoResult.Continue;
            return deviceInfoResult;
        });
    }

    public bool MatchDriver(IDeviceDescriptor descriptor, IDriverDescriptor driverInfo)
    {
        return EnumerateDevices(descriptor.SetupClass.Guid, 2U, (IntPtr hDevInfo, ref SP_DEVINFO_DATA did) =>
        {
            var str = QueryHardwareId(hDevInfo, ref did, out _);
            if (str == null)
                return ProcessDeviceInfoResult.Continue;
            LogHelper.Instance.Log("[MatchDriver] Processing HWID {0}", str);
            var left = FromQueryString(str);
            if (left == null || !Match(left, descriptor))
                return ProcessDeviceInfoResult.Continue;
            LogHelper.Instance.Log("[MatchDriver] HWID found {0}", str);
            return !MatchDriverInner(hDevInfo, ref did, driverInfo, str)
                ? ProcessDeviceInfoResult.Continue
                : ProcessDeviceInfoResult.Success;
        });
    }

    public IUsbDeviceSearchResult FindDevice(IDeviceDescriptor descriptor)
    {
        var lastError = 0;
        var result = new UsbDeviceSearchResult();
        if (Debug)
            LogHelper.Instance.Log("[FindDevice] Search device class {0}", descriptor.SetupClass.Class.ToString());
        EnumerateDevices(descriptor.SetupClass.Guid, SearchFlags(descriptor),
            (IntPtr devInfo, ref SP_DEVINFO_DATA did) =>
            {
                var hwid = QueryHardwareId(devInfo, ref did, out lastError);
                if (hwid != null)
                {
                    LogHelper.Instance.Log("  [FindDeviceWithVid] found hwid = {0}", hwid);
                    var left = FromQueryString(hwid);
                    if (left != null && Match(left, descriptor))
                        result.Matches.Add(left);
                }

                return ProcessDeviceInfoResult.Continue;
            });
        return result;
    }

    public IUsbDeviceSearchResult FindVendorDevices(string _vendor)
    {
        var lastError = 0;
        var result = new UsbDeviceSearchResult();
        if (Debug)
            LogHelper.Instance.Log("[FindVendorDevices] Search for devices from vendor {0}", _vendor);
        EnumerateDevices(Guid.Empty, 6U, (IntPtr devInfo, ref SP_DEVINFO_DATA did) =>
        {
            var hwid = QueryHardwareId(devInfo, ref did, out lastError);
            if (Debug && hwid != null)
                LogHelper.Instance.Log("  [FindVendorDevices] Examine hwid = {0} for match", hwid);
            var deviceDescriptor = FromQueryString(hwid);
            if (deviceDescriptor != null &&
                _vendor.Equals(deviceDescriptor.Vendor, StringComparison.CurrentCultureIgnoreCase))
                result.Matches.Add(deviceDescriptor);
            return ProcessDeviceInfoResult.Continue;
        });
        return result;
    }

    public void EnumDevices(Action<string, string> onDeviceFound)
    {
        var lastError = 0;
        var clazzNameBuffer = MakeBuffer();
        EnumerateDevices(Guid.Empty, 6U, (IntPtr devInfo, ref SP_DEVINFO_DATA did) =>
        {
            if (!SetupDiGetDeviceRegistryProperty(devInfo, ref did, 7U, 0U, clazzNameBuffer, 1000U, IntPtr.Zero))
            {
                lastError = Marshal.GetLastWin32Error();
                LogHelper.Instance.Log("[{0}] SetupDiGetDeviceRegistryProperty returned error {1}", GetType().Name,
                    lastError);
                return ProcessDeviceInfoResult.Error;
            }

            var str = QueryHardwareId(devInfo, ref did, out lastError);
            if (!string.IsNullOrEmpty(str))
                onDeviceFound(clazzNameBuffer.ToString(), str);
            return ProcessDeviceInfoResult.Continue;
        });
    }

    public List<IDeviceDescriptor> FindDevices(DeviceClass clazz)
    {
        var lastError = 0;
        var result = new List<IDeviceDescriptor>();
        if (clazz == DeviceClass.None)
            return result;
        var clazzNameBuffer = MakeBuffer();
        var deviceClass = Guid.Empty;
        var deviceSetupClass = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>().Get(clazz);
        if (deviceSetupClass != null)
            deviceClass = deviceSetupClass.Guid;
        EnumerateDevices(deviceClass, 2U, (IntPtr devInfo, ref SP_DEVINFO_DATA did) =>
        {
            if (!SetupDiGetDeviceRegistryProperty(devInfo, ref did, 7U, 0U, clazzNameBuffer, 1000U, IntPtr.Zero))
            {
                lastError = Marshal.GetLastWin32Error();
                return ProcessDeviceInfoResult.Error;
            }

            var ignoringCase = Enum<DeviceClass>.ParseIgnoringCase(clazzNameBuffer.ToString(), DeviceClass.None);
            if (Debug)
                LogHelper.Instance.Log("[{0}] Requested search = {1}, found clazz = {2}", GetType().Name,
                    clazz.ToString(), clazzNameBuffer.ToString());
            if (ignoringCase == clazz)
            {
                var hwid = QueryHardwareId(devInfo, ref did, out lastError);
                if (!string.IsNullOrEmpty(hwid))
                {
                    if (Debug)
                        LogHelper.Instance.Log("[{0}] Requested search = {1}, found clazz = {2} HWID = {3}",
                            GetType().Name, clazz.ToString(), clazzNameBuffer.ToString(), hwid);
                    var deviceDescriptor = GenericDeviceDescriptor.Create(hwid, ignoringCase);
                    if (deviceDescriptor != null)
                        result.Add(deviceDescriptor);
                }
            }

            return ProcessDeviceInfoResult.Continue;
        });
        return result;
    }

    public ITouchscreenDescriptor FindTouchScreen(bool matchDriver)
    {
        foreach (var screenDescriptor in TouchScreenDescriptors)
        {
            LogHelper.Instance.Log("[UsbDeviceService] Searching for device {0}", screenDescriptor.ToString());
            if (screenDescriptor.Locate())
            {
                LogHelper.Instance.Log("[UsbDeviceService] Found Touchscreen device {0}.", screenDescriptor.ToString());
                if (!matchDriver)
                    return screenDescriptor;
                if (screenDescriptor.MatchDriver())
                {
                    LogHelper.Instance.Log("[UsbDeviceService] Touch screen driver matched.");
                    return screenDescriptor;
                }
            }
        }

        LogHelper.Instance.Log("[UsbDeviceService] Could not match TS driver.");
        return null;
    }

    public ITouchscreenDescriptor FindTouchScreen()
    {
        return FindTouchScreen(false);
    }

    public IQueryUsbDeviceResult FindCCR()
    {
        return OnLocate(CreditCardReaders);
    }

    public IQueryUsbDeviceResult FindCamera()
    {
        return OnLocate(Cameras);
    }

    [DllImport("setupapi.dll", SetLastError = true)]
    internal static extern IntPtr SetupDiGetClassDevs(
        ref Guid gClass,
        uint iEnumerator,
        IntPtr hParent,
        uint nFlags);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern int CM_Get_DevNode_Status(
        out uint status,
        out uint probNum,
        uint devInst,
        int flags);

    [DllImport("setupapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetupDiBuildDriverInfoList(
        IntPtr deviceInfoSet,
        ref SP_DEVINFO_DATA deviceInfoData,
        DriverType driverType);

    [DllImport("setupapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetupDiEnumDriverInfo(
        IntPtr deviceInfoSet,
        ref SP_DEVINFO_DATA deviceInfoData,
        DriverType driverType,
        uint memberIndex,
        ref DriverInfoData driverInfoData);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern int SetupDiDestroyDeviceInfoList(IntPtr lpInfoSet);

    [DllImport("setupapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetupDiEnumDeviceInfo(
        IntPtr deviceInfoSet,
        uint memberIndex,
        ref SP_DEVINFO_DATA data);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern bool SetupDiGetDeviceRegistryProperty(
        IntPtr lpInfoSet,
        ref SP_DEVINFO_DATA infoData,
        uint Property,
        uint PropertyRegDataType,
        StringBuilder PropertyBuffer,
        uint PropertyBufferSize,
        IntPtr RequiredSize);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool SetupDiSetClassInstallParams(
        IntPtr dis,
        ref SP_DEVINFO_DATA did,
        ref SP_PROPCHANGE_PARAMS spp,
        int sz);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool SetupDiCallClassInstaller(
        uint func,
        IntPtr dis,
        ref SP_DEVINFO_DATA did);

    private uint SearchFlags(IDeviceDescriptor desc)
    {
        uint num = 2;
        if (desc.SetupClass.Class == DeviceClass.None)
            num |= 4U;
        return num;
    }

    private string QueryDeviceDescription(
        IntPtr devInfo,
        ref SP_DEVINFO_DATA deviceInfoData,
        out int lastError)
    {
        lastError = 0;
        var PropertyBuffer = MakeBuffer();
        if (SetupDiGetDeviceRegistryProperty(devInfo, ref deviceInfoData, 0U, 0U, PropertyBuffer, 1000U, IntPtr.Zero))
            return PropertyBuffer.ToString().ToLower();
        lastError = Marshal.GetLastWin32Error();
        return null;
    }

    private string QueryHardwareId(
        IntPtr devInfo,
        ref SP_DEVINFO_DATA did,
        out int lastError)
    {
        lastError = 0;
        var PropertyBuffer = MakeBuffer();
        if (SetupDiGetDeviceRegistryProperty(devInfo, ref did, 1U, 0U, PropertyBuffer, 1000U, IntPtr.Zero))
            return PropertyBuffer.ToString().ToLower();
        lastError = Marshal.GetLastWin32Error();
        return null;
    }

    private bool ChangeState(
        IntPtr hDevInfo,
        ref SP_DEVINFO_DATA devInfoData,
        DeviceState devState)
    {
        try
        {
            var spp = new SP_PROPCHANGE_PARAMS();
            spp.ClassInstallHeader = new SP_CLASSINSTALL_HEADER();
            if (DeviceState.Enable == devState)
            {
                spp.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(SP_CLASSINSTALL_HEADER));
                spp.ClassInstallHeader.InstallFunction = 18;
                spp.StateChange = 1;
                spp.Scope = 1;
                spp.HwProfile = 0;
                if (SetupDiSetClassInstallParams(hDevInfo, ref devInfoData, ref spp,
                        Marshal.SizeOf(typeof(SP_PROPCHANGE_PARAMS))))
                    SetupDiCallClassInstaller(18U, hDevInfo, ref devInfoData);
                spp.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(SP_CLASSINSTALL_HEADER));
                spp.ClassInstallHeader.InstallFunction = 18;
                spp.StateChange = 1;
                spp.Scope = 2;
                spp.HwProfile = 0;
            }
            else
            {
                spp.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(SP_CLASSINSTALL_HEADER));
                spp.ClassInstallHeader.InstallFunction = 18;
                spp.StateChange = 2;
                spp.Scope = 2;
                spp.HwProfile = 0;
            }

            if (!SetupDiSetClassInstallParams(hDevInfo, ref devInfoData, ref spp,
                    Marshal.SizeOf(typeof(SP_PROPCHANGE_PARAMS))))
            {
                LogHelper.Instance.Log("SetupDiSetClassInstallParams returned false.");
                return false;
            }

            if (SetupDiCallClassInstaller(18U, hDevInfo, ref devInfoData))
                return true;
            LogHelper.Instance.Log("SetupDiCallClassInstaller returned false ( Win32 error = {0} )",
                Marshal.GetLastWin32Error());
            return false;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private QueryUsbDeviceResult OnLocate(List<IDeviceDescriptor> descriptors)
    {
        foreach (var descriptor in descriptors)
        {
            var deviceStatus = FindDeviceStatus(descriptor);
            if ((deviceStatus & DeviceStatus.Found) != DeviceStatus.None)
                return new QueryUsbDeviceResult(descriptor)
                {
                    Status = deviceStatus
                };
        }

        return new QueryUsbDeviceResult(null)
        {
            Status = DeviceStatus.None
        };
    }

    private bool MatchDriverInner(
        IntPtr hDevInfo,
        ref SP_DEVINFO_DATA did,
        IDriverDescriptor driverInfo,
        string hwid)
    {
        if (driverInfo != null && SetupDiBuildDriverInfoList(hDevInfo, ref did, DriverType.SPDIT_COMPATDRIVER))
        {
            var driverInfoData = new DriverInfoData();
            driverInfoData.Size = Marshal.SizeOf(driverInfoData);
            for (uint memberIndex = 0;
                 SetupDiEnumDriverInfo(hDevInfo, ref did, DriverType.SPDIT_COMPATDRIVER, memberIndex,
                     ref driverInfoData);
                 ++memberIndex)
            {
                if (Debug)
                {
                    LogHelper.Instance.Log("Driver data:", LogEntryType.Info);
                    LogHelper.Instance.Log(driverInfoData.ToString(), LogEntryType.Info);
                }

                if (driverInfoData.GetVersion() != driverInfo.DriverVersion)
                {
                    if (Debug)
                        LogHelper.Instance.Log("** driver version {0} didn't match {1}",
                            driverInfoData.GetVersion().ToString(), driverInfo.DriverVersion.ToString());
                }
                else
                {
                    if (driverInfoData.ProviderName.Equals(driverInfo.Provider,
                            StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (Debug)
                        {
                            LogHelper.Instance.Log("Found HWID {0}; driver info:", hwid);
                            LogHelper.Instance.Log("{0}", driverInfoData.ToString());
                        }

                        return true;
                    }

                    if (Debug)
                        LogHelper.Instance.Log("** driver provider {0} didn't match {1}", driverInfoData.ProviderName,
                            driverInfo.Provider);
                }
            }
        }

        return false;
    }

    private bool EnumerateDevices(
        Guid deviceClass,
        uint searchFlags,
        ProcessDeviceInfo callback)
    {
        var num = IntPtr.Zero;
        try
        {
            num = SetupDiGetClassDevs(ref deviceClass, 0U, IntPtr.Zero, searchFlags);
            if (num.ToInt32() == -1)
                return false;
            var structure = new SP_DEVINFO_DATA();
            structure.cbSize = Marshal.SizeOf(structure);
            for (uint memberIndex = 0; SetupDiEnumDeviceInfo(num, memberIndex, ref structure); ++memberIndex)
            {
                var deviceInfoResult = callback(num, ref structure);
                if (ProcessDeviceInfoResult.Error == deviceInfoResult)
                    return false;
                if (ProcessDeviceInfoResult.Success == deviceInfoResult)
                    return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log("[UsbDeviceService] Unable to enumerate device tree.");
            return false;
        }
        finally
        {
            try
            {
                if (num != IntPtr.Zero)
                    if (num.ToInt32() != -1)
                        SetupDiDestroyDeviceInfoList(num);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("[UsbDeviceService] Clearing the device info list caught an exception.", ex);
            }
        }
    }

    private StringBuilder MakeBuffer()
    {
        return new StringBuilder("") { Capacity = 1000 };
    }

    private bool Match(IDeviceDescriptor left, IDeviceDescriptor right)
    {
        return Match(left, right, DeviceDescriptorMatchOption.VidPid);
    }

    private bool Match(
        IDeviceDescriptor left,
        IDeviceDescriptor right,
        DeviceDescriptorMatchOption option)
    {
        if (DeviceDescriptorMatchOption.Product == option)
            return left.Product.ToLower().Equals(right.Product.ToLower(), StringComparison.CurrentCultureIgnoreCase);
        return (DeviceDescriptorMatchOption.Vendor == option || left.Product.ToLower()
            .Equals(right.Product.ToLower(), StringComparison.CurrentCultureIgnoreCase)) && left.Vendor.ToLower()
            .Equals(right.Vendor.ToLower(), StringComparison.CurrentCultureIgnoreCase);
    }

    private enum DriverType : uint
    {
        SPDIT_NODRIVER,
        SPDIT_CLASSDRIVER,
        SPDIT_COMPATDRIVER
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DriverInfoData
    {
        public int Size;
        public DriverType DriverType;
        public IntPtr Reserved;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Description;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string MfgName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string ProviderName;

        public long DriverDate;
        public ulong DriverVersion;

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(DriverType.ToString());
            stringBuilder.AppendLine(Description);
            stringBuilder.AppendLine(MfgName);
            stringBuilder.AppendLine(ProviderName);
            stringBuilder.AppendLine(DateTime.FromFileTime(DriverDate).ToShortDateString());
            stringBuilder.AppendLine(GetVersion().ToString());
            return stringBuilder.ToString();
        }

        public Version GetVersion()
        {
            return new Version((int)(DriverVersion >> 48), (int)((long)(DriverVersion >> 32) & ushort.MaxValue),
                (int)((long)(DriverVersion >> 16) & ushort.MaxValue), (int)((long)DriverVersion & ushort.MaxValue));
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct SP_DEVINFO_DATA
    {
        internal int cbSize;
        internal Guid classGuid;
        internal uint devInst;
        internal IntPtr reserved;
    }

    internal struct SP_DEVICE_INTERFACE_DATA
    {
        internal int cbSize;
        internal Guid interfaceClassGuid;
        internal int flags;
        private UIntPtr reserved;
    }

    internal struct SP_PROPCHANGE_PARAMS
    {
        internal SP_CLASSINSTALL_HEADER ClassInstallHeader;
        internal int StateChange;
        internal int Scope;
        internal int HwProfile;
    }

    internal struct SP_CLASSINSTALL_HEADER
    {
        internal int cbSize;
        internal int InstallFunction;
    }

    private enum DeviceDescriptorMatchOption
    {
        VidPid,
        Vendor,
        Product
    }

    private enum ProcessDeviceInfoResult
    {
        None,
        Continue,
        Error,
        Success
    }

    private delegate ProcessDeviceInfoResult ProcessDeviceInfo(
        IntPtr devInfo,
        ref SP_DEVINFO_DATA did);
}