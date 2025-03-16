using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel
{
    public static class KioskEventType
    {
        public const int KioskHealthTouchScreen = 1;
        public const int KioskHealthArcus = 2;
        public const int KioskHealthRouter = 3;
        public const int KioskHealthHardwareCorrectionStat = 4;
        public const int KioskHealthView = 5;
        public const int KioskHealthCCReader = 6;
        public const int KioskHealthEMVReader = 7;

        private static readonly IDictionary<int, string> Map = new Dictionary<int, string>
        {
            {
                1,
                nameof(KioskHealthTouchScreen)
            },
            {
                2,
                nameof(KioskHealthArcus)
            },
            {
                3,
                nameof(KioskHealthRouter)
            },
            {
                4,
                nameof(KioskHealthHardwareCorrectionStat)
            },
            {
                5,
                nameof(KioskHealthView)
            },
            {
                6,
                nameof(KioskHealthCCReader)
            },
            {
                7,
                nameof(KioskHealthEMVReader)
            }
        };

        public static string GetName(int value)
        {
            return !Map.ContainsKey(value) ? "" : Map[value];
        }
    }
}