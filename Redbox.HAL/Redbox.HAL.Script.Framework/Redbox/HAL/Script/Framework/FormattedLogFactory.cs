using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    public sealed class FormattedLogFactory : IFormattedLogFactoryService
    {
        public string CreateSubpath(string subfolder)
        {
            var path = Path.Combine(LogsBasePath, subfolder);
            if (!Directory.Exists(path))
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("[FormattedLogFactory] Unable to create path {0}: {1}", path, ex.Message);
                }

            return path;
        }

        public string LogsBasePath => "c:\\Program Files\\Redbox\\KioskLogs";

        public IFormattedLog NilLog { get; } = new NullLog();

        private class NullLog : IFormattedLog
        {
            public void WriteFormatted(string msg)
            {
            }

            public void WriteFormatted(string format, params object[] stuff)
            {
            }

            public void WriteFormatted(string msg, LogEntryType type)
            {
            }

            public void WriteFormatted(LogEntryType type, string format, params object[] stuff)
            {
            }
        }
    }
}