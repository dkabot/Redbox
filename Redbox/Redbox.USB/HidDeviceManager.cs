using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Redbox.USB
{
    public class HidDeviceManager : IDisposable
    {
        internal const int DIGCF_PRESENT = 2;
        internal const int DIGCF_DEVICEINTERFACE = 16;
        internal const int DIGCF_INTERFACEDEVICE = 16;
        private Guid m_hidGuid;
        private bool m_disposed;
        private int m_hDevInfo = -1;
        private HIDD_ATTRIBUTES m_hidAttributes;
        private HIDP_VALUE_CAPS[] m_hidValueCaps;
        private SP_DEVICE_INTERFACE_DATA m_deviceInterfaceData;
        private PSP_DEVICE_INTERFACE_DETAIL_DATA m_deviceInterfaceDetailData;

        public HidDeviceManager()
        {
            _HidGuid();
            _SetupDiGetClassDevs();
        }

        public static void TestDeviceManager()
        {
            using (var hidDeviceManager = new HidDeviceManager())
            {
                foreach (var enumerateDevice in hidDeviceManager.EnumerateDevices())
                    Console.WriteLine(enumerateDevice.DevicePath);
            }
        }

        public HidDevice[] EnumerateDevices()
        {
            var arrayList = new ArrayList();
            for (var index = 0; _SetupDiEnumDeviceInterfaces(index); ++index)
            {
                var deviceInterfaceDetail = _SetupDiGetDeviceInterfaceDetail();
                var hidDevice = new HidDevice(index, deviceInterfaceDetail);
                arrayList.Add((object)hidDevice);
            }

            return (HidDevice[])arrayList.ToArray(typeof(HidDevice));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected void Dispose(bool disposeManagedResources)
        {
            if (m_disposed)
                return;
            var num = disposeManagedResources ? 1 : 0;
            if (m_hDevInfo != -1)
                _SetupDiDestroyDeviceInfoList();
            m_disposed = true;
        }

        protected internal void _HidGuid()
        {
            HidD_GetHidGuid(ref m_hidGuid);
        }

        protected internal unsafe int _SetupDiGetClassDevs()
        {
            m_hDevInfo = SetupDiGetClassDevs(ref m_hidGuid, (int*)null, (int*)null, 18);
            return m_hDevInfo;
        }

        protected internal bool _SetupDiEnumDeviceInterfaces(int memberIndex)
        {
            m_deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
            m_deviceInterfaceData.cbSize = Marshal.SizeOf<SP_DEVICE_INTERFACE_DATA>(m_deviceInterfaceData);
            return SetupDiEnumDeviceInterfaces(m_hDevInfo, 0, ref m_hidGuid, memberIndex, ref m_deviceInterfaceData) ==
                   1;
        }

        protected internal unsafe string _SetupDiGetDeviceInterfaceDetail()
        {
            var requiredSize = 0;
            SetupDiGetDeviceInterfaceDetail(m_hDevInfo, ref m_deviceInterfaceData, (int*)null, 0, ref requiredSize,
                (int*)null);
            m_deviceInterfaceDetailData = new PSP_DEVICE_INTERFACE_DETAIL_DATA();
            m_deviceInterfaceDetailData.cbSize = 5;
            if (SetupDiGetDeviceInterfaceDetail(m_hDevInfo, ref m_deviceInterfaceData, ref m_deviceInterfaceDetailData,
                    requiredSize, ref requiredSize, (int*)null) == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return m_deviceInterfaceDetailData.DevicePath;
        }

        protected internal int _HidD_GetAttributes(int hObject)
        {
            m_hidAttributes = new HIDD_ATTRIBUTES()
            {
                Size = sizeof(HIDD_ATTRIBUTES)
            };
            return HidD_GetAttributes(hObject, ref m_hidAttributes);
        }

        protected internal int _HidP_GetValueCaps(ref int CalsCapsLength, int pPHIDP_PREPARSED_DATA)
        {
            m_hidValueCaps = new HIDP_VALUE_CAPS[5];
            return HidP_GetValueCaps(HIDP_REPORT_TYPE.HidP_Input, m_hidValueCaps, ref CalsCapsLength,
                pPHIDP_PREPARSED_DATA);
        }

        protected internal int _SetupDiDestroyDeviceInfoList()
        {
            return SetupDiDestroyDeviceInfoList(m_hDevInfo);
        }

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern void HidD_GetHidGuid(ref Guid lpHidGuid);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern unsafe int SetupDiGetClassDevs(
            ref Guid lpHidGuid,
            int* Enumerator,
            int* hwndParent,
            int Flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern int SetupDiEnumDeviceInterfaces(
            int DeviceInfoSet,
            int DeviceInfoData,
            ref Guid lpHidGuid,
            int MemberIndex,
            ref SP_DEVICE_INTERFACE_DATA lpDeviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern unsafe int SetupDiGetDeviceInterfaceDetail(
            int DeviceInfoSet,
            ref SP_DEVICE_INTERFACE_DATA lpDeviceInterfaceData,
            int* aPtr,
            int detailSize,
            ref int requiredSize,
            int* bPtr);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern unsafe int SetupDiGetDeviceInterfaceDetail(
            int DeviceInfoSet,
            ref SP_DEVICE_INTERFACE_DATA lpDeviceInterfaceData,
            ref PSP_DEVICE_INTERFACE_DETAIL_DATA myPSP_DEVICE_INTERFACE_DETAIL_DATA,
            int detailSize,
            ref int requiredSize,
            int* bPtr);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern int HidD_GetAttributes(
            int hObject,
            ref HIDD_ATTRIBUTES Attributes);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern int HidP_GetValueCaps(
            HIDP_REPORT_TYPE ReportType,
            [In] [Out] HIDP_VALUE_CAPS[] ValueCaps,
            ref int ValueCapsLength,
            int pPHIDP_PREPARSED_DATA);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern int SetupDiDestroyDeviceInfoList(int DeviceInfoSet);

        internal struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public int Flags;
            public int Reserved;
        }

        internal struct PSP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int cbSize;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string DevicePath;
        }

        internal struct HIDD_ATTRIBUTES
        {
            public int Size;
            public ushort VendorID;
            public ushort ProductID;
            public ushort VersionNumber;
        }

        internal enum HIDP_REPORT_TYPE
        {
            HidP_Input,
            HidP_Output,
            HidP_Feature
        }

        internal struct Range
        {
            public ushort UsageMin;
            public ushort UsageMax;
            public ushort StringMin;
            public ushort StringMax;
            public ushort DesignatorMin;
            public ushort DesignatorMax;
            public ushort DataIndexMin;
            public ushort DataIndexMax;
        }

        internal struct NotRange
        {
            public ushort Usage;
            public ushort Reserved1;
            public ushort StringIndex;
            public ushort Reserved2;
            public ushort DesignatorIndex;
            public ushort Reserved3;
            public ushort DataIndex;
            public ushort Reserved4;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct HIDP_VALUE_CAPS
        {
            [FieldOffset(0)] public ushort UsagePage;
            [FieldOffset(2)] public byte ReportID;

            [MarshalAs(UnmanagedType.I1)] [FieldOffset(3)]
            public bool IsAlias;

            [FieldOffset(4)] public ushort BitField;
            [FieldOffset(6)] public ushort LinkCollection;
            [FieldOffset(8)] public ushort LinkUsage;
            [FieldOffset(10)] public ushort LinkUsagePage;

            [MarshalAs(UnmanagedType.I1)] [FieldOffset(12)]
            public bool IsRange;

            [MarshalAs(UnmanagedType.I1)] [FieldOffset(13)]
            public bool IsStringRange;

            [MarshalAs(UnmanagedType.I1)] [FieldOffset(14)]
            public bool IsDesignatorRange;

            [MarshalAs(UnmanagedType.I1)] [FieldOffset(15)]
            public bool IsAbsolute;

            [MarshalAs(UnmanagedType.I1)] [FieldOffset(16)]
            public bool HasNull;

            [FieldOffset(17)] public char Reserved;
            [FieldOffset(18)] public ushort BitSize;
            [FieldOffset(20)] public ushort ReportCount;
            [FieldOffset(22)] public ushort Reserved2a;
            [FieldOffset(24)] public ushort Reserved2b;
            [FieldOffset(26)] public ushort Reserved2c;
            [FieldOffset(28)] public ushort Reserved2d;
            [FieldOffset(30)] public ushort Reserved2e;
            [FieldOffset(32)] public ushort UnitsExp;
            [FieldOffset(34)] public ushort Units;
            [FieldOffset(36)] public short LogicalMin;
            [FieldOffset(38)] public short LogicalMax;
            [FieldOffset(40)] public short PhysicalMin;
            [FieldOffset(42)] public short PhysicalMax;
            [FieldOffset(44)] public Range Range;
            [FieldOffset(44)] public Range NotRange;
        }
    }
}