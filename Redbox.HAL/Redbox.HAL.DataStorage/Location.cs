using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage
{
    internal sealed class Location : ILocation
    {
        private readonly string m_stringRep;

        internal Location(int deck, int slot)
        {
            Deck = deck;
            Slot = slot;
            ID = "EMPTY";
            ReturnDate = new DateTime?();
            Excluded = false;
            StuckCount = 0;
            Flags = MerchFlags.None;
            m_stringRep = string.Format("Deck = {0} Slot = {1}", Deck, Slot);
        }

        public bool IsEmpty => ID == "EMPTY";

        public bool IsWide =>
            ServiceLocator.Instance.GetService<IDecksService>().GetByNumber(Deck).IsSlotSellThru(Slot);

        public int Deck { get; }

        public int Slot { get; }

        public string ID { get; set; }

        public DateTime? ReturnDate { get; set; }

        public bool Excluded { get; set; }

        public int StuckCount { get; set; }

        public MerchFlags Flags { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            var location = (Location)obj;
            return location.Deck == Deck && location.Slot == Slot;
        }

        public override string ToString()
        {
            return m_stringRep;
        }

        public override int GetHashCode()
        {
            return 101 * Deck + Slot;
        }
    }
}