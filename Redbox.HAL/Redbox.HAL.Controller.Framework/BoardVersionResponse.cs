using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class BoardVersionResponse : IBoardVersionResponse
    {
        internal BoardVersionResponse(ControlBoards board, CoreResponse response)
        {
            ReadSuccess = response.Success;
            Version = ReadSuccess ? response.OpCodeResponse.Trim() : string.Empty;
            BoardName = board.ToString();
        }

        public bool ReadSuccess { get; }

        public string Version { get; }

        public string BoardName { get; }

        public override string ToString()
        {
            return !ReadSuccess ? ErrorCodes.Timeout.ToString().ToUpper() : Version;
        }
    }
}