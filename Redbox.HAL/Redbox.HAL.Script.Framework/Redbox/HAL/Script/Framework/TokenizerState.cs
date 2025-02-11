namespace Redbox.HAL.Script.Framework
{
    public enum TokenizerState
    {
        Start = 1,
        Label = 2,
        Error = 3,
        Symbol = 4,
        Comment = 5,
        Mnemonic = 6,
        Operator = 7,
        KeyValuePair = 8,
        NumberLiteral = 9,
        StringLiteral = 10, // 0x0000000A
        RangeOperator = 11, // 0x0000000B
        WhitespaceIgnore = 12, // 0x0000000C
        EqualityOperator = 13, // 0x0000000D
        RelationalOperator = 14, // 0x0000000E
        PatternOperator = 15 // 0x0000000F
    }
}