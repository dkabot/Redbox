namespace Redbox.HAL.IPC.Framework;

internal sealed class ServerResponse : AbstractIPCResponse
{
    internal ServerResponse(bool logDetails)
        : base(logDetails)
    {
        Command = string.Empty;
    }

    internal ServerResponse()
        : this(true)
    {
    }

    internal string Command { get; private set; }

    protected override void OnClear()
    {
        Command = string.Empty;
    }

    protected override bool OnTestResponse()
    {
        ReadBuilder.ToString();
        if (BufferHasMoreLines())
            return false;
        while (true)
        {
            var nextBufferLine = GetNextBufferLine();
            if (!(nextBufferLine == string.Empty))
                Command = nextBufferLine;
            else
                break;
        }

        return true;
    }
}