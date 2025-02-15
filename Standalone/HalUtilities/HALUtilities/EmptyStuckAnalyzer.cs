using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace HALUtilities
{
    internal class EmptyStuckAnalyzer
    {
        private EmptyStuckAnalyzer()
        {
        }

        internal static EmptyStuckAnalyzer Instance => Singleton<EmptyStuckAnalyzer>.Instance;

        internal void AnalyzeFile(string file, bool dumpSlots)
        {
            var fileList = new List<string>();
            try
            {
                fileList.Add(file);
                Analyze(fileList, dumpSlots);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("There was an error during analysis.", ex);
            }
            finally
            {
                fileList.Clear();
            }
        }

        internal void AnalyzeByPattern(bool dumpSlots)
        {
            var fileList = new List<string>();
            try
            {
                var files = Directory.GetFiles("c:\\Program Files\\Redbox\\KioskLogs\\Vend", "vend*");
                fileList.AddRange(files);
                Analyze(fileList, dumpSlots);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("There was an error during analysis.", ex);
            }
            finally
            {
                fileList.Clear();
            }
        }

        private void Analyze(List<string> fileList, bool dump)
        {
            var typeStatList = new List<TypeStat>();
            var lookupFailStatList = new List<LookupFailStat>();
            var locStatList = new List<LocStat>();
            try
            {
                typeStatList.Add(new TypeStat(StuckType.Empty));
                typeStatList.Add(new TypeStat(StuckType.Stuck));
                typeStatList.Add(new TypeStat(StuckType.ExtendFail));
                typeStatList.Add(new TypeStat(StuckType.LookupFail));
                var sampleCount = 0;
                var num1 = 0;
                var str1 = "GET Deck=";
                var str2 = "Slot=";
                var service = ServiceLocator.Instance.GetService<IRuntimeService>();
                var path = service.RuntimePath("EmptyStuckAnalysis.log");
                if (File.Exists(path))
                    try
                    {
                        service.CreateBackup(path, BackupAction.Move);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unable to backup analysis file: {0}", ex.Message);
                    }

                using (var log = new StreamWriter(path))
                {
                    using (var enumerator = fileList.GetEnumerator())
                    {
                        label_35:
                        while (enumerator.MoveNext())
                        {
                            var textReader = (TextReader)new StreamReader(enumerator.Current);
                            label_8:
                            try
                            {
                                string str3;
                                int num2;
                                StuckType type;
                                do
                                {
                                    do
                                    {
                                        type = StuckType.None;
                                        str3 = textReader.ReadLine();
                                        if (str3 != null)
                                        {
                                            if (str3.IndexOf("ITEMSTUCK") != -1)
                                            {
                                                type = StuckType.Stuck;
                                            }
                                            else if (str3.IndexOf("SLOTEMPTY") != -1)
                                            {
                                                type = StuckType.Empty;
                                            }
                                            else if (str3.IndexOf("GRIPPEREXTEND") != -1)
                                            {
                                                type = StuckType.ExtendFail;
                                            }
                                            else if (str3.IndexOf("LOOKUP") != -1)
                                            {
                                                type = StuckType.LookupFail;
                                                ++sampleCount;
                                                typeStatList.Find(each => each.Type == type).Increment();
                                                if (type == StuckType.LookupFail)
                                                {
                                                    var startIndex = str3.IndexOf("item") + 5;
                                                    var num3 = str3.IndexOf("returned") - 1;
                                                    var _bc = str3.Substring(startIndex, num3 - startIndex);
                                                    var lookupFailStat =
                                                        lookupFailStatList.Find(each => each.Matrix == _bc);
                                                    if (lookupFailStat == null)
                                                    {
                                                        lookupFailStat = new LookupFailStat(_bc);
                                                        lookupFailStatList.Add(lookupFailStat);
                                                    }

                                                    lookupFailStat.Increment();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            goto label_35;
                                        }
                                    } while (type == StuckType.None);

                                    num2 = str3.IndexOf(str1);
                                } while (-1 == num2);

                                ++sampleCount;
                                typeStatList.Find(each => each.Type == type).Increment();
                                var deck = int.Parse(str3.Substring(num2 + str1.Length, 1));
                                var num4 = str3.IndexOf(str2);
                                var str4 = str3.Substring(num4 + str2.Length);
                                var charArray = str4.ToCharArray();
                                for (var index = 0; index < charArray.Length; ++index)
                                    if (char.IsWhiteSpace(charArray[index]))
                                    {
                                        var s = str4.Substring(0, index + 1);
                                        int _targetSlot;
                                        if (!int.TryParse(s, out _targetSlot))
                                        {
                                            log.WriteLine("Unable to parse {0}", s);
                                            Environment.Exit(1);
                                        }

                                        var locStat = locStatList.Find(each =>
                                            each.Deck == deck && each.Slot == _targetSlot);
                                        if (locStat == null)
                                        {
                                            locStat = new LocStat(deck, _targetSlot);
                                            locStatList.Add(locStat);
                                        }

                                        locStat.Increment();
                                        if (_targetSlot >= 1 && _targetSlot <= 15) ++num1;
                                        break;
                                    }

                                goto label_8;
                            }
                            finally
                            {
                                textReader?.Dispose();
                            }
                        }
                    }

                    log.WriteLine("Analysis: There were {0} total samples.", sampleCount);
                    lookupFailStatList.ForEach(fail =>
                        log.WriteLine(" There were {0} lookup failures for matrix {1}", fail.Count, fail.Matrix));
                    log.WriteLine(" Breakdown by deck: ");
                    for (var _d = 1; _d <= 8; _d++)
                    {
                        var all = locStatList.FindAll(each => each.Deck == _d);
                        log.WriteLine("  Deck {0} had {1} unique failures.", _d, all.Count);
                        if (dump)
                            all.ForEach(_s => log.WriteLine("   Slot {0} had {1} failures.", _s.Slot, _s.Count));
                    }

                    log.WriteLine(" VMZ breakdown");
                    var pct = num1 / (decimal)sampleCount;
                    pct *= 100M;
                    log.WriteLine("    Of those, {0} were in the VMZ ( {1}% )", num1, pct.ToString("00.00"));
                    log.WriteLine(" Failure type:");
                    typeStatList.ForEach(t =>
                    {
                        pct = t.Count / (decimal)sampleCount * 100M;
                        log.WriteLine("  There were {0} instances of {1} ( {2}% )", t.Count, t.Type.ToString(),
                            pct.ToString("00.00"));
                    });
                }
            }
            finally
            {
                typeStatList.Clear();
                lookupFailStatList.Clear();
                locStatList.Clear();
            }
        }
    }
}