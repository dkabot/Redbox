using System;
using System.IO;
using System.Runtime.Serialization;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class ApplicationLog : IFormattedLog
    {
        private const int MaxFileSize = 1024000;
        private const int BackDays = 60;
        private const int BackDaysForImages = 168;
        internal readonly ExecutionContext Context;
        private bool Configured;
        private string Directory;
        private string FileName;
        private TextWriter Log = StreamWriter.Null;
        private bool m_open;

        internal ApplicationLog(ExecutionContext ctx)
        {
            Context = ctx;
            Log = StreamWriter.Null;
        }

        internal bool AppendToExisting { get; private set; }

        internal bool FlushOnWrite { get; private set; }

        public void WriteFormatted(string msg)
        {
            Write(msg);
        }

        public void WriteFormatted(string fmt, params object[] parm)
        {
            Write(string.Format(fmt, parm));
        }

        public static implicit operator TextWriter(ApplicationLog log)
        {
            return log.Log;
        }

        internal void Serialize(SerializationInfo info)
        {
            info.AddValue("AppHasLog", Configured);
            if (!Configured)
                return;
            info.AddValue("AppLogDirectory", Directory);
            info.AddValue("AppLogName", FileName);
            info.AddValue("AppLogFlushOnWrite", FlushOnWrite);
            info.AddValue("AppLogAppendToExisting", AppendToExisting);
        }

        internal static ApplicationLog Deserialize(SerializationInfo info, ExecutionContext context)
        {
            var applicationLog = new ApplicationLog(context);
            if (!info.GetBoolean("AppHasLog"))
                return applicationLog;
            applicationLog.Directory = info.GetString("AppLogDirectory");
            applicationLog.FileName = info.GetString("AppLogName");
            applicationLog.AppendToExisting = info.GetBoolean("AppLogAppendToExisting");
            applicationLog.FlushOnWrite = info.GetBoolean("AppLogFlushOnWrite");
            applicationLog.Configured = true;
            return applicationLog;
        }

        internal static ApplicationLog ConfigureLog(
            ExecutionContext ctx,
            bool logAppends,
            string subDirectory,
            bool flushOnWrite,
            string overrideName)
        {
            var fileName = !logAppends
                ? string.Format("{0}-{1}.log", ctx.ProgramName, ctx.ID)
                : string.Format("{0}.log", string.IsNullOrEmpty(overrideName) ? ctx.ProgramName : (object)overrideName);
            if (ControllerConfiguration.Instance.IsVMZMachine && ctx.ProgramName.Equals("qlm-unload"))
                fileName = string.Format("VMZ-Unload-{0}.log", ctx.ID);
            ctx.AppLog.ConfigureAndOpen(subDirectory, fileName, flushOnWrite, logAppends);
            return ctx.AppLog;
        }

        internal void Write(string msg)
        {
            try
            {
                var now1 = DateTime.Now;
                var shortDateString1 = now1.ToShortDateString();
                now1 = DateTime.Now;
                var shortTimeString1 = now1.ToShortTimeString();
                var str1 = string.Format("{0} {1}", shortDateString1, shortTimeString1);
                string str2;
                if (!AppendToExisting)
                {
                    var now2 = DateTime.Now;
                    var shortTimeString2 = now2.ToShortTimeString();
                    now2 = DateTime.Now;
                    var shortDateString2 = now2.ToShortDateString();
                    var str3 = msg;
                    str2 = string.Format("{0} {1} {2}", shortTimeString2, shortDateString2, str3);
                }
                else
                {
                    str2 = string.Format("{0} {1} {2}", str1, Context.ID, msg);
                }

                Log.WriteLine(str2);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("APPLOG caught an exception writing data.", ex);
            }
        }

        internal void Close()
        {
            if (!m_open)
                return;
            Log.Flush();
            Log.Close();
            m_open = false;
            Log = StreamWriter.Null;
        }

        internal bool Open()
        {
            if (!Configured)
                return false;
            if (m_open)
                return true;
            var path = Path.Combine(Directory, FileName);
            try
            {
                Log = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    AutoFlush = FlushOnWrite
                };
                return m_open = true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(string.Format("Unable to open file {0}", path), LogEntryType.Error);
                Log = StreamWriter.Null;
                return false;
            }
        }

        private void ConfigureAndOpen(
            string directory,
            string fileName,
            bool flushOnWrite,
            bool appends)
        {
            var logsBasePath = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().LogsBasePath;
            var path = logsBasePath;
            if (!string.IsNullOrEmpty(directory))
            {
                path = Path.Combine(logsBasePath, directory);
                if (!System.IO.Directory.Exists(path))
                    try
                    {
                        System.IO.Directory.CreateDirectory(path);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log("Unable to create subdirectory.", ex);
                        path = logsBasePath;
                    }
            }

            Configured = true;
            Directory = path;
            FileName = fileName;
            FlushOnWrite = flushOnWrite;
            AppendToExisting = appends;
            var str = Path.Combine(Directory, FileName);
            if (AppendToExisting && File.Exists(str) && new FileInfo(str).Length >= 1024000L)
                CreateBackup(str);
            Open();
        }

        private void CreateBackup(string originalPath)
        {
            var now = DateTime.Now;
            var destFileName = Path.Combine(Directory,
                string.Format("{0}-m{1}d{2}y{3}h{4}m{5}s{6}.log", FileName, now.Month, now.Day, now.Year, now.Hour,
                    now.Minute, now.Second));
            try
            {
                File.Move(originalPath, destFileName);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(string.Format("Unable to move {0} to {1}.", originalPath, destFileName), ex);
            }
        }
    }
}