using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;
using System.Net;

namespace Redbox.UpdateManager.Kernel
{
    internal static class TransferFunctions
    {
        [KernelFunction(Name = "kernel.transferarejobsrunning")]
        internal static LuaTable AreJobRunning()
        {
            bool isRunning;
            LuaTable resultTable = TransferFunctions.CreateResultTable(ServiceLocator.Instance.GetService<ITransferService>().AreJobsRunning(out isRunning));
            resultTable[(object)"isrunning"] = (object)isRunning;
            return resultTable;
        }

        [KernelFunction(Name = "kernel.transferjobsetpriority")]
        internal static LuaTable SetPriority(string id, string priority)
        {
            ErrorList errors = new ErrorList();
            ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
            ITransferJob job;
            errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)service.GetJob(new Guid(id), out job));
            if (errors.ContainsError())
                return TransferFunctions.CreateResultTable(errors);
            switch (priority)
            {
                case "high":
                    errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)job.SetPriority(TransferJobPriority.High));
                    break;
                case "foreground":
                    errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)job.SetPriority(TransferJobPriority.Foreground));
                    break;
                case "normal":
                    errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)job.SetPriority(TransferJobPriority.Normal));
                    break;
                case "low":
                    errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)job.SetPriority(TransferJobPriority.Foreground));
                    break;
            }
            return TransferFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.resettransfers")]
        internal static LuaTable Reset()
        {
            return TransferFunctions.CreateResultTable(ServiceLocator.Instance.GetService<ITransferService>().CancelAll());
        }

        [KernelFunction(Name = "kernel.transfergetrepositoriesintransit")]
        internal static LuaTable GetRevisionsInTransit()
        {
            HashSet<string> inTransit;
            ErrorList repositoriesInTransit = ServiceLocator.Instance.GetService<ITransferService>().GetRepositoriesInTransit(out inTransit);
            if (repositoriesInTransit.ContainsError())
            {
                repositoriesInTransit.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}")));
                return (LuaTable)null;
            }
            LuaTable revisionsInTransit = new LuaTable(KernelService.Instance.LuaRuntime);
            int key = 1;
            foreach (string str in inTransit)
            {
                revisionsInTransit[(object)key] = (object)str;
                ++key;
            }
            return revisionsInTransit;
        }

        [KernelFunction(Name = "kernel.checkconnectivity")]
        internal static bool CheckConnectivity(string url, int timeout)
        {
            WebRequest webRequest = WebRequest.Create(url);
            webRequest.Timeout = timeout * 1000;
            WebResponse response;
            try
            {
                response = webRequest.GetResponse();
            }
            catch
            {
                return false;
            }
            return response != null;
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
    }
}
