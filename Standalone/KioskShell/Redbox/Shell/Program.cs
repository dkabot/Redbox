using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Outerwall.Shell;
using Redbox.Core;
using Redbox.GetOpts;
using Redbox.Log.Framework;
using Redbox.Shell.Properties;
using Redbox.Shell.Remoting;

namespace Redbox.Shell
{
    public class Program
    {
        private static ILogger logger;
        private static CommandLine commandLine;

        private static CommandLine CommandLine
        {
            get
            {
                if (commandLine == null)
                    commandLine = CommandLine.ParseTo(Options.Instance);
                return commandLine;
            }
        }

        [STAThread]
        public static int Main()
        {
            logger = LogHelper.Instance.CreateLog4NetLogger(typeof(Program));
            logger.Configure(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "shell-log4net.config"));
            LogHelper.Instance.Logger = logger;
            LogHelper.Instance.Log("Shell starting up...");
            AppDomain.CurrentDomain.UnhandledException += DomainExceptionHandler;
            Application.ThreadException += ApplicationExceptionHandler;
            if (CommandLine.Errors.Count > 0 && !CommandLine.HelpRequested)
            {
                CommandLine.WriteUsage(false);
                Console.Error.WriteLine("Command Line errors forcing exit.");
                return 1;
            }

            if (CommandLine.HelpRequested)
            {
                CommandLine.WriteUsage(true);
                return 0;
            }

            KioskEngineService.Instance.Initialize(Settings.Default.KioskEnginePath,
                Settings.Default.KioskEngineIPCUrl);
            ApplySystemParameters();
            var startupFile = Settings.Default.StartupFile;
            if (!string.IsNullOrEmpty(startupFile) && File.Exists(startupFile))
                ExecuteShellStartupFile(startupFile);
            var startupFileMask = Settings.Default.StartupFileMask;
            List<string> stringList;
            if (startupFileMask != null && startupFileMask.Count != 0)
            {
                stringList = startupFileMask.ToList();
            }
            else
            {
                stringList = new List<string>();
                stringList.Add("*.lnk");
            }

            var searchPatterns = stringList;
            var list1 = Settings.Default.StartupDirectories.Cast<string>().Where(Directory.Exists).SelectMany(
                sd => searchPatterns, (sd, sp) => new
                {
                    sd, sp
                }).SelectMany(_param1 => Directory.GetFiles(_param1.sd, _param1.sp), (_param1, f) => f).ToList();
            var startupExclusions = Settings.Default.StartupExclusions.Cast<string>();
            var list2 = list1
                .Where(f => startupExclusions.All(e => f.IndexOf(e, StringComparison.OrdinalIgnoreCase) == -1))
                .ToList();
            list2.ForEach(ef => LogHelper.Instance.Log("Skipping \"{0}\" since file matches exclusion list", ef));
            list1.Except(list2).ToList().ForEach(ExecuteWindowsStartupFile);
            Application.Run(new Shell());
            return 0;
        }

        private static void DomainExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                LogHelper.Instance.Log("Unhandled Application Exception. Exiting Application",
                    (Exception)e.ExceptionObject);
            }
            finally
            {
                Application.Exit();
            }
        }

        private static void ApplicationExceptionHandler(object sender, ThreadExceptionEventArgs e)
        {
            try
            {
                LogHelper.Instance.Log("Unhandled Windows Forms Thread Exception. Exiting Application", e.Exception);
            }
            finally
            {
                Application.Exit();
            }
        }

        private static void ApplySystemParameters()
        {
            try
            {
                var str = Settings.Default.Wallpaper ?? string.Empty;
                if (string.IsNullOrEmpty(str))
                {
                    LogHelper.Instance.Log(
                        "The wallpaper cannot be set because no file was supplied in the configuration file.");
                }
                else if (!File.Exists(str))
                {
                    LogHelper.Instance.Log(
                        string.Format("The wallpaper cannot be set because the file:\"{0}\" cannot be found.", str));
                }
                else
                {
                    SystemParametersUtility.SetWallpaper(str, WallpaperStyle.FillScreen, TileWallpaper.NotTiled);
                    LogHelper.Instance.Log(string.Format("The current user's wallpaper was set to:\"{0}\".", str));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unable to apply user system parameters.", ex);
            }
        }

        private static void ExecuteShellStartupFile(string filename)
        {
            try
            {
                LogHelper.Instance.Log("Executing start up file:\"{0}\"", filename);
                using (var process = new Process())
                {
                    var processStartInfo = new ProcessStartInfo(filename)
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    process.StartInfo = processStartInfo;
                    process.OutputDataReceived += (sender, args) =>
                    {
                        if (string.IsNullOrEmpty(args.Data))
                            return;
                        LogHelper.Instance.Log(string.Format("stdout: {0}", args.Data));
                    };
                    process.Start();
                    process.BeginOutputReadLine();
                    var end = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(end))
                        LogHelper.Instance.Log(string.Format("stderr: {0}", end));
                    process.WaitForExit();
                    var destFileName = string.Format("{0}.{1}.bak", filename, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                    LogHelper.Instance.Log("Moving start up file to:\"{0}\"", destFileName);
                    File.Move(filename, destFileName);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(string.Format("An error occurred executing start up file:\"{0}\"", filename),
                    ex);
            }
        }

        private static void ExecuteWindowsStartupFile(string filename)
        {
            var fileName = Path.GetFileName(filename);
            try
            {
                using (var process = new Process())
                {
                    LogHelper.Instance.Log("Executing start up file:\"{0}\"", fileName);
                    var processStartInfo = new ProcessStartInfo(filename)
                    {
                        UseShellExecute = true
                    };
                    process.StartInfo = processStartInfo;
                    process.Start();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(string.Format("An error occurred executing start up file:\"{0}\"", fileName),
                    ex);
            }
        }
    }
}