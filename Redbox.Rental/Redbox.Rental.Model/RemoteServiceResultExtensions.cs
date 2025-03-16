using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Lua;
using System.Collections;
using System.Collections.Generic;

namespace Redbox.Rental.Model
{
    public static class RemoteServiceResultExtensions
    {
        public static LuaTable AsTable(this IRemoteServiceResult result)
        {
            var service = ServiceLocator.Instance.GetService<IKernelService>();
            var luaTable1 = (LuaTable)service.NewTable();
            luaTable1[(object)"success"] = (object)result.Success;
            var luaTable2 = (LuaTable)service.NewTable();
            luaTable2[(object)"hours"] = (object)result.ExecutionTime.Hours;
            luaTable2[(object)"minutes"] = (object)result.ExecutionTime.Minutes;
            luaTable2[(object)"seconds"] = (object)result.ExecutionTime.Seconds;
            luaTable2[(object)"milliseconds"] = (object)result.ExecutionTime.Milliseconds;
            luaTable1[(object)"execution_time"] = (object)luaTable2;
            var luaTable3 = (LuaTable)service.NewTable();
            foreach (var key in (IEnumerable<string>)result.Properties.Keys)
            {
                var property = result.Properties[key];
                if (!(property is IEnumerable) || property is string)
                    luaTable3[(object)key] = property;
            }

            luaTable1[(object)"properties"] = (object)luaTable3;
            var num = 1;
            var luaTable4 = (LuaTable)service.NewTable();
            foreach (var error in (List<Redbox.KioskEngine.ComponentModel.Error>)result.Errors)
            {
                var luaTable5 = (LuaTable)service.NewTable();
                luaTable5[(object)"code"] = (object)error.Code;
                luaTable5[(object)"is_warning"] = (object)error.IsWarning;
                luaTable5[(object)"description"] = (object)error.Description;
                luaTable5[(object)"details"] = (object)error.Details;
                luaTable4[(object)num++] = (object)luaTable5;
            }

            luaTable1[(object)"errors"] = (object)luaTable4;
            return luaTable1;
        }

        public static LuaTable AsTableExtended(this IRemoteServiceResult result)
        {
            var service = ServiceLocator.Instance.GetService<IKernelService>();
            var luaTable1 = (LuaTable)service.NewTable();
            luaTable1[(object)"success"] = (object)result.Success;
            var luaTable2 = (LuaTable)service.NewTable();
            luaTable2[(object)"hours"] = (object)result.ExecutionTime.Hours;
            luaTable2[(object)"minutes"] = (object)result.ExecutionTime.Minutes;
            luaTable2[(object)"seconds"] = (object)result.ExecutionTime.Seconds;
            luaTable2[(object)"milliseconds"] = (object)result.ExecutionTime.Milliseconds;
            luaTable1[(object)"execution_time"] = (object)luaTable2;
            if (result.Properties is Dictionary<string, object> properties)
            {
                luaTable1[(object)"properties"] = (object)ConvertDictionary(properties);
            }
            else
            {
                LogHelper.Instance.Log(
                    "IRemoteServiceResult: IRemoteServiceResult.Properties could not be cast as Dictionary");
                luaTable1[(object)"properties"] = (object)(LuaTable)service.NewTable();
            }

            var num = 1;
            var luaTable3 = (LuaTable)service.NewTable();
            foreach (var error in (List<Redbox.KioskEngine.ComponentModel.Error>)result.Errors)
            {
                var luaTable4 = (LuaTable)service.NewTable();
                luaTable4[(object)"code"] = (object)error.Code;
                luaTable4[(object)"is_warning"] = (object)error.IsWarning;
                luaTable4[(object)"description"] = (object)error.Description;
                luaTable4[(object)"details"] = (object)error.Details;
                luaTable3[(object)num++] = (object)luaTable4;
            }

            luaTable1[(object)"errors"] = (object)luaTable3;
            return luaTable1;
        }

        private static LuaTable ConvertDictionary(Dictionary<string, object> properties)
        {
            var luaTable = (LuaTable)ServiceLocator.Instance.GetService<IKernelService>().NewTable();
            foreach (var key in properties.Keys)
            {
                var property = properties[key];
                switch (property)
                {
                    case Dictionary<string, object> _:
                        if (property is Dictionary<string, object> properties1)
                        {
                            luaTable[(object)key] = (object)ConvertDictionary(properties1);
                            continue;
                        }

                        LogHelper.Instance.Log("IRemoteServiceResult: Property[" + key +
                                               "] is Dictionary<string,object> but could be converted.");
                        continue;
                    case string _:
                    case bool _:
                    case int _:
                        luaTable[(object)key] = property;
                        continue;
                    case null:
                        continue;
                    default:
                        LogHelper.Instance.Log("IRemoteServiceResult: unexpected data type for Property[" + key +
                                               "] skipped.");
                        continue;
                }
            }

            return luaTable;
        }
    }
}