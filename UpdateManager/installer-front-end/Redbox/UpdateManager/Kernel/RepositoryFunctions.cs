using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.Kernel
{
    internal static class RepositoryFunctions
    {
        [KernelFunction(Name = "kernel.repositoryverify")]
        internal static LuaTable Verify(string name)
        {
            ErrorList errors = ServiceLocator.Instance.GetService<IRepositoryService>().Verify(name);
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            LuaTable resultTable = RepositoryFunctions.CreateResultTable(errors);
            resultTable[(object)"corrupt"] = (object)errors.ContainsError();
            return resultTable;
        }

        [KernelFunction(Name = "kernel.repositoryreset")]
        internal static LuaTable Reset(string name)
        {
            bool defferedDelete;
            ServiceLocator.Instance.GetService<IRepositoryService>().Reset(name, out defferedDelete);
            return new LuaTable(KernelService.Instance.LuaRuntime)
            {
                [(object)"should_restart"] = (object)defferedDelete
            };
        }

        [KernelFunction(Name = "kernel.repositoryrepair")]
        internal static LuaTable Repair(string name)
        {
            bool deferredMove;
            ErrorList errors = ServiceLocator.Instance.GetService<IRepositoryService>().Repair(name, out deferredMove);
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            LuaTable resultTable = RepositoryFunctions.CreateResultTable(errors);
            resultTable[(object)"should_restart"] = (object)deferredMove;
            return resultTable;
        }

        [KernelFunction(Name = "kernel.repositorygetunfinishedchanges")]
        internal static LuaTable GetOutstandingChanges(string name)
        {
            return RepositoryFunctions.ChangeSetToFileList((IEnumerable<IRevLog>)ServiceLocator.Instance.GetService<IRepositoryService>().GetUnfinishedChanges(name));
        }

        [KernelFunction(Name = "kernel.repositoryformatstagedfile")]
        internal static string FormatStagedFile(string path)
        {
            return ServiceLocator.Instance.GetService<IRepositoryService>().FormatStagedFileName(path);
        }

        [KernelFunction(Name = "kernel.repositoryactivateto")]
        internal static LuaTable ActivateTo(string name, string hash)
        {
            bool deferredMove;
            ErrorList errors = ServiceLocator.Instance.GetService<IRepositoryService>().ActivateTo(name, hash, out deferredMove);
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            LuaTable resultTable = RepositoryFunctions.CreateResultTable(errors);
            resultTable[(object)"should_restart"] = (object)deferredMove;
            return resultTable;
        }

        [KernelFunction(Name = "kernel.repositoryactivate")]
        internal static LuaTable Activate(string name)
        {
            bool deferredMove;
            ErrorList head = ServiceLocator.Instance.GetService<IRepositoryService>().ActivateToHead(name, out deferredMove);
            if (head.ContainsError())
                head.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            LuaTable resultTable = RepositoryFunctions.CreateResultTable(head);
            resultTable[(object)"should_restart"] = (object)deferredMove;
            return resultTable;
        }

        [KernelFunction(Name = "kernel.repositoryupdateto")]
        internal static LuaTable UpdateTo(string name, string hash)
        {
            ErrorList errors = ServiceLocator.Instance.GetService<IRepositoryService>().UpdateTo(name, hash);
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return RepositoryFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.repositoryupdate")]
        internal static LuaTable Update(string name)
        {
            ErrorList head = ServiceLocator.Instance.GetService<IRepositoryService>().UpdateToHead(name);
            if (head.ContainsError())
                head.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return RepositoryFunctions.CreateResultTable(head);
        }

        [KernelFunction(Name = "kernel.repositorypendingchanges")]
        internal static LuaTable GetPendingChanges(string name)
        {
            return RepositoryFunctions.ChangeSetToFileList((IEnumerable<IRevLog>)ServiceLocator.Instance.GetService<IRepositoryService>().GetPendingChanges(name));
        }

        [KernelFunction(Name = "kernel.repositorypendingactivations")]
        internal static LuaTable GetPendingActivations(string name)
        {
            IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
            List<IRevLog> logs = new List<IRevLog>();
            try
            {
                logs = service.GetPendingActivations(name);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unhandled exception in kernel.repositorypendingactivations.", ex);
            }
            return RepositoryFunctions.ChangeSetToFileList((IEnumerable<IRevLog>)logs);
        }

        [KernelFunction(Name = "kernel.repositorylist")]
        internal static LuaTable ListRepositories()
        {
            List<string> allRepositories = ServiceLocator.Instance.GetService<IRepositoryService>().GetAllRepositories();
            LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
            int key = 1;
            foreach (string str in allRepositories)
            {
                luaTable[(object)key] = (object)str;
                ++key;
            }
            return luaTable;
        }

        [KernelFunction(Name = "kernel.repositoryisactivated")]
        internal static bool IsRepositoryActivated(string name)
        {
            IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
            if (service == null)
                return false;
            try
            {
                return service.GetActiveRevision(name) != "0000000000000000000000000000000000000000";
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [KernelFunction(Name = "kernel.repositorygetheadrevision")]
        internal static string GetHeadRevision(string name)
        {
            return ServiceLocator.Instance.GetService<IRepositoryService>().GetHeadRevision(name);
        }

        [KernelFunction(Name = "kernel.repositorytrim")]
        internal static void Trim(string name)
        {
            ServiceLocator.Instance.GetService<IRepositoryService>().TrimRepository(name);
        }

        private static LuaTable CreateResultTable(ErrorList errors)
        {
            LuaTable resultTable = new LuaTable(KernelService.Instance.LuaRuntime);
            resultTable[(object)"success"] = (object)!errors.ContainsError();
            LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
            foreach (Redbox.UpdateManager.ComponentModel.Error error in (List<Redbox.UpdateManager.ComponentModel.Error>)errors)
                luaTable[(object)error.Code] = (object)error.Description;
            resultTable[(object)nameof(errors)] = (object)luaTable;
            return resultTable;
        }

        private static LuaTable ChangeSetToFileList(IEnumerable<IRevLog> logs)
        {
            LuaTable fileList = new LuaTable(KernelService.Instance.LuaRuntime);
            HashSet<string> stringSet = new HashSet<string>();
            int key = 1;
            foreach (IRevLog log in logs)
            {
                foreach (ChangeItem change in log.Changes)
                {
                    string str = change.FormatFileName();
                    if (!stringSet.Contains(str))
                    {
                        stringSet.Add(str);
                        fileList[(object)key] = (object)str;
                        ++key;
                    }
                }
            }
            return fileList;
        }
    }
}
