using System;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class CommandTrace : IDisposable
    {
        private readonly StringBuilder TraceBuffer;
        private readonly bool TraceEnabled;
        private int spaceCount;

        internal CommandTrace(bool enabled)
        {
            TraceEnabled = enabled;
            if (!TraceEnabled)
                return;
            TraceBuffer = new StringBuilder();
        }

        public void Dispose()
        {
            if (!TraceEnabled)
                return;
            LogHelper.Instance.Log(TraceBuffer.ToString());
        }

        internal void Enter()
        {
            ++spaceCount;
        }

        internal void Exit()
        {
            --spaceCount;
        }

        internal void Trace(string fmt, params object[] p)
        {
            if (!TraceEnabled)
                return;
            Trace(string.Format(fmt, p));
        }

        internal void Trace(string msg)
        {
            if (!TraceEnabled)
                return;
            for (var index = 0; index < spaceCount; ++index)
                TraceBuffer.Append(" ");
            TraceBuffer.AppendLine(msg);
        }
    }
}