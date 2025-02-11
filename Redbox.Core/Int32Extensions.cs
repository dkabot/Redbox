using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Redbox.Core
{
    public static class Int32Extensions
    {
        public static string FormatAsByteSize(this int fileSize)
        {
            var buffer = new StringBuilder(20);
            StrFormatByteSize(fileSize, buffer, 20);
            return buffer.ToString();
        }

        public static string FormatAsByteSize(this long fileSize)
        {
            var buffer = new StringBuilder(20);
            StrFormatByteSize(fileSize, buffer, 20);
            return buffer.ToString();
        }

        public static string FormatAsByteSize(this ulong fileSize)
        {
            var buffer = new StringBuilder(20);
            StrFormatByteSize(Convert.ToInt64(fileSize), buffer, 20);
            return buffer.ToString();
        }

        public static bool NotInRanges(this int number, Pair<int, int>[] ranges)
        {
            return !number.BetweenAnyRange(ranges);
        }

        public static bool BetweenAnyRange(this int theNumber, Pair<int, int>[] ranges)
        {
            var flag = false;
            for (var index = 0; index < ranges.Length; ++index)
                flag = flag || theNumber.Between(ranges[index].First, ranges[index].Second);
            return flag;
        }

        public static bool Between(this int theNumber, int min, int max)
        {
            return theNumber >= min && theNumber <= max;
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern long StrFormatByteSize(
            long fileSize,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer,
            int bufferSize);
    }
}