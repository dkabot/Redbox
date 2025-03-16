using DeviceService.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

namespace Redbox.Rental.Model.KioskClientService.Transactions
{
    public class ShoppingCartMessageHelper
    {
        public static CartMessage CreateCartMessage(string customerProfileNumber, string messageType)
        {
            var service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
            return service?.CurrentRentalShoppingCart != null
                ? CreateCartMessage(service.CurrentRentalShoppingCart.ShoppingSessionId, customerProfileNumber,
                    messageType)
                : (CartMessage)null;
        }

        public static CartMessage CreateCartMessage(
            Guid sessionId,
            string customerProfileNumber,
            string messageType)
        {
            var cartMessage = new CartMessage();
            cartMessage.MessageType = messageType;
            cartMessage.KioskId = (long)ServiceLocator.Instance.GetService<IStoreServices>().StoreNumberInteger;
            cartMessage.CustomerProfileNumber = customerProfileNumber;
            cartMessage.ShoppingCart = CreateShoppingCart(sessionId, true);
            return cartMessage;
        }

        public static MessageShoppingCart CreateShoppingCart(Guid sessionId, bool titleItemsOnly = false)
        {
            var rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()
                ?.GetRentalShoppingCart(sessionId);
            return CreateShoppingCart(rentalShoppingCart, rentalShoppingCart.Promotion, titleItemsOnly);
        }

        public static MessageShoppingCart CreateShoppingCart(
            IRentalShoppingCart rentalShoppingCart,
            IPromotion promo,
            bool titleItemsOnly = false)
        {
            var shoppingCart = new MessageShoppingCart();
            int num;
            if (!titleItemsOnly)
            {
                var rentalShoppingCart1 = rentalShoppingCart;
                bool? nullable;
                if (rentalShoppingCart1 == null)
                {
                    nullable = new bool?();
                }
                else
                {
                    var items = rentalShoppingCart1.Items;
                    nullable = items != null ? new bool?(items.Any<IBaseRentalShoppingCartItem>()) : new bool?();
                }

                num = nullable.GetValueOrDefault() ? 1 : 0;
            }
            else
            {
                var rentalShoppingCart2 = rentalShoppingCart;
                bool? nullable;
                if (rentalShoppingCart2 == null)
                {
                    nullable = new bool?();
                }
                else
                {
                    var titleItems = rentalShoppingCart2.TitleItems;
                    nullable = titleItems != null
                        ? new bool?(titleItems.Any<IRentalShoppingCartTitleItem>())
                        : new bool?();
                }

                num = nullable.GetValueOrDefault() ? 1 : 0;
            }

            if (num != 0)
            {
                var rentalShoppingCart3 = rentalShoppingCart;
                IEnumerable<IRentalShoppingCartTitleItem> items;
                if (rentalShoppingCart3 == null)
                {
                    items = (IEnumerable<IRentalShoppingCartTitleItem>)null;
                }
                else
                {
                    var titleItems = rentalShoppingCart3.TitleItems;
                    items = titleItems != null
                        ? titleItems.Where<IRentalShoppingCartTitleItem>(
                            (Func<IRentalShoppingCartTitleItem, bool>)(x =>
                                x?.LoyaltyPointsRecord?.ToBeRedeemed ?? false))
                        : (IEnumerable<IRentalShoppingCartTitleItem>)null;
                }

                if (items != null)
                    items.ForEach<IRentalShoppingCartTitleItem>((Action<IRentalShoppingCartTitleItem>)(x =>
                    {
                        var discounts = shoppingCart.Discounts;
                        var discount = new Discount();
                        discount.ProductId = x.ProductId;
                        discount.DiscountType = DiscountType.Loyalty;
                        discount.RedemptionPoints = x.LoyaltyPointsRecord.PointsToRedeem;
                        var pricingRecord = x.PricingRecord;
                        discount.Amount = pricingRecord != null ? pricingRecord.InitialNight : 0M;
                        discounts.Add(discount);
                    }));
                if (rentalShoppingCart.Promotion != null)
                {
                    var promotion = promo;
                    if (promotion != null)
                    {
                        var promotionItems = promotion.PromotionItems;
                        if (promotionItems != null)
                            promotionItems.ForEach<IPromotionItem>((Action<IPromotionItem>)(x =>
                                shoppingCart.Discounts.Add(new Discount()
                                {
                                    Amount = x.PromotionAmount,
                                    DiscountType = (DiscountType)rentalShoppingCart.Promotion.DiscountType,
                                    PromotionCode = promo.Name,
                                    PromotionCodeValue = new decimal?(promo.OriginalPrice),
                                    PromotionIntentCode = new PromotionIntentCode?(promo.Intent),
                                    ProductId = x.RentalShoppingCartItem.ProductId,
                                    ApplyOnlyToProduct = promo.IsValidatedPostVend
                                        ? new bool?(promo.IsProductSpecific)
                                        : new bool?()
                                })));
                    }
                }

                foreach (var grouping in rentalShoppingCart.Items
                             .GroupBy<IBaseRentalShoppingCartItem, RentalShoppingCartItemAction>(
                                 (Func<IBaseRentalShoppingCartItem, RentalShoppingCartItemAction>)(x => x.Action)))
                {
                    var item = grouping;
                    var lineItemList = new List<LineItem>();
                    var lineItemGroupType = item.Key == RentalShoppingCartItemAction.subscription
                        ?
                        LineItemGroupType.Subscription
                        : item.Key == RentalShoppingCartItemAction.rental
                            ? LineItemGroupType.Rental
                            : LineItemGroupType.Purchase;
                    if (item.Key == RentalShoppingCartItemAction.subscription)
                    {
                        if (!titleItemsOnly)
                        {
                            foreach (IRentalShoppingCartSubscriptionItem subscriptionItem in
                                     (IEnumerable<IBaseRentalShoppingCartItem>)item)
                                lineItemList.Add(new LineItem()
                                {
                                    SubscriptionId = subscriptionItem.SubscriptionProduct.SubscriptionId,
                                    TempPassword = subscriptionItem.TempPassword
                                });
                            shoppingCart.Groups.Add(new LineItemGroup()
                            {
                                GroupType = lineItemGroupType,
                                Items = lineItemList
                            });
                        }
                    }
                    else
                    {
                        foreach (IRentalShoppingCartTitleItem rentalShoppingCartItem in
                                 (IEnumerable<IBaseRentalShoppingCartItem>)item)
                        {
                            var shoppingCartItem =
                                rentalShoppingCart.UpsellInfo.GetUpsellItemByRentalShoppingCartItem(
                                    rentalShoppingCartItem);
                            var blurayUpsell = (BlurayUpsell)null;
                            if (shoppingCartItem != null)
                                blurayUpsell = new BlurayUpsell()
                                {
                                    ProductGroupId = (int)shoppingCartItem.ProductGroupId,
                                    TitleId = rentalShoppingCartItem.ProductId,
                                    Offer = shoppingCartItem.OfferResponse.ToString(),
                                    TypeId = new int?((int)shoppingCartItem.TypeId)
                                };
                            var pricingRecord = rentalShoppingCartItem?.PricingRecord;
                            lineItemList.Add(new LineItem()
                            {
                                Barcode = rentalShoppingCartItem.Barcode,
                                BlurayUpsell = blurayUpsell,
                                SourceType = new ItemSourceType?(rentalShoppingCartItem.SourceType),
                                ProductFamily = rentalShoppingCartItem.TitleFamily.ToString(),
                                ProductId = rentalShoppingCartItem.ProductId,
                                ProductType = rentalShoppingCartItem.TitleType.GetDisplayName(),
                                ProductName = rentalShoppingCartItem.TitleProduct.ProductName,
                                OfferCode = rentalShoppingCartItem.OfferCode,
                                MultiDisc = rentalShoppingCartItem.MultiDisc,
                                DiscNumber = rentalShoppingCartItem.DiscNumber,
                                Pricing = new Pricing()
                                {
                                    DefaultExtraNight = (decimal?)pricingRecord?.DefaultExtraNightPrice,
                                    DefaultInitialNight = (decimal?)pricingRecord?.DefaultInitialNightPrice,
                                    Expiration = pricingRecord != null ? pricingRecord.ExpirationPrice : 0M,
                                    ExtraNight = pricingRecord != null ? pricingRecord.ExtraNight : 0M,
                                    InitialDays = (int?)pricingRecord?.InitialDays,
                                    InitialNight = pricingRecord != null ? pricingRecord.InitialNight : 0M,
                                    NonReturn = pricingRecord != null ? pricingRecord.NonReturn : 0M,
                                    NonReturnDays = (int)(pricingRecord != null ? pricingRecord.NonReturnDays : 0M),
                                    PriceSetId = ((int?)pricingRecord?.PriceSetId).GetValueOrDefault(),
                                    ProductPriceId =
                                        new int?((int)((long?)pricingRecord?.ProductPricingId).GetValueOrDefault()),
                                    Purchase = pricingRecord != null ? pricingRecord.Purchase : 0M
                                },
                                VendStatus = ConvertToVendStatus(rentalShoppingCartItem.Status),
                                TivoQueryId = rentalShoppingCartItem.TivoQueryId,
                                PersonalizationTitleTags = rentalShoppingCartItem.PersonalizationTitleTags
                            });
                        }

                        var subTotalGroup =
                            rentalShoppingCart.Totals.SubTotalGroups.FirstOrDefault<ISubTotalGroup>(
                                (Func<ISubTotalGroup, bool>)(y => item.Key == y.Action));
                        shoppingCart.Groups.Add(new LineItemGroup()
                        {
                            GroupType = lineItemGroupType,
                            Items = lineItemList,
                            Totals = new Totals()
                            {
                                DiscountedSubtotal = subTotalGroup.SubTotal,
                                Subtotal = subTotalGroup.Amount,
                                GrandTotal = subTotalGroup.GrantTotal,
                                TaxAmount = subTotalGroup.TaxAmount,
                                TaxRate = subTotalGroup.TaxRate * 100M
                            }
                        });
                    }
                }
            }

            if (rentalShoppingCart.ServiceFee != null && shoppingCart.ServiceFee != null)
            {
                shoppingCart.ServiceFee.DefaultAmount = rentalShoppingCart.ServiceFee.DefaultAmount;
                shoppingCart.ServiceFee.ActualAmount = rentalShoppingCart.ServiceFee.ActualAmount;
                shoppingCart.ServiceFee.TaxRate = rentalShoppingCart.ServiceFee.TaxRate;
                shoppingCart.ServiceFee.TaxAmount = rentalShoppingCart.ServiceFee.TaxAmount;
            }

            return shoppingCart;
        }

        public static string CalcUtcOffset(DateTime now)
        {
            var utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(now);
            return utcOffset.TotalHours.ToString("00") + ":" + utcOffset.Minutes.ToString("00");
        }

        private static VendStatus ConvertToVendStatus(LineItemStatus status)
        {
            switch (status)
            {
                case LineItemStatus.Pending:
                case LineItemStatus.InventoryError:
                    return VendStatus.EmptyOrStuck;
                case LineItemStatus.Fullfilled:
                    return VendStatus.Vended;
                case LineItemStatus.ItemNotTaken:
                    return VendStatus.DiskNotTaken;
                case LineItemStatus.HardwareError:
                    return VendStatus.HardwareError;
                case LineItemStatus.InventoryCapacity:
                    return VendStatus.KioskFull;
                case LineItemStatus.CustomerDisabled:
                    return VendStatus.CustomerDisabled;
                default:
                    return VendStatus.NotVended;
            }
        }

        public static Dictionary<string, object> GetCreditCardData(
            IShoppingSession session,
            bool includeTrack2)
        {
            var currentSession = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession();
            var trackData = currentSession?.TrackData;
            var unencryptedTrackData = trackData as IUnencryptedTrackData;
            var encryptedTrackData = trackData as IEncryptedTrackData;
            var emvTrackData = trackData as IEMVTrackData;
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("KeyId", (object)unencryptedTrackData?.EncryptionCerificateKeyId);
            dictionary.Add("Number", (object)unencryptedTrackData?.EncryptedAccountNumber);
            dictionary.Add("CardType", (object)(Core.CardType?)trackData?.CardType);
            dictionary.Add("CardId", (object)trackData?.CardHashId);
            dictionary.Add("LastFour", (object)(encryptedTrackData?.LastFour ?? trackData?.LastFour));
            dictionary.Add("LastName", (object)trackData?.LastName);
            dictionary.Add("FirstName", (object)trackData?.FirstName);
            dictionary.Add("PostalCode", (object)session?.ShoppingCart?.PostalCode);
            dictionary.Add("ExpirationYear", (object)trackData?.ExpiryYear);
            dictionary.Add("ExpirationMonth", (object)trackData?.ExpiryMonth);
            dictionary.Add("IsEncrypted", (object)(encryptedTrackData != null));
            dictionary.Add("BIN", (object)trackData?.FirstSix);
            dictionary.Add("KSN", (object)encryptedTrackData?.KSN);
            dictionary.Add("ICEncryptedData", (object)encryptedTrackData?.ICEncData);
            dictionary.Add("ReaderSerialNumber", (object)encryptedTrackData?.MfgSerialNumber);
            dictionary.Add("EmvEnabledTerminal", (object)trackData?.EmvEnabled);
            dictionary.Add("EmvTags", (object)emvTrackData?.Tags);
            var nullable =
                trackData == null || !trackData.IsInTechnicalFallback ||
                trackData.CardSourceType != CardSourceType.Swipe || !trackData.CardHasChip
                    ? new FallbackType?()
                    : currentSession?.LastFallbackType;
            dictionary.Add("FallbackType",
                (object)(nullable.HasValue ? new int?((int)nullable.GetValueOrDefault()) : new int?()));
            dictionary.Add("CardSourceType",
                (object)(CardSourceType)(trackData != null ? (int)trackData.CardSourceType : 0));
            dictionary.Add("ContactlessEnabled", (object)trackData?.ContactlessEnabledAndSupportsEmv);
            var creditCardData = dictionary;
            if (includeTrack2)
                creditCardData["Track2"] = (object)ExtractFromSecure(unencryptedTrackData?.EncryptedTrack2);
            return creditCardData;
        }

        private static string ExtractFromSecure(SecureString value)
        {
            try
            {
                return Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(value));
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}