using Redbox.Core;
using System;
using System.Text;

namespace Redbox.UpdateService.Model
{
  public class StatusMessage
  {
    public StatusMessage.StatusMessageType Type;
    public string Key;
    public string SubKey;
    public string Description;
    public DateTime TimeStamp;
    public string Data;
    public bool Encode;

    public void DecodeData() => this.Data = Encoding.Unicode.GetString(this.Data.Base64ToBytes());

    public void EncodeData() => this.Data = Encoding.Unicode.GetBytes(this.Data).ToBase64();

    public enum StatusMessageType
    {
      Info = 1,
      Error = 2,
      Warning = 3,
    }
  }
}
