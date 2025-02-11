using System.Runtime.InteropServices;

namespace Redbox.DirectShow.Interop
{
    internal static class Tools
    {
        public static IPin GetPin(IBaseFilter filter, PinDirection dir, int num)
        {
            var pins = new IPin[1];
            var enumPins = (IEnumPins)null;
            if (filter.EnumPins(out enumPins) == 0)
                try
                {
                    while (enumPins.Next(1, pins, out _) == 0)
                    {
                        PinDirection pinDirection;
                        pins[0].QueryDirection(out pinDirection);
                        if (pinDirection == dir)
                        {
                            if (num == 0)
                                return pins[0];
                            --num;
                        }

                        Marshal.ReleaseComObject(pins[0]);
                        pins[0] = null;
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(enumPins);
                }

            return null;
        }

        public static IPin GetInPin(IBaseFilter filter, int num)
        {
            return GetPin(filter, PinDirection.Input, num);
        }

        public static IPin GetOutPin(IBaseFilter filter, int num)
        {
            return GetPin(filter, PinDirection.Output, num);
        }
    }
}