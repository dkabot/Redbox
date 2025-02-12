using Microsoft.Win32;
using System;
using System.ComponentModel;

namespace HALUtilities
{
  internal sealed class RegistryStore
  {
    private readonly string Key;

    internal T GetSecretValue<T>(string option, T def)
    {
      RegistryKey registryKey = (RegistryKey) null;
      try
      {
        registryKey = Registry.LocalMachine.CreateSubKey(this.Key);
        if (registryKey == null)
          return def;
        object obj = registryKey.GetValue(option);
        return obj == null ? def : (T) this.ChangeType(obj, typeof (T));
      }
      catch (Exception ex)
      {
        Console.WriteLine("Unable to read secret option {0}", (object) option);
        return def;
      }
      finally
      {
        registryKey?.Close();
      }
    }

    internal void SetSecretValue<T>(string option, T value)
    {
      RegistryKey registryKey = (RegistryKey) null;
      try
      {
        registryKey = Registry.LocalMachine.CreateSubKey(this.Key);
        registryKey?.SetValue(option, (object) value);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Unable to set secret option {0}", (object) option);
      }
      finally
      {
        registryKey?.Close();
      }
    }

    internal void RemoveValue(string option)
    {
      RegistryKey registryKey = (RegistryKey) null;
      try
      {
        registryKey = Registry.LocalMachine.OpenSubKey(this.Key, true);
        registryKey?.DeleteValue(option);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Unable to delete secret option {0}", (object) option);
      }
      finally
      {
        registryKey?.Close();
      }
    }

    internal RegistryStore(string key) => this.Key = key;

    private object ChangeType(object value, Type convertToType)
    {
      if (value == null)
        return (object) null;
      if (convertToType.IsEnum && value is string)
        return Enum.Parse(convertToType, (string) value, true);
      TypeConverter converter1 = TypeDescriptor.GetConverter(convertToType);
      if (converter1 != null && converter1.CanConvertFrom(typeof (string)) && value is string)
        return converter1.ConvertFrom(value);
      if ((object) convertToType == (object) typeof (string))
      {
        TypeConverter converter2 = TypeDescriptor.GetConverter(value.GetType());
        if (converter2 != null && converter2.CanConvertTo(typeof (string)))
          return (object) converter2.ConvertToString(value);
      }
      if (!convertToType.IsInstanceOfType(value))
      {
        try
        {
          return Convert.ChangeType(value, this.GetNullableTypeIfWrapped(convertToType));
        }
        catch (InvalidCastException ex)
        {
        }
      }
      return value;
    }

    private Type GetNullableTypeIfWrapped(Type type)
    {
      return type.IsGenericType && (object) type.GetGenericTypeDefinition() == (object) typeof (Nullable<>) ? type.GetGenericArguments()[0] : type;
    }
  }
}
