using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;

namespace HALUtilities
{
    internal class SyncCheck
    {
        private readonly string JobID;
        private readonly List<SyncLocation> Locations = new List<SyncLocation>();
        private readonly IRuntimeService RuntimeService;

        internal SyncCheck(string jobId, IRuntimeService rts)
        {
            JobID = jobId;
            RuntimeService = rts;
        }

        internal void Summarize()
        {
            var path1 = "c:\\Program Files\\Redbox\\KioskLogs\\Sync";
            var str1 = Path.Combine(path1, string.Format("sync-{0}.log", JobID));
            if (File.Exists(str1))
            {
                ParseFile(str1);
            }
            else
            {
                var str2 = Path.Combine(path1, string.Format("sync-locations-{0}.log", JobID));
                if (!File.Exists(str2))
                    return;
                ParseFile(str2);
            }
        }

        internal void AnalyzeCortex(string file)
        {
            var path = RuntimeService.RuntimePath(string.Format("Cortex-{0}-analysis.log", JobID));
            if (File.Exists(path))
                try
                {
                    RuntimeService.CreateBackup(path, BackupAction.Move);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to backup analysis file: {0}", ex.Message);
                }

            var readData = (ReadData)null;
            var readDataList = new List<ReadData>();
            try
            {
                using (var streamWriter = new StreamWriter(path))
                {
                    streamWriter.WriteLine("Analyzing file '{0}'", file);
                    using (var textReader = (TextReader)new StreamReader(file))
                    {
                        while (true)
                        {
                            string str1;
                            string str2;
                            do
                            {
                                do
                                {
                                    do
                                    {
                                        str1 = textReader.ReadLine();
                                        if (str1 != null)
                                        {
                                            if (str1.IndexOf("Sync location") != -1)
                                            {
                                                var str3 = "Deck = ";
                                                var str4 = "Slot = ";
                                                var num1 = str1.IndexOf(str3);
                                                if (-1 != num1)
                                                {
                                                    var result = -1;
                                                    var deck = int.Parse(str1.Substring(num1 + str3.Length, 1));
                                                    var num2 = str1.IndexOf(str4);
                                                    var s = str1.Substring(num2 + str4.Length).TrimEnd();
                                                    if (!int.TryParse(s, out result))
                                                    {
                                                        streamWriter.WriteLine("Unable to parse {0}", s);
                                                        Environment.Exit(1);
                                                    }

                                                    readData = new ReadData(deck, result);
                                                    readDataList.Add(readData);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            goto label_23;
                                        }
                                    } while (str1.Contains("ReadDisc"));

                                    if (str1.Contains("GET: no disk in picker after pull."))
                                        readData.IsEmpty = true;
                                } while (str1.Contains("PickerSensors: 1"));

                                str2 = "data (barcode = ";
                            } while (-1 == str1.IndexOf(">> Reponse packet") || str1.IndexOf(str2) == -1);

                            if (str1.Contains("YES") || str1.Contains("yes"))
                                readData.SecureReadOk();
                            else
                                readData.ReadOk();
                        }
                    }

                    label_23:
                    readDataList.RemoveAll(each => each.IsEmpty);
                    Console.WriteLine("There are {0} readable disks.", readDataList.Count);
                    var all1 = readDataList.FindAll(each => each.SecureFound && !each.Read);
                    Console.WriteLine(" {0} read only the secure code.", all1.Count);
                    all1.ForEach(item => Console.WriteLine("   YesOnly: deck = {0} slot = {1}", item.Deck, item.Slot));
                    var all2 = readDataList.FindAll(each => each.Read && !each.SecureFound);
                    Console.WriteLine(" {0} read only the std code.", all2.Count);
                    all2.ForEach(
                        item => Console.WriteLine("   MatrixOnly: deck = {0} slot = {1}", item.Deck, item.Slot));
                    Console.WriteLine(" {0} read both.",
                        readDataList.FindAll(each => each.Read && each.SecureFound).Count);
                    Console.WriteLine(" {0} were no read.",
                        readDataList.FindAll(each => !each.Read && !each.SecureFound).Count);
                    readDataList.FindAll(each => !each.Read && !each.SecureFound).ForEach(item =>
                        Console.WriteLine("   Noread: deck = {0} slot = {1}", item.Deck, item.Slot));
                }
            }
            finally
            {
                readDataList.Clear();
            }
        }

        private string ParseItem(string statement, int startIdx, int tokLength)
        {
            var str = statement.Substring(startIdx + tokLength);
            var charArray = str.ToCharArray();
            for (var index = 0; index < charArray.Length; ++index)
                if (char.IsWhiteSpace(charArray[index]))
                    return str.Substring(0, index + 1);
            return string.Empty;
        }

        private void ParseFile(string file)
        {
            var path = RuntimeService.RuntimePath(string.Format("Sync-{0}-analysis.log", JobID));
            if (File.Exists(path))
                try
                {
                    RuntimeService.CreateBackup(path, BackupAction.Move);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to backup analysis file: {0}", ex.Message);
                }

            using (var log = new StreamWriter(path))
            {
                log.WriteLine("Analyzing file '{0}'", file);
                using (var textReader = (TextReader)new StreamReader(file))
                {
                    while (true)
                    {
                        var statement = textReader.ReadLine();
                        if (statement != null)
                            try
                            {
                                var newItem = new SyncLocation(statement);
                                var syncLocation = Locations.Find(each =>
                                    each.Deck == newItem.Deck && each.Slot == newItem.Slot);
                                if (syncLocation == null)
                                    Locations.Add(newItem);
                                else
                                    syncLocation.Merge(newItem);
                            }
                            catch (ArgumentException ex)
                            {
                                Console.WriteLine("Ignoring line '{0}'", statement);
                                Console.WriteLine(" Exception: {0}", ex.Message);
                            }
                        else
                            break;
                    }
                }

                log.WriteLine("{0} requested locations to sync.", Locations.Count);
                log.WriteLine("  There are {0} empty slots.", Locations.FindAll(each => each.IsEmpty).Count);
                log.WriteLine("  There are {0} excluded slots.", Locations.FindAll(each => each.IsExcluded).Count);
                Locations.ForEach(loc =>
                {
                    if (!(loc.PutMatrix != loc.GetMatrix))
                        return;
                    log.WriteLine("The location {0}, {1} went from {2} -> {3}", loc.Deck, loc.Slot, loc.GetMatrix,
                        loc.PutMatrix);
                });
            }
        }

        private class SyncLocation
        {
            private const string deckTok = "Deck =";
            private const string slotTok = "Slot =";
            private const string idTok = "ID=";
            internal string GetMatrix;
            internal string PutMatrix;

            internal SyncLocation(string statement)
            {
                if (statement.IndexOf("excluded") != -1)
                {
                    IsExcluded = true;
                    ExtractDeckSlot(statement);
                }
                else
                {
                    var flag1 = statement.IndexOf("PUT") != -1;
                    var flag2 = statement.IndexOf("GET") != -1;
                    if (!flag1 && !flag2)
                        throw new ArgumentException(string.Format("Unrecognized line '{0}'", statement));
                    ExtractDeckSlot(statement);
                    if (statement.IndexOf("SLOTEMPTY") != -1)
                    {
                        GetMatrix = PutMatrix = "EMPTY";
                        IsEmpty = true;
                    }
                    else
                    {
                        var num = statement.IndexOf("ID=");
                        var str = statement.Substring(num + "ID=".Length);
                        if (flag2)
                        {
                            GetMatrix = str;
                        }
                        else
                        {
                            if (!flag1)
                                return;
                            PutMatrix = str;
                        }
                    }
                }
            }

            internal int Deck { get; private set; }

            internal int Slot { get; private set; }

            internal bool IsEmpty { get; }

            internal bool IsExcluded { get; }

            internal void Merge(SyncLocation loc)
            {
                if (string.IsNullOrEmpty(GetMatrix))
                {
                    GetMatrix = loc.GetMatrix;
                }
                else
                {
                    if (!string.IsNullOrEmpty(PutMatrix))
                        return;
                    PutMatrix = loc.PutMatrix;
                }
            }

            private void ExtractDeckSlot(string statement)
            {
                var num1 = statement.IndexOf("Deck =");
                if (-1 == num1)
                    throw new ArgumentException(string.Format("Unrecognized line '{0}'", statement));
                var result = -1;
                Deck = int.Parse(statement.Substring(num1 + "Deck =".Length + 1, 1));
                var num2 = statement.IndexOf("Slot =");
                var str = statement.Substring(num2 + "Slot =".Length + 1);
                var charArray = str.ToCharArray();
                for (var index = 0; index < charArray.Length; ++index)
                    if (char.IsWhiteSpace(charArray[index]))
                    {
                        if (!int.TryParse(str.Substring(0, index + 1), out result))
                            throw new ArgumentException(string.Format("Unrecognized line '{0}'", statement));
                        Slot = result;
                        break;
                    }
            }
        }

        private class ReadData
        {
            private int Reads;
            private int SecureReads;

            internal ReadData(int deck, int slot)
            {
                Deck = deck;
                Slot = slot;
            }

            internal int Deck { get; }

            internal int Slot { get; }

            internal bool Read => Reads > 0;

            internal bool SecureFound => SecureReads > 0;

            internal bool IsEmpty { get; set; }

            internal void ReadOk()
            {
                ++Reads;
            }

            internal void SecureReadOk()
            {
                ++SecureReads;
            }
        }
    }
}