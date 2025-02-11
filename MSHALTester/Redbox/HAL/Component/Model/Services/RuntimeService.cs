using System;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.Component.Model.Services;

public sealed class RuntimeService : IRuntimeService
{
    public const string RunningPath = "RunningPath";
    private readonly WaitHandle m_waitObject = new ManualResetEvent(false);
    private readonly StringDictionary Values = new();
    private string m_kioskId;
    private Platform Platform;

    public RuntimeService()
    {
        AssemblyDirectory = Path.GetDirectoryName(typeof(RuntimeService).Assembly.Location);
        DataPath = Path.Combine(AssemblyDirectory, "data");
        try
        {
            InstallRoot = Directory.GetParent(AssemblyDirectory).FullName;
            ScriptsPath = Path.Combine(InstallRoot, "Scripts");
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log("[RuntimeService] Unable to find parent folder.", ex.Message);
            InstallRoot = AssemblyDirectory;
        }

        Platform = Platform.None;
        Values[nameof(RunningPath)] = AssemblyDirectory;
        var lpSystemInfo = new SYSTEM_INFO();
        GetSystemInfo(ref lpSystemInfo);
        PageSize = (int)lpSystemInfo.dwPageSize;
    }

    public string ExpandRuntimeMacros(string value)
    {
        return ExpandMacros(value, "${", "}");
    }

    public string ExpandConstantMacros(string value)
    {
        return ExpandMacros(value, "$(", ")");
    }

    public string RuntimePath(string fileName)
    {
        return Path.Combine(AssemblyDirectory, fileName);
    }

    public string InstallPath(string subfolder)
    {
        return Path.Combine(InstallRoot, subfolder);
    }

    public string CreateBackup(string path)
    {
        return CreateBackup(path, BackupAction.Delete);
    }

    public string CreateBackup(string path, BackupAction action)
    {
        if (string.IsNullOrEmpty(path))
            return path;
        var backupName = CreateBackupName(path);
        if (action == BackupAction.Move)
            File.Move(path, backupName);
        else if (BackupAction.Copy == action)
            File.Copy(path, backupName);
        else if (BackupAction.Delete == action)
            SafeDelete(path);
        return backupName;
    }

    public string GenerateUniqueFile(string suffix)
    {
        if (!suffix.StartsWith("."))
            suffix = string.Format(".{0}", suffix);
        var now = DateTime.Now;
        return string.Format("m{0}d{1}y{2}_h{3}m{4}s{5}t{6}{7}", now.Month, now.Day, now.Year, now.Hour, now.Minute,
            now.Second, now.Millisecond, suffix);
    }

    public byte[] ReadToBuffer(string fileName)
    {
        if (fileName == null)
            return null;
        var buffer = (byte[])null;
        using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            buffer = new byte[fileStream.Length];
            fileStream.Read(buffer, 0, (int)fileStream.Length);
        }

        return buffer;
    }

    public bool ForceSafeCopy(string source, string dest)
    {
        return ForceSafeCopy(TextWriter.Null, source, dest);
    }

    public bool ForceSafeCopy(TextWriter w, string source, string dest)
    {
        return OnCopy(w, source, dest, true);
    }

    public bool SafeCopy(string source, string dest)
    {
        return SafeCopy(TextWriter.Null, source, dest);
    }

    public bool SafeCopy(TextWriter writer, string source, string dest)
    {
        return OnCopy(writer, source, dest, false);
    }

    public bool SafeMove(string source, string dest)
    {
        return SafeMove(TextWriter.Null, source, dest);
    }

    public bool SafeMove(TextWriter writer, string source, string dest)
    {
        var str = string.Format("move {0} -> {1}", source, dest);
        try
        {
            File.Move(source, dest);
            writer.WriteLine("[{0}] {1}: SUCCESS", GetType().Name, str);
            return true;
        }
        catch (Exception ex)
        {
            writer.WriteLine("[{0}] {1}: FAILURE reason = {2}", GetType().Name, str, ex.Message);
            return false;
        }
    }

    public bool SafeDelete(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return false;
        try
        {
            File.Delete(path);
            return true;
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log("[{0}] Delete file failed:", GetType().Name);
            LogHelper.Instance.Log(ex.Message);
            return false;
        }
    }

    public Platform GetPlatform()
    {
        if (Platform != Platform.None)
            return Platform;
        Platform = Platform.Unknown;
        var osVersion = Environment.OSVersion;
        if (osVersion.Platform != PlatformID.Win32NT)
            return Platform;
        switch (osVersion.Version.Major)
        {
            case 5:
                if (osVersion.Version.Minor != 0) Platform = Platform.WindowsXP;
                break;
            case 6:
                if (osVersion.Version.Minor != 0) Platform = Platform.Windows7;
                break;
        }

        return Platform;
    }

    public void SpinWait(int milliseconds)
    {
        SpinWait(new TimeSpan(0, 0, 0, 0, milliseconds));
    }

    public void SpinWait(TimeSpan timespan)
    {
        using (var executionTimer = new ExecutionTimer())
        {
            do
            {
                ;
            } while (executionTimer.ElapsedMilliseconds < timespan.TotalMilliseconds);
        }
    }

    public void Wait(int ms)
    {
        if (ms < 1000)
            SpinWait(ms);
        else
            m_waitObject.WaitOne(ms, false);
    }

    public string KioskId
    {
        get
        {
            if (!string.IsNullOrEmpty(m_kioskId))
                return m_kioskId;
            var registryKey = (RegistryKey)null;
            try
            {
                registryKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store");
                if (registryKey != null)
                {
                    var obj = registryKey.GetValue("ID");
                    if (obj != null)
                        m_kioskId = ConversionHelper.ChangeType<string>(obj);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("[{0}] Unable to read Kiosk ID", GetType().Name);
                LogHelper.Instance.Log(ex.Message);
                m_kioskId = null;
            }
            finally
            {
                registryKey?.Close();
            }

            return !string.IsNullOrEmpty(m_kioskId) ? m_kioskId : "UNKNOWN";
        }
    }

    public string DataPath { get; }

    public string AssemblyDirectory { get; }

    public string InstallRoot { get; }

    public int PageSize { get; }

    public string ScriptsPath { get; }

    private bool OnCopy(TextWriter writer, string source, string dest, bool force)
    {
        var str = string.Format("copy {0} -> {1}", source, dest);
        try
        {
            File.Copy(source, dest, force);
            writer.WriteLine("[{0}] {1}: SUCCESS", GetType().Name, str);
            return true;
        }
        catch (Exception ex)
        {
            LogHelper.Instance.WithContext(false, LogEntryType.Error, "COPY FAILURE {0}: reason = {1}", str,
                ex.Message);
            return false;
        }
    }

    private string CreateBackupName(string path)
    {
        var withoutExtension = Path.GetFileNameWithoutExtension(path);
        var directoryName = Path.GetDirectoryName(path);
        var extension = Path.GetExtension(path);
        var path2 = string.Format("{0}-{1}", withoutExtension, GenerateUniqueFile(extension));
        return Path.Combine(directoryName, path2);
    }

    private string ExpandMacros(string value, string preamble, string postamble)
    {
        if (value == null)
            return null;
        while (true)
        {
            var codeFromBrackets = StringExtensions.ExtractCodeFromBrackets(value, preamble, postamble);
            if (!string.IsNullOrEmpty(codeFromBrackets))
            {
                var newValue = Values[codeFromBrackets] ?? string.Empty;
                value = value.Replace(string.Format("{0}{1}{2}", preamble, codeFromBrackets, postamble), newValue);
            }
            else
            {
                break;
            }
        }

        return value;
    }

    [DllImport("kernel32.dll")]
    private static extern void GetSystemInfo([MarshalAs(UnmanagedType.Struct)] ref SYSTEM_INFO lpSystemInfo);

    private struct SYSTEM_INFO
    {
        internal _PROCESSOR_INFO_UNION uProcessorInfo;
        public uint dwPageSize;
        public IntPtr lpMinimumApplicationAddress;
        public IntPtr lpMaximumApplicationAddress;
        public IntPtr dwActiveProcessorMask;
        public uint dwNumberOfProcessors;
        public uint dwProcessorType;
        public uint dwAllocationGranularity;
        public ushort dwProcessorLevel;
        public ushort dwProcessorRevision;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct _PROCESSOR_INFO_UNION
    {
        [FieldOffset(0)] internal uint dwOemId;
        [FieldOffset(0)] internal ushort wProcessorArchitecture;
        [FieldOffset(2)] internal ushort wReserved;
    }
}