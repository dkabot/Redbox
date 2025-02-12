using Redbox.HAL.Component.Model;
using System;

namespace Redbox.HAL.Client.Services
{
  internal sealed class ClientBoardRevision : IBoardVersionResponse
  {
    public bool ReadSuccess { get; private set; }

    public string Version { get; private set; }

    public string BoardName { get; private set; }

    internal ClientBoardRevision(ControlBoards board, string response)
    {
      this.BoardName = board.ToString().ToUpper();
      this.ReadSuccess = !response.Equals("TIMEOUT", StringComparison.CurrentCultureIgnoreCase);
      this.Version = response;
    }
  }
}
