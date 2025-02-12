using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Services
{
    internal sealed class ClientControllerPosition : IControllerPosition
    {
        internal ClientControllerPosition(bool ok, int? x, int? y)
        {
            ReadOk = ok;
            XCoordinate = x;
            YCoordinate = y;
        }

        public int? XCoordinate { get; }

        public int? YCoordinate { get; }

        public bool ReadOk { get; }
    }
}