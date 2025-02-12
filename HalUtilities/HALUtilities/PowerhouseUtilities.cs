using Redbox.HAL.Component.Model;
using System;
using System.IO;

namespace HALUtilities
{
  internal sealed class PowerhouseUtilities : IDisposable
  {
    private readonly string BackupDirectory = string.Empty;
    private readonly IRuntimeService RuntimeService;
    private readonly string GampFolder = "c:\\Gamp";
    private readonly string HALInstallFolder = "c:\\Program Files\\Redbox\\HALService\\bin";
    private readonly string HALConfig = "hal.xml";
    private readonly string[] GampFiles = new string[2]
    {
      "systemdata.dat",
      "slotdata.dat"
    };

    public void Dispose()
    {
    }

    internal static void Run(PowerhouseOperations op, IRuntimeService rts)
    {
      if (op == PowerhouseOperations.None)
        return;
      using (PowerhouseUtilities powerhouseUtilities = new PowerhouseUtilities(rts))
      {
        if (PowerhouseOperations.Restore == op)
        {
          if (powerhouseUtilities.BackupOk)
            powerhouseUtilities.Restore();
        }
        else if (PowerhouseOperations.Backup == op && powerhouseUtilities.BackupOk)
          powerhouseUtilities.Backup();
        Console.WriteLine("Press any key to exit.");
        Console.ReadLine();
      }
    }

    internal void Backup()
    {
      this.ForFiles(this.GampFolder, this.BackupDirectory);
      string str = Path.Combine(this.HALInstallFolder, this.HALConfig);
      if (File.Exists(str))
        this.DoCopy(str, Path.Combine(this.BackupDirectory, this.HALConfig));
      string kioskId = this.RuntimeService.KioskId;
      if (!("UNKNOWN" != kioskId))
        return;
      Console.WriteLine("Found kiosk id {0}", (object) kioskId);
      using (StreamWriter streamWriter = new StreamWriter(Path.Combine(this.BackupDirectory, "KID.txt")))
        streamWriter.WriteLine("Kiosk id = {0}", (object) kioskId);
    }

    internal void Restore()
    {
      if (this.CreateDir(this.GampFolder))
        this.ForFiles(this.BackupDirectory, this.GampFolder);
      string str = Path.Combine(this.BackupDirectory, this.HALConfig);
      if (!File.Exists(str) || !this.CreateDir(this.HALInstallFolder))
        return;
      this.DoCopy(str, Path.Combine(this.HALInstallFolder, this.HALConfig));
    }

    internal PowerhouseUtilities(IRuntimeService rts)
    {
      this.RuntimeService = rts;
      Console.WriteLine("Enter identifier:");
      string path2 = Console.ReadLine();
      Console.WriteLine("Identifier entered: {0}", (object) path2);
      string directory = Path.Combine(this.RuntimeService.AssemblyDirectory, path2);
      if (!this.CreateDir(directory))
        return;
      this.BackupDirectory = directory;
      this.BackupOk = true;
    }

    internal bool BackupOk { get; private set; }

    private bool ForFiles(string sourceDirectory, string backup)
    {
      int copies = 0;
      Array.ForEach<string>(this.GampFiles, (Action<string>) (cf =>
      {
        string str = Path.Combine(sourceDirectory, cf);
        if (File.Exists(str))
        {
          string dest = Path.Combine(backup, cf);
          if (!this.DoCopy(str, dest))
            return;
          ++copies;
        }
        else
          Console.WriteLine("The file '{0}' doesn't exist.");
      }));
      return copies == this.GampFiles.Length;
    }

    private bool CreateDir(string directory)
    {
      if (Directory.Exists(directory))
        return true;
      try
      {
        Directory.CreateDirectory(directory);
        return true;
      }
      catch (Exception ex)
      {
        Console.WriteLine("Failed to create directory '{0}'", (object) directory);
        Console.WriteLine(ex.Message);
        return false;
      }
    }

    private bool DoCopy(string src, string dest)
    {
      try
      {
        File.Copy(src, dest, true);
        Console.WriteLine("Copied {0} --> {1}", (object) src, (object) dest);
        return true;
      }
      catch (Exception ex)
      {
        Console.WriteLine("Failed to copy {0} --> {1}", (object) src, (object) dest);
        Console.WriteLine(ex.Message);
        return false;
      }
    }
  }
}
