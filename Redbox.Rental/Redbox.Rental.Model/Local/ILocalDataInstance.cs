using System.Collections.Generic;

namespace Redbox.Rental.Model.Local
{
    public interface ILocalDataInstance
    {
        bool IsAccountOnOfflineBadCardList(string hashId);

        void UpdateAccountOnOfflineBadCardList(string hashId);

        void RemoveAccountFromOfflineBadCardList(string hashId);

        Dictionary<string, IBadCard> BadCardList { get; }
    }
}