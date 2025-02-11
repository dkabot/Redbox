namespace Redbox.UpdateService.DeltaCompression.Instruction
{
    internal class Instruction
    {
        public static Redbox.UpdateService.DeltaCompression.Instruction.Instruction CreateUpdate(
          string key,
          string value,
          int startIndex)
        {
            return new Redbox.UpdateService.DeltaCompression.Instruction.Instruction()
            {
                Type = InstructionType.Update,
                Key = key,
                Value = value,
                StartIndex = startIndex
            };
        }

        public static Redbox.UpdateService.DeltaCompression.Instruction.Instruction CreateInsert(
          string key,
          string value)
        {
            return new Redbox.UpdateService.DeltaCompression.Instruction.Instruction()
            {
                Type = InstructionType.Insert,
                Key = key,
                Value = value
            };
        }

        public static Redbox.UpdateService.DeltaCompression.Instruction.Instruction CreateDelete(
          string key)
        {
            return new Redbox.UpdateService.DeltaCompression.Instruction.Instruction()
            {
                Type = InstructionType.Delete,
                Key = key
            };
        }

        public static Redbox.UpdateService.DeltaCompression.Instruction.Instruction CreateCacheUpdateInsert(
          string key,
          byte[] data)
        {
            return new Redbox.UpdateService.DeltaCompression.Instruction.Instruction()
            {
                Type = InstructionType.CacheUpdateInsert,
                Key = key,
                CacheValue = data
            };
        }

        public static Redbox.UpdateService.DeltaCompression.Instruction.Instruction CreateCacheDelete(
          string key)
        {
            return new Redbox.UpdateService.DeltaCompression.Instruction.Instruction()
            {
                Type = InstructionType.CacheDelete,
                Key = key
            };
        }

        public InstructionType Type { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public byte[] CacheValue { get; set; }

        public int StartIndex { get; set; }
    }
}
