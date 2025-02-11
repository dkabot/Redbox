using System;
using System.Collections.Generic;
using System.Reflection;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class PollerManager
    {
        private readonly Dictionary<string, ScriptPoller> PollerMap = new Dictionary<string, ScriptPoller>();

        public ScriptPoller Locate(string name)
        {
            foreach (var key in PollerMap.Keys)
                if (key == name)
                    return PollerMap[key];
            return null;
        }

        public void StartPollers()
        {
            DoForAll(poller =>
            {
                poller.Start();
                return true;
            });
        }

        public void StopPollers()
        {
            DoForAll(poller =>
            {
                poller.Shutdown(false);
                return true;
            });
        }

        public void InitializePollers()
        {
            using (var executionTimer = new ExecutionTimer())
            {
                var assemblyFile = ServiceLocator.Instance.GetService<IRuntimeService>()
                    .RuntimePath(Assembly.GetExecutingAssembly().GetName().Name + ".dll");
                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(assemblyFile);
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log(
                        string.Format("[PollerManager] Unable to load assembly '{0}' to scan for commands.",
                            assemblyFile), ex);
                    return;
                }

                foreach (var type in assembly.GetTypes())
                {
                    var customAttributes = (PollerAttribute[])type.GetCustomAttributes(typeof(PollerAttribute), false);
                    if (customAttributes != null && customAttributes.Length != 0)
                        if (!string.IsNullOrEmpty(customAttributes[0].Name))
                            try
                            {
                                var instance = Activator.CreateInstance(type, true) as ScriptPoller;
                                PollerMap[customAttributes[0].Name] = instance;
                                instance.PollerName = customAttributes[0].Name;
                                instance.ConfigNotifications = customAttributes[0].ConfigNotifications;
                                LogHelper.Instance.Log("[PollerManager] Registered poller {0}", instance.PollerName);
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Instance.Log(
                                    string.Format("[PollerManager] Unable to create type {0}", type), ex);
                            }
                }

                LogHelper.Instance.Log("[PollerManager] Time to scan for {0} pollers: {1}", PollerMap.Keys.Count,
                    executionTimer.Elapsed);
                DoForAll(poller =>
                {
                    poller.Initialize();
                    return true;
                });
            }
        }

        private void DoForAll(Predicate<ScriptPoller> predicate)
        {
            foreach (var key in PollerMap.Keys)
            {
                var poller = PollerMap[key];
                if (!predicate(poller))
                    break;
            }
        }
    }
}