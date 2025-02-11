using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Services;

internal sealed class ClientBoardRevision : IBoardVersionResponse
{
    internal ClientBoardRevision(ControlBoards board, string response)
    {
        BoardName = board.ToString().ToUpper();
        ReadSuccess = !response.Equals("TIMEOUT", StringComparison.CurrentCultureIgnoreCase);
        Version = response;
    }

    public bool ReadSuccess { get; }

    public string Version { get; }

    public string BoardName { get; }
}