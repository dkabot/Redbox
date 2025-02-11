using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class ArcusCommandResponse
    {
        internal readonly ErrorCodes Error;
        internal readonly string Response;

        internal ArcusCommandResponse(string response, ErrorCodes error)
        {
            Response = response;
            Error = error;
        }
    }
}