using Redbox.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Redbox.DeltaCompression.Binary
{
    internal static class XDeltaHelper
    {
        private static readonly string Name = "xdelta3.exe";

        public static ErrorList Apply(string source, string patch, string target)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    FileName = XDeltaHelper.Name,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = XDeltaHelper.FormatApplyArguments(Path.GetFullPath(source), Path.GetFullPath(target), Path.GetFullPath(patch)),
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                using (Process process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    process.Start();
                    process.WaitForExit();
                    string end = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(end))
                    {
                        LogHelper.Instance.Log("XDELTA ERRORS: {0}", (object)end);
                        errorList.Add(Redbox.DeltaCompression.Error.NewError("X999", "Xdelta errors.", end));
                    }
                    process.StandardError.Close();
                    process.StandardInput.Close();
                    process.StandardOutput.Close();
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.DeltaCompression.Error.NewError("X998", "Error calling xdelta.", ex));
            }
            return errorList;
        }

        public static ErrorList Apply(Stream source, Stream patch, Stream target)
        {
            ErrorList errorList = new ErrorList();
            string tempFileName1 = Path.GetTempFileName();
            string tempFileName2 = Path.GetTempFileName();
            string tempFileName3 = Path.GetTempFileName();
            try
            {
                XDeltaHelper.BufferedWriteToFile(source, tempFileName1);
                XDeltaHelper.BufferedWriteToFile(patch, tempFileName3);
                errorList.AddRange((IEnumerable<Redbox.DeltaCompression.Error>)XDeltaHelper.Apply(tempFileName1, tempFileName3, tempFileName2));
                if (!errorList.ContainsError())
                    XDeltaHelper.BufferedReadFromFile(tempFileName2, target);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.DeltaCompression.Error.NewError("X999", "Error applying patch", ex));
            }
            finally
            {
                File.Delete(tempFileName1);
                File.Delete(tempFileName2);
                File.Delete(tempFileName3);
            }
            return errorList;
        }

        public static ErrorList Create(string source, string target, string patch)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    FileName = XDeltaHelper.Name,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = XDeltaHelper.FormatCreateArguments(Path.GetFullPath(source), Path.GetFullPath(target), Path.GetFullPath(patch)),
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                using (Process process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    process.Start();
                    process.WaitForExit();
                    string end = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(end))
                        errorList.Add(Redbox.DeltaCompression.Error.NewError("X999", "Xdelta errors.", end));
                    process.StandardError.Close();
                    process.StandardInput.Close();
                    process.StandardOutput.Close();
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.DeltaCompression.Error.NewError("X998", "Error calling xdelta.", ex));
            }
            return errorList;
        }

        public static ErrorList Create(Stream source, Stream target, Stream patch)
        {
            ErrorList errorList = new ErrorList();
            string tempFileName1 = Path.GetTempFileName();
            string tempFileName2 = Path.GetTempFileName();
            string tempFileName3 = Path.GetTempFileName();
            try
            {
                XDeltaHelper.BufferedWriteToFile(source, tempFileName1);
                XDeltaHelper.BufferedWriteToFile(target, tempFileName2);
                errorList.AddRange((IEnumerable<Redbox.DeltaCompression.Error>)XDeltaHelper.Create(tempFileName1, tempFileName2, tempFileName3));
                if (!errorList.ContainsError())
                    XDeltaHelper.BufferedReadFromFile(tempFileName3, patch);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.DeltaCompression.Error.NewError("X999", "Error creating a patch.", ex));
            }
            finally
            {
                File.Delete(tempFileName1);
                File.Delete(tempFileName2);
                File.Delete(tempFileName3);
            }
            return errorList;
        }

        private static void BufferedWriteToFile(Stream s, string target)
        {
            byte[] buffer = new byte[65536];
            using (FileStream fileStream = File.Create(target))
            {
                for (int count = s.Read(buffer, 0, buffer.Length); count > 0; count = s.Read(buffer, 0, buffer.Length))
                    fileStream.Write(buffer, 0, count);
            }
            if (!s.CanSeek)
                return;
            s.Seek(0L, SeekOrigin.Begin);
        }

        private static void BufferedReadFromFile(string source, Stream target)
        {
            byte[] buffer = new byte[65536];
            using (FileStream fileStream = File.OpenRead(source))
            {
                for (int count = fileStream.Read(buffer, 0, buffer.Length); count > 0; count = fileStream.Read(buffer, 0, buffer.Length))
                    target.Write(buffer, 0, count);
            }
            if (!target.CanSeek)
                return;
            target.Seek(0L, SeekOrigin.Begin);
        }

        private static string FormatApplyArguments(string source, string target, string patch)
        {
            return string.Format("-f -d -s \"{0}\" \"{1}\" \"{2}\"", (object)source, (object)patch, (object)target);
        }

        private static string FormatCreateArguments(string source, string target, string patch)
        {
            return string.Format("-f -e -s \"{0}\" \"{1}\" \"{2}\"", (object)source, (object)target, (object)patch);
        }
    }
}
