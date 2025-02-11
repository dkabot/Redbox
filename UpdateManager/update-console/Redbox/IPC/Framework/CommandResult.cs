using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.IPC.Framework
{
    internal class CommandResult
    {
        private ErrorList m_errors;
        private MessageList m_messages;

        public override string ToString() => this.ToString(true);

        public string ToString(bool includeReturnResult)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (includeReturnResult)
            {
                foreach (string message in (List<string>)this.Messages)
                    stringBuilder.AppendLine(message);
            }
            if (this.Success)
            {
                stringBuilder.Append("203 Command Success.");
            }
            else
            {
                stringBuilder.Append("545 Command failed.");
                if (this.ExtendedErrorMessage != null)
                    stringBuilder.AppendFormat(" {0}", (object)this.ExtendedErrorMessage);
            }
            stringBuilder.AppendLine(" (Execution Time:" + (object)Math.Truncate(this.ExecutionTime.TotalMilliseconds) + ")");
            return stringBuilder.ToString();
        }

        public bool Success { get; internal set; }

        public TimeSpan ExecutionTime { get; internal set; }

        public string ExtendedErrorMessage { get; internal set; }

        public MessageList Messages
        {
            get
            {
                if (this.m_messages == null)
                    this.m_messages = new MessageList();
                return this.m_messages;
            }
        }

        public ErrorList Errors
        {
            get
            {
                if (this.m_errors == null)
                    this.m_errors = new ErrorList();
                return this.m_errors;
            }
        }
    }
}
