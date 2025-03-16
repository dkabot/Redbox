using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Reservation.DataFile;
using Redbox.Rental.Model.ShoppingCart;
using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Reservation
{
    public class ReservationDataInstance : IReservationDataInstance
    {
        private Dictionary<string, IReservationDataFile> _dataFiles = new Dictionary<string, IReservationDataFile>();
        private IDataFileStoreService _dataFileStoreService;

        public ReservationDataInstance(IDataFileStoreService dataFileStoreService)
        {
            _dataFileStoreService = dataFileStoreService;
        }

        public Dictionary<string, IReservationDataFile> DataFiles
        {
            get => _dataFiles;
            set => _dataFiles = value;
        }

        public bool Save(IReservationDataFile reservationDataFile)
        {
            var flag = false;
            if (reservationDataFile != null)
                flag = _dataFileStoreService.Set(reservationDataFile.FileName, (object)reservationDataFile);
            return flag;
        }

        public void Delete(IReservationDataFile reservationDataFile)
        {
            _dataFileStoreService?.Delete(reservationDataFile.FileName);
        }

        public void LoadAll()
        {
            if (_dataFileStoreService == null)
                return;
            var all = _dataFileStoreService.GetAll<IReservationDataFile>();
            if (all == null)
                return;
            foreach (var reservationDataFile in all)
                if (!(reservationDataFile is ReservationDataFile_V002 reservationDataFileV002_1))
                {
                    var flag = false;
                    var v002Files = Conversions.ConvertReservationDataFileV001ToV002Files(reservationDataFile);
                    if ((v002Files != null ? v002Files.Count : 0) > 0)
                    {
                        flag = true;
                        foreach (var reservationDataFileV002 in v002Files)
                        {
                            DataFiles[reservationDataFileV002.ReferenceNumber] =
                                (IReservationDataFile)reservationDataFileV002;
                            flag = flag && Save((IReservationDataFile)reservationDataFileV002);
                        }
                    }

                    if (flag)
                        Delete(reservationDataFile);
                    else
                        LogHelper.Instance.Log(
                            "Unable to convert reservation data file " + reservationDataFile.FileName);
                }
                else
                {
                    DataFiles[reservationDataFileV002_1.ReferenceNumber] =
                        (IReservationDataFile)reservationDataFileV002_1;
                }
        }

        public static class Conversions
        {
            public static IReservation ConvertToReservation(ReservationDataFile_V002 reservationDataFile)
            {
                var reservation = (IReservation)null;
                if (reservationDataFile != null)
                {
                    reservation = (IReservation)new Reservation()
                    {
                        HashedCardId = reservationDataFile.HashedCardId,
                        Id = reservationDataFile.ID,
                        ReferenceNumber = reservationDataFile.ReferenceNumber
                    };
                    var reservationDetails = new ReservationDetails()
                    {
                        ReservationVersion = reservationDataFile.ReservationDataInfo.ReservationVersion,
                        ReferenceNumber = reservationDataFile.ReservationDataInfo.ReferenceNumber,
                        TransactionDate = reservationDataFile.ReservationDataInfo.TransactionDate,
                        IsMultiNightPrice = reservationDataFile.ReservationDataInfo.IsMultiNightPrice,
                        IsMultiDiscVend = reservationDataFile.ReservationDataInfo.IsMultiDiscVend,
                        IsGiftCard = reservationDataFile.ReservationDataInfo.IsGiftCard,
                        AuthorizeAtPickup = reservationDataFile.ReservationDataInfo.AuthorizeAtPickup,
                        IsLoyaltyRedemption = reservationDataFile.ReservationDataInfo.IsLoyaltyRedemption,
                        AppliedTitleMarketing = reservationDataFile.ReservationDataInfo.AppliedTitleMarketing,
                        AppliedHiveOnlinePromo = reservationDataFile.ReservationDataInfo.AppliedHiveOnlinePromo,
                        SubTotal = reservationDataFile.ReservationDataInfo.SubTotal,
                        DiscountedSubTotal = reservationDataFile.ReservationDataInfo.DiscountedSubTotal,
                        HiveOnlinePromoDiscount = reservationDataFile.ReservationDataInfo.HiveOnlinePromoDiscount,
                        Tax = reservationDataFile.ReservationDataInfo.Tax,
                        TaxRate = reservationDataFile.ReservationDataInfo.TaxRate,
                        DefaultServiceFee = reservationDataFile.ReservationDataInfo.DefaultServiceFee,
                        ActualServiceFee = reservationDataFile.ReservationDataInfo.ActualServiceFee,
                        GrandTotal = reservationDataFile.ReservationDataInfo.GrandTotal,
                        PromoCode = reservationDataFile.ReservationDataInfo.PromoCode,
                        ZipCode = reservationDataFile.ReservationDataInfo.ZipCode,
                        CustomerNumber = reservationDataFile.ReservationDataInfo.CustomerNumber,
                        EmailAddress = reservationDataFile.ReservationDataInfo.EmailAddress
                    };
                    reservationDetails.HistoryTitleIds.AddRange(
                        (IEnumerable<int>)reservationDataFile.ReservationDataInfo.HistoryTitleIds);
                    reservationDetails.CreditCardIds.AddRange(
                        (IEnumerable<string>)reservationDataFile.ReservationDataInfo.CreditCardIds);
                    reservationDetails.GiftCardIds.AddRange(
                        (IEnumerable<string>)reservationDataFile.ReservationDataInfo.GiftCardIds);
                    foreach (var reservationTitle in reservationDataFile.ReservationDataInfo.ReservationTitles)
                    {
                        var reservedTitle = new ReservedTitle()
                        {
                            Barcode = reservationTitle.Barcode,
                            TitleId = reservationTitle.TitleId,
                            Discount = reservationTitle.Discount,
                            DiscountedPrice = reservationTitle.DiscountedPrice,
                            Price = reservationTitle.Price,
                            LoyaltyPoints = reservationTitle.LoyaltyPoints,
                            DiscountType = reservationTitle.DiscountType
                        };
                        RentalShoppingCartItemAction result;
                        if (Enum.TryParse<RentalShoppingCartItemAction>(reservationTitle.ReservationType.ToString(),
                                true, out result))
                            reservedTitle.Action = result;
                        reservationDetails.ReservedTitles.Add((IReservedTitle)reservedTitle);
                    }

                    if (reservationDataFile.ReservationDataInfo.PricingSet != null)
                    {
                        reservationDetails.PricingSet = (IPricingSet)new PricingSet()
                        {
                            PriceSetId = reservationDataFile.ReservationDataInfo.PricingSet.PriceSetId,
                            Name = reservationDataFile.ReservationDataInfo.PricingSet.Name,
                            ProgramName = reservationDataFile.ReservationDataInfo.PricingSet.ProgramName
                        };
                        if (reservationDataFile.ReservationDataInfo.PricingSet.PriceRecords != null)
                            foreach (var priceRecord in reservationDataFile.ReservationDataInfo.PricingSet.PriceRecords)
                            {
                                TitleFamily result1;
                                Enum.TryParse<TitleFamily>(priceRecord.TitleFamily, out result1);
                                TitleType result2;
                                Enum.TryParse<TitleType>(priceRecord.TitleType, out result2);
                                var pricingRecord = (IPricingRecord)new PricingRecord()
                                {
                                    TitleFamily = result1,
                                    TitleType = result2,
                                    InitialNight = priceRecord.InitialNight,
                                    ExtraNight = priceRecord.ExtraNight,
                                    ExpirationPrice = priceRecord.ExpirationPrice,
                                    NonReturn = priceRecord.NonReturn,
                                    NonReturnDays = priceRecord.NonReturnDays
                                };
                                reservationDetails.PricingSet.PriceRecords.Add(pricingRecord);
                            }
                    }

                    reservation.ReservationDetails = (IReservationDetails)reservationDetails;
                }

                return reservation;
            }

            public static IReservationDataFile ConvertToReservationDataFile(IReservation reservation)
            {
                var reservationDataFile = (ReservationDataFile_V002)null;
                if (reservation != null)
                {
                    reservationDataFile = new ReservationDataFile_V002()
                    {
                        HashedCardId = reservation.HashedCardId,
                        ID = reservation.Id,
                        ReferenceNumber = reservation.ReferenceNumber
                    };
                    var reservationDataInfoV001 = new ReservationDataInfo_V001()
                    {
                        ReservationVersion = reservation.ReservationDetails.ReservationVersion,
                        ReferenceNumber = reservation.ReservationDetails.ReferenceNumber,
                        TransactionDate = reservation.ReservationDetails.TransactionDate,
                        IsMultiNightPrice = reservation.ReservationDetails.IsMultiNightPrice,
                        IsMultiDiscVend = reservation.ReservationDetails.IsMultiDiscVend,
                        IsGiftCard = reservation.ReservationDetails.IsGiftCard,
                        AuthorizeAtPickup = reservation.ReservationDetails.AuthorizeAtPickup,
                        IsLoyaltyRedemption = reservation.ReservationDetails.IsLoyaltyRedemption,
                        AppliedTitleMarketing = reservation.ReservationDetails.AppliedTitleMarketing,
                        AppliedHiveOnlinePromo = reservation.ReservationDetails.AppliedHiveOnlinePromo,
                        SubTotal = reservation.ReservationDetails.SubTotal,
                        DiscountedSubTotal = reservation.ReservationDetails.DiscountedSubTotal,
                        HiveOnlinePromoDiscount = reservation.ReservationDetails.HiveOnlinePromoDiscount,
                        Tax = reservation.ReservationDetails.Tax,
                        TaxRate = reservation.ReservationDetails.TaxRate,
                        DefaultServiceFee = reservation.ReservationDetails.DefaultServiceFee,
                        ActualServiceFee = reservation.ReservationDetails.ActualServiceFee,
                        GrandTotal = reservation.ReservationDetails.GrandTotal,
                        PromoCode = reservation.ReservationDetails.PromoCode,
                        ZipCode = reservation.ReservationDetails.ZipCode,
                        CustomerNumber = reservation.ReservationDetails.CustomerNumber,
                        EmailAddress = reservation.ReservationDetails.EmailAddress
                    };
                    reservationDataInfoV001.HistoryTitleIds.AddRange(
                        (IEnumerable<int>)reservation.ReservationDetails.HistoryTitleIds);
                    reservationDataInfoV001.CreditCardIds.AddRange(
                        (IEnumerable<string>)reservation.ReservationDetails.CreditCardIds);
                    reservationDataInfoV001.GiftCardIds.AddRange(
                        (IEnumerable<string>)reservation.ReservationDetails.GiftCardIds);
                    foreach (var reservedTitle in reservation.ReservationDetails.ReservedTitles)
                    {
                        var reservationTitleV001 = new ReservationTitle_V001()
                        {
                            Barcode = reservedTitle.Barcode,
                            TitleId = reservedTitle.TitleId,
                            Discount = reservedTitle.Discount,
                            DiscountedPrice = reservedTitle.DiscountedPrice,
                            Price = reservedTitle.Price,
                            LoyaltyPoints = reservedTitle.LoyaltyPoints,
                            DiscountType = reservedTitle.DiscountType
                        };
                        ReservationType_V001 result;
                        if (Enum.TryParse<ReservationType_V001>(reservedTitle.Action.ToString(), true, out result))
                            reservationTitleV001.ReservationType = result;
                        reservationDataInfoV001.ReservationTitles.Add(reservationTitleV001);
                    }

                    if (reservation.ReservationDetails.PricingSet != null)
                    {
                        reservationDataInfoV001.PricingSet = new ReservationPricingSet_V001()
                        {
                            PriceSetId = reservation.ReservationDetails.PricingSet.PriceSetId,
                            Name = reservation.ReservationDetails.PricingSet.Name,
                            ProgramName = reservation.ReservationDetails.PricingSet.ProgramName
                        };
                        if (reservation.ReservationDetails.PricingSet.PriceRecords != null &&
                            reservation.ReservationDetails.PricingSet.PriceRecords.Count > 0)
                        {
                            reservationDataInfoV001.PricingSet.PriceRecords = new List<ReservationPricingRecord_V001>();
                            foreach (var priceRecord in reservation.ReservationDetails.PricingSet.PriceRecords)
                            {
                                var pricingRecordV001 = new ReservationPricingRecord_V001()
                                {
                                    TitleFamily = priceRecord.TitleFamily.ToString(),
                                    TitleType = priceRecord.TitleType.ToString(),
                                    InitialNight = priceRecord.InitialNight,
                                    ExtraNight = priceRecord.ExtraNight,
                                    ExpirationPrice = priceRecord.ExpirationPrice,
                                    NonReturn = priceRecord.NonReturn,
                                    NonReturnDays = priceRecord.NonReturnDays
                                };
                                reservationDataInfoV001.PricingSet.PriceRecords.Add(pricingRecordV001);
                            }
                        }
                    }

                    reservationDataFile.ReservationDataInfo = reservationDataInfoV001;
                }

                return (IReservationDataFile)reservationDataFile;
            }

            internal static List<ReservationDataFile_V002> ConvertReservationDataFileV001ToV002Files(
                IReservationDataFile inputDataFile)
            {
                var v002Files = new List<ReservationDataFile_V002>();
                if (inputDataFile is ReservationDataFile_V001 reservationDataFileV001)
                    foreach (var reservationDataInfo in reservationDataFileV001.ReservationDataInfos)
                    {
                        var reservationDataFileV002 = new ReservationDataFile_V002()
                        {
                            HashedCardId = reservationDataFileV001.HashedCardId,
                            ID = Guid.NewGuid(),
                            ReferenceNumber = reservationDataInfo.ReferenceNumber,
                            ReservationDataInfo = reservationDataInfo
                        };
                        v002Files.Add(reservationDataFileV002);
                    }

                return v002Files;
            }
        }
    }
}