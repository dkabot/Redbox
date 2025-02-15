using System;
using Redbox.Services.KioskBrokerServices.KioskShared.Enums;

namespace Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects
{
    [Serializable]
    public class DiscStatus
    {
        public string Barcode { get; set; }

        public int DiscTypeID { get; set; }

        public DiscType DiscType => Enum.IsDefined(typeof(DiscType), DiscTypeID)
            ? (DiscType)Enum.ToObject(typeof(DiscType), DiscTypeID)
            : DiscType._Unspecified;
    }
}