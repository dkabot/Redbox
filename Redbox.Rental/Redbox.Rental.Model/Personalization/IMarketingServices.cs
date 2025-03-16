using Redbox.KioskEngine.ComponentModel.KioskServices;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Personalization
{
    public interface IMarketingServices
    {
        void SendEmailConfirm(
            long kioskid,
            string emailAddress,
            string accountNumber,
            SendEmailConfirmCallback completeCallback);

        void KioskOptIn(long kioskId, List<IOptInItem> optInItems, RemoteServiceCallback callback);

        IOptInItem NewOptInItem();
    }
}