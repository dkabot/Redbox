using Redbox.HAL.Component.Model;
using System.IO;

namespace HALUtilities
{
  internal sealed class _3MTouchscreen
  {
    private readonly Operation_3m Op;

    internal void Execute(TextWriter writer)
    {
      ITouchscreenDescriptor touchScreen = ServiceLocator.Instance.GetService<IUsbDeviceService>().FindTouchScreen(false);
      if (touchScreen.Friendlyname != "3M")
      {
        writer.WriteLine("Could not locate 3m descriptor; found {0}", (object) touchScreen.Friendlyname);
      }
      else
      {
        switch (this.Op)
        {
          case Operation_3m.SoftReset:
            writer.WriteLine("3M soft reset returned {0}", (object) touchScreen.SoftReset());
            break;
          case Operation_3m.ReadFirmware:
            writer.WriteLine("Firmware: {0}", (object) touchScreen.ReadFirmware());
            break;
          case Operation_3m.HardReset:
            writer.WriteLine("3M hard reset returned {0}", (object) touchScreen.HardReset());
            break;
        }
      }
    }

    internal _3MTouchscreen(Operation_3m op) => this.Op = op;
  }
}
