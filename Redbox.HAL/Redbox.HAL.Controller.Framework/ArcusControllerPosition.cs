using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal class ArcusControllerPosition : IControllerPosition
    {
        internal ArcusControllerPosition()
        {
            var nullable = new int?();
            YCoordinate = nullable;
            XCoordinate = nullable;
        }

        public int? XCoordinate { get; internal set; }

        public int? YCoordinate { get; internal set; }

        public bool ReadOk => XCoordinate.HasValue && YCoordinate.HasValue;
    }
}