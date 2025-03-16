using System;

namespace Redbox.Rental.Model
{
    public interface IStoreManager
    {
        long KioskId { get; }

        string Market { get; }

        string Banner { get; }

        string GetDefaultCulture();

        string GetAlternateCulture();

        int GetSellThruDays();

        bool SellThru { get; set; }

        decimal PurchasePrice { get; set; }

        bool CanSellUsed { get; }

        bool CanSellUsedMovies { get; }

        bool CanSellUsedGames { get; }

        string ApplicationDataPath { get; }

        string RunningPath { get; }
    }
}