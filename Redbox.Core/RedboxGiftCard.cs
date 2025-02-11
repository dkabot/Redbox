using System.Collections.Generic;
using System.Linq;

namespace Redbox.Core
{
    public sealed class RedboxGiftCard
    {
        private readonly List<BinRange> m_overlappingDiscoverRanges = new List<BinRange>
        {
            new BinRange(622126, 622925),
            new BinRange(624000, 626999),
            new BinRange(628200, 628899),
            new BinRange(644000, 659999)
        };

        private readonly List<BinRange> m_redboxGiftCardRanges = new List<BinRange>();

        public RedboxGiftCard()
        {
        }

        public RedboxGiftCard(List<BinRange> binRanges)
            : this()
        {
            AddBinRanges(binRanges);
        }

        public bool IsRedboxGiftCard(string number)
        {
            if (string.IsNullOrEmpty(number) || number.Length < 6)
                return false;
            var bin = number.Substring(0, 6).ToInt32();
            return number.Length == 16 && bin.HasValue &&
                   m_redboxGiftCardRanges.Any(binRange => binRange.Between(bin.Value)) &&
                   !m_overlappingDiscoverRanges.Any(binRange => binRange.Between(bin.Value));
        }

        public void AddBinRanges(List<BinRange> binRanges)
        {
            m_redboxGiftCardRanges.AddRange(binRanges);
        }

        public void ClearRanges()
        {
            m_redboxGiftCardRanges.Clear();
        }
    }
}