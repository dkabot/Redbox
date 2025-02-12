using System;
using System.ComponentModel;
using Microsoft.Win32;

namespace HALUtilities
{
    internal sealed class RegistryStore
    {
        private readonly string Key;

        internal RegistryStore(string key)
        {
            Key = key;
        }

        internal T GetSecretValue<T>(string option, T def)
        {
            var registryKey = (RegistryKey)null;
            try
            {
                registryKey = Registry.LocalMachine.CreateSubKey(Key);
                if (registryKey == null)
                    return def;
                var obj = registryKey.GetValue(option);
                return obj == null ? def : (T)ChangeType(obj, typeof(T));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to read secret option {0}", option);
                return def;
            }
            finally
            {
                registryKey?.Close();
            }
        }

        internal void SetSecretValue<T>(string option, T value)
        {
            var registryKey = (RegistryKey)null;
            try
            {
                registryKey = Registry.LocalMachine.CreateSubKey(Key);
                registryKey?.SetValue(option, value);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to set secret option {0}", option);
            }
            finally
            {
                registryKey?.Close();
            }
        }

        internal void RemoveValue(string option)
        {
            var registryKey = (RegistryKey)null;
            try
            {
                registryKey = Registry.LocalMachine.OpenSubKey(Key, true);
                registryKey?.DeleteValue(option);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to delete secret option {0}", option);
            }
            finally
            {
                registryKey?.Close();
            }
        }

        private object ChangeType(object value, Type convertToType)
        {
            if (value == null)
                return null;
            if (convertToType.IsEnum && value is string)
                return Enum.Parse(convertToType, (string)value, true);
            var converter1 = TypeDescriptor.GetConverter(convertToType);
            if (converter1 != null && converter1.CanConvertFrom(typeof(string)) && value is string)
                return converter1.ConvertFrom(value);
            if (convertToType == (object)typeof(string))
            {
                var converter2 = TypeDescriptor.GetConverter(value.GetType());
                if (converter2 != null && converter2.CanConvertTo(typeof(string)))
                    return converter2.ConvertToString(value);
            }

            if (!convertToType.IsInstanceOfType(value))
                try
                {
                    return Convert.ChangeType(value, GetNullableTypeIfWrapped(convertToType));
                }
                catch (InvalidCastException ex)
                {
                }

            return value;
        }

        private Type GetNullableTypeIfWrapped(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == (object)typeof(Nullable<>)
                ? type.GetGenericArguments()[0]
                : type;
        }
    }
}