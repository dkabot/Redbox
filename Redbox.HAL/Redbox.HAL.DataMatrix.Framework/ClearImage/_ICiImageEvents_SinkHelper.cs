using System.Runtime.InteropServices;

namespace ClearImage
{
    [TypeLibType(TypeLibTypeFlags.FHidden)]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class _ICiImageEvents_SinkHelper : _ICiImageEvents
    {
        public int m_dwCookie;
        public _ICiImageEvents_LogEventHandler m_LogDelegate;

        internal _ICiImageEvents_SinkHelper()
        {
            m_dwCookie = 0;
            m_LogDelegate = null;
        }

        public virtual void Log(string LogSignature, string LogRecord, string RsltRecord)
        {
            if (m_LogDelegate == null)
                return;
            m_LogDelegate(LogSignature, LogRecord, RsltRecord);
        }
    }
}