using System;
using Redbox.Services.KioskBrokerServices.KioskShared.Enums;

namespace Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects
{
    [Serializable]
    public class ReservationTitle
    {
        public int ItemID { get; set; }

        public int TitleID { get; set; }

        public int ReservationTypeID { get; set; }

        public ReservationType ReservationType => Enum.IsDefined(typeof(ReservationType), ReservationTypeID)
            ? (ReservationType)Enum.ToObject(typeof(ReservationType), ReservationTypeID)
            : ReservationType._Unspecified;

        public int DiscFormatID { get; set; }

        public DiscFormat DiscFormat => Enum.IsDefined(typeof(DiscFormat), DiscFormatID)
            ? (DiscFormat)Enum.ToObject(typeof(DiscFormat), DiscFormatID)
            : DiscFormat._Unspecified;
    }
}