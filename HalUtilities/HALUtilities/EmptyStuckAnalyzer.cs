using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace HALUtilities
{
  internal class EmptyStuckAnalyzer
  {
    internal static EmptyStuckAnalyzer Instance => Singleton<EmptyStuckAnalyzer>.Instance;

    internal void AnalyzeFile(string file, bool dumpSlots)
    {
      List<string> fileList = new List<string>();
      try
      {
        fileList.Add(file);
        this.Analyze(fileList, dumpSlots);
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
      List<string> fileList = new List<string>();
      try
      {
        string[] files = Directory.GetFiles("c:\\Program Files\\Redbox\\KioskLogs\\Vend", "vend*");
        fileList.AddRange((IEnumerable<string>) files);
        this.Analyze(fileList, dumpSlots);
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
      List<TypeStat> typeStatList = new List<TypeStat>();
      List<LookupFailStat> lookupFailStatList = new List<LookupFailStat>();
      List<LocStat> locStatList = new List<LocStat>();
      try
      {
        typeStatList.Add(new TypeStat(StuckType.Empty));
        typeStatList.Add(new TypeStat(StuckType.Stuck));
        typeStatList.Add(new TypeStat(StuckType.ExtendFail));
        typeStatList.Add(new TypeStat(StuckType.LookupFail));
        int sampleCount = 0;
        int num1 = 0;
        string str1 = "GET Deck=";
        string str2 = "Slot=";
        IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
        string path = service.RuntimePath("EmptyStuckAnalysis.log");
        if (File.Exists(path))
        {
          try
          {
            service.CreateBackup(path, BackupAction.Move);
          }
          catch (Exception ex)
          {
            Console.WriteLine("Unable to backup analysis file: {0}", (object) ex.Message);
          }
        }
        using (StreamWriter log = new StreamWriter(path))
        {
          using (List<string>.Enumerator enumerator = fileList.GetEnumerator())
          {
label_35:
            while (enumerator.MoveNext())
            {
              TextReader textReader = (TextReader) new StreamReader(enumerator.Current);
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
                        type = StuckType.Stuck;
                      else if (str3.IndexOf("SLOTEMPTY") != -1)
                        type = StuckType.Empty;
                      else if (str3.IndexOf("GRIPPEREXTEND") != -1)
                        type = StuckType.ExtendFail;
                      else if (str3.IndexOf("LOOKUP") != -1)
                      {
                        type = StuckType.LookupFail;
                        ++sampleCount;
                        typeStatList.Find((Predicate<TypeStat>) (each => each.Type == type)).Increment();
                        if (type == StuckType.LookupFail)
                        {
                          int startIndex = str3.IndexOf("item") + 5;
                          int num3 = str3.IndexOf("returned") - 1;
                          string _bc = str3.Substring(startIndex, num3 - startIndex);
                          LookupFailStat lookupFailStat = lookupFailStatList.Find((Predicate<LookupFailStat>) (each => each.Matrix == _bc));
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
                      goto label_35;
                  }
                  while (type == StuckType.None);
                  num2 = str3.IndexOf(str1);
                }
                while (-1 == num2);
                ++sampleCount;
                typeStatList.Find((Predicate<TypeStat>) (each => each.Type == type)).Increment();
                int deck = int.Parse(str3.Substring(num2 + str1.Length, 1));
                int num4 = str3.IndexOf(str2);
                string str4 = str3.Substring(num4 + str2.Length);
                char[] charArray = str4.ToCharArray();
                for (int index = 0; index < charArray.Length; ++index)
                {
                  if (char.IsWhiteSpace(charArray[index]))
                  {
                    string s = str4.Substring(0, index + 1);
                    int _targetSlot;
                    if (!int.TryParse(s, out _targetSlot))
                    {
                      log.WriteLine("Unable to parse {0}", (object) s);
                      Environment.Exit(1);
                    }
                    LocStat locStat = locStatList.Find((Predicate<LocStat>) (each => each.Deck == deck && each.Slot == _targetSlot));
                    if (locStat == null)
                    {
                      locStat = new LocStat(deck, _targetSlot);
                      locStatList.Add(locStat);
                    }
                    locStat.Increment();
                    if (_targetSlot >= 1 && _targetSlot <= 15)
                    {
                      ++num1;
                      break;
                    }
                    break;
                  }
                }
                goto label_8;
              }
              finally
              {
                textReader?.Dispose();
              }
            }
          }
          log.WriteLine("Analysis: There were {0} total samples.", (object) sampleCount);
          lookupFailStatList.ForEach((Action<LookupFailStat>) (fail => log.WriteLine(" There were {0} lookup failures for matrix {1}", (object) fail.Count, (object) fail.Matrix)));
          log.WriteLine(" Breakdown by deck: ");
          for (int _d = 1; _d <= 8; _d++)
          {
            List<LocStat> all = locStatList.FindAll((Predicate<LocStat>) (each => each.Deck == _d));
            log.WriteLine("  Deck {0} had {1} unique failures.", (object) _d, (object) all.Count);
            if (dump)
              all.ForEach((Action<LocStat>) (_s => log.WriteLine("   Slot {0} had {1} failures.", (object) _s.Slot, (object) _s.Count)));
          }
          log.WriteLine(" VMZ breakdown");
          Decimal pct = (Decimal) num1 / (Decimal) sampleCount;
          pct *= 100M;
          log.WriteLine("    Of those, {0} were in the VMZ ( {1}% )", (object) num1, (object) pct.ToString("00.00"));
          log.WriteLine(" Failure type:");
          typeStatList.ForEach((Action<TypeStat>) (t =>
          {
            pct = (Decimal) t.Count / (Decimal) sampleCount * 100M;
            log.WriteLine("  There were {0} instances of {1} ( {2}% )", (object) t.Count, (object) t.Type.ToString(), (object) pct.ToString("00.00"));
          }));
        }
      }
      finally
      {
        typeStatList.Clear();
        lookupFailStatList.Clear();
        locStatList.Clear();
      }
    }

    private EmptyStuckAnalyzer()
    {
    }
  }
}
