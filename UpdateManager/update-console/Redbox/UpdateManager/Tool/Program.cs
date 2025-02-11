using Redbox.Core;
using Redbox.GetOpts;
using Redbox.JSONPrettyPrinter;
using Redbox.Log.Framework;
using Redbox.Macros;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Tool.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Redbox.UpdateManager.Tool
{
    public class Program
    {
        private const string ApplicationId = "{4C9D4DA5-8E6C-40a0-867C-65E462A71ADB}";
        private static NamedLock m_instanceLock;
        private static CommandLine m_commandLine;
        private static readonly PropertyDictionary m_properties = new PropertyDictionary();
        private static readonly ILogger m_logger = (ILogger)LogHelper.Instance.CreateLog4NetLogger(typeof(Program));

        public static int Main(string[] args)
        {
            ErrorList instance1 = new ErrorList();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                Program.m_instanceLock = new NamedLock("{4C9D4DA5-8E6C-40a0-867C-65E462A71ADB}", LockScope.Local);
                if (!Program.m_instanceLock.Exists())
                {
                    Console.Error.WriteLine("Update Console is already running.");
                    return -1;
                }
                Program.m_logger.Configure(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "update-console-log4net.config"));
                LogHelper.Instance.Logger = Program.m_logger;
                LogHelper.Instance.Log("Executing command line: {0}", (object)System.Environment.CommandLine);
                Console.Out.WriteLine("Lock Gained for Process Id: {0} under user: {1}\\{2}", Process.GetCurrentProcess().Id, System.Environment.UserDomainName, System.Environment.UserName);
                if (Program.CommandLine.Errors.Count > 0 && !Program.CommandLine.HelpRequested)
                {
                    Program.CommandLine.WriteUsage(false);
                    Console.Error.WriteLine("Command Line errors forcing exit.");
                    return 1;
                }
                if (Program.CommandLine.HelpRequested)
                {
                    Program.CommandLine.WriteUsage(true);
                    return 0;
                }
                string str1 = Program.Opts.StoreNumber;
                if (Program.Opts.ResetAll || Program.Opts.ResetBITS || Program.Opts.ResetManifest)
                    str1 = "0";
                else if (string.IsNullOrEmpty(Program.Opts.StoreNumber))
                {
                    if (string.IsNullOrEmpty(Program.Opts.KaseyaId))
                    {
                        Program.CommandLine.WriteUsage(false);
                        return 1;
                    }
                    string str2 = Program.Opts.KaseyaId.Substring(0, Program.Opts.KaseyaId.IndexOf('.'));
                    str1 = str2.StartsWith("redbox") ? str2.Substring(7) : str2;
                }
                LogHelper.Instance.Log("Starting store updater with store number: {0}", (object)str1);
                Updater updater = new Updater()
                {
                    StoreNumber = str1
                };
                string updateServiceUrl = string.IsNullOrEmpty(Program.Opts.UpdateServiceUrl) ? Settings.Default.UpdateServiceUrl : Program.Opts.UpdateServiceUrl;
                instance1.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)updater.Initialize(updateServiceUrl));
                if (instance1.ContainsError())
                {
                    instance1.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
                    return 1;
                }
                if (Program.Opts.ResetAll)
                {
                    updater.Reset();
                    return 0;
                }
                if (Program.Opts.ResetBITS)
                {
                    updater.ResetBITS();
                    return 0;
                }
                if (Program.Opts.ResetManifest)
                {
                    updater.ResetManifest();
                    return 0;
                }
                if (!string.IsNullOrEmpty(Program.Opts.Script))
                {
                    string fullPath = Path.GetFullPath(Program.m_properties.ExpandProperties(Program.Opts.Script, Location.UnknownLocation));
                    if (!File.Exists(fullPath))
                    {
                        Console.Error.WriteLine("{0} does not exists.", (object)fullPath);
                        return -1;
                    }
                    LogHelper.Instance.Log("Executing script {0}.", (object)fullPath);
                    IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
                    try
                    {
                        service.ExecuteChunk(File.ReadAllText(fullPath), true);
                    }
                    catch (Exception ex)
                    {
                        instance1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E99", "Unhandled exception in ExecuteChunk.", ex));
                    }
                    finally
                    {
                        if (!service.ScriptCompleted)
                            instance1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("S99", "Script failed to complete.", "Check logs."));
                    }
                }
                else
                {
                    if (Program.Opts.PrintUnfinishedChanges)
                    {
                        IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
                        foreach (string allRepository in service.GetAllRepositories())
                            Console.Out.WriteLine(service.GetUnfinishedChanges(allRepository).ToJson());
                    }
                    if (Program.Opts.Update)
                    {
                        if (!string.IsNullOrEmpty(Program.Opts.Tree))
                        {
                            Console.Out.WriteLine(updater.GetPendingChangeSets(Program.Opts.Tree).ToJson());
                            if (string.IsNullOrEmpty(Program.Opts.Hash))
                                instance1.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)updater.UpdateTo(Program.Opts.Tree));
                            else
                                instance1.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)updater.UpdateTo(Program.Opts.Tree, Program.Opts.Hash));
                        }
                        else
                        {
                            IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
                            foreach (string allRepository in service.GetAllRepositories())
                                service.UpdateToHead(allRepository);
                        }
                    }
                    if (Program.Opts.Activate)
                    {
                        if (!string.IsNullOrEmpty(Program.Opts.Tree))
                        {
                            if (string.IsNullOrEmpty(Program.Opts.Hash))
                                instance1.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)updater.Activate(Program.Opts.Tree));
                            else
                                instance1.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)updater.Activate(Program.Opts.Tree, Program.Opts.Hash));
                        }
                        else
                        {
                            IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
                            foreach (string allRepository in service.GetAllRepositories())
                                instance1.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)service.ActivateToHead(allRepository, out bool _));
                        }
                    }
                    if (Program.Opts.Diff)
                    {
                        List<string> manifests = updater.GetManifests();
                        Dictionary<string, object> instance2 = new Dictionary<string, object>();
                        foreach (string str3 in manifests)
                        {
                            List<IRevLog> pendingChangeSets = updater.GetPendingChangeSets(str3);
                            pendingChangeSets.Reverse();
                            instance2.Add(str3, (object)pendingChangeSets);
                        }
                        Console.Out.WriteLine(instance2.ToJson());
                    }
                    if (Program.Opts.List)
                    {
                        List<string> manifests = updater.GetManifests();
                        Dictionary<string, object> instance3 = new Dictionary<string, object>();
                        foreach (string str4 in manifests)
                        {
                            List<IRevLog> appliedChangeSets = updater.GetAppliedChangeSets(str4);
                            appliedChangeSets.Reverse();
                            instance3.Add(str4, (object)appliedChangeSets);
                        }
                        Console.Out.WriteLine(instance3.ToJson());
                    }
                    if (Program.Opts.ListAll)
                    {
                        List<string> manifests = updater.GetManifests();
                        Dictionary<string, object> instance4 = new Dictionary<string, object>();
                        foreach (string str5 in manifests)
                        {
                            List<IRevLog> appliedChangeSets = updater.GetAppliedChangeSets(str5);
                            appliedChangeSets.Reverse();
                            instance4.Add(str5, (object)appliedChangeSets);
                        }
                        Console.Out.WriteLine(instance4.ToJson());
                    }
                    if (Program.Opts.StartDownload)
                        instance1.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)updater.StartUpdate());
                    if (Program.Opts.Poll)
                    {
                        instance1.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)updater.FinishUpdates());
                        instance1.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)updater.StartUpdate());
                    }
                    if (!string.IsNullOrEmpty(Program.Opts.Dump))
                    {
                        if (string.IsNullOrEmpty(Program.Opts.RepositoryName))
                        {
                            instance1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("U998", "repository-name is required.", "Specify repository-name."));
                            return 1;
                        }
                        IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
                        if (!service.ContainsRepository(Program.Opts.RepositoryName))
                        {
                            instance1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("U998", "repository-name does not exist.", "Check Name."));
                            return 1;
                        }
                        switch (Program.Opts.Dump.ToLower())
                        {
                            case "active":
                                Console.Write(service.GetActiveRevision(Program.Opts.RepositoryName));
                                break;
                            case "current":
                                Console.Write(service.GetStagedRevision(Program.Opts.RepositoryName));
                                break;
                            case "label":
                                Console.Write(service.GetActiveLabel(Program.Opts.RepositoryName));
                                break;
                            case "revlog":
                                Console.Write(PrettyPrinter.GetPrettyString(service.GetAllChanges(Program.Opts.RepositoryName).ToJson()));
                                break;
                            default:
                                instance1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("U998", "Dump file is invalid.", "Valid values: active, current, label, revlog"));
                                return 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                instance1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("U999", "An unhandled exception occurred in Updater.Program.Main.", ex));
            }
            finally
            {
                stopwatch.Stop();
                try
                {
                    instance1.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", e, e.Details)));
                    Console.Error.WriteLine(instance1.ToJson());
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("Exception in Program.Main.finally.", ex);
                }
                IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
                LogHelper.Instance.Log("Lock Released for Process Id: {0} under user: {1}\\{2} at UTC: {3}", Process.GetCurrentProcess().Id, System.Environment.UserDomainName, System.Environment.UserName, DateTime.UtcNow);
                if (service != null)
                {
                    switch (service.ShutdownType)
                    {
                        case ShutdownType.Reboot:
                            ShutdownTool.Shutdown(ShutdownFlags.Reboot | ShutdownFlags.Force, ShutdownReason.FlagPlanned);
                            break;
                        case ShutdownType.Shutdown:
                            ShutdownTool.Shutdown(ShutdownFlags.ShutDown | ShutdownFlags.Force, ShutdownReason.FlagPlanned);
                            break;
                    }
                }
                if (Program.m_instanceLock != null)
                    Program.m_instanceLock.Dispose();
            }
            return instance1 != null && instance1.ContainsError() ? 1 : 0;
        }

        private static CommandLine CommandLine
        {
            get
            {
                if (Program.m_commandLine == null)
                    Program.m_commandLine = CommandLine.ParseTo((object)Program.Opts);
                return Program.m_commandLine;
            }
        }

        private static Options Opts => Singleton<Options>.Instance;
    }
}
