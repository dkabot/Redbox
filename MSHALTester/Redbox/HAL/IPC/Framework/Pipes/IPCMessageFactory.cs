using System;
using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework.Pipes;

public sealed class IPCMessageFactory : IIPCMessageFactory
{
    public IIPCMessage Create(MessageType p, MessageSeverity s, string message)
    {
        return new IPCMessage
        {
            Type = p,
            Severity = s,
            Message = message,
            UID = Guid.NewGuid(),
            Timestamp = DateTime.Now
        };
    }

    public IIPCMessage CreateAck(Guid guid)
    {
        return new IPCMessage
        {
            Type = MessageType.Ack,
            Severity = MessageSeverity.None,
            Message = string.Empty,
            UID = guid,
            Timestamp = DateTime.Now
        };
    }

    public IIPCMessage CreateNack(Guid guid)
    {
        return new IPCMessage
        {
            Type = MessageType.Nack,
            Severity = MessageSeverity.None,
            Message = string.Empty,
            UID = guid,
            Timestamp = DateTime.Now
        };
    }

    public IIPCMessage Read(IIPCChannel channel)
    {
        var numArray1 = channel.Read();
        var num = 4;
        var startIndex1 = 0;
        var int32_1 = BitConverter.ToInt32(numArray1, startIndex1);
        var sourceIndex = startIndex1 + num;
        var numArray2 = new byte[int32_1];
        Array.Copy(numArray1, sourceIndex, numArray2, 0, int32_1);
        var guid = new Guid(numArray2);
        var startIndex2 = sourceIndex + int32_1;
        var int32_2 = (MessageType)BitConverter.ToInt32(numArray1, startIndex2);
        var startIndex3 = startIndex2 + num;
        var int32_3 = (MessageSeverity)BitConverter.ToInt32(numArray1, startIndex3);
        var startIndex4 = startIndex3 + num;
        var int32_4 = BitConverter.ToInt32(numArray1, startIndex4);
        var index = startIndex4 + num;
        var str = Encoding.ASCII.GetString(numArray1, index, int32_4);
        var startIndex5 = index + int32_4;
        var dateTime = DateTime.FromBinary(BitConverter.ToInt64(numArray1, startIndex5));
        return new IPCMessage
        {
            Type = int32_2,
            Severity = int32_3,
            Message = str,
            UID = guid,
            Timestamp = dateTime
        };
    }

    public bool Write(IIPCMessage msg, IIPCChannel channel)
    {
        var byteList = new List<byte>();
        try
        {
            var byteArray = msg.UID.ToByteArray();
            byteList.AddRange(BitConverter.GetBytes(byteArray.Length));
            byteList.AddRange(byteArray);
            byteList.AddRange(BitConverter.GetBytes((int)msg.Type));
            byteList.AddRange(BitConverter.GetBytes((int)msg.Severity));
            byteList.AddRange(BitConverter.GetBytes(msg.Message.Length));
            byteList.AddRange(Encoding.ASCII.GetBytes(msg.Message));
            byteList.AddRange(BitConverter.GetBytes(msg.Timestamp.Ticks));
            return channel.Write(byteList.ToArray());
        }
        finally
        {
            byteList.Clear();
        }
    }
}