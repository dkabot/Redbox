using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.App;

namespace UpdateClientService.API.Services.FileCache
{
    public static class XDeltaHelper
    {
        private static readonly string _path =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Environment.Is64BitOperatingSystem ? "etc\\xdelta3-64.exe" : "etc\\xdelta3.exe");

        private static string FormatApplyArguments(string source, string target, string patch)
        {
            return string.Format("-f -d -s \"{0}\" \"{1}\" \"{2}\"", source, patch, target);
        }

        private static string FormatCreateArguments(string source, string target, string patch)
        {
            return string.Format("-f -e -s \"{0}\" \"{1}\" \"{2}\"", source, target, patch);
        }

        public static List<Error> Apply(string source, string patch, string target)
        {
            if (!File.Exists(_path))
                return new List<Error>
                {
                    new Error
                    {
                        Message = "XDelta was not found at '" + _path + "'"
                    }
                };
            var errorList = new List<Error>();
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    FileName = _path,
                    WindowStyle = (ProcessWindowStyle)1,
                    Arguments = FormatApplyArguments(Path.GetFullPath(source), Path.GetFullPath(target),
                        Path.GetFullPath(patch)),
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                using (var process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    process.Start();
                    process.WaitForExit();
                    var end = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(end))
                        errorList.Add(new Error { Message = end });
                    process.StandardError.Close();
                    process.StandardInput.Close();
                    process.StandardOutput.Close();
                }
            }
            catch (Exception ex)
            {
                errorList.Add(new Error
                {
                    Message = ex.GetFullMessage()
                });
            }

            return errorList;
        }

        public static List<Error> Apply(Stream source, Stream patch, Stream target)
        {
            var source1 = new List<Error>();
            var tempFileName1 = Path.GetTempFileName();
            var tempFileName2 = Path.GetTempFileName();
            var tempFileName3 = Path.GetTempFileName();
            try
            {
                BufferedWriteToFile(source, tempFileName1);
                BufferedWriteToFile(patch, tempFileName3);
                source1.AddRange(Apply(tempFileName1, tempFileName3, tempFileName2));
                if (!source1.Any())
                    BufferedReadFromFile(tempFileName2, target);
            }
            catch (Exception ex)
            {
                source1.Add(new Error
                {
                    Message = "X999 Error applying patch " + ex.GetFullMessage()
                });
            }
            finally
            {
                File.Delete(tempFileName1);
                File.Delete(tempFileName2);
                File.Delete(tempFileName3);
            }

            return source1;
        }

        public static List<Error> Create(string source, string target, string patch)
        {
            if (!File.Exists(_path))
                return new List<Error>
                {
                    new Error
                    {
                        Message = "XDelta was not found at '" + _path + "'"
                    }
                };
            var errorList = new List<Error>();
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    FileName = _path,
                    WindowStyle = (ProcessWindowStyle)1,
                    Arguments = FormatCreateArguments(Path.GetFullPath(source), Path.GetFullPath(target),
                        Path.GetFullPath(patch)),
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                using (var process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    process.Start();
                    process.WaitForExit();
                    var end = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(end))
                        errorList.Add(new Error { Message = end });
                    process.StandardError.Close();
                    process.StandardInput.Close();
                    process.StandardOutput.Close();
                }
            }
            catch (Exception ex)
            {
                errorList.Add(new Error
                {
                    Message = ex.GetFullMessage()
                });
            }

            return errorList;
        }

        private static void BufferedWriteToFile(Stream s, string target)
        {
            var numArray = new byte[65536];
            using (var fileStream = File.Create(target))
            {
                for (var index = s.Read(numArray, 0, numArray.Length);
                     index > 0;
                     index = s.Read(numArray, 0, numArray.Length))
                    fileStream.Write(numArray, 0, index);
            }

            if (!s.CanSeek)
                return;
            s.Seek(0L, 0);
        }

        private static void BufferedReadFromFile(string source, Stream target)
        {
            var numArray = new byte[65536];
            using (var fileStream = File.OpenRead(source))
            {
                for (var index = fileStream.Read(numArray, 0, numArray.Length);
                     index > 0;
                     index = fileStream.Read(numArray, 0, numArray.Length))
                    target.Write(numArray, 0, index);
            }

            if (!target.CanSeek)
                return;
            target.Seek(0L, 0);
        }
    }
}