using Redbox.HAL.Core;

namespace Redbox.HAL.Client
{
    internal class NullSink : IClientOutputSink
    {
        private NullSink()
        {
        }

        internal static NullSink Instance => Singleton<NullSink>.Instance;

        public void WriteMessage(string msg)
        {
        }

        public void WriteMessage(string fmt, params object[] stuff)
        {
        }
    }
}