using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Transactions
{
    public abstract class MessageBase : IMessage
    {
        public Guid MessageId { get; private set; }

        public long KioskId { get; set; }

        public string EngineVersion { get; set; }

        public string BundleVersion { get; set; }

        public virtual string MessageType { get; set; }

        public bool UseMessageControl { get; set; }

        public Dictionary<string, string> PropertyBag { get; set; }

        protected MessageBase()
        {
            MessageId = Guid.NewGuid();
            var service = ServiceLocator.Instance.GetService<IMacroService>();
            EngineVersion = service[nameof(EngineVersion)];
            BundleVersion = service["ProductVersion"];
        }
    }
}