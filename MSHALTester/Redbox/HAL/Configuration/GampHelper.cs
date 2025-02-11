using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Configuration;

public sealed class GampHelper
{
    public const string GampPath = "c:\\Gamp";
    internal const int DenseQuadrants = 6;
    internal const int SparseQuadrants = 12;
    internal const int DenseSlotsPerQuadrant = 15;
    internal const int SparseSlotsPerQuadrant = 6;
    internal const int DefaultSellThru = 915;
    internal const decimal DenseSlotWidth = 166.6667M;
    internal const decimal QlmSlotWidth = 177.7M;
    internal const string SlotData = "C:\\gamp\\SlotData.dat";
    internal const string SystemData = "C:\\gamp\\SystemData.dat";

    public int GetPlatterSlots()
    {
        var intListList = new GampHelper().ReadLegacySlotDataFile(false);
        if (intListList.Count == 0)
            return -1;
        return intListList[0].Count == 7 || intListList[0][7] == 0 ? 90 : 72;
    }

    public IDictionary<string, int> ReadLegacySystemDataFile()
    {
        var parameters = new Dictionary<string, int>();
        OnReadSystemData((key, value) => parameters[key] = value);
        return parameters;
    }

    public GampBackupResult WriteSystemData(IDictionary<string, int> data, bool createBackup)
    {
        var service = ServiceLocator.Instance.GetService<IRuntimeService>();
        var gampBackupResult = new GampBackupResult();
        if (createBackup)
            gampBackupResult.BackupFile = service.CreateBackup("C:\\gamp\\SystemData.dat", BackupAction.Move);
        using (var streamWriter = new StreamWriter("C:\\gamp\\SystemData.dat"))
        {
            foreach (var key in data.Keys)
                streamWriter.WriteLine("{0},{1}", key, data[key]);
        }

        gampBackupResult.Success = true;
        return gampBackupResult;
    }

    public GampBackupResult WriteSlotDataFile(List<List<int>> data, bool createBackup)
    {
        var gampBackupResult = new GampBackupResult();
        var service = ServiceLocator.Instance.GetService<IRuntimeService>();
        if (createBackup)
            gampBackupResult.BackupFile = service.CreateBackup("C:\\gamp\\SlotData.dat", BackupAction.Move);
        using (var streamWriter = new StreamWriter("C:\\gamp\\SlotData.dat"))
        {
            var stringBuilder = new StringBuilder();
            foreach (var intList in data)
            {
                foreach (var num in intList)
                {
                    stringBuilder.Append(num);
                    stringBuilder.Append(",");
                }

                stringBuilder.Remove(stringBuilder.Length - 1, 1);
                streamWriter.WriteLine(stringBuilder.ToString());
                stringBuilder.Capacity = 512;
                stringBuilder.Length = 0;
            }
        }

        gampBackupResult.Success = true;
        return gampBackupResult;
    }

    public List<List<int>> ReadLegacySlotDataFile(bool removeZeroes)
    {
        var path = "C:\\gamp\\SlotData.dat";
        var intListList = new List<List<int>>();
        if (!File.Exists(path))
            return intListList;
        var num = 1;
        var flag = false;
        var stringBuilder = new StringBuilder();
        foreach (var readAllLine in File.ReadAllLines(path))
        {
            var intList = new List<int>();
            int result;
            for (var index = 0; index < readAllLine.Length; ++index)
                if (char.IsWhiteSpace(readAllLine[index]) || readAllLine[index] == ',')
                {
                    if (stringBuilder.Length > 0)
                    {
                        if (!int.TryParse(stringBuilder.ToString(), out result))
                        {
                            result = 0;
                            LogHelper.Instance.Log(
                                string.Format("The value {0} in slotdata.dat (line {1}) is invalid",
                                    stringBuilder.ToString(), num), LogEntryType.Error);
                            flag = true;
                        }
                        else
                        {
                            intList.Add(result);
                        }

                        stringBuilder.Remove(0, stringBuilder.Length);
                    }
                }
                else
                {
                    stringBuilder.Append(readAllLine[index]);
                }

            if (stringBuilder.Length > 0)
            {
                if (!int.TryParse(stringBuilder.ToString(), out result))
                {
                    result = 0;
                    LogHelper.Instance.Log(
                        string.Format("The value {0} in slotdata.dat (line {1}) is invalid", stringBuilder.ToString(),
                            num), LogEntryType.Error);
                    flag = true;
                }
                else
                {
                    intList.Add(result);
                }

                stringBuilder.Remove(0, stringBuilder.Length);
            }

            ++num;
            if (intList.Count > 0)
                intListList.Add(intList);
        }

        if (flag)
            intListList.Clear();
        else if (removeZeroes)
            foreach (var intList in intListList)
                intList.RemoveAll(each => each == 0);
        return intListList;
    }

    public List<int> ComputeDenseQuadrants(decimal? startOffset)
    {
        var denseQuadrants = new List<int>();
        var nullable1 = startOffset;
        denseQuadrants.Add((int)nullable1.Value);
        for (var index = 0; index < 5; ++index)
        {
            decimal? nullable2 = 2666.6672M;
            var nullable3 = nullable1;
            var nullable4 = nullable2;
            nullable1 = nullable3.HasValue & nullable4.HasValue
                ? nullable3.GetValueOrDefault() + nullable4.GetValueOrDefault()
                : new decimal?();
            denseQuadrants.Add((int)nullable1.Value);
        }

        return denseQuadrants;
    }

    public void ConvertToVMZ()
    {
        var data = ReadLegacySystemDataFile();
        for (var index = 1; index <= 9; ++index)
        {
            var key = string.Format("PlatterMaxSlots{0}", index);
            if (data.ContainsKey(key))
                data[key] = 90;
        }

        if (data.ContainsKey("QLMDeckNumber"))
            data["QLMDeckNumber"] = 0;
        if (data.ContainsKey("LastDecknNumber"))
            data["LastDecknNumber"] = 9;
        WriteSystemData(data, true);
    }

    private void OnReadSystemData(Action<string, int> action)
    {
        if (!File.Exists("C:\\gamp\\SystemData.dat"))
            return;
        foreach (var readAllLine in File.ReadAllLines("C:\\gamp\\SystemData.dat"))
        {
            var strArray = readAllLine.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int result;
            if (strArray.Length >= 2 && int.TryParse(strArray[1], out result))
                action(strArray[0], result);
        }
    }
}