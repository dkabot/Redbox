namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    internal sealed class BigEndianConverter
    {
        internal int ToInt32(byte[] buffer, int startIndex)
        {
            return Read(buffer, startIndex, 4);
        }

        internal short ToInt16(byte[] buffer, int startIndex)
        {
            return (short)Read(buffer, startIndex, 2);
        }

        internal ushort ToUInt16(byte[] buffer, int startIndex)
        {
            return (ushort)Read(buffer, startIndex, 2);
        }

        private int Read(byte[] buffer, int startIndex, int bytesToConvert)
        {
            var num = 0;
            for (var index = 0; index < bytesToConvert; ++index)
                num = (num << 8) | buffer[startIndex + index];
            return num;
        }
    }
}