using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Core;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.ControllersLogic
{
    public class MemberPerksLogic : BaseLogic
    {
        private const string ImagePath = "pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/";

        private const string perks_hero = "perks-star-white-icon.png";

        private const string perks_superhero = "perks-superstar-white-icon.png";

        private const string perks_legend = "perks-legend-white-icon.png";

        public MemberPerksLogic(MemberPerksModel perksModel, PerksMemberInfo memberInfo, PromoListLogic promoLogic)
        {
            Instructions = true;
            CloseAction = memberInfo.CancelAction;
            PromoLogic = promoLogic;
            Model = perksModel;
            Model.CancelButtonCommand =
                RegisterRoutedCommand(Commands.CancelButtonCommand, CancelButtonCommand_Execute);
            Model.SelectedButtonCommand = RegisterRoutedCommand(Commands.SelectedButtonCommand,
                new Action<string>(SelectedButtonCommand_Execute), null);
            Model.NextPagePressedCommand =
                RegisterRoutedCommand(new DynamicRoutedCommand(), PromoLogic.AddNextPagePressedAnalytics);
            Model.PreviousPagePressedCommand = RegisterRoutedCommand(new DynamicRoutedCommand(),
                PromoLogic.AddPreviousPagePressedAnalytics);
            Model.PageNumberPressedCommand =
                RegisterRoutedCommand<int>(new DynamicRoutedCommand(), PromoLogic.AddPageNumberPressedAnalytics);
            PopulatePerks(memberInfo);
        }

        public bool Instructions { get; set; }

        public Action CloseAction { get; set; }

        public MemberPerksModel Model { get; }

        public PromoListLogic PromoLogic { get; }

        private static List<string> ImageNames => new List<string>
            { null, "perks-star-white-icon.png", "perks-superstar-white-icon.png", "perks-legend-white-icon.png" };

        protected static IRentalShoppingCartService RentalShoppingCartService =>
            ServiceLocator.Instance.GetService<IRentalShoppingCartService>();

        public bool IsRedboxPlus => Model.RedboxPlusButtonVisibility == Visibility.Visible;

        public bool IsPerks => Model.PerksButtonVisibility == Visibility.Visible;

        public bool IsPromo => Model.PromoButtonVisibility == Visibility.Visible;

        protected void PopulatePerks(PerksMemberInfo memberInfo)
        {
            var array = new[]
            {
                Resources.perks_tier_title0,
                Resources.perks_tier_title1,
                Resources.perks_tier_title2,
                Resources.perks_tier_title3
            };
            double num = memberInfo.YearRentals;
            Model.SnapshotPoints = string.Format("{0:n0}", memberInfo.Points);
            Model.SnapshotPointsLabel = Resources.perks_snapshot_points;
            Model.SnapshotExpire = memberInfo.ExpirePoints > 0U
                ? string.Format(Resources.perks_snapshot_expire, memberInfo.ExpirePoints,
                    memberInfo.ExpireDate.Value.ToString("MM/dd/yyyy"))
                : "";
            var memberTier = (int)Model.MemberTier;
            var num2 = GetPerksTierIndex(num);
            num2 = num2 < memberTier ? memberTier : num2;
            SetSimpleNextLevel(num2, num);
            var text = array[num2];
            var flag = false;
            switch (num2)
            {
                case 1:
                    Model.MemberTier = TierType.Star;
                    Model.PerksLevelMessage = Resources.perks_level1_message;
                    break;
                case 2:
                    Model.MemberTier = TierType.Superstar;
                    Model.PerksLevelMessage = Resources.perks_level2_message;
                    break;
                case 3:
                    Model.MemberTier = TierType.Legend;
                    Model.PerksLevelMessage = Resources.perks_level3_message;
                    break;
                default:
                    flag = true;
                    Model.MemberTier = TierType.Member;
                    Model.PerksLevelMessage = Resources.perks_level0_message;
                    break;
            }

            Model.PerksLevel = text;
            Model.PerksImagePath =
                flag ? null : "pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/" + ImageNames[num2];
            Model.EarnedRewardsMessage = GetEarnedRewardsMessage(memberInfo.Points);
            Model.PerksMarketing = Resources.perks_marketing;
            Model.PerksImageVisibility = flag ? Visibility.Collapsed : Visibility.Visible;
            Model.PerksTitleVisibility = flag ? Visibility.Visible : Visibility.Collapsed;
            Model.PromoTitle = Resources.perks_promo_title;
        }

        private string GetEarnedRewardsMessage(uint points)
        {
            if (points < 1000U) return Resources.perks_earned_level0_message;
            if (points < 1500U) return string.Format(Resources.perks_earned_level1_message, 1500U - points);
            if (points < 3000U) return Resources.perks_earned_level2_message;
            if (points < 4500U) return Resources.perks_earned_level3_message;
            if (points < 6000U) return Resources.perks_earned_level4_message;
            if (points < 7500U) return Resources.perks_earned_level5_message;
            return Resources.perks_earned_level6_message;
        }

        private void SetSimpleNextLevel(int index, double rentals)
        {
            double num;
            do
            {
                switch (index)
                {
                    default:
                        num = 10.0;
                        break;
                    case 1:
                        num = 20.0;
                        break;
                    case 2:
                        num = 50.0;
                        break;
                    case 3:
                        num = -1.0;
                        break;
                }
            } while (num >= 0.0 && num - rentals < 0.0);
        }

        private int GetPerksTierIndex(double rentals)
        {
            int num;
            if (rentals < 10.0)
                num = 0;
            else if (rentals < 20.0)
                num = 1;
            else if (rentals < 50.0)
                num = 2;
            else
                num = 3;
            return num;
        }

        public void RedboxPlusButtonCommand_Execute()
        {
            Model.RedboxPlusSelected = true;
            Model.PerksSelected = false;
            Model.PromosSelected = false;
            HandleWPFHit();
            var analyticsService = AnalyticsService;
            if (analyticsService == null) return;
            analyticsService.AddButtonPressEvent("MemberPerksLogicRedboxPlus");
        }

        public void PerksButtonCommand_Execute()
        {
            Model.RedboxPlusSelected = false;
            Model.PerksSelected = true;
            Model.PromosSelected = false;
            HandleWPFHit();
            var analyticsService = AnalyticsService;
            if (analyticsService == null) return;
            analyticsService.AddButtonPressEvent("MemberPerksLogicPerks");
        }

        public void PromoButtonCommand_Execute()
        {
            Model.RedboxPlusSelected = false;
            Model.PerksSelected = false;
            Model.PromosSelected = true;
            HandleWPFHit();
            var analyticsService = AnalyticsService;
            if (analyticsService == null) return;
            analyticsService.AddButtonPressEvent("MemberPerksLogicPromo");
        }

        public void CancelButtonCommand_Execute()
        {
            var closeAction = CloseAction;
            if (closeAction != null) closeAction();
            HandleWPFHit();
            var analyticsService = AnalyticsService;
            if (analyticsService == null) return;
            analyticsService.AddButtonPressEvent("MemberPerksLogicBack");
        }

        public void SelectedButtonCommand_Execute(string buttonValue)
        {
            var analyticsService = AnalyticsService;
            if (analyticsService == null) return;
            analyticsService.AddButtonPressEvent("MemberPerksLogicButtonValue", buttonValue);
        }
    }
}