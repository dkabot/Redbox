using System.Drawing;

namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    public sealed class CortexDecodeResult
    {
        internal Point[] PolyPoints
        {
            get
            {
                return new Point[4]
                {
                    corner0,
                    corner1,
                    corner2,
                    corner3
                };
            }
        }

        internal string DecodeData { get; set; }

        internal int dataLength { get; set; }

        internal Point corner0 { get; set; }

        internal Point corner1 { get; set; }

        internal Point corner2 { get; set; }

        internal Point corner3 { get; set; }

        internal Point center { get; set; }

        internal int symbolType { get; set; }

        internal int symbolModifier { get; set; }

        public override string ToString()
        {
            return string.Format("{0},{1};{2},{3};{4},{5};{6},{7} Matrix = {8}", corner0.X, corner0.Y, corner1.X,
                corner1.Y, corner2.X, corner2.Y, corner3.X, corner3.Y, DecodeData);
        }
    }
}