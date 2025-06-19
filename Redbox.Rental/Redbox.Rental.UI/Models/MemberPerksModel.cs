using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Redbox.Rental.UI.ControllersLogic;

namespace Redbox.Rental.UI.Models
{
    public class MemberPerksModel : BaseModel<MemberPerksModel>, IPromoListModel
    {
        public static readonly DependencyProperty SnapshotPointsProperty =
            CreateDependencyProperty("SnapshotPoints", TYPES.STRING);

        public static readonly DependencyProperty SnapshotPointsLabelProperty =
            CreateDependencyProperty("SnapshotPointsLabel", TYPES.STRING);

        public static readonly DependencyProperty SnapshotExpireProperty =
            CreateDependencyProperty("SnapshotExpire", TYPES.STRING);

        public static readonly DependencyProperty EarnedRewardsMessageProperty =
            CreateDependencyProperty("EarnedRewardsMessage", TYPES.STRING);

        public static readonly DependencyProperty PerksLevelMessageProperty =
            CreateDependencyProperty("PerksLevelMessage", TYPES.STRING);

        public static readonly DependencyProperty PerksImageVisibilityProperty =
            CreateDependencyProperty("PerksImageVisibility", TYPES.VISIBILITY, Visibility.Visible);

        public static readonly DependencyProperty PerksTitleVisibilityProperty =
            CreateDependencyProperty("PerksTitleVisibility", TYPES.VISIBILITY, Visibility.Visible);

        public static readonly DependencyProperty PerksButtonVisibilityProperty =
            CreateDependencyProperty("PerksButtonVisibility", TYPES.VISIBILITY, Visibility.Visible);

        public static readonly DependencyProperty PerksLevelProperty =
            CreateDependencyProperty("PerksLevel", TYPES.STRING);

        public static readonly DependencyProperty PerksImagePathProperty =
            CreateDependencyProperty("PerksImagePath", TYPES.STRING);

        public static readonly DependencyProperty PerksRentalsProperty =
            CreateDependencyProperty("PerksRentals", TYPES.STRING);

        public static readonly DependencyProperty PromoButtonVisibilityProperty =
            CreateDependencyProperty("PromoButtonVisibility", TYPES.VISIBILITY, Visibility.Visible);

        public static readonly DependencyProperty PromoTitleProperty =
            CreateDependencyProperty("PromoTitle", TYPES.STRING);

        public static readonly DependencyProperty HeaderTextProperty =
            CreateDependencyProperty("HeaderText", TYPES.STRING);

        public static readonly DependencyProperty PerksButtonTextProperty =
            CreateDependencyProperty("PerksButtonText", TYPES.STRING);

        public static readonly DependencyProperty PromosButtonTextProperty =
            CreateDependencyProperty("PromosButtonText", TYPES.STRING);

        public static readonly DependencyProperty PerksMarketingProperty =
            CreateDependencyProperty("PerksMarketing", TYPES.STRING);

        public static readonly DependencyProperty PerksSelectedProperty =
            CreateDependencyProperty("PerksSelected", TYPES.BOOL, true);

        public static readonly DependencyProperty PromosSelectedProperty =
            CreateDependencyProperty("PromosSelected", TYPES.BOOL, false);

        public static readonly DependencyProperty MemberTierProperty =
            CreateDependencyProperty("MemberTier", typeof(TierType), TierType.Member);

        public static readonly DependencyProperty CancelButtonTextProperty =
            CreateDependencyProperty("CancelButtonText", TYPES.STRING);

        public static readonly DependencyProperty InfoButtonTextProperty =
            CreateDependencyProperty("InfoButtonText", TYPES.STRING);

        public static readonly DependencyProperty HowPointsWorkButtonTextProperty =
            CreateDependencyProperty("HowPointsWorkButtonText", TYPES.STRING);

        public static readonly DependencyProperty PromoListMessageProperty =
            CreateDependencyProperty("PromoListMessage", TYPES.STRING);

        public static readonly DependencyProperty ShowPromoListMessageProperty =
            CreateDependencyProperty("ShowPromoListMessage", TYPES.BOOL, false);

        public static readonly DependencyProperty CurrentPagePromosProperty =
            CreateDependencyProperty("CurrentPagePromos", typeof(ObservableCollection<IPerksOfferListItem>),
                new ObservableCollection<IPerksOfferListItem>());

        public static readonly DependencyProperty CurrentPageNumberProperty =
            CreateDependencyProperty("CurrentPageNumber", TYPES.INT, 1);

        public static readonly DependencyProperty IsAdaModeProperty =
            CreateDependencyProperty("IsAdaMode", TYPES.BOOL, false);

        public static readonly DependencyProperty TabButtonsColumnsProperty =
            CreateDependencyProperty("TabButtonsColumns", TYPES.INT, 2);

        public static readonly DependencyProperty RedboxPlusButtonVisibilityProperty =
            CreateDependencyProperty("RedboxPlusButtonVisibility", TYPES.VISIBILITY, Visibility.Collapsed);

        public static readonly DependencyProperty RedboxPlusButtonTextProperty =
            CreateDependencyProperty("RedboxPlusButtonText", TYPES.STRING);

        public static readonly DependencyProperty RedboxPlusSelectedProperty =
            CreateDependencyProperty("RedboxPlusSelected", TYPES.BOOL, false);

        public static readonly DependencyProperty RedboxPlusPlanId1VisibilityProperty =
            CreateDependencyProperty("RedboxPlusPlanId1Visibility", TYPES.VISIBILITY, Visibility.Visible);

        public static readonly DependencyProperty RedboxPlusPlanId2VisibilityProperty =
            CreateDependencyProperty("RedboxPlusPlanId2Visibility", TYPES.VISIBILITY, Visibility.Collapsed);

        public static readonly DependencyProperty RedboxPlusFreeMonthlyTextProperty =
            CreateDependencyProperty("RedboxPlusFreeMonthlyText", TYPES.STRING);

        public static readonly DependencyProperty RedboxPlusFreeThisMonthTextProperty =
            CreateDependencyProperty("RedboxPlusFreeThisMonthText", TYPES.STRING);

        public static readonly DependencyProperty RedboxPlusSeeMoviesTextProperty =
            CreateDependencyProperty("RedboxPlusSeeMoviesText", TYPES.STRING);

        public static readonly DependencyProperty RedboxPlusChooseTextProperty =
            CreateDependencyProperty("RedboxPlusChooseText", TYPES.STRING);

        public static readonly DependencyProperty RedboxPlusExtendedTextProperty =
            CreateDependencyProperty("RedboxPlusExtendedText", TYPES.STRING);

        public static readonly DependencyProperty RedboxPlusKeepTextProperty =
            CreateDependencyProperty("RedboxPlusKeepText", TYPES.STRING);

        public static readonly DependencyProperty RedboxPlusExtraNightTextProperty =
            CreateDependencyProperty("RedboxPlusExtraNightText", TYPES.STRING);

        public static readonly DependencyProperty RedboxPlusExtraThisMonthTextProperty =
            CreateDependencyProperty("RedboxPlusExtraThisMonthText", TYPES.STRING);

        public static readonly DependencyProperty RedboxPlusNeedAnotherTextProperty =
            CreateDependencyProperty("RedboxPlusNeedAnotherText", TYPES.STRING);

        public static readonly DependencyProperty RedboxPlusAwardedTextProperty =
            CreateDependencyProperty("RedboxPlusAwardedText", TYPES.STRING);

        public static readonly DependencyProperty RedboxPlusPlanRenewsTextProperty =
            CreateDependencyProperty("RedboxPlusPlanRenewsText", TYPES.STRING);

        public static readonly DependencyProperty RedboxPlusLegalTextProperty =
            CreateDependencyProperty("RedboxPlusLegalText", TYPES.STRING);

        public static readonly DependencyProperty RedboxPlusTermsTextProperty =
            CreateDependencyProperty("RedboxPlusTermsText", TYPES.STRING);

        public MemberPerksLogic Logic { get; set; }

        public DynamicRoutedCommand SelectedButtonCommand { get; set; }

        public DynamicRoutedCommand SnapshotButtonCommand { get; set; }

        public DynamicRoutedCommand PerksButtonCommand { get; set; }

        public DynamicRoutedCommand PromoButtonCommand { get; set; }

        public DynamicRoutedCommand CancelButtonCommand { get; set; }

        public DynamicRoutedCommand InfoButtonCommand { get; set; }

        public DynamicRoutedCommand HowPointsWorkButtonCommand { get; set; }

        public DynamicRoutedCommand SeeRedboxPlusMoviesButtonCommand { get; set; }

        public DynamicRoutedCommand TermsButtonCommand { get; set; }

        public bool IsAdaMode
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(IsAdaModeProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(IsAdaModeProperty, value); }); }
        }

        public string SnapshotPoints
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(SnapshotPointsProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(SnapshotPointsProperty, value); }); }
        }

        public string SnapshotPointsLabel
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(SnapshotPointsLabelProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(SnapshotPointsLabelProperty, value); }); }
        }

        public string SnapshotExpire
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(SnapshotExpireProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(SnapshotExpireProperty, value); }); }
        }

        public string EarnedRewardsMessage
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(EarnedRewardsMessageProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(EarnedRewardsMessageProperty, value); }); }
        }

        public string PerksLevelMessage
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PerksLevelMessageProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PerksLevelMessageProperty, value); }); }
        }

        public Visibility PerksImageVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(PerksImageVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PerksImageVisibilityProperty, value); }); }
        }

        public Visibility PerksTitleVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(PerksTitleVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PerksTitleVisibilityProperty, value); }); }
        }

        public Visibility PerksButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(PerksButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PerksButtonVisibilityProperty, value); }); }
        }

        public string PerksLevel
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PerksLevelProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PerksLevelProperty, value); }); }
        }

        public string PerksImagePath
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PerksImagePathProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PerksImagePathProperty, value); }); }
        }

        public string PerksRentals
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PerksRentalsProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PerksRentalsProperty, value); }); }
        }

        public Visibility PromoButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(PromoButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PromoButtonVisibilityProperty, value); }); }
        }

        public string PromoTitle
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PromoTitleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PromoTitleProperty, value); }); }
        }

        public string HeaderText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(HeaderTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(HeaderTextProperty, value); }); }
        }

        public string PerksButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PerksButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PerksButtonTextProperty, value); }); }
        }

        public string PromosButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PromosButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PromosButtonTextProperty, value); }); }
        }

        public string PerksMarketing
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PerksMarketingProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PerksMarketingProperty, value); }); }
        }

        public bool PerksSelected
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(PerksSelectedProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PerksSelectedProperty, value); }); }
        }

        public bool PromosSelected
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(PromosSelectedProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PromosSelectedProperty, value); }); }
        }

        public TierType MemberTier
        {
            get { return Dispatcher.Invoke(() => (TierType)GetValue(MemberTierProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MemberTierProperty, value); }); }
        }

        public string CancelButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(CancelButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CancelButtonTextProperty, value); }); }
        }

        public string InfoButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(InfoButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(InfoButtonTextProperty, value); }); }
        }

        public string HowPointsWorkButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(HowPointsWorkButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(HowPointsWorkButtonTextProperty, value); }); }
        }

        public string PromoListMessage
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PromoListMessageProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PromoListMessageProperty, value); }); }
        }

        public bool ShowPromoListMessage
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(ShowPromoListMessageProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ShowPromoListMessageProperty, value); }); }
        }

        public int TabButtonsColumns
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(TabButtonsColumnsProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TabButtonsColumnsProperty, value); }); }
        }

        public Visibility RedboxPlusButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(RedboxPlusButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusButtonVisibilityProperty, value); }); }
        }

        public string RedboxPlusButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(RedboxPlusButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusButtonTextProperty, value); }); }
        }

        public bool RedboxPlusSelected
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(RedboxPlusSelectedProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusSelectedProperty, value); }); }
        }

        public Visibility RedboxPlusPlanId1Visibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(RedboxPlusPlanId1VisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusPlanId1VisibilityProperty, value); }); }
        }

        public Visibility RedboxPlusPlanId2Visibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(RedboxPlusPlanId2VisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusPlanId2VisibilityProperty, value); }); }
        }

        public string RedboxPlusFreeMonthlyText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(RedboxPlusFreeMonthlyTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusFreeMonthlyTextProperty, value); }); }
        }

        public string RedboxPlusFreeThisMonthText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(RedboxPlusFreeThisMonthTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusFreeThisMonthTextProperty, value); }); }
        }

        public string RedboxPlusSeeMoviesText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(RedboxPlusSeeMoviesTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusSeeMoviesTextProperty, value); }); }
        }

        public string RedboxPlusChooseText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(RedboxPlusChooseTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusChooseTextProperty, value); }); }
        }

        public string RedboxPlusExtendedText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(RedboxPlusExtendedTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusExtendedTextProperty, value); }); }
        }

        public string RedboxPlusKeepText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(RedboxPlusKeepTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusKeepTextProperty, value); }); }
        }

        public string RedboxPlusExtraNightText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(RedboxPlusExtraNightTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusExtraNightTextProperty, value); }); }
        }

        public string RedboxPlusExtraThisMonthText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(RedboxPlusExtraThisMonthTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusExtraThisMonthTextProperty, value); }); }
        }

        public string RedboxPlusNeedAnotherText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(RedboxPlusNeedAnotherTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusNeedAnotherTextProperty, value); }); }
        }

        public string RedboxPlusAwardedText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(RedboxPlusAwardedTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusAwardedTextProperty, value); }); }
        }

        public string RedboxPlusPlanRenewsText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(RedboxPlusPlanRenewsTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusPlanRenewsTextProperty, value); }); }
        }

        public string RedboxPlusLegalText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(RedboxPlusLegalTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusLegalTextProperty, value); }); }
        }

        public string RedboxPlusTermsText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(RedboxPlusTermsTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusTermsTextProperty, value); }); }
        }

        public bool IsPromoCodeListVisible { get; set; }

        public bool IsEmptyPromoListMessageVisible { get; set; }

        public string EmptyPromoListMessage { get; set; }

        public DynamicRoutedCommand NextPagePressedCommand { get; set; }

        public DynamicRoutedCommand PreviousPagePressedCommand { get; set; }

        public DynamicRoutedCommand PageNumberPressedCommand { get; set; }

        public DynamicRoutedCommand AddOrRemovePromoCommand { get; set; }

        public DynamicRoutedCommand DetailsCommand { get; set; }

        public ObservableCollection<IPerksOfferListItem> CurrentPagePromos
        {
            get
            {
                return Dispatcher.Invoke(() =>
                    (ObservableCollection<IPerksOfferListItem>)GetValue(CurrentPagePromosProperty));
            }
            set { Dispatcher.Invoke(delegate { SetValue(CurrentPagePromosProperty, value); }); }
        }

        public int CurrentPageNumber
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(CurrentPageNumberProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CurrentPageNumberProperty, value); }); }
        }

        public List<IPerksOfferListItem> StoredPromoCodes { get; set; }

        public int NumberOfPages { get; set; }

        public int PromosPerPage { get; set; }

        public PromoListLogic PromoLogic => Logic.PromoLogic;
    }
}