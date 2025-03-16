using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Transactions
{
    public interface IMessage
    {
        Guid MessageId { get; }

        long KioskId { set; get; }

        string EngineVersion { set; get; }

        string BundleVersion { set; get; }

        string MessageType { set; get; }

        bool UseMessageControl { get; }

        Dictionary<string, string> PropertyBag { set; get; }
    }
}