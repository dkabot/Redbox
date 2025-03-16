using System;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.Rental.Model.Pricing
{
    public class PricingSet : IPricingSet
    {
        private List<IPricingRecord> _priceRecords = new List<IPricingRecord>();

        public string Name { get; set; }

        public string ProgramName { get; set; }

        public string PriceSetId { get; set; }

        public List<IPricingRecord> PriceRecords
        {
            get => _priceRecords;
            set => _priceRecords = value;
        }

        public bool IsEqual(IPricingSet comparePricingSet)
        {
            return comparePricingSet != null && PricingRecordsAreEqual(PriceRecords, comparePricingSet.PriceRecords) &&
                   PricingRecordsAreEqual(comparePricingSet.PriceRecords, PriceRecords);
        }

        private bool PricingRecordsAreEqual(List<IPricingRecord> list1, List<IPricingRecord> list2)
        {
            var flag = true;
            foreach (var pricingRecord1 in list1)
            {
                var eachSourcePriceRecord = pricingRecord1;
                var pricingRecord2 = list2.FirstOrDefault<IPricingRecord>((Func<IPricingRecord, bool>)(x =>
                    x.TitleFamily == eachSourcePriceRecord.TitleFamily &&
                    x.TitleType == eachSourcePriceRecord.TitleType));
                if (pricingRecord2 != null)
                {
                    if (!eachSourcePriceRecord.IsEqual(pricingRecord2))
                    {
                        flag = false;
                        break;
                    }
                }
                else
                {
                    flag = false;
                    break;
                }
            }

            return flag;
        }
    }
}