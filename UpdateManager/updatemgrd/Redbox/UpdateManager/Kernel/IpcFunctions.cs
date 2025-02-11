using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.Lua;
using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.Kernel
{
    internal static class IpcFunctions
    {
        [KernelFunction(Name = "kernel.executeservicecommand")]
        internal static LuaTable ExecuteServiceCommand(string url, string command, int timeout)
        {
            try
            {
                LuaTable luaTable1 = new LuaTable(KernelService.Instance.LuaRuntime);
                using (ClientSession clientSession = ClientSession.GetClientSession(IPCProtocol.Parse(url), new int?(timeout)))
                {
                    clientSession.Connect();
                    SystemFunctions.Wait(5000);
                    ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(clientSession, true, command);
                    luaTable1[(object)"success"] = (object)clientCommandResult.Success;
                    luaTable1[(object)"execution_time"] = (object)clientCommandResult.ExecutionTime.ToString();
                    luaTable1[(object)"command_text"] = (object)clientCommandResult.CommandText;
                    LuaTable luaTable2 = new LuaTable(KernelService.Instance.LuaRuntime);
                    luaTable1[(object)"command_messages"] = (object)luaTable2;
                    int num1 = 1;
                    foreach (string commandMessage in clientCommandResult.CommandMessages)
                        luaTable2[(object)num1++] = (object)commandMessage;
                    int num2 = 1;
                    LuaTable luaTable3 = new LuaTable(KernelService.Instance.LuaRuntime);
                    foreach (Redbox.IPC.Framework.Error error in (List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors)
                        luaTable3[(object)num2++] = (object)new LuaTable(KernelService.Instance.LuaRuntime)
                        {
                            [(object)"code"] = (object)error.Code,
                            [(object)"description"] = (object)error.Description,
                            [(object)"is_warning"] = (object)error.IsWarning,
                            [(object)"details"] = (object)error.Details
                        };
                    luaTable1[(object)"errors"] = (object)luaTable3;
                }
                return luaTable1;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Exception in kernel.executeservicecommand:", ex);
                throw;
            }
        }
    }
}
