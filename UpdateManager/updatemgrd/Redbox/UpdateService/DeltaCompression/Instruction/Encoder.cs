using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Redbox.UpdateService.DeltaCompression.Instruction
{
    internal class Encoder
    {
        private BinaryWriter m_writer;

        public Stream Strm { get; set; }

        public void Encode(string tableName, List<Redbox.UpdateService.DeltaCompression.Instruction.Instruction> instructions)
        {
            this.Writer.Write(tableName.Length);
            this.Writer.Write(Encoding.ASCII.GetBytes(tableName));
            this.Writer.Write(instructions.Count);
            foreach (Redbox.UpdateService.DeltaCompression.Instruction.Instruction instruction in instructions)
            {
                switch (instruction.Type)
                {
                    case InstructionType.Insert:
                        Encoder.EncodeInsert(instruction, this.Writer);
                        continue;
                    case InstructionType.Update:
                        Encoder.EncodeUpdate(instruction, this.Writer);
                        continue;
                    case InstructionType.Delete:
                        Encoder.EncodeDelete(instruction, this.Writer);
                        continue;
                    case InstructionType.CacheUpdateInsert:
                        Encoder.EncodeCacheUpdateInsert(instruction, this.Writer);
                        continue;
                    case InstructionType.CacheDelete:
                        Encoder.EncodeCacheDelete(instruction, this.Writer);
                        continue;
                    default:
                        continue;
                }
            }
        }

        public void BeginBlock(string tableName, int totalInstructions)
        {
            BinaryWriter binaryWriter = new BinaryWriter(this.Strm);
            binaryWriter.Write(tableName.Length);
            binaryWriter.Write(Encoding.ASCII.GetBytes(tableName));
            binaryWriter.Write(totalInstructions);
        }

        public void Encode(Redbox.UpdateService.DeltaCompression.Instruction.Instruction i)
        {
            switch (i.Type)
            {
                case InstructionType.Insert:
                    Encoder.EncodeInsert(i, this.Writer);
                    break;
                case InstructionType.Update:
                    Encoder.EncodeUpdate(i, this.Writer);
                    break;
                case InstructionType.Delete:
                    Encoder.EncodeDelete(i, this.Writer);
                    break;
                case InstructionType.CacheUpdateInsert:
                    Encoder.EncodeCacheUpdateInsert(i, this.Writer);
                    break;
                case InstructionType.CacheDelete:
                    Encoder.EncodeCacheDelete(i, this.Writer);
                    break;
            }
        }

        private BinaryWriter Writer
        {
            get
            {
                if (this.m_writer == null)
                    this.m_writer = new BinaryWriter(this.Strm);
                return this.m_writer;
            }
        }

        private static void EncodeInsert(Redbox.UpdateService.DeltaCompression.Instruction.Instruction i, BinaryWriter writer)
        {
            writer.Write((byte)i.Type);
            if (i.Key.Length > (int)ushort.MaxValue)
                throw new InvalidOperationException("Keys greater than short.maxvalue are not supported by this instrucution encoder. Use a literal expression to insert.");
            writer.Write((ushort)i.Key.Length);
            writer.Write(Encoding.ASCII.GetBytes(i.Key));
            if (i.Value.Length > int.MaxValue)
                throw new InvalidOperationException("Values greater than int.maxvalue are not supported by this instrucution encoder. Use a literal expression to insert.");
            writer.Write(i.Value.Length);
            writer.Write(Encoding.ASCII.GetBytes(i.Value));
        }

        private static void EncodeDelete(Redbox.UpdateService.DeltaCompression.Instruction.Instruction i, BinaryWriter writer)
        {
            writer.Write((byte)i.Type);
            if (i.Key.Length > (int)ushort.MaxValue)
                throw new InvalidOperationException("Keys greater than short.maxvalue are not supported by this instrucution encoder. Use a literal expression to insert.");
            writer.Write((ushort)i.Key.Length);
            writer.Write(Encoding.ASCII.GetBytes(i.Key));
        }

        private static void EncodeUpdate(Redbox.UpdateService.DeltaCompression.Instruction.Instruction i, BinaryWriter writer)
        {
            writer.Write((byte)i.Type);
            if (i.Key.Length > (int)ushort.MaxValue)
                throw new InvalidOperationException("Keys greater than short.maxvalue are not supported by this instrucution encoder. Use a literal expression to insert.");
            writer.Write((ushort)i.Key.Length);
            writer.Write(Encoding.ASCII.GetBytes(i.Key));
            writer.Write(i.StartIndex);
            writer.Write(i.Value.Length);
            writer.Write(Encoding.ASCII.GetBytes(i.Value));
        }

        private static void EncodeCacheUpdateInsert(Redbox.UpdateService.DeltaCompression.Instruction.Instruction i, BinaryWriter writer)
        {
            writer.Write((byte)i.Type);
            if (i.Key.Length > (int)ushort.MaxValue)
                throw new InvalidOperationException("Keys greater than short.maxvalue are not supported by this instrucution encoder. Use a literal expression to insert.");
            writer.Write((ushort)i.Key.Length);
            writer.Write(Encoding.ASCII.GetBytes(i.Key));
            writer.Write(i.CacheValue.Length);
            writer.Write(i.CacheValue);
        }

        private static void EncodeCacheDelete(Redbox.UpdateService.DeltaCompression.Instruction.Instruction i, BinaryWriter writer)
        {
            writer.Write((byte)i.Type);
            if (i.Key.Length > (int)ushort.MaxValue)
                throw new InvalidOperationException("Keys greater than short.maxvalue are not supported by this instrucution encoder. Use a literal expression to insert.");
            writer.Write((ushort)i.Key.Length);
            writer.Write(Encoding.ASCII.GetBytes(i.Key));
        }
    }
}
