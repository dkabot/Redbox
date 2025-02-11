using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.Kernel
{
    internal static class KioskEngineFunctions
    {
        [KernelFunction(Name = "kernel.kioskengineexportqueue")]
        internal static LuaTable ExportQueue(string path)
        {
            ErrorList errors = ServiceLocator.Instance.GetService<IKioskEngineService>().ExportQueue(path);
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return KioskEngineFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.kioskengineexportsession")]
        internal static LuaTable ExportSession(string path)
        {
            ErrorList errors = ServiceLocator.Instance.GetService<IKioskEngineService>().ExportSessions(path);
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return KioskEngineFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.kioskenginestart")]
        internal static LuaTable StartEngine()
        {
            ErrorList errors = ServiceLocator.Instance.GetService<IKioskEngineService>().StartEngine();
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return KioskEngineFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.kioskengineisrunning")]
        internal static LuaTable IsEngineRunning()
        {
            bool isRunning;
            ErrorList errors = ServiceLocator.Instance.GetService<IKioskEngineService>().IsEngineRunning(out isRunning);
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            LuaTable resultTable = KioskEngineFunctions.CreateResultTable(errors);
            resultTable[(object)"isrunning"] = (object)isRunning;
            return resultTable;
        }

        [KernelFunction(Name = "kernel.kioskenginestartwithbundle")]
        internal static LuaTable StartEngine(string bundle)
        {
            ErrorList errors = ServiceLocator.Instance.GetService<IKioskEngineService>().StartEngine(bundle);
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return KioskEngineFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.kioskenginechangebundle")]
        internal static LuaTable ChangeBundle(string bundle)
        {
            ErrorList errors = ServiceLocator.Instance.GetService<IKioskEngineService>().ChangeBundle(bundle);
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return KioskEngineFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.kioskengineshutdownblocking")]
        public static LuaTable ShutdownBlocking(int timeout, int tries)
        {
            ErrorList errors = ServiceLocator.Instance.GetService<IKioskEngineService>().Shutdown(timeout, tries);
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return KioskEngineFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.kioskengineshutdown")]
        internal static LuaTable Shutdown()
        {
            ErrorList errors = ServiceLocator.Instance.GetService<IKioskEngineService>().Shutdown();
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return KioskEngineFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.kioskenginebringtofront")]
        internal static LuaTable BringToFront()
        {
            ErrorList front = ServiceLocator.Instance.GetService<IKioskEngineService>().BringToFront();
            if (front.ContainsError())
                front.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return KioskEngineFunctions.CreateResultTable(front);
        }

        [KernelFunction(Name = "kernel.kioskengineexecutescript")]
        internal static LuaTable ExecuteScript(string path)
        {
            ErrorList errors = ServiceLocator.Instance.GetService<IKioskEngineService>().ExecuteScript(path);
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return KioskEngineFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.kioskenginereloadbundle")]
        internal static LuaTable ReloadBundle()
        {
            ErrorList errors = ServiceLocator.Instance.GetService<IKioskEngineService>().ReloadBundle();
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return KioskEngineFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.kioskenginegetmemoryusage")]
        internal static long GetMemoryUsage(string path)
        {
            long total;
            ErrorList memoryUsage = ServiceLocator.Instance.GetService<IKioskEngineService>().GetMemoryUsage(out total);
            if (memoryUsage.ContainsError())
                memoryUsage.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return total;
        }

        [KernelFunction(Name = "kernel.kioskengineshowofflinescreen")]
        internal static LuaTable ShowOfflineScreen()
        {
            ErrorList errors = ServiceLocator.Instance.GetService<IKioskEngineService>().ShowOfflineScreen();
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
            return KioskEngineFunctions.CreateResultTable(errors);
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
