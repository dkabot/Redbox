using Redbox.HAL.Component.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace HALUtilities
{
  internal class SyncCheck
  {
    private readonly string JobID;
    private readonly List<SyncCheck.SyncLocation> Locations = new List<SyncCheck.SyncLocation>();
    private readonly IRuntimeService RuntimeService;

    internal SyncCheck(string jobId, IRuntimeService rts)
    {
      this.JobID = jobId;
      this.RuntimeService = rts;
    }

    internal void Summarize()
    {
      string path1 = "c:\\Program Files\\Redbox\\KioskLogs\\Sync";
      string str1 = Path.Combine(path1, string.Format("sync-{0}.log", (object) this.JobID));
      if (File.Exists(str1))
      {
        this.ParseFile(str1);
      }
      else
      {
        string str2 = Path.Combine(path1, string.Format("sync-locations-{0}.log", (object) this.JobID));
        if (!File.Exists(str2))
          return;
        this.ParseFile(str2);
      }
    }

    internal void AnalyzeCortex(string file)
    {
      string path = this.RuntimeService.RuntimePath(string.Format("Cortex-{0}-analysis.log", (object) this.JobID));
      if (File.Exists(path))
      {
        try
        {
          this.RuntimeService.CreateBackup(path, BackupAction.Move);
        }
        catch (Exception ex)
        {
          Console.WriteLine("Unable to backup analysis file: {0}", (object) ex.Message);
        }
      }
      SyncCheck.ReadData readData = (SyncCheck.ReadData) null;
      List<SyncCheck.ReadData> readDataList = new List<SyncCheck.ReadData>();
      try
      {
        using (StreamWriter streamWriter = new StreamWriter(path))
        {
          streamWriter.WriteLine("Analyzing file '{0}'", (object) file);
          using (TextReader textReader = (TextReader) new StreamReader(file))
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
                        string str3 = "Deck = ";
                        string str4 = "Slot = ";
                        int num1 = str1.IndexOf(str3);
                        if (-1 != num1)
                        {
                          int result = -1;
                          int deck = int.Parse(str1.Substring(num1 + str3.Length, 1));
                          int num2 = str1.IndexOf(str4);
                          string s = str1.Substring(num2 + str4.Length).TrimEnd();
                          if (!int.TryParse(s, out result))
                          {
                            streamWriter.WriteLine("Unable to parse {0}", (object) s);
                            Environment.Exit(1);
                          }
                          readData = new SyncCheck.ReadData(deck, result);
                          readDataList.Add(readData);
                        }
                      }
                    }
                    else
                      goto label_23;
                  }
                  while (str1.Contains("ReadDisc"));
                  if (str1.Contains("GET: no disk in picker after pull."))
                    readData.IsEmpty = true;
                }
                while (str1.Contains("PickerSensors: 1"));
                str2 = "data (barcode = ";
              }
              while (-1 == str1.IndexOf(">> Reponse packet") || str1.IndexOf(str2) == -1);
              if (str1.Contains("YES") || str1.Contains("yes"))
                readData.SecureReadOk();
              else
                readData.ReadOk();
            }
          }
label_23:
          readDataList.RemoveAll((Predicate<SyncCheck.ReadData>) (each => each.IsEmpty));
          Console.WriteLine("There are {0} readable disks.", (object) readDataList.Count);
          List<SyncCheck.ReadData> all1 = readDataList.FindAll((Predicate<SyncCheck.ReadData>) (each => each.SecureFound && !each.Read));
          Console.WriteLine(" {0} read only the secure code.", (object) all1.Count);
          all1.ForEach((Action<SyncCheck.ReadData>) (item => Console.WriteLine("   YesOnly: deck = {0} slot = {1}", (object) item.Deck, (object) item.Slot)));
          List<SyncCheck.ReadData> all2 = readDataList.FindAll((Predicate<SyncCheck.ReadData>) (each => each.Read && !each.SecureFound));
          Console.WriteLine(" {0} read only the std code.", (object) all2.Count);
          all2.ForEach((Action<SyncCheck.ReadData>) (item => Console.WriteLine("   MatrixOnly: deck = {0} slot = {1}", (object) item.Deck, (object) item.Slot)));
          Console.WriteLine(" {0} read both.", (object) readDataList.FindAll((Predicate<SyncCheck.ReadData>) (each => each.Read && each.SecureFound)).Count);
          Console.WriteLine(" {0} were no read.", (object) readDataList.FindAll((Predicate<SyncCheck.ReadData>) (each => !each.Read && !each.SecureFound)).Count);
          readDataList.FindAll((Predicate<SyncCheck.ReadData>) (each => !each.Read && !each.SecureFound)).ForEach((Action<SyncCheck.ReadData>) (item => Console.WriteLine("   Noread: deck = {0} slot = {1}", (object) item.Deck, (object) item.Slot)));
        }
      }
      finally
      {
        readDataList.Clear();
      }
    }

    private string ParseItem(string statement, int startIdx, int tokLength)
    {
      string str = statement.Substring(startIdx + tokLength);
      char[] charArray = str.ToCharArray();
      for (int index = 0; index < charArray.Length; ++index)
      {
        if (char.IsWhiteSpace(charArray[index]))
          return str.Substring(0, index + 1);
      }
      return string.Empty;
    }

    private void ParseFile(string file)
    {
      string path = this.RuntimeService.RuntimePath(string.Format("Sync-{0}-analysis.log", (object) this.JobID));
      if (File.Exists(path))
      {
        try
        {
          this.RuntimeService.CreateBackup(path, BackupAction.Move);
        }
        catch (Exception ex)
        {
          Console.WriteLine("Unable to backup analysis file: {0}", (object) ex.Message);
        }
      }
      using (StreamWriter log = new StreamWriter(path))
      {
        log.WriteLine("Analyzing file '{0}'", (object) file);
        using (TextReader textReader = (TextReader) new StreamReader(file))
        {
          while (true)
          {
            string statement = textReader.ReadLine();
            if (statement != null)
            {
              try
              {
                SyncCheck.SyncLocation newItem = new SyncCheck.SyncLocation(statement);
                SyncCheck.SyncLocation syncLocation = this.Locations.Find((Predicate<SyncCheck.SyncLocation>) (each => each.Deck == newItem.Deck && each.Slot == newItem.Slot));
                if (syncLocation == null)
                  this.Locations.Add(newItem);
                else
                  syncLocation.Merge(newItem);
              }
              catch (ArgumentException ex)
              {
                Console.WriteLine("Ignoring line '{0}'", (object) statement);
                Console.WriteLine(" Exception: {0}", (object) ex.Message);
              }
            }
            else
              break;
          }
        }
        log.WriteLine("{0} requested locations to sync.", (object) this.Locations.Count);
        log.WriteLine("  There are {0} empty slots.", (object) this.Locations.FindAll((Predicate<SyncCheck.SyncLocation>) (each => each.IsEmpty)).Count);
        log.WriteLine("  There are {0} excluded slots.", (object) this.Locations.FindAll((Predicate<SyncCheck.SyncLocation>) (each => each.IsExcluded)).Count);
        this.Locations.ForEach((Action<SyncCheck.SyncLocation>) (loc =>
        {
          if (!(loc.PutMatrix != loc.GetMatrix))
            return;
          log.WriteLine("The location {0}, {1} went from {2} -> {3}", (object) loc.Deck, (object) loc.Slot, (object) loc.GetMatrix, (object) loc.PutMatrix);
        }));
      }
    }

    private class SyncLocation
    {
      internal string GetMatrix;
      internal string PutMatrix;
      private const string deckTok = "Deck =";
      private const string slotTok = "Slot =";
      private const string idTok = "ID=";

      internal int Deck { get; private set; }

      internal int Slot { get; private set; }

      internal bool IsEmpty { get; private set; }

      internal bool IsExcluded { get; private set; }

      internal void Merge(SyncCheck.SyncLocation loc)
      {
        if (string.IsNullOrEmpty(this.GetMatrix))
        {
          this.GetMatrix = loc.GetMatrix;
        }
        else
        {
          if (!string.IsNullOrEmpty(this.PutMatrix))
            return;
          this.PutMatrix = loc.PutMatrix;
        }
      }

      internal SyncLocation(string statement)
      {
        if (statement.IndexOf("excluded") != -1)
        {
          this.IsExcluded = true;
          this.ExtractDeckSlot(statement);
        }
        else
        {
          bool flag1 = statement.IndexOf("PUT") != -1;
          bool flag2 = statement.IndexOf("GET") != -1;
          if (!flag1 && !flag2)
            throw new ArgumentException(string.Format("Unrecognized line '{0}'", (object) statement));
          this.ExtractDeckSlot(statement);
          if (statement.IndexOf("SLOTEMPTY") != -1)
          {
            this.GetMatrix = this.PutMatrix = "EMPTY";
            this.IsEmpty = true;
          }
          else
          {
            int num = statement.IndexOf("ID=");
            string str = statement.Substring(num + "ID=".Length);
            if (flag2)
            {
              this.GetMatrix = str;
            }
            else
            {
              if (!flag1)
                return;
              this.PutMatrix = str;
            }
          }
        }
      }

      private void ExtractDeckSlot(string statement)
      {
        int num1 = statement.IndexOf("Deck =");
        if (-1 == num1)
          throw new ArgumentException(string.Format("Unrecognized line '{0}'", (object) statement));
        int result = -1;
        this.Deck = int.Parse(statement.Substring(num1 + "Deck =".Length + 1, 1));
        int num2 = statement.IndexOf("Slot =");
        string str = statement.Substring(num2 + "Slot =".Length + 1);
        char[] charArray = str.ToCharArray();
        for (int index = 0; index < charArray.Length; ++index)
        {
          if (char.IsWhiteSpace(charArray[index]))
          {
            if (!int.TryParse(str.Substring(0, index + 1), out result))
              throw new ArgumentException(string.Format("Unrecognized line '{0}'", (object) statement));
            this.Slot = result;
            break;
          }
        }
      }
    }

    private class ReadData
    {
      private int Reads;
      private int SecureReads;

      internal int Deck { get; private set; }

      internal int Slot { get; private set; }

      internal bool Read => this.Reads > 0;

      internal bool SecureFound => this.SecureReads > 0;

      internal bool IsEmpty { get; set; }

      internal void ReadOk() => ++this.Reads;

      internal void SecureReadOk() => ++this.SecureReads;

      internal ReadData(int deck, int slot)
      {
        this.Deck = deck;
        this.Slot = slot;
      }
    }
  }
}
