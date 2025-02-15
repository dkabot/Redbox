using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace HALUtilities
{
    internal sealed class PowerhouseUtilities : IDisposable
    {
        private readonly string BackupDirectory = string.Empty;

        private readonly string[] GampFiles = new string[2]
        {
            "systemdata.dat",
            "slotdata.dat"
        };

        private readonly string GampFolder = "c:\\Gamp";
        private readonly string HALConfig = "hal.xml";
        private readonly string HALInstallFolder = "c:\\Program Files\\Redbox\\HALService\\bin";
        private readonly IRuntimeService RuntimeService;

        internal PowerhouseUtilities(IRuntimeService rts)
        {
            RuntimeService = rts;
            Console.WriteLine("Enter identifier:");
            var path2 = Console.ReadLine();
            Console.WriteLine("Identifier entered: {0}", path2);
            var directory = Path.Combine(RuntimeService.AssemblyDirectory, path2);
            if (!CreateDir(directory))
                return;
            BackupDirectory = directory;
            BackupOk = true;
        }

        internal bool BackupOk { get; }

        public void Dispose()
        {
        }

        internal static void Run(PowerhouseOperations op, IRuntimeService rts)
        {
            if (op == PowerhouseOperations.None)
                return;
            using (var powerhouseUtilities = new PowerhouseUtilities(rts))
            {
                if (PowerhouseOperations.Restore == op)
                {
                    if (powerhouseUtilities.BackupOk)
                        powerhouseUtilities.Restore();
                }
                else if (PowerhouseOperations.Backup == op && powerhouseUtilities.BackupOk)
                {
                    powerhouseUtilities.Backup();
                }

                Console.WriteLine("Press any key to exit.");
                Console.ReadLine();
            }
        }

        internal void Backup()
        {
            ForFiles(GampFolder, BackupDirectory);
            var str = Path.Combine(HALInstallFolder, HALConfig);
            if (File.Exists(str))
                DoCopy(str, Path.Combine(BackupDirectory, HALConfig));
            var kioskId = RuntimeService.KioskId;
            if (!("UNKNOWN" != kioskId))
                return;
            Console.WriteLine("Found kiosk id {0}", kioskId);
            using (var streamWriter = new StreamWriter(Path.Combine(BackupDirectory, "KID.txt")))
            {
                streamWriter.WriteLine("Kiosk id = {0}", kioskId);
            }
        }

        internal void Restore()
        {
            if (CreateDir(GampFolder))
                ForFiles(BackupDirectory, GampFolder);
            var str = Path.Combine(BackupDirectory, HALConfig);
            if (!File.Exists(str) || !CreateDir(HALInstallFolder))
                return;
            DoCopy(str, Path.Combine(HALInstallFolder, HALConfig));
        }

        private bool ForFiles(string sourceDirectory, string backup)
        {
            var copies = 0;
            Array.ForEach(GampFiles, cf =>
            {
                var str = Path.Combine(sourceDirectory, cf);
                if (File.Exists(str))
                {
                    var dest = Path.Combine(backup, cf);
                    if (!DoCopy(str, dest))
                        return;
                    ++copies;
                }
                else
                {
                    Console.WriteLine("The file '{0}' doesn't exist.");
                }
            });
            return copies == GampFiles.Length;
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
                Console.WriteLine("Failed to create directory '{0}'", directory);
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private bool DoCopy(string src, string dest)
        {
            try
            {
                File.Copy(src, dest, true);
                Console.WriteLine("Copied {0} --> {1}", src, dest);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to copy {0} --> {1}", src, dest);
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}