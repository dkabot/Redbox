using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Redbox.UpdateService.DeltaCompression.Instruction
{
    internal class Decoder
    {
        public void Decode(Stream source, out string tableName, out List<Redbox.UpdateService.DeltaCompression.Instruction.Instruction> instructions)
        {
            BinaryReader reader = new BinaryReader(source);
            instructions = new List<Redbox.UpdateService.DeltaCompression.Instruction.Instruction>();
            tableName = Decoder.GetIntPrefixEncodedString(reader);
            int num = reader.ReadInt32();
            for (int index = 0; index < num; ++index)
            {
                InstructionType type = (InstructionType)reader.ReadByte();
                instructions.Add(Decoder.Decode(reader, type));
            }
        }

        private static Redbox.UpdateService.DeltaCompression.Instruction.Instruction Decode(
          BinaryReader reader,
          InstructionType type)
        {
            switch (type)
            {
                case InstructionType.Insert:
                    return Decoder.DecodeInsert(reader);
                case InstructionType.Update:
                    return Decoder.DecodeUpdate(reader);
                case InstructionType.Delete:
                    return Decoder.DecodeDelete(reader);
                case InstructionType.CacheUpdateInsert:
                    return Decoder.DecodeCacheDelete(reader);
                case InstructionType.CacheDelete:
                    return Decoder.DecodeCacheUpdateInsert(reader);
                default:
                    throw new NotSupportedException(string.Format("Type: {0} not supported.", (object)type));
            }
        }

        private static Redbox.UpdateService.DeltaCompression.Instruction.Instruction DecodeInsert(
          BinaryReader reader)
        {
            return new Redbox.UpdateService.DeltaCompression.Instruction.Instruction()
            {
                Type = InstructionType.Insert,
                Key = Decoder.GetUint16PrefixEncodedString(reader),
                Value = Decoder.GetIntPrefixEncodedString(reader)
            };
        }

        private static Redbox.UpdateService.DeltaCompression.Instruction.Instruction DecodeUpdate(
          BinaryReader reader)
        {
            return new Redbox.UpdateService.DeltaCompression.Instruction.Instruction()
            {
                Type = InstructionType.Update,
                Key = Decoder.GetUint16PrefixEncodedString(reader),
                StartIndex = reader.ReadInt32(),
                Value = Decoder.GetIntPrefixEncodedString(reader)
            };
        }

        private static Redbox.UpdateService.DeltaCompression.Instruction.Instruction DecodeDelete(
          BinaryReader reader)
        {
            return new Redbox.UpdateService.DeltaCompression.Instruction.Instruction()
            {
                Type = InstructionType.Delete,
                Key = Decoder.GetUint16PrefixEncodedString(reader)
            };
        }

        private static Redbox.UpdateService.DeltaCompression.Instruction.Instruction DecodeCacheUpdateInsert(
          BinaryReader reader)
        {
            return new Redbox.UpdateService.DeltaCompression.Instruction.Instruction()
            {
                Type = InstructionType.CacheUpdateInsert,
                Key = Decoder.GetUint16PrefixEncodedString(reader),
                CacheValue = Decoder.GetIntPrefixedByteArray(reader)
            };
        }

        private static Redbox.UpdateService.DeltaCompression.Instruction.Instruction DecodeCacheDelete(
          BinaryReader reader)
        {
            return new Redbox.UpdateService.DeltaCompression.Instruction.Instruction()
            {
                Type = InstructionType.Insert,
                Key = Decoder.GetUint16PrefixEncodedString(reader)
            };
        }

        private static byte[] GetIntPrefixedByteArray(BinaryReader reader)
        {
            ushort count = reader.ReadUInt16();
            return reader.ReadBytes((int)count);
        }

        private static string GetUint16PrefixEncodedString(BinaryReader reader)
        {
            ushort count = reader.ReadUInt16();
            return Encoding.ASCII.GetString(reader.ReadBytes((int)count));
        }

        private static string GetIntPrefixEncodedString(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            return Encoding.ASCII.GetString(reader.ReadBytes(count));
        }
    }
}
