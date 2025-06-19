using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.UI.ControllersLogic
{
    public class PromoListLogic : BaseLogic
    {
        private IPromoListModel _model;

        public PromoListLogic(IPromoListModel model)
        {
            _model = model;
        }

        public static object RentalShoppingCartService { get; private set; }

        protected static IApplicationState ApplicationState => ServiceLocator.Instance.GetService<IApplicationState>();

        public void AddPageNumberPressedAnalytics(int pageNumber)
        {
            ProcessWPFHit();
            var analyticsService = AnalyticsService;
            if (analyticsService == null) return;
            analyticsService.AddButtonPressEvent("PromoListLogicPageChangeNumber");
        }

        public void AddPreviousPagePressedAnalytics()
        {
            ProcessWPFHit();
            var analyticsService = AnalyticsService;
            if (analyticsService == null) return;
            analyticsService.AddButtonPressEvent("PromoListLogicPageBack");
        }

        public void AddNextPagePressedAnalytics()
        {
            ProcessWPFHit();
            var analyticsService = AnalyticsService;
            if (analyticsService == null) return;
            analyticsService.AddButtonPressEvent("PromoListLogicPageNext");
        }

        public void AddRemovePromoButtonPressedAnalytics(StoredPromoCodeModel promoCodeModel)
        {
            ProcessWPFHit();
            var text = promoCodeModel.IsAdded ? "PromoListLogicCancelRedeem" : "PromoListLogicRedeem";
            var analyticsService = AnalyticsService;
            if (analyticsService == null) return;
            analyticsService.AddButtonPressEvent(text)
                .AddData(new NameValueData("PromoCode", promoCodeModel.PromoCode));
        }

        public static List<IPerksOfferListItem> CreateOfferListItemModels(IPromoListModel model,
            IPersonalizationSession personalizationSession, string activePromoCode, char maskCharacter,
            bool includeOffers, bool includeRedboxPlus)
        {
            var list = new List<IPerksOfferListItem>();
            if (personalizationSession != null && (personalizationSession.StoredPromoCodes != null ||
                                                   personalizationSession.AcceptedOffers != null))
            {
                AddStoredPromos(model, personalizationSession, activePromoCode, maskCharacter, list, includeRedboxPlus);
                AddCustomerOffers(model, personalizationSession, includeOffers, list);
            }

            model.StoredPromoCodes = list;
            return list;
        }

        private static void AddCustomerOffers(IPromoListModel model, IPersonalizationSession personalizationSession,
            bool includeOffers, List<IPerksOfferListItem> promoModels)
        {
            if (includeOffers)
            {
                var acceptedOffers = personalizationSession.AcceptedOffers;
                if ((acceptedOffers != null ? acceptedOffers.Count : 0) > 0)
                {
                    var num = 0;
                    foreach (var acceptedOffer in personalizationSession.AcceptedOffers)
                    {
                        var perksOfferListItemModel =
                            CreatePersOfferListItemModelFromAcceptedOffer(model, acceptedOffer);
                        if (promoModels.Count >= num)
                            promoModels.Insert(num, perksOfferListItemModel);
                        else
                            promoModels.Add(perksOfferListItemModel);
                        if (num == 0)
                        {
                            num = 3;
                        }
                        else
                        {
                            num++;
                            if (num > 4) break;
                        }
                    }
                }
            }
        }

        private static void AddStoredPromos(IPromoListModel model, IPersonalizationSession personalizationSession,
            string activePromoCode, char maskCharacter, List<IPerksOfferListItem> promoModels, bool includeRedboxPlus)
        {
            if (personalizationSession.StoredPromoCodes != null)
            {
                var service = ServiceLocator.Instance.GetService<IDataService>();
                var flag = false;
                foreach (var storedPromoCode in personalizationSession.StoredPromoCodes)
                {
                    var text = DetermineExpirationText(storedPromoCode);
                    var flag2 =
                        (service != null ? service.GetRedboxPlusPromoCampaign(storedPromoCode.CampaignId) : null) !=
                        null;
                    if (!flag2 || (includeRedboxPlus && !flag))
                    {
                        var storedPromoCodeModel = new StoredPromoCodeModel
                        {
                            PromoCode = storedPromoCode.PromoCode,
                            CampaignId = storedPromoCode.CampaignId,
                            PromoCodeDescription =
                                flag2 ? Resources.redbox_plus_free_1_night_rental : storedPromoCode.CampaignName,
                            IsRedboxPlusPromo = flag2,
                            PromoCodeExpirationText = text,
                            ExpirationDate = storedPromoCode.ExpirationDate,
                            MaskCharacter = maskCharacter,
                            IsAdded = storedPromoCode.PromoCode ==
                                      (activePromoCode ?? personalizationSession.SelectedPromo),
                            AddRemoveButtonCommand = model.AddOrRemovePromoCommand,
                            AddedToBagText = Resources.promo_added_to_bag,
                            Logic = model.PromoLogic,
                            IsAdaMode = ApplicationState.IsADAAccessible
                        };
                        if (flag2)
                        {
                            InsertPromoModel(promoModels, 0, storedPromoCodeModel);
                            flag = true;
                        }
                        else
                        {
                            promoModels.Add(storedPromoCodeModel);
                        }
                    }
                }
            }
        }

        private static void InsertPromoModel(List<IPerksOfferListItem> promoModels, int insertPos,
            IPerksOfferListItem promoModelToInsert)
        {
            if (promoModels.Count > insertPos)
            {
                promoModels.Insert(insertPos, promoModelToInsert);
                return;
            }

            promoModels.Add(promoModelToInsert);
        }

        private static PerksOfferListItemModel CreatePersOfferListItemModelFromAcceptedOffer(
            IPromoListModel promoListModel, IAcceptedOffer offer)
        {
            var status = offer.Status;
            var text = status == "Active" ? Resources.loyalty_offer_in_progress : string.Empty;
            var text2 = FormatTimeLeft(offer.TimeToComplete, offer.CompleteDate);
            var perksOfferListItemModel = new PerksOfferListItemModel
            {
                OfferStatus = status,
                StatusText = text,
                Name = offer.Name,
                TimeToComplete = text2,
                Description = offer.Description,
                LegalInformation = offer.LegalInformation,
                CurrentValue = offer.CurrentValue,
                MaxValue = offer.MaxValue,
                RemainderValue = offer.RemainderValue,
                StartDate = offer.StartDate,
                EndDate = offer.EndDate,
                DetailsButtonText = Resources.loyalty_offer_details_button_text,
                CongratsText = Resources.loyalty_offer_congrats,
                YouEarnedText = Resources.loyalty_offer_you_earned,
                ForCompletingText = Resources.loyalty_offer_for_completing,
                IsAdaMode = ApplicationState.IsADAAccessible,
                DetailsButtonCommand = promoListModel.DetailsCommand
            };
            if (!string.IsNullOrEmpty(offer.EndValueText))
            {
                var array = offer.EndValueText.Split(' ');
                perksOfferListItemModel.OfferValueText = array[0];
                if (array.Length > 1) perksOfferListItemModel.OfferUnitsText = array[1];
            }

            return perksOfferListItemModel;
        }

        private static string FormatTimeLeft(string timeToComplete, DateTime? completeDate)
        {
            var text = string.Empty;
            var num = -1;
            if (completeDate != null)
            {
                num = (completeDate.Value.Date - DateTime.Now.Date).Days;
            }
            else if (!string.IsNullOrEmpty(timeToComplete))
            {
                var array = timeToComplete.Split(' ');
                if (array.Length == 2 && array[1] == "D") int.TryParse(array[0], out num);
            }

            if (num > 1 && num < 15)
                text = string.Format(Resources.loyalty_offer_days_left, num);
            else if (num == 1)
                text = Resources.loyalty_offer_one_day_left;
            else if (num == 0) text = Resources.loyalty_offer_last_day;
            return text;
        }

        private static string DetermineExpirationText(IStoredPromoCode promo)
        {
            var text = string.Empty;
            if (!string.IsNullOrEmpty(promo.ExpirationText))
            {
                text = promo.ExpirationText;
            }
            else if (promo.DaysUntilExpiration != null)
            {
                if (promo.DaysUntilExpiration.Value == 0)
                {
                    text = Resources.promo_expires_today;
                }
                else if (promo.DaysUntilExpiration.Value == 1)
                {
                    text = Resources.promo_expires_tomorrow;
                }
                else
                {
                    var num = promo.DaysUntilExpiration;
                    var num2 = 1;
                    if ((num.GetValueOrDefault() > num2) & (num != null))
                    {
                        num = promo.DaysUntilExpiration;
                        num2 = 3;
                        if ((num.GetValueOrDefault() <= num2) & (num != null))
                            text = string.Format(Resources.promo_expires_in_days, promo.DaysUntilExpiration.Value);
                    }
                }
            }

            return text;
        }

        private static object GetWordForNumber(int value)
        {
            string text;
            switch (value)
            {
                case 1:
                    text = Resources.number_1;
                    break;
                case 2:
                    text = Resources.number_2;
                    break;
                case 3:
                    text = Resources.number_3;
                    break;
                default:
                    text = value.ToString();
                    break;
            }

            return text;
        }

        public static int FindPageOfSelectedPromo(IPromoListModel viewModel)
        {
            var num = Math.Round(
                (viewModel.StoredPromoCodes.IndexOf(viewModel.StoredPromoCodes.FirstOrDefault(p =>
                    p is StoredPromoCode && ((StoredPromoCodeModel)p).IsAdded)) + 1) / (double)viewModel.PromosPerPage +
                0.4);
            if (num < 1.0) num = 1.0;
            return (int)num;
        }
    }
}