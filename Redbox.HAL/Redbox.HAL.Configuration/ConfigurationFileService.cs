using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Configuration
{
    public sealed class ConfigurationFileService : IConfigurationFileService
    {
        private readonly List<IConfigurationFile> Configs = new List<IConfigurationFile>();
        private readonly IRuntimeService RuntimeService;

        public ConfigurationFileService(IRuntimeService rts)
        {
            RuntimeService = rts;
            Configs.Add(new RedboxConfigurationFile());
            Configs.Add(new LegacySlotData());
            Configs.Add(new LegacySystemData());
        }

        public IConfigurationFile Get(SystemConfigurations config)
        {
            if (config == SystemConfigurations.None)
                throw new ArgumentException("Unspecified configuration");
            return Configs.Find(each => each.Type == config);
        }

        public void DoForEach(Predicate<IConfigurationFile> a)
        {
            foreach (var config in Configs)
                if (!a(config))
                    break;
        }

        public void BackupTo(IConfigurationFile f, string targetDir, ErrorList errors)
        {
            var dest = Path.Combine(targetDir, f.FileName);
            ForceCopy(f.FullSourcePath, dest, errors);
        }

        public void Restore(IConfigurationFile f, string fromDir, ErrorList errors)
        {
            var str = Path.Combine(fromDir, f.FileName);
            if (!File.Exists(str))
                errors.Add(Error.NewError("F002", "File doesn't exist",
                    string.Format("Backup file {0} doesn't exist.", str)));
            else
                ForceCopy(str, f.FullSourcePath, errors);
        }

        public bool Backup(IConfigurationFile f, ErrorList errors)
        {
            if (File.Exists(f.FullSourcePath))
                try
                {
                    RuntimeService.CreateBackup(f.FullSourcePath, BackupAction.Move);
                }
                catch (Exception ex)
                {
                    errors.Add(Error.NewError("F003", "Backup failure", ex.Message));
                    return false;
                }

            return !File.Exists(f.FullSourcePath);
        }

        private void ForceCopy(string src, string dest, ErrorList errors)
        {
            try
            {
                File.Copy(src, dest, true);
            }
            catch (Exception ex)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(string.Format("Failed to copy {0} --> {1}", src, dest));
                stringBuilder.AppendLine(ex.Message);
                errors.Add(Error.NewError("F001", "Copy failure", stringBuilder.ToString()));
            }
        }
    }
}