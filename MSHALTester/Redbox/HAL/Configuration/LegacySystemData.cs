using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Configuration;

public sealed class LegacySystemData : IConfigurationFile, IDisposable
{
    private bool Disposed;

    public LegacySystemData()
    {
        FullSourcePath = System.IO.Path.Combine(Path, FileName);
        SystemData = new Dictionary<string, int>();
        OnReadSystemData((key, value) => SystemData[key] = value);
        KioskConfig = KioskConfiguration.None;
        if (SystemData.Keys.Count <= 0)
            return;
        var key1 = "PlatterMaxSlots1";
        if (!SystemData.ContainsKey(key1))
            return;
        if (72 == SystemData[key1])
        {
            KioskConfig = KioskConfiguration.R504;
        }
        else
        {
            var key2 = "QLMDeckNumber";
            if (!SystemData.ContainsKey(key2))
                return;
            KioskConfig = SystemData[key2] == 0 ? KioskConfiguration.R717 : KioskConfiguration.R630;
        }
    }

    public IDictionary<string, int> SystemData { get; }

    public KioskConfiguration KioskConfig { get; private set; }

    public SystemConfigurations Type => SystemConfigurations.SystemData;

    public string Path => "c:\\Gamp";

    public string FileName => "SystemData.dat";

    public string FullSourcePath { get; }

    public void ImportFrom(IConfigurationFile config, ErrorList errors)
    {
        throw new NotImplementedException();
    }

    public ConversionResult ConvertTo(KioskConfiguration newConfig, ErrorList errors)
    {
        if (SystemData.Keys.Count == 0)
            return ConversionResult.InvalidFile;
        if (KioskConfig == KioskConfiguration.R504)
            return ConversionResult.UnsupportedConversion;
        return KioskConfiguration.R630 == KioskConfig && (!ConvertToVMZ() || !Write())
            ? ConversionResult.Failure
            : ConversionResult.Success;
    }

    public void Dispose()
    {
        if (Disposed)
            return;
        Disposed = true;
        SystemData.Clear();
    }

    public bool Write()
    {
        if (SystemData.Keys.Count == 0)
            return false;
        using (var streamWriter = new StreamWriter(FullSourcePath))
        {
            foreach (var key in SystemData.Keys)
                streamWriter.WriteLine("{0},{1}", key, SystemData[key]);
        }

        return true;
    }

    private bool ConvertToVMZ()
    {
        if (SystemData.Keys.Count == 0)
            return false;
        for (var index = 1; index <= 9; ++index)
        {
            var key = string.Format("PlatterMaxSlots{0}", index);
            if (SystemData.ContainsKey(key))
                SystemData[key] = 90;
        }

        if (SystemData.ContainsKey("QLMDeckNumber"))
            SystemData["QLMDeckNumber"] = 0;
        if (SystemData.ContainsKey("LastDecknNumber"))
            SystemData["LastDecknNumber"] = 9;
        KioskConfig = KioskConfiguration.R717;
        return true;
    }

    private void OnReadSystemData(Action<string, int> action)
    {
        if (!File.Exists(FullSourcePath))
            return;
        foreach (var readAllLine in File.ReadAllLines(FullSourcePath))
        {
            var strArray = readAllLine.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int result;
            if (strArray.Length >= 2 && int.TryParse(strArray[1], out result))
                action(strArray[0], result);
        }
    }
}