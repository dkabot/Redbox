using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.IPC.Framework
{
    internal class ClientCommandResult
    {
        private ErrorList m_errors;
        private List<string> m_commandMessages;

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string commandMessage in this.CommandMessages)
                stringBuilder.AppendLine(commandMessage);
            foreach (Error error in (List<Error>)this.Errors)
            {
                stringBuilder.AppendLine(error.ToString());
                stringBuilder.AppendFormat("Details: {0}\n\n", (object)error.Details);
            }
            if (this.Success)
                stringBuilder.Append("203 Command Success. (Execution Time:" + (object)Math.Truncate(this.ExecutionTime.TotalMilliseconds) + ")");
            else if (!string.IsNullOrEmpty(this.StatusMessage))
                stringBuilder.Append(this.StatusMessage);
            else
                stringBuilder.Append("545 Command failed. (Execution Time:" + (object)Math.Truncate(this.ExecutionTime.TotalMilliseconds) + ")");
            stringBuilder.AppendLine();
            return stringBuilder.ToString();
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

        public List<string> CommandMessages
        {
            get
            {
                if (this.m_commandMessages == null)
                    this.m_commandMessages = new List<string>();
                return this.m_commandMessages;
            }
        }

        public bool Success { get; set; }

        public string CommandText { get; internal set; }

        public string StatusMessage { get; internal set; }

        public TimeSpan ExecutionTime { get; internal set; }
    }
}
