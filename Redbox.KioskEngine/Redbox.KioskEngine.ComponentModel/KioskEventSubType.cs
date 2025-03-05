using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel
{
  public static class KioskEventSubType
  {
    public const int KioskHealth_TouchScreenInactivity = 100;
    public const int KioskHealth_TouchScreenReset = 101;
    public const int KioskHealth_TouchScreenActivity = 102;
    public const int KioskHealth_ArcusReset = 200;
    public const int KisokHealth_ArcusResetVendStatus = 201;
    public const int KioskHealth_RouterIsOffline = 300;
    public const int KioskHealth_RouterReset = 301;
    public const int KioskHealth_RouterIsOnline = 302;
    public const int KioskHealth_HardwareStatNone = 400;
    public const int KioskHealth_HardwareStatArcus = 401;
    public const int KioskHealth_HardwareStatCamera = 402;
    public const int KioskHealth_HardwareStatTouchscreen = 403;
    public const int KioskHealth_HardwareStatControllerRestart = 404;
    public const int KioskHealth_HardwareStatRouterRecycle = 405;
    public const int KioskHealth_HardwareStatUnexpectedPowerLoss = 406;
    public const int KioskHealth_ViewTimeout = 500;
    public const int KioskHealth_ViewReset = 501;
    public const int KioskHealth_ViewActivity = 502;
    public const int KioskHealth_CCReaderInactivity = 600;
    public const int KioskHealth_CCReaderReadError = 601;
    public const int KioskHealth_CCReaderActivity = 602;
    public const int KioskHealth_EMVReaderTechnicalFallbackSession = 700;
    public const int KioskHealth_EMVReaderTechnicalFallbackSessionReset = 701;
    private static readonly IDictionary<int, string> Map = (IDictionary<int, string>) new Dictionary<int, string>()
    {
      {
        100,
        nameof (KioskHealth_TouchScreenInactivity)
      },
      {
        101,
        nameof (KioskHealth_TouchScreenReset)
      },
      {
        102,
        nameof (KioskHealth_TouchScreenActivity)
      },
      {
        200,
        nameof (KioskHealth_ArcusReset)
      },
      {
        201,
        nameof (KisokHealth_ArcusResetVendStatus)
      },
      {
        300,
        nameof (KioskHealth_RouterIsOffline)
      },
      {
        301,
        nameof (KioskHealth_RouterReset)
      },
      {
        302,
        nameof (KioskHealth_RouterIsOnline)
      },
      {
        400,
        nameof (KioskHealth_HardwareStatNone)
      },
      {
        401,
        nameof (KioskHealth_HardwareStatArcus)
      },
      {
        402,
        nameof (KioskHealth_HardwareStatCamera)
      },
      {
        403,
        nameof (KioskHealth_HardwareStatTouchscreen)
      },
      {
        404,
        nameof (KioskHealth_HardwareStatControllerRestart)
      },
      {
        405,
        nameof (KioskHealth_HardwareStatRouterRecycle)
      },
      {
        406,
        nameof (KioskHealth_HardwareStatUnexpectedPowerLoss)
      },
      {
        500,
        nameof (KioskHealth_ViewTimeout)
      },
      {
        501,
        nameof (KioskHealth_ViewReset)
      },
      {
        502,
        nameof (KioskHealth_ViewActivity)
      },
      {
        600,
        nameof (KioskHealth_CCReaderInactivity)
      },
      {
        601,
        nameof (KioskHealth_CCReaderReadError)
      },
      {
        602,
        nameof (KioskHealth_CCReaderActivity)
      },
      {
        700,
        nameof (KioskHealth_EMVReaderTechnicalFallbackSession)
      },
      {
        701,
        nameof (KioskHealth_EMVReaderTechnicalFallbackSessionReset)
      }
    };

    public static string GetName(int value)
    {
      return !KioskEventSubType.Map.ContainsKey(value) ? "" : KioskEventSubType.Map[value];
    }
  }
}
