namespace Redbox.Command.Tokenizer
{
    public enum CommandParserState
    {
        Start = 1,
        Command = 2,
        Comment = 3,
        Symbol = 4,
        Key = 5,
        Value = 6,
        Whitespace = 7,
        Error = 8
    }
}