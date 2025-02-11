using System;
using System.Text;
using Redbox.Core;

namespace Redbox.UpdateService.Model
{
    public class StatusMessage
    {
        public enum StatusMessageType
        {
            Info = 1,
            Error = 2,
            Warning = 3
        }

        public string Data;
        public string Description;
        public bool Encode;
        public string Key;
        public string SubKey;
        public DateTime TimeStamp;
        public StatusMessageType Type;

        public void DecodeData()
        {
            Data = Encoding.Unicode.GetString(StringExtensions.Base64ToBytes(Data));
        }

        public void EncodeData()
        {
            Data = ByteArrayExtensions.ToBase64(Encoding.Unicode.GetBytes(Data));
        }
    }
}