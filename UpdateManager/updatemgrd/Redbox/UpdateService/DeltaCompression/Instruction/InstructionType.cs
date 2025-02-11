namespace Redbox.UpdateService.DeltaCompression.Instruction
{
    internal enum InstructionType : byte
    {
        Insert,
        Update,
        Delete,
        Literal,
        CacheUpdateInsert,
        CacheDelete,
    }
}
