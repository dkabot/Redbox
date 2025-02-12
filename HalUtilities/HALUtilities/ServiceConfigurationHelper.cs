using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace HALUtilities
{
    internal sealed class ServiceConfigurationHelper : IDisposable
    {
        private readonly HardwareService Service;

        internal ServiceConfigurationHelper(HardwareService service)
        {
            Service = service;
        }

        public void Dispose()
        {
        }

        internal void BackupConfiguration(string kioskID)
        {
            var service = ServiceLocator.Instance.GetService<IRuntimeService>();
            using (var streamWriter = new StreamWriter(File.Open(service.RuntimePath("backup-log.txt"), FileMode.Append,
                       FileAccess.Write, FileShare.Read)))
            {
                var str = service.RuntimePath(kioskID);
                if (!Directory.Exists(str))
                    try
                    {
                        Directory.CreateDirectory(str);
                    }
                    catch
                    {
                        streamWriter.WriteLine("Unable to create backup directory.");
                        return;
                    }

                try
                {
                    File.Copy("c:\\gamp\\SystemData.dat", Path.Combine(str, "SystemData.dat"), true);
                    File.Copy("c:\\gamp\\SlotData.dat", Path.Combine(str, "SlotData.dat"), true);
                    File.Copy("c:\\Program Files\\Redbox\\HALService\\bin\\HAL.xml", Path.Combine(str, "HAL.xml"),
                        true);
                }
                catch
                {
                    streamWriter.WriteLine("Error backing up file.");
                }
            }
        }

        internal bool UpdateOption(string updateConfigurationString)
        {
            var service = ServiceLocator.Instance.GetService<IRuntimeService>();
            var ch = ':';
            if (-1 != updateConfigurationString.IndexOf('|'))
                ch = '|';
            var strArray = updateConfigurationString.Split(ch);
            if (strArray.Length < 3)
            {
                Console.WriteLine("Unable to parse option {0}", updateConfigurationString);
                return false;
            }

            var path1 = service.RuntimePath("ConfigurationUpdateError.log");
            var str1 = strArray[0];
            var str2 = strArray[1];
            var str3 = strArray[2];
            var flag1 = strArray.Length == 4;
            if (File.Exists(path1))
                try
                {
                    File.Delete(path1);
                }
                catch
                {
                }

            if (Service == null)
            {
                using (var streamWriter = new StreamWriter(path1))
                {
                    streamWriter.WriteLine("Unable to talk to HAL.");
                }

                return false;
            }

            HardwareJob job;
            var hardwareCommandResult1 =
                Service.ExecuteImmediate(string.Format("SETCFG \"{0}\" \"{1}\" TYPE={2}", str2, str3, str1), out job);
            if (!hardwareCommandResult1.Success)
            {
                using (var log = new StreamWriter(path1))
                {
                    log.WriteLine("The configuration update failed.");
                    hardwareCommandResult1.Errors.ForEach(err => log.WriteLine(err.ToString()));
                }

                return false;
            }

            if (!flag1)
                return true;
            var hardwareCommandResult2 =
                Service.ExecuteImmediate(string.Format("GETCFG \"{0}\" TYPE={1}", str2, str1), out job);
            var path2 = service.RuntimePath("ConfigurationUpdateValidateError.log");
            service.SafeDelete(path2);
            var flag2 = false;
            using (var validateFile = new StreamWriter(path2))
            {
                if (!hardwareCommandResult2.Success)
                {
                    validateFile.WriteLine("The configuration update failed.");
                    hardwareCommandResult2.Errors.ForEach(err => validateFile.WriteLine(err.ToString()));
                    return false;
                }

                Stack<string> stack;
                if (!job.GetStack(out stack).Success)
                {
                    validateFile.WriteLine("Couldn't validate: unable to get stack.");
                }
                else
                {
                    var str4 = stack.Pop();
                    if (!str4.Equals(str3, StringComparison.CurrentCultureIgnoreCase))
                        validateFile.WriteLine("The new value {0} doesn't match {1}", str4, str3);
                    else
                        flag2 = true;
                }
            }

            return flag2;
        }
    }
}