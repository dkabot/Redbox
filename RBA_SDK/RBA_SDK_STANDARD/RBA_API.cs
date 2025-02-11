using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using RBA_SDK_ComponentModel;

namespace RBA_SDK
{
    public class RBA_API : IRBA_API
    {
        private readonly BatteryLevelCallBack vsUnmanagedBatteryLevelCallBack;
        private readonly ConnectCallBack vsUnmanagedConnectCallBack;
        private readonly DisconnectCallBack vsUnmanagedDisconnectCallBack;
        private readonly PinPadMessageCallBack vsUnmanagedPinpadCallBack;
        private readonly LOUT vsUnmanagedTraceLog;
        public BatteryLevelHandler batterylevelHandler;
        private ConnectHandler connectHandler;
        private DisconnectHandler disconnectHandler;
        private bool m_SetUnmanagedLogCalBackOnce = true;
        private bool m_SetUnmanagedPinpadCalBackOnce = true;

        public RBA_API()
        {
            vsUnmanagedPinpadCallBack = unmanagedPinpadCallBack;
            vsUnmanagedBatteryLevelCallBack = unmanagedBatteryLevelCallBack;
            vsUnmanagedConnectCallBack = unmanagedConnectCallBack;
            vsUnmanagedDisconnectCallBack = unmanagedDisconnectCallBack;
            vsUnmanagedTraceLog = unmanagedTraceLog;
        }

        private static string AssemblyVersion
        {
            get
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
                return string.Format("{0}.{1}.{2}", versionInfo.FileMajorPart, versionInfo.FileMinorPart,
                    versionInfo.ProductBuildPart);
            }
        }

        public LogHandler logHandler { get; set; }

        public PinPadMessageHandler pinpadHandler { get; set; }

        public string GetVersion_NonStatic()
        {
            return GetVersion();
        }

        public ERROR_ID Initialize()
        {
            if (m_SetUnmanagedLogCalBackOnce)
            {
                SetLogCallBackNative(vsUnmanagedTraceLog);
                m_SetUnmanagedLogCalBackOnce = false;
            }

            var number = RBA_SDK_InitializeNative();
            if (number == 0 && m_SetUnmanagedPinpadCalBackOnce)
            {
                m_SetUnmanagedPinpadCalBackOnce = false;
                SetMessageCallBackNative(vsUnmanagedPinpadCallBack);
            }

            return NumToEnum<ERROR_ID>(number);
        }

        public ERROR_ID Connect(string comPort)
        {
            return Connect(new SETTINGS_COMMUNICATION
            {
                interface_id = 1U,
                rs232_config = new SETTINGS_RS232
                {
                    ComPort = comPort,
                    BaudRate = 115200U,
                    DataBits = 8U,
                    Parity = 0U,
                    FlowControl = 0U,
                    StopBits = 1U
                }
            });
        }

        public ERROR_ID SetParam(PARAMETER_ID id, string data)
        {
            return NumToEnum<ERROR_ID>(SetParamNative((int)id, data, data != null ? data.Length : 0));
        }

        public ERROR_ID SetParam(PARAMETER_ID id, byte[] data)
        {
            return NumToEnum<ERROR_ID>(SetParamNative((int)id, data, data.Length));
        }

        public ERROR_ID AddParam(PARAMETER_ID id, string data)
        {
            return NumToEnum<ERROR_ID>(AddParamNative((int)id, data, data.Length));
        }

        public int GetParamLen(PARAMETER_ID id)
        {
            return GetParamLenNative((int)id);
        }

        public string GetParam(PARAMETER_ID id)
        {
            if (GetParamLen(id) < 0)
                return "";
            byte[] data;
            var num = (int)GetParam(id, out data);
            return Encoding.Default.GetString(data);
        }

        public ERROR_ID ResetParam(PARAMETER_ID id)
        {
            return NumToEnum<ERROR_ID>(ResetParamNative((int)id));
        }

        public ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, byte[] data)
        {
            return NumToEnum<ERROR_ID>(AddTagParamNative((int)msg, tagId, data, data.Length));
        }

        public ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, string data)
        {
            var bytes = new ASCIIEncoding().GetBytes(data);
            return NumToEnum<ERROR_ID>(AddTagParamNative((int)msg, tagId, bytes, bytes.Length));
        }

        public int GetTagParamLen(MESSAGE_ID msg)
        {
            return GetTagParamLenNative((int)msg);
        }

        public int GetTagParam(MESSAGE_ID msg, out byte[] data)
        {
            var tagParam = -1;
            var tagParamLen = GetTagParamLen(msg);
            data = new byte[0];
            if (tagParamLen > 0)
            {
                data = new byte[tagParamLen];
                tagParam = GetTagParamNative((int)msg, data, ref tagParamLen);
            }

            return tagParam;
        }

        public ERROR_ID ProcessMessage(MESSAGE_ID messageId)
        {
            try
            {
                return NumToEnum<ERROR_ID>(ProcessMessageNative((int)messageId));
            }
            catch (Exception ex)
            {
                return ERROR_ID.RESULT_ERROR;
            }
        }

        public ERROR_ID SetCommTimeouts(SETTINGS_COMM_TIMEOUTS timeouts)
        {
            return NumToEnum<ERROR_ID>(SetCommTimeoutsNative(timeouts));
        }

        public ERROR_ID Disconnect()
        {
            return NumToEnum<ERROR_ID>(DisconnectNative());
        }

        public ERROR_ID SendCustomMessage(string RawData, bool bWaitResponce)
        {
            var RawData1 = new StringBuilder(RawData);
            return NumToEnum<ERROR_ID>(SendCustomMessageNative(RawData1, RawData1.Length, bWaitResponce));
        }

        public void SetDefaultLogLevel(LOG_LEVEL level)
        {
            SetDefaultLogLevelNative(level);
        }

        public ERROR_ID SetNotifyRbaDisconnected(DisconnectHandler handler)
        {
            if (handler != null)
            {
                disconnectHandler = handler;
                return NumToEnum<ERROR_ID>(SetNotifyRbaDisconnectedNative(vsUnmanagedDisconnectCallBack));
            }

            disconnectHandler = null;
            return NumToEnum<ERROR_ID>(SetNotifyRbaDisconnectedNative(null));
        }

        object IRBA_API.GetMutableParam(PARAMETER_ID id)
        {
            throw new NotImplementedException();
        }

        private void unmanagedTraceLog(string line)
        {
            try
            {
                var logHandler = this.logHandler;
                if (logHandler == null)
                    return;
                logHandler(line);
            }
            catch (Exception ex)
            {
                var logHandler = this.logHandler;
                if (logHandler == null)
                    return;
                logHandler(ex.ToString());
            }
        }

        private void unmanagedPinpadCallBack(int msgId)
        {
            try
            {
                if (pinpadHandler != null)
                    pinpadHandler((MESSAGE_ID)Enum.ToObject(typeof(MESSAGE_ID), msgId));
                else
                    LogOut("RBA_SDK_C#", LOG_LEVEL.WARNING, "No pinpadHandler defined");
            }
            catch (Exception ex)
            {
                var logHandler = this.logHandler;
                if (logHandler == null)
                    return;
                logHandler(ex.ToString());
            }
        }

        private void unmanagedBatteryLevelCallBack(
            int level,
            BATTERY_STATE btSate,
            BATTERY_LEVEL_STATE btLevelState)
        {
            try
            {
                var batterylevelHandler = this.batterylevelHandler;
                if (batterylevelHandler == null)
                    return;
                batterylevelHandler(level, btSate, btLevelState);
            }
            catch (Exception ex)
            {
                var logHandler = this.logHandler;
                if (logHandler == null)
                    return;
                logHandler(ex.ToString());
            }
        }

        private void unmanagedConnectCallBack()
        {
            try
            {
                if (connectHandler != null)
                    connectHandler();
                else
                    LogOut("RBA_SDK_C#", LOG_LEVEL.WARNING, "No connectHandler defined");
            }
            catch (Exception ex)
            {
                var logHandler = this.logHandler;
                if (logHandler == null)
                    return;
                logHandler(ex.ToString());
            }
        }

        private void unmanagedDisconnectCallBack()
        {
            try
            {
                if (disconnectHandler != null)
                    disconnectHandler();
                else
                    LogOut("RBA_SDK_C#", LOG_LEVEL.WARNING, "No disconnectHandler defined");
            }
            catch (Exception ex)
            {
                var logHandler = this.logHandler;
                if (logHandler == null)
                    return;
                logHandler(ex.ToString());
            }
        }

        public static unsafe string GetVersion()
        {
            return new string((sbyte*)GetVersionNative());
        }

        public ERROR_ID Shutdown()
        {
            pinpadHandler = null;
            batterylevelHandler = null;
            SetMessageCallBackNative(null);
            var num = (int)NumToEnum<ERROR_ID>(RBA_SDK_ShutdownNative());
            SetMessageCallBackNative(null);
            logHandler = null;
            m_SetUnmanagedLogCalBackOnce = true;
            m_SetUnmanagedPinpadCalBackOnce = true;
            return (ERROR_ID)num;
        }

        private ERROR_ID CheckVersion()
        {
            var maxValue = uint.MaxValue;
            LogOut(maxValue, "RBA_SDK_C#", LOG_LEVEL.INFO, "Version: " + AssemblyVersion);
            var str = GetVersion().Split(' ')[0];
            if (!(AssemblyVersion != str))
                return ERROR_ID.RESULT_SUCCESS;
            LogOut(maxValue, "RBA_SDK_C#", LOG_LEVEL.ERROR,
                "Version inconsistency: RBA_SDK_CSharp.dll version is " + AssemblyVersion + " RBA_SDK.DLL version is " +
                str);
            return ERROR_ID.RESULT_ERROR_NOT_INITIALIZED;
        }

        public static string GetAssemblyVersion()
        {
            return AssemblyVersion;
        }

        public ERROR_ID Connect(SETTINGS_COMMUNICATION Settings)
        {
            return NumToEnum<ERROR_ID>(ConnectNative(Settings));
        }

        public ERROR_ID Reconnect()
        {
            return NumToEnum<ERROR_ID>(ReconnectNative());
        }

        public ERROR_ID AddParam(PARAMETER_ID id, byte[] data)
        {
            return NumToEnum<ERROR_ID>(AddParamNative((int)id, data, data.Length));
        }

        public ERROR_ID GetParam(PARAMETER_ID id, out StringBuilder data)
        {
            var paramLen = GetParamLen(id);
            ERROR_ID errorId;
            if (paramLen < 0)
            {
                data = new StringBuilder(0);
                errorId = (ERROR_ID)paramLen;
            }
            else
            {
                data = new StringBuilder(paramLen + 1);
                errorId = (ERROR_ID)GetParamNative((int)id, data, ref paramLen);
                data.Length = paramLen;
            }

            return errorId;
        }

        private ERROR_ID GetParam(PARAMETER_ID id, out byte[] data)
        {
            var paramLen = GetParamLen(id);
            ERROR_ID errorId;
            if (paramLen < 0)
            {
                data = new byte[0];
                errorId = (ERROR_ID)paramLen;
            }
            else
            {
                data = new byte[paramLen];
                errorId = (ERROR_ID)GetParamNative((int)id, data, ref paramLen);
            }

            return errorId;
        }

        public string GetParam(PARAMETER_ID id, out ERROR_ID errorId)
        {
            var paramLen = GetParamLen(id);
            if (paramLen < 0)
            {
                errorId = (ERROR_ID)paramLen;
                return "";
            }

            byte[] data;
            errorId = GetParam(id, out data);
            return Encoding.Default.GetString(data);
        }

        public byte[] GetParamAsByteArray(PARAMETER_ID id, out ERROR_ID errorId)
        {
            var data = new byte[0];
            var paramLen = GetParamLen(id);
            errorId = paramLen >= 0 ? GetParam(id, out data) : (ERROR_ID)paramLen;
            return data;
        }

        public byte[] GetParamAsByteArray(PARAMETER_ID id)
        {
            var data = (byte[])null;
            var num = (int)GetParam(id, out data);
            return data;
        }

        public StringBuilder GetMutableParam(PARAMETER_ID id)
        {
            StringBuilder data;
            var num = (int)GetParam(id, out data);
            return data;
        }

        public ERROR_ID LockParam()
        {
            return NumToEnum<ERROR_ID>(LockParamNative());
        }

        public ERROR_ID UnlockParam()
        {
            return NumToEnum<ERROR_ID>(UnlockParamNative());
        }

        public ERROR_ID SetAttribute(ATTRIBUTE_ID id, string data)
        {
            return NumToEnum<ERROR_ID>(SetAttributeNative((int)id, data, data.Length));
        }

        public int GetAttributeLen(ATTRIBUTE_ID id)
        {
            return GetAttributeLenNative((int)id);
        }

        public string GetAttribute(ATTRIBUTE_ID id)
        {
            var attributeLen = GetAttributeLen(id);
            var attribute = "";
            if (attributeLen > 0)
            {
                var data = new StringBuilder(attributeLen);
                if (GetAttributeNative((int)id, data, ref attributeLen) == 0 && attributeLen <= data.Length)
                    attribute = data.ToString().Substring(0, attributeLen);
            }

            return attribute;
        }

        public ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, bool emvTag, byte[] data)
        {
            return NumToEnum<ERROR_ID>(AddTagParamWithFlagNative((int)msg, tagId, emvTag ? 1 : 0, data, data.Length));
        }

        public ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, bool emvTag, string data)
        {
            var bytes = new ASCIIEncoding().GetBytes(data);
            return NumToEnum<ERROR_ID>(AddTagParamWithFlagNative((int)msg, tagId, emvTag ? 1 : 0, bytes, bytes.Length));
        }

        public Tag GetTag(MESSAGE_ID msg, out byte[] data)
        {
            var tag = new Tag();
            var tagParamLen = GetTagParamLen(msg);
            data = new byte[0];
            if (tagParamLen > 0)
            {
                var emvTag = 0;
                data = new byte[tagParamLen];
                tag.id = GetTagParamAndFlagNative((int)msg, data, ref tagParamLen, ref emvTag);
                tag.emvTag = emvTag == 1;
            }

            return tag;
        }

        public ERROR_ID ResetTagParam(MESSAGE_ID msg)
        {
            return NumToEnum<ERROR_ID>(ResetTagParamNative((int)msg, uint.MaxValue));
        }

        public ERROR_ID ResetTagParam()
        {
            return NumToEnum<ERROR_ID>(ResetTagParamNative(-1, uint.MaxValue));
        }

        public ERROR_ID SetProcessMessageAsyncMode()
        {
            return NumToEnum<ERROR_ID>(SetProcessMessageAsyncModeNative());
        }

        public ERROR_ID SetProcessMessageSyncMode()
        {
            return NumToEnum<ERROR_ID>(SetProcessMessageSyncModeNative());
        }

        public ERROR_ID SetResponseTimeout(uint timeoutMsec)
        {
            var timeouts = GetCommTimeouts();
            timeouts.ReceiveTimeout = timeoutMsec;
            return SetCommTimeouts(timeouts);
        }

        public SETTINGS_COMM_TIMEOUTS GetCommTimeouts()
        {
            return GetCommTimeoutsNative();
        }

        public uint GetResponseTimeout()
        {
            return GetCommTimeouts().ReceiveTimeout;
        }

        public CONNECTION_STATUS GetConnectionStatus()
        {
            return NumToEnum<CONNECTION_STATUS>(GetConnectionStatusNative());
        }

        public void SetModuleLogLevel(string moduleName, LOG_LEVEL level)
        {
            SetModuleLogLevelNative(moduleName, level);
        }

        public void SetTraceOutputFormatOption(LOG_OUTPUT_FORMAT_OPTIONS option)
        {
            SetTraceOutputFormatOptionNative(option);
        }

        public ERROR_ID SetBatteryNotifyThreshold(int threshold)
        {
            return NumToEnum<ERROR_ID>(SetBatteryNotifyThresholdNative(threshold, vsUnmanagedBatteryLevelCallBack));
        }

        public ERROR_ID SetBatteryNotifyThreshold(int threshold, BatteryLevelHandler handler)
        {
            batterylevelHandler = handler;
            return NumToEnum<ERROR_ID>(SetBatteryNotifyThresholdNative(threshold, vsUnmanagedBatteryLevelCallBack));
        }

        public ERROR_ID SetNotifyRbaConnected(ConnectHandler handler)
        {
            if (handler != null)
            {
                connectHandler = handler;
                return NumToEnum<ERROR_ID>(SetNotifyRbaConnectedNative(vsUnmanagedConnectCallBack));
            }

            connectHandler = null;
            return NumToEnum<ERROR_ID>(SetNotifyRbaConnectedNative(null));
        }

        private static T NumToEnum<T>(int number)
        {
            return (T)Enum.ToObject(typeof(T), number);
        }

        public void LogOut(string moduleName, LOG_LEVEL logLevel, string logString)
        {
            try
            {
                LogOutNative(moduleName, logLevel, logString);
            }
            catch (Exception ex)
            {
                var logHandler = this.logHandler;
                if (logHandler == null)
                    return;
                logHandler(ex?.ToString());
            }
        }

        public static void LogOut(
            uint instanceId,
            string moduleName,
            LOG_LEVEL logLevel,
            string logString)
        {
            try
            {
                LogOutNativeFor(instanceId, moduleName, logLevel, logString);
            }
            catch (Exception ex)
            {
            }
        }

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetVersion", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe char* GetVersionNative();

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_Initialize", CallingConvention = CallingConvention.Cdecl)]
        private static extern int RBA_SDK_InitializeNative();

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_InitializeInstance", CallingConvention = CallingConvention.Cdecl)]
        private static extern int RBA_SDK_InitializeInstanceNative(string name);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetInstanceName", CallingConvention = CallingConvention.Cdecl)]
        private static extern void RBA_SDK_SetInstanceNameNative(uint instanceId, string name);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_Shutdown", CallingConvention = CallingConvention.Cdecl)]
        private static extern int RBA_SDK_ShutdownNative();

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_ShutdownInstance", CallingConvention = CallingConvention.Cdecl)]
        private static extern int RBA_SDK_ShutdownInstanceNative(uint instanceId);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetParam", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetParamNative(int id, string data, int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetParamFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetParamNativeFor(uint instanceId, int id, string data, int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetParam", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetParamNative(int id, byte[] data, int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetParamFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetParamNativeFor(uint instanceId, int id, byte[] data, int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_AddParam", CallingConvention = CallingConvention.Cdecl)]
        private static extern int AddParamNative(int id, string data, int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_AddParamFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int AddParamNativeFor(uint instanceId, int id, string data, int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_AddParam", CallingConvention = CallingConvention.Cdecl)]
        private static extern int AddParamNative(int id, byte[] data, int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_AddParamFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int AddParamNativeFor(uint instanceId, int id, byte[] data, int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetParam", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetParamNative(int id, StringBuilder data, ref int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetParamFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetParamNativeFor(
            uint instanceId,
            int id,
            StringBuilder data,
            ref int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetParam", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetParamNative(int id, byte[] data, ref int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetParamFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetParamNativeFor(uint instanceId, int id, byte[] data, ref int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetParamLen", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetParamLenNative(int id);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetParamLenFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetParamLenNativeFor(uint instanceId, int id);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_ResetParam", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ResetParamNative(int id);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_ResetParamFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ResetParamNativeFor(uint instanceId, int id);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_LockParam", CallingConvention = CallingConvention.Cdecl)]
        private static extern int LockParamNative();

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_LockParamFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int LockParamNativeFor(uint instanceId);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_UnlockParam", CallingConvention = CallingConvention.Cdecl)]
        private static extern int UnlockParamNative();

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_UnlockParamFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int UnlockParamNativeFor(uint instanceId);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetAttribute", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetAttributeNative(int id, string data, int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetAttributeFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetAttributeNativeFor(uint instanceId, int id, string data, int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetAttributeLen", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetAttributeLenNative(int id);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetAttributeLenFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetAttributeLenNativeFor(uint instanceId, int id);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetAttribute", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetAttributeNative(int id, StringBuilder data, ref int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetAttributeFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetAttributeNativeFor(
            uint instanceId,
            int id,
            StringBuilder data,
            ref int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_AddTagParam", CallingConvention = CallingConvention.Cdecl)]
        private static extern int AddTagParamNative(int msg, int tagId, byte[] data, int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_AddTagParamFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int AddTagParamNativeFor(
            uint instanceId,
            int msg,
            int tagId,
            byte[] data,
            int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_AddTagParamWithFlag", CallingConvention = CallingConvention.Cdecl)]
        private static extern int AddTagParamWithFlagNative(
            int msg,
            int tagId,
            int emvTag,
            byte[] data,
            int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_AddTagParamWithFlagFor",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern int AddTagParamWithFlagNativeFor(
            uint instanceId,
            int msg,
            int tagId,
            int EmvTag,
            byte[] data,
            int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetTagParam", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetTagParamNative(int msg, byte[] data, ref int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetTagParamFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetTagParamNativeFor(
            uint instanceId,
            int msg,
            byte[] data,
            ref int len);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetTagParamAndFlag", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetTagParamAndFlagNative(
            int msg,
            byte[] data,
            ref int len,
            ref int emvTag);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetTagParamAndFlagFor",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetTagParamAndFlagNativeFor(
            uint instanceId,
            int msg,
            byte[] data,
            ref int len,
            ref int emvTag);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetTagParamLen", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetTagParamLenNative(int msg);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetTagParamLenFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetTagParamLenNativeFor(uint instanceId, int msg);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_ResetTagParam", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ResetTagParamNative(int msg, uint tagId);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_ResetTagParamFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ResetTagParamNativeFor(uint instanceId, int msg, uint tagId);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_ProcessMessage", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ProcessMessageNative(int CommandID);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_ProcessMessageFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ProcessMessageNativeFor(uint instanceId, int CommandID);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetProcessMessageAsyncMode",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetProcessMessageAsyncModeNative();

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetProcessMessageAsyncModeFor",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetProcessMessageAsyncModeNativeFor(uint instanceId);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetProcessMessageSyncMode",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetProcessMessageSyncModeNative();

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetProcessMessageSyncModeFor",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetProcessMessageSyncModeNativeFor(uint instanceId);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetCommTimeouts", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetCommTimeoutsNative(SETTINGS_COMM_TIMEOUTS Timeouts);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetCommTimeoutsFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetCommTimeoutsNativeFor(
            uint instanceId,
            SETTINGS_COMM_TIMEOUTS Timeouts);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetCommTimeouts", CallingConvention = CallingConvention.Cdecl)]
        private static extern SETTINGS_COMM_TIMEOUTS GetCommTimeoutsNative();

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetCommTimeoutsFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern SETTINGS_COMM_TIMEOUTS GetCommTimeoutsNativeFor(uint instanceId);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_Connect", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ConnectNative(SETTINGS_COMMUNICATION Settings);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_ConnectFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ConnectNativeFor(uint instanceId, SETTINGS_COMMUNICATION Settings);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_Reconnect", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ReconnectNative();

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_ReconnectFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ReconnectNativeFor(uint instanceId);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetConnectionStatus", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetConnectionStatusNative();

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_GetConnectionStatusFor",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetConnectionStatusNativeFor(uint instanceId);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_Disconnect", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DisconnectNative();

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_DisconnectFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DisconnectNativeFor(uint instanceId);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SendCustomMessage", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SendCustomMessageNative(
            StringBuilder RawData,
            int DataLength,
            [MarshalAs(UnmanagedType.I1)] bool bWaitResponce);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SendCustomMessageFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SendCustomMessageNativeFor(
            uint instanceId,
            StringBuilder RawData,
            int DataLength,
            [MarshalAs(UnmanagedType.I1)] bool bWaitResponce);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetMessageCallBack", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetMessageCallBackNative(
            PinPadMessageCallBack cbMessageHandler);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetMessageCallBackWithInstanceId",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int SetMessageCallBackWithInstanceIdNative(
            uint instanceId,
            PinPadMessageCallBackWithInstanceId cbMessageHandler,
            void* userParam);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetNotifyRbaConnected",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetNotifyRbaConnectedNative(ConnectCallBack cbConnectHandler);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetNotifyRbaConnectedFor",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetNotifyRbaConnectedForNative(
            uint instanceId,
            ConnectCallBackWithInstanceId cbConnectHandler);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetNotifyRbaDisconnected",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetNotifyRbaDisconnectedNative(
            DisconnectCallBack cbConnectHandler);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetNotifyRbaDisconnectedFor",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetNotifyRbaDisconnectedForNative(
            uint instanceId,
            DisconnectCallBackWithInstanceId cbConnectHandler);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetLogCallBack", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetLogCallBackNative(LOUT cbFunction);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetDefaultLogLevel", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetDefaultLogLevelNative(LOG_LEVEL level);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetModuleLogLevel", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetModuleLogLevelNative([MarshalAs(UnmanagedType.LPStr)] [In] string moduleName,
            LOG_LEVEL level);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetTraceOutputFormatOption",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetTraceOutputFormatOptionNative(LOG_OUTPUT_FORMAT_OPTIONS level);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_SetBatteryNotifyThreshold",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetBatteryNotifyThresholdNative(
            int threshold,
            BatteryLevelCallBack cbMessageHandler);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_LogOut", CallingConvention = CallingConvention.Cdecl)]
        private static extern int LogOutNative([MarshalAs(UnmanagedType.LPStr)] [In] string module, LOG_LEVEL level,
            [MarshalAs(UnmanagedType.LPStr)] [In] string line);

        [DllImport("RBA_SDK", EntryPoint = "RBA_SDK_LogOutFor", CallingConvention = CallingConvention.Cdecl)]
        private static extern int LogOutNativeFor(
            uint instanceId,
            [MarshalAs(UnmanagedType.LPStr)] [In] string module,
            LOG_LEVEL level,
            [MarshalAs(UnmanagedType.LPStr)] [In] string line);

        public unsafe class RBASDK : IDisposable
        {
            private static readonly PinPadMessageCallBackWithInstanceId vsPinpadCallBackWithInstanceId =
                PinpadCallBackWithInstanceId;

            private static readonly ConnectCallBackWithInstanceId vsUnmanagedConnectCallBackWithInstanceId =
                UnmanagedConnectCallBackWithInstanceId;

            private static readonly DisconnectCallBackWithInstanceId vsUnmanagedDisconnectCallBackWithInstanceId =
                UnmanagedDisconnectCallBackWithInstance;

            private static readonly List<RBASDK> m_rba_sdk_instances = new List<RBASDK>();
            private ConnectHandler connectHandler;
            public DisconnectHandler disconnectHandler;
            private bool m_disposed;
            private int m_instance_id = -50;
            private string m_name;
            public PinPadMessageHandler pinpadHandler;

            public RBASDK(RBA_API rba_api, string name)
            {
                if (rba_api.m_SetUnmanagedLogCalBackOnce)
                {
                    SetLogCallBackNative(rba_api.vsUnmanagedTraceLog);
                    rba_api.m_SetUnmanagedLogCalBackOnce = false;
                }

                m_instance_id = RBA_SDK_InitializeInstanceNative(name);
                if (m_instance_id <= 0)
                    return;
                m_rba_sdk_instances.Add(this);
                SetMessageCallBackWithInstanceIdNative((uint)m_instance_id, vsPinpadCallBackWithInstanceId, null);
                SetNotifyRbaDisconnectedForNative((uint)m_instance_id, vsUnmanagedDisconnectCallBackWithInstanceId);
                m_name = name;
            }

            public void Dispose()
            {
                if (m_disposed)
                    return;
                var num = (int)Shutdown();
                GC.SuppressFinalize(this);
                m_disposed = true;
            }

            ~RBASDK()
            {
                Dispose();
            }

            public string GetInstanceName()
            {
                return m_name;
            }

            public void SetInstanceName(string name)
            {
                m_name = name;
                RBA_SDK_SetInstanceNameNative((uint)m_instance_id, name);
            }

            public int GetInstanceId()
            {
                return m_instance_id;
            }

            public string GetVersion()
            {
                return RBA_API.GetVersion();
            }

            public string GetAssemblyVersion()
            {
                return RBA_API.GetAssemblyVersion();
            }

            private ERROR_ID Shutdown()
            {
                pinpadHandler = null;
                connectHandler = null;
                disconnectHandler = null;
                var errorId = ERROR_ID.RESULT_ERROR_NOT_INITIALIZED;
                if (m_instance_id > 0)
                {
                    m_rba_sdk_instances.Remove(this);
                    errorId = NumToEnum<ERROR_ID>(RBA_SDK_ShutdownInstanceNative((uint)m_instance_id));
                    m_instance_id = -50;
                    m_name = "";
                }

                return errorId;
            }

            private static void PinpadCallBackWithInstanceId(
                uint instanceId,
                int msgId,
                void* userParam)
            {
                foreach (var rbaSdkInstance in m_rba_sdk_instances)
                    if (rbaSdkInstance.m_instance_id == instanceId)
                    {
                        if (rbaSdkInstance.pinpadHandler != null)
                            rbaSdkInstance.pinpadHandler((MESSAGE_ID)Enum.ToObject(typeof(MESSAGE_ID), msgId));
                        else
                            LogOut(instanceId, "RBA_SDK_C#", LOG_LEVEL.WARNING, "No pinpadHandler defined");
                    }
            }

            private static void UnmanagedConnectCallBackWithInstanceId(uint instanceId)
            {
                foreach (var rbaSdkInstance in m_rba_sdk_instances)
                    if (rbaSdkInstance.m_instance_id == instanceId)
                    {
                        if (rbaSdkInstance.connectHandler != null)
                            rbaSdkInstance.connectHandler();
                        else
                            LogOut(instanceId, "RBA_SDK_C#", LOG_LEVEL.WARNING, "No connectHandler defined");
                    }
            }

            private static void UnmanagedDisconnectCallBackWithInstance(uint instanceId)
            {
                foreach (var rbaSdkInstance in m_rba_sdk_instances)
                    if (rbaSdkInstance.m_instance_id == instanceId)
                    {
                        if (rbaSdkInstance.disconnectHandler != null)
                            rbaSdkInstance.disconnectHandler();
                        else
                            LogOut(instanceId, "RBA_SDK_C#", LOG_LEVEL.WARNING, "No disconnectHandler defined");
                    }
            }

            public ERROR_ID SetNotifyRbaConnected(ConnectHandler handler)
            {
                if (handler != null)
                {
                    connectHandler = handler;
                    return NumToEnum<ERROR_ID>(SetNotifyRbaConnectedForNative((uint)m_instance_id,
                        vsUnmanagedConnectCallBackWithInstanceId));
                }

                connectHandler = null;
                return NumToEnum<ERROR_ID>(SetNotifyRbaConnectedForNative((uint)m_instance_id, null));
            }

            public ERROR_ID Connect(SETTINGS_COMMUNICATION Settings)
            {
                return NumToEnum<ERROR_ID>(ConnectNativeFor((uint)m_instance_id, Settings));
            }

            public ERROR_ID Reconnect()
            {
                return NumToEnum<ERROR_ID>(ReconnectNativeFor((uint)m_instance_id));
            }

            public ERROR_ID SetParam(PARAMETER_ID id, string data)
            {
                return NumToEnum<ERROR_ID>(SetParamNativeFor((uint)m_instance_id, (int)id, data, data.Length));
            }

            public ERROR_ID SetParam(PARAMETER_ID id, byte[] data)
            {
                return NumToEnum<ERROR_ID>(SetParamNativeFor((uint)m_instance_id, (int)id, data, data.Length));
            }

            public ERROR_ID AddParam(PARAMETER_ID id, string data)
            {
                return NumToEnum<ERROR_ID>(AddParamNativeFor((uint)m_instance_id, (int)id, data, data.Length));
            }

            public ERROR_ID AddParam(PARAMETER_ID id, byte[] data)
            {
                return NumToEnum<ERROR_ID>(AddParamNativeFor((uint)m_instance_id, (int)id, data, data.Length));
            }

            public int GetParamLen(PARAMETER_ID id)
            {
                return GetParamLenNativeFor((uint)m_instance_id, (int)id);
            }

            public ERROR_ID GetParam(PARAMETER_ID id, out StringBuilder data)
            {
                var paramLen = GetParamLen(id);
                ERROR_ID errorId;
                if (paramLen < 0)
                {
                    data = new StringBuilder(0);
                    errorId = (ERROR_ID)paramLen;
                }
                else
                {
                    data = new StringBuilder(paramLen);
                    errorId = (ERROR_ID)GetParamNativeFor((uint)m_instance_id, (int)id, data, ref paramLen);
                    data.Length = paramLen;
                }

                return errorId;
            }

            private ERROR_ID GetParam(PARAMETER_ID id, out byte[] data)
            {
                var paramLen = GetParamLen(id);
                ERROR_ID errorId;
                if (paramLen < 0)
                {
                    data = new byte[0];
                    errorId = (ERROR_ID)paramLen;
                }
                else
                {
                    data = new byte[paramLen];
                    errorId = (ERROR_ID)GetParamNativeFor((uint)m_instance_id, (int)id, data, ref paramLen);
                }

                return errorId;
            }

            public ERROR_ID GetParamAsByteArray(PARAMETER_ID id, out byte[] data)
            {
                return GetParam(id, out data);
            }

            public byte[] GetParamAsByteArray(PARAMETER_ID id, out ERROR_ID errorId)
            {
                byte[] data;
                errorId = GetParam(id, out data);
                return data;
            }

            public byte[] GetParamAsByteArray(PARAMETER_ID id)
            {
                byte[] data;
                var num = (int)GetParam(id, out data);
                return data;
            }

            public string GetParam(PARAMETER_ID id, out ERROR_ID errorId)
            {
                var paramLen = GetParamLen(id);
                if (paramLen < 0)
                {
                    errorId = (ERROR_ID)paramLen;
                    return "";
                }

                byte[] data;
                errorId = GetParam(id, out data);
                return Encoding.Default.GetString(data);
            }

            public string GetParam(PARAMETER_ID id)
            {
                if (GetParamLen(id) < 0)
                    return "";
                byte[] data;
                var num = (int)GetParam(id, out data);
                return Encoding.Default.GetString(data);
            }

            public StringBuilder GetMutableParam(PARAMETER_ID id)
            {
                StringBuilder data;
                var num = (int)GetParam(id, out data);
                return data;
            }

            public ERROR_ID ResetParam(PARAMETER_ID id)
            {
                return NumToEnum<ERROR_ID>(ResetParamNativeFor((uint)m_instance_id, (int)id));
            }

            public ERROR_ID LockParam()
            {
                return NumToEnum<ERROR_ID>(LockParamNativeFor((uint)m_instance_id));
            }

            public ERROR_ID UnlockParam()
            {
                return NumToEnum<ERROR_ID>(UnlockParamNativeFor((uint)m_instance_id));
            }

            public ERROR_ID SetAttribute(ATTRIBUTE_ID id, string data)
            {
                return NumToEnum<ERROR_ID>(SetAttributeNativeFor((uint)m_instance_id, (int)id, data, data.Length));
            }

            public int GetAttributeLen(ATTRIBUTE_ID id)
            {
                return GetAttributeLenNativeFor((uint)m_instance_id, (int)id);
            }

            public string GetAttribute(ATTRIBUTE_ID id)
            {
                var attributeLen = GetAttributeLen(id);
                var attribute = "";
                if (attributeLen > 0)
                {
                    var data = new StringBuilder(attributeLen);
                    if (GetAttributeNativeFor((uint)m_instance_id, (int)id, data, ref attributeLen) == 0 &&
                        attributeLen <= data.Length)
                        attribute = data.ToString().Substring(0, attributeLen);
                }

                return attribute;
            }

            public ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, byte[] data)
            {
                return NumToEnum<ERROR_ID>(
                    AddTagParamNativeFor((uint)m_instance_id, (int)msg, tagId, data, data.Length));
            }

            public ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, bool emvTag, byte[] data)
            {
                return NumToEnum<ERROR_ID>(AddTagParamWithFlagNativeFor((uint)m_instance_id, (int)msg, tagId,
                    emvTag ? 1 : 0, data, data.Length));
            }

            public ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, string data)
            {
                var bytes = new ASCIIEncoding().GetBytes(data);
                return NumToEnum<ERROR_ID>(AddTagParamNativeFor((uint)m_instance_id, (int)msg, tagId, bytes,
                    bytes.Length));
            }

            public ERROR_ID AddTagParam(MESSAGE_ID msg, int tagId, bool emvTag, string data)
            {
                var bytes = new ASCIIEncoding().GetBytes(data);
                return NumToEnum<ERROR_ID>(AddTagParamWithFlagNativeFor((uint)m_instance_id, (int)msg, tagId,
                    emvTag ? 1 : 0, bytes, bytes.Length));
            }

            public int GetTagParamLen(MESSAGE_ID msg)
            {
                return GetTagParamLenNativeFor((uint)m_instance_id, (int)msg);
            }

            public int GetTagParam(MESSAGE_ID msg, out byte[] data)
            {
                var tagParam = -1;
                var paramLenNativeFor = GetTagParamLenNativeFor((uint)m_instance_id, (int)msg);
                data = new byte[0];
                if (paramLenNativeFor > 0)
                {
                    data = new byte[paramLenNativeFor];
                    tagParam = GetTagParamNativeFor((uint)m_instance_id, (int)msg, data, ref paramLenNativeFor);
                }

                return tagParam;
            }

            public Tag GetTag(MESSAGE_ID msg, out byte[] data)
            {
                var tag = new Tag();
                var paramLenNativeFor = GetTagParamLenNativeFor((uint)m_instance_id, (int)msg);
                data = new byte[0];
                if (paramLenNativeFor > 0)
                {
                    var emvTag = 0;
                    data = new byte[paramLenNativeFor];
                    tag.id = GetTagParamAndFlagNativeFor((uint)m_instance_id, (int)msg, data, ref paramLenNativeFor,
                        ref emvTag);
                    tag.emvTag = emvTag == 1;
                }

                return tag;
            }

            public ERROR_ID ResetTagParam(MESSAGE_ID msg)
            {
                return NumToEnum<ERROR_ID>(ResetTagParamNativeFor((uint)m_instance_id, (int)msg, uint.MaxValue));
            }

            public ERROR_ID ResetTagParam()
            {
                return NumToEnum<ERROR_ID>(ResetTagParamNativeFor((uint)m_instance_id, -1, uint.MaxValue));
            }

            public ERROR_ID ProcessMessage(MESSAGE_ID messageId)
            {
                try
                {
                    return NumToEnum<ERROR_ID>(ProcessMessageNativeFor((uint)m_instance_id, (int)messageId));
                }
                catch (Exception ex)
                {
                    return ERROR_ID.RESULT_ERROR;
                }
            }

            public ERROR_ID SetProcessMessageAsyncMode()
            {
                return NumToEnum<ERROR_ID>(SetProcessMessageAsyncModeNativeFor((uint)m_instance_id));
            }

            public ERROR_ID SetProcessMessageSyncMode()
            {
                return NumToEnum<ERROR_ID>(SetProcessMessageSyncModeNativeFor((uint)m_instance_id));
            }

            public ERROR_ID SetCommTimeouts(SETTINGS_COMM_TIMEOUTS timeouts)
            {
                return NumToEnum<ERROR_ID>(SetCommTimeoutsNativeFor((uint)m_instance_id, timeouts));
            }

            public ERROR_ID SetResponseTimeout(uint timeoutMsec)
            {
                var timeouts = GetCommTimeouts();
                timeouts.ReceiveTimeout = timeoutMsec;
                return SetCommTimeouts(timeouts);
            }

            public SETTINGS_COMM_TIMEOUTS GetCommTimeouts()
            {
                return GetCommTimeoutsNativeFor((uint)m_instance_id);
            }

            public uint GetResponseTimeout()
            {
                return GetCommTimeouts().ReceiveTimeout;
            }

            public CONNECTION_STATUS GetConnectionStatus()
            {
                return NumToEnum<CONNECTION_STATUS>(GetConnectionStatusNativeFor((uint)m_instance_id));
            }

            public ERROR_ID Disconnect()
            {
                return NumToEnum<ERROR_ID>(DisconnectNativeFor((uint)m_instance_id));
            }

            public ERROR_ID SendCustomMessage(string RawData, bool bWaitResponce)
            {
                var RawData1 = new StringBuilder(RawData);
                var length = RawData1.Length;
                return NumToEnum<ERROR_ID>(SendCustomMessageNativeFor((uint)m_instance_id, RawData1, length,
                    bWaitResponce));
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LOUT([MarshalAs(UnmanagedType.LPStr)] [In] string line);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PinPadMessageCallBack(int msgId);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void PinPadMessageCallBackWithInstanceId(
            uint instaneId,
            int msgId,
            void* userParam);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void BatteryLevelCallBack(
            int level,
            BATTERY_STATE btSate,
            BATTERY_LEVEL_STATE btLevelState);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ConnectCallBack();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ConnectCallBackWithInstanceId(uint instaneId);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DisconnectCallBack();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DisconnectCallBackWithInstanceId(uint instaneId);
    }
}