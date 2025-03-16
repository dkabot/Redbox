using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using System;

namespace Redbox.Rental.Model
{
    public sealed class ResultCallbackEntry : ICallbackEntry, IDisposable
    {
        public ResultCallbackEntry()
        {
            EnableDebugger = true;
        }

        public string Name => nameof(ResultCallbackEntry);

        public LuaFunction Function { get; set; }

        public object[] Parameters { get; set; }

        public bool EnableDebugger { get; set; }

        public void Dispose()
        {
            if (Parameters != null)
                foreach (var parameter in Parameters)
                    if (parameter is IDisposable disposable)
                        disposable.Dispose();

            if (Function == null)
                return;
            Function.Dispose();
        }

        public void Invoke()
        {
            if (Function == null)
                return;
            try
            {
                Function.Call(Parameters);
            }
            catch (LuaException ex)
            {
                if (EnableDebugger)
                    ServiceLocator.Instance.GetService<IKernelService>()
                        ?.RaiseDebuggerOrError((Exception)ex, nameof(ResultCallbackEntry));
                else
                    LogHelper.Instance.Log("An unhandled Lua exception was raised, debugger is not enabled.",
                        (Exception)ex);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in SimpleCallback handler.", ex);
            }
        }
    }
}