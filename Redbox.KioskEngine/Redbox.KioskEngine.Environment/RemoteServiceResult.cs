using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Redbox.KioskEngine.Environment
{
  public class RemoteServiceResult : IRemoteServiceResult
  {
    private readonly ErrorList m_errors = new ErrorList();
    private readonly IDictionary<string, object> m_properties = (IDictionary<string, object>) new Dictionary<string, object>();
    private readonly List<IRemoteServiceProviderInstruction> m_instructions = new List<IRemoteServiceProviderInstruction>();

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(this.ToJson());
      return stringBuilder.ToString();
    }

    public string ToObfuscatedString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("{");
      stringBuilder.Append("\"Success\":" + this.Success.ToJson() + ",");
      ErrorList errors = this.Errors;
      stringBuilder.Append("\"Errors\":" + (errors != null ? errors.ToJson() : (string) null) + ",");
      stringBuilder.Append("\"ExecutionTime\":" + this.ExecutionTime.ToJson() + ",");
      Dictionary<string, object> dictionary = new Dictionary<string, object>(this.Properties);
      ServiceLocator.Instance.GetService<IObfuscationService>()?.ObfuscateDictionary(dictionary);
      stringBuilder.Append("\"Properties\":" + dictionary.ToJson() + ",");
      ReadOnlyCollection<IRemoteServiceProviderInstruction> instructions = this.Instructions;
      stringBuilder.Append("\"Instructions\":" + (instructions != null ? instructions.ToJson() : (string) null));
      stringBuilder.Append("}");
      return stringBuilder.ToString();
    }

    public bool Success { get; set; }

    public ErrorList Errors => this.m_errors;

    public void AddError(string code, string message, string details)
    {
      this.m_errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError(code, message, details));
    }

    public TimeSpan ExecutionTime { get; set; }

    public IDictionary<string, object> Properties => this.m_properties;

    public ReadOnlyCollection<IRemoteServiceProviderInstruction> Instructions
    {
      get => this.InnerInstructions.AsReadOnly();
    }

    public T GetProperty<T>(string keyName)
    {
      object obj = this.m_properties.ContainsKey(keyName) ? this.m_properties[keyName] : (object) null;
      if (typeof (T) == typeof (string))
        obj = obj != null ? (object) obj.ToString() : (object) string.Empty;
      else if (typeof (T) == typeof (int))
        obj = (object) RemoteServiceResult.GetInt(obj);
      return obj != null ? (T) obj : default (T);
    }

    private static int GetInt(object obj)
    {
      int result = 0;
      Type type = obj.GetType();
      if (type == typeof (int))
        result = (int) obj;
      else if (type == typeof (string))
        int.TryParse((string) obj, out result);
      return result;
    }

    public List<IRemoteServiceProviderInstruction> InnerInstructions => this.m_instructions;
  }
}
