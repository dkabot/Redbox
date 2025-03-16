using Redbox.Rental.Model.KioskProduct;
using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Profile
{
    public class Title
    {
        public long ProductId { get; set; }

        public string RunningTime { get; set; }

        public List<string> Starring { get; set; }

        public int ProductTypeId { get; set; }

        public long? RatingId { get; set; }

        public string LongName { get; set; }

        public string SortName { get; set; }

        public string ImageFile { get; set; }

        public string Description { get; set; }

        public List<long> Genres { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public bool SellThru { get; set; }

        public bool SellThruNew { get; set; }

        public decimal BoxOfficeGross { get; set; }

        public List<string> Directors { get; set; }

        public int ComingSoonDays { get; set; }

        public DateTime? SortDate { get; set; }

        public DateTime? NationalStreetDate { get; set; }

        public bool ClosedCaptioned { get; set; }

        public DateTime? MerchandiseDate { get; set; }

        public long? SubPlatformId { get; set; }

        public string NumberOfPlayersText { get; set; }

        public DateTime? DoNotRentDate { get; set; }

        public DateTime? SellThruDate { get; set; }

        public decimal? SellThruPrice { get; set; }

        public int? DiscNumber { get; set; }

        public decimal? DigitalSDRentPrice { get; set; }

        public decimal? DigitalHDRentPrice { get; set; }

        public string Studio { get; set; }

        public ProductGroup ProductGroup { get; set; }

        public int SortOrderPosition
        {
            get
            {
                var productGroup = ProductGroup;
                return productGroup == null ? 0 : productGroup.SortOrderPosition;
            }
        }

        public bool IsEmptyCase => ProductId == 1139L;

        public bool IsRentable => !IsEmptyCase && !DoNotRent && IsAvailable;

        public DateTime? RedboxPlusEligibleDate { get; set; }

        public bool IsRedboxPlusEligible
        {
            get
            {
                if (!RedboxPlusEligibleDate.HasValue)
                    return false;
                var plusEligibleDate = RedboxPlusEligibleDate;
                var now = DateTime.Now;
                return plusEligibleDate.HasValue && plusEligibleDate.GetValueOrDefault() <= now;
            }
        }

        public bool DoNotRent
        {
            get
            {
                if (!DoNotRentDate.HasValue)
                    return false;
                var doNotRentDate = DoNotRentDate;
                var now = DateTime.Now;
                return doNotRentDate.HasValue && doNotRentDate.GetValueOrDefault() <= now;
            }
        }

        public bool IsAvailable => DateTime.Now > (MerchandiseDate ?? DateTime.Today);

        public DiscountRestriction DiscountRestriction { get; set; }

        public override string ToString()
        {
            return string.Format("MovieTitle:{0} {1} SortOrder: {2} Group: {3} {4}", (object)ProductId,
                (object)SortName, (object)SortOrderPosition, (object)ProductGroup?.ProductGroupId,
                (object)ProductGroup?.ProductGroupName);
        }
    }
}