using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.Rental.Model.Analytics
{
    public class AnalyticsShoppingCart
    {
        public decimal SubTotal { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal GrandTotal { get; set; }

        public List<AnalyticsShoppingCartItem> Items { get; } = new List<AnalyticsShoppingCartItem>();

        public List<AnalyticsShoppingCartSubscriptionItem> SubscriptionItems { get; } =
            new List<AnalyticsShoppingCartSubscriptionItem>();

        public List<AnalyticsDiscount> Discounts { get; } = new List<AnalyticsDiscount>();

        public static AnalyticsShoppingCart ToShoppingCart(IRentalShoppingCart sc)
        {
            var cart = new AnalyticsShoppingCart()
            {
                SubTotal = sc.Totals == null ? 0M : sc.Totals.SubTotal,
                TaxAmount = sc.Totals == null ? 0M : sc.Totals.TaxAmount,
                GrandTotal = sc.Totals == null ? 0M : sc.Totals.GrandTotal
            };
            var rentalShoppingCart = sc;
            IEnumerable<IRentalShoppingCartTitleItem> source;
            if (rentalShoppingCart == null)
            {
                source = (IEnumerable<IRentalShoppingCartTitleItem>)null;
            }
            else
            {
                var titleItems = rentalShoppingCart.TitleItems;
                source = titleItems != null
                    ? titleItems.Where<IRentalShoppingCartTitleItem>(
                        (Func<IRentalShoppingCartTitleItem, bool>)(x => x?.LoyaltyPointsRecord?.ToBeRedeemed ?? false))
                    : (IEnumerable<IRentalShoppingCartTitleItem>)null;
            }

            if (source != null)
                source.ToList<IRentalShoppingCartTitleItem>().ForEach((Action<IRentalShoppingCartTitleItem>)(x =>
                {
                    var discounts = cart.Discounts;
                    var analyticsDiscount = new AnalyticsDiscount();
                    analyticsDiscount.ProductId = new long?(x.ProductId);
                    analyticsDiscount.DiscountType = DiscountType.Loyalty;
                    analyticsDiscount.RedemptionPoints = x.LoyaltyPointsRecord.PointsToRedeem;
                    var pricingRecord = x.PricingRecord;
                    analyticsDiscount.Amount = pricingRecord != null ? pricingRecord.InitialNight : 0M;
                    discounts.Add(analyticsDiscount);
                }));
            if (sc.Promotion != null)
            {
                var promotionItems = sc.Promotion.PromotionItems;
                if (promotionItems != null)
                    promotionItems.ToList<IPromotionItem>().ForEach((Action<IPromotionItem>)(x =>
                        cart.Discounts.Add(new AnalyticsDiscount()
                        {
                            Amount = x.PromotionAmount,
                            DiscountType = DiscountType.PromotionCode,
                            PromotionCode = sc.Promotion.Name,
                            PromotionCodeValue = new decimal?(sc.Promotion.OriginalPrice),
                            PromotionIntentCode = new PromotionIntentCode?(sc.Promotion.Intent),
                            ProductId = new long?(x.RentalShoppingCartItem.ProductId),
                            ApplyOnlyToProduct = sc.Promotion.IsValidatedPostVend
                                ? new bool?(sc.Promotion.IsProductSpecific)
                                : new bool?()
                        })));
            }

            try
            {
                var service = ServiceLocator.Instance.GetService<IDataService>();
                if (service != null)
                {
                    var movieBrowseProducts = service.GetMovieBrowseProducts();
                    var rollupOnly = movieBrowseProducts
                        .Where<ITitleProduct>((Func<ITitleProduct, bool>)(x => x is ITitleRollupProduct))
                        .Select<ITitleProduct, ITitleRollupProduct>(
                            (Func<ITitleProduct, ITitleRollupProduct>)(x => x as ITitleRollupProduct))
                        .ToList<ITitleRollupProduct>();
                    sc.TitleItems.ToList<IRentalShoppingCartTitleItem>().ForEach(
                        (Action<IRentalShoppingCartTitleItem>)(item =>
                        {
                            var titleRollupProduct = rollupOnly
                                .Where<ITitleRollupProduct>((Func<ITitleRollupProduct, bool>)(tr =>
                                    tr.KioskProducts.Contains(item.TitleProduct)))
                                .FirstOrDefault<ITitleRollupProduct>();
                            if (titleRollupProduct != null)
                            {
                                var num = movieBrowseProducts.IndexOf((ITitleProduct)titleRollupProduct);
                                item.RentalPosition = num + 1;
                            }
                            else
                            {
                                var num = movieBrowseProducts.IndexOf(item.TitleProduct);
                                if (num <= -1)
                                    return;
                                item.RentalPosition = num + 1;
                            }
                        }));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException("AnalyticsShoppingCart.ToShoppingCart - unhandled exception occurred",
                    ex);
            }

            sc.TitleItems.ToList<IRentalShoppingCartTitleItem>().ForEach(
                (Action<IRentalShoppingCartTitleItem>)(item =>
                    cart.Items.Add(AnalyticsShoppingCartItem.ToShoppingCartItem(item))));
            sc.SubscriptionItems.ToList<IRentalShoppingCartSubscriptionItem>().ForEach(
                (Action<IRentalShoppingCartSubscriptionItem>)(subscriptionItem =>
                    cart.SubscriptionItems.Add(
                        AnalyticsShoppingCartSubscriptionItem
                            .ToAnalyticsShoppingCartSubscriptionItem(subscriptionItem))));
            return cart;
        }
    }
}