namespace Redbox.HAL.Component.Model.Compression
{
    internal sealed class MemoryStream : StreamBase
    {
        internal MemoryStream()
        {
            Stream = new System.IO.MemoryStream();
        }

        internal MemoryStream(byte[] buffer)
        {
            Stream = new System.IO.MemoryStream(buffer);
        }

        internal byte[] GetBuffer()
        {
            return Stream != null ? ((System.IO.MemoryStream)Stream).GetBuffer() : null;
        }

        internal byte[] ToArray()
        {
            return Stream != null ? ((System.IO.MemoryStream)Stream).ToArray() : null;
        }
    }
}