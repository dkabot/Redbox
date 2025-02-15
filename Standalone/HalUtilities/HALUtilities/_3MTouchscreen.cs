using System.IO;
using Redbox.HAL.Component.Model;

namespace HALUtilities
{
    internal sealed class _3MTouchscreen
    {
        private readonly Operation_3m Op;

        internal _3MTouchscreen(Operation_3m op)
        {
            Op = op;
        }

        internal void Execute(TextWriter writer)
        {
            var touchScreen = ServiceLocator.Instance.GetService<IUsbDeviceService>().FindTouchScreen(false);
            if (touchScreen.Friendlyname != "3M")
                writer.WriteLine("Could not locate 3m descriptor; found {0}", touchScreen.Friendlyname);
            else
                switch (Op)
                {
                    case Operation_3m.SoftReset:
                        writer.WriteLine("3M soft reset returned {0}", touchScreen.SoftReset());
                        break;
                    case Operation_3m.ReadFirmware:
                        writer.WriteLine("Firmware: {0}", touchScreen.ReadFirmware());
                        break;
                    case Operation_3m.HardReset:
                        writer.WriteLine("3M hard reset returned {0}", touchScreen.HardReset());
                        break;
                }
        }
    }
}