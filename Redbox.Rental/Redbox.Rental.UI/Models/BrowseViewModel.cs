using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models
{
    public class BrowseViewModel : DependencyObject
    {
        public static readonly DependencyProperty BrowseGroupSelectionProperty =
            DependencyProperty.Register("BrowseGroupSelection", typeof(DisplayGroupSelectionModel),
                typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty DisplayGroupItemsProperty =
            DependencyProperty.Register("DisplayGroupItems", typeof(List<DisplayGroupSelectionItemModel>),
                typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty CheckOutButtonTextProperty =
            DependencyProperty.Register("CheckOutButtonText", typeof(string), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty CheckOutButtonCountTextProperty =
            DependencyProperty.Register("CheckOutButtonCountText", typeof(string), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty CheckOutButtonStyleProperty =
            DependencyProperty.Register("CheckOutButtonStyle", typeof(Style), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty AddToADACartLabelTextProperty =
            DependencyProperty.Register("AddToADACartLabelText", typeof(string), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SwitchProductFamilyButton1VisibilityProperty =
            DependencyProperty.Register("SwitchProductFamilyButton1Visibility", typeof(Visibility),
                typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty AddToADACartLabelVisibilityProperty =
            DependencyProperty.Register("AddToADACartLabelVisibility", typeof(Visibility), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty MiniCartVisibilityProperty =
            DependencyProperty.Register("MiniCartVisibility", typeof(Visibility), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty BackgroundImageVisibilityProperty =
            DependencyProperty.Register("BackgroundImageVisibility", typeof(Visibility), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty SwitchProductFamilyButton1TextProperty =
            DependencyProperty.Register("SwitchProductFamilyButton1Text", typeof(string), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty BrowseProductControlModelProperty =
            DependencyProperty.Register("BrowseProductControlModel", typeof(BrowseControlModel),
                typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(null, OnBrowseProductControlModelPropertyChanged));

        public static readonly DependencyProperty MiniCartControlModelProperty =
            DependencyProperty.Register("MiniCartControlModel", typeof(BrowseControlModel), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty BrowseFilterBarModelProperty =
            DependencyProperty.Register("BrowseFilterBarModel", typeof(FilterBarModel), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty BrowseTopMenuModelProperty =
            DependencyProperty.Register("BrowseTopMenuModel", typeof(TopMenuModel), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SignInButtonVisibilityProperty =
            DependencyProperty.Register("SignInButtonVisibility", typeof(Visibility), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty SignOutButtonVisibilityProperty =
            DependencyProperty.Register("SignOutButtonVisibility", typeof(Visibility), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty MyPerksButtonVisibilityProperty =
            DependencyProperty.Register("MyPerksButtonVisibility", typeof(Visibility), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty MyPerksRedboxPlusButtonVisibilityProperty =
            DependencyProperty.Register("MyPerksRedboxPlusButtonVisibility", typeof(Visibility),
                typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ViewRedboxPlusMoviesButtonVisibilityProperty =
            DependencyProperty.Register("ViewRedboxPlusMoviesButtonVisibility", typeof(Visibility),
                typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty RedboxPlusMiniCartVisiblityProperty =
            DependencyProperty.Register("RedboxPlusMiniCartVisiblity", typeof(Visibility), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ADARedboxPlusSignOutGridVisibilityProperty =
            DependencyProperty.Register("ADARedboxPlusSignOutGridVisibility", typeof(Visibility),
                typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty RedboxPlusEmptyCartSpaceVisiblityProperty =
            DependencyProperty.Register("RedboxPlusEmptyCartSpaceVisiblity", typeof(Visibility),
                typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty RedboxPlusMiniCartMessageTextProperty =
            DependencyProperty.Register("RedboxPlusMiniCartMessageText", typeof(string), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty BackToAllMoviesButtonVisibilityProperty =
            DependencyProperty.Register("BackToAllMoviesButtonVisibility", typeof(Visibility), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty EmptyCartSpacesVisibilityProperty =
            DependencyProperty.Register("EmptyCartSpacesVisibilityVisibility", typeof(Visibility),
                typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty EmptyCartAddItemMessageVisibilityProperty =
            DependencyProperty.Register("EmptyCartAddItemMessageVisibility", typeof(Visibility),
                typeof(BrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty ContinueButtonVisibilityProperty =
            DependencyProperty.Register("ContinueButtonVisibility", typeof(Visibility), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty CheckOutButtonVisibilityProperty =
            DependencyProperty.Register("CheckOutButtonVisibility", typeof(Visibility), typeof(BrowseViewModel),
                new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty BrowseRedboxPlusTooltipModelProperty =
            DependencyProperty.Register("BrowseRedboxPlusTooltipModel", typeof(BrowseRedboxPlusTooltipModel),
                typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty BrowseMessagePopupModelProperty =
            DependencyProperty.Register("BrowseMessagePopupModel", typeof(BrowseMessagePopupModel),
                typeof(BrowseViewModel), new FrameworkPropertyMetadata(null));

        public Action OnCheckOutButtonClicked;

        public Func<ISpeechControl> OnGetSpeechControl;

        public Action OnMyPerksButtonClicked;

        public Action OnSignInButtonClicked;

        public Action OnSignOutButtonClicked;

        public Action OnToggleTitleFamilyButton1Clicked;
        public DynamicRoutedCommand BackToAllMoviesCommand { get; set; }

        public DynamicRoutedCommand ContinueCommand { get; set; }

        public DynamicRoutedCommand ViewRedboxPlusMoviesCommand { get; set; }

        public string SignInButtonText { get; set; }

        public string SignUpButtonText { get; set; }

        public string SignInUpButtonSeparator { get; set; }

        public string SignOutButtonText { get; set; }

        public string MyPerksButtonText { get; set; }

        public string ViewRedboxPlusMoviesButtonText { get; set; }

        public string BackToAllMoviesButtonText { get; set; }

        public string ContinueButtonText { get; set; }

        public string DontMissYourPerksText { get; set; }

        public BrowseControlModel BrowseProductControlModel
        {
            get { return Dispatcher.Invoke(() => (BrowseControlModel)GetValue(BrowseProductControlModelProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BrowseProductControlModelProperty, value); }); }
        }

        public BrowseControlModel MiniCartControlModel
        {
            get { return Dispatcher.Invoke(() => (BrowseControlModel)GetValue(MiniCartControlModelProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MiniCartControlModelProperty, value); }); }
        }

        public FilterBarModel BrowseFilterBarModel
        {
            get { return Dispatcher.Invoke(() => (FilterBarModel)GetValue(BrowseFilterBarModelProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BrowseFilterBarModelProperty, value); }); }
        }

        public TopMenuModel BrowseTopMenuModel
        {
            get { return Dispatcher.Invoke(() => (TopMenuModel)GetValue(BrowseTopMenuModelProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BrowseTopMenuModelProperty, value); }); }
        }

        public string CheckOutButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(CheckOutButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CheckOutButtonTextProperty, value); }); }
        }

        public string CheckOutButtonCountText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(CheckOutButtonCountTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CheckOutButtonCountTextProperty, value); }); }
        }

        public Style CheckOutButtonStyle
        {
            get { return Dispatcher.Invoke(() => (Style)GetValue(CheckOutButtonStyleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CheckOutButtonStyleProperty, value); }); }
        }

        public string AddToADACartLabelText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(AddToADACartLabelTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AddToADACartLabelTextProperty, value); }); }
        }

        public Visibility AddToADACartLabelVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(AddToADACartLabelVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AddToADACartLabelVisibilityProperty, value); }); }
        }

        public Visibility MiniCartVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(MiniCartVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MiniCartVisibilityProperty, value); }); }
        }

        public Visibility SwitchProductFamilyButton1Visibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(SwitchProductFamilyButton1VisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(SwitchProductFamilyButton1VisibilityProperty, value); }); }
        }

        public Visibility BackgroundImageVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(BackgroundImageVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BackgroundImageVisibilityProperty, value); }); }
        }

        public string SwitchProductFamilyButton1Text
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(SwitchProductFamilyButton1TextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(SwitchProductFamilyButton1TextProperty, value); }); }
        }

        public bool IsADAAccessible { get; set; }

        public DisplayGroupSelectionModel BrowseGroupSelection
        {
            get { return Dispatcher.Invoke(() => (DisplayGroupSelectionModel)GetValue(BrowseGroupSelectionProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BrowseGroupSelectionProperty, value); }); }
        }

        public Visibility SignInButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(SignInButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(SignInButtonVisibilityProperty, value); }); }
        }

        public Visibility SignOutButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(SignOutButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(SignOutButtonVisibilityProperty, value); }); }
        }

        public Visibility MyPerksButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(MyPerksButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MyPerksButtonVisibilityProperty, value); }); }
        }

        public Visibility MyPerksRedboxPlusButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(MyPerksRedboxPlusButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MyPerksRedboxPlusButtonVisibilityProperty, value); }); }
        }

        public Visibility ViewRedboxPlusMoviesButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(ViewRedboxPlusMoviesButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ViewRedboxPlusMoviesButtonVisibilityProperty, value); }); }
        }

        public Visibility RedboxPlusMiniCartVisiblity
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(RedboxPlusMiniCartVisiblityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusMiniCartVisiblityProperty, value); }); }
        }

        public Visibility ADARedboxPlusSignOutGridVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(ADARedboxPlusSignOutGridVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ADARedboxPlusSignOutGridVisibilityProperty, value); }); }
        }

        public Visibility RedboxPlusEmptyCartSpaceVisiblity
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(RedboxPlusEmptyCartSpaceVisiblityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusEmptyCartSpaceVisiblityProperty, value); }); }
        }

        public string RedboxPlusMiniCartMessageText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(RedboxPlusMiniCartMessageTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusMiniCartMessageTextProperty, value); }); }
        }

        public Visibility BackToAllMoviesButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(BackToAllMoviesButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BackToAllMoviesButtonVisibilityProperty, value); }); }
        }

        public Visibility EmptyCartSpacesVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(EmptyCartSpacesVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(EmptyCartSpacesVisibilityProperty, value); }); }
        }

        public Visibility EmptyCartAddItemMessageVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(EmptyCartAddItemMessageVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(EmptyCartAddItemMessageVisibilityProperty, value); }); }
        }

        public Visibility ContinueButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(ContinueButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ContinueButtonVisibilityProperty, value); }); }
        }

        public Visibility CheckOutButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(CheckOutButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CheckOutButtonVisibilityProperty, value); }); }
        }

        public BrowseMessagePopupModel BrowseMessagePopupModel
        {
            get { return Dispatcher.Invoke(() => (BrowseMessagePopupModel)GetValue(BrowseMessagePopupModelProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BrowseMessagePopupModelProperty, value); }); }
        }

        public BrowseRedboxPlusTooltipModel BrowseRedboxPlusTooltipModel
        {
            get
            {
                return Dispatcher.Invoke(() =>
                    (BrowseRedboxPlusTooltipModel)GetValue(BrowseRedboxPlusTooltipModelProperty));
            }
            set { Dispatcher.Invoke(delegate { SetValue(BrowseRedboxPlusTooltipModelProperty, value); }); }
        }

        public string AddToCartLabel1Text { get; set; }

        public string AddToCartLabel2Text { get; set; }

        public ISpeechControl ProcessGetSpeechControl()
        {
            ISpeechControl speechControl = null;
            if (OnGetSpeechControl != null) speechControl = OnGetSpeechControl();
            return speechControl;
        }

        public void ProcessOnCheckOutButtonClicked()
        {
            if (OnCheckOutButtonClicked != null) OnCheckOutButtonClicked();
        }

        public void ProcessOnToggleTitleFamilyButton1Clicked()
        {
            if (OnToggleTitleFamilyButton1Clicked != null) OnToggleTitleFamilyButton1Clicked();
        }

        public void ProcessOnSignInButtonClicked()
        {
            var onSignInButtonClicked = OnSignInButtonClicked;
            if (onSignInButtonClicked == null) return;
            onSignInButtonClicked();
        }

        public void ProcessOnSignOutButtonClicked()
        {
            var onSignOutButtonClicked = OnSignOutButtonClicked;
            if (onSignOutButtonClicked == null) return;
            onSignOutButtonClicked();
        }

        public void ProcessOnMyPerksButtonClicked()
        {
            var onMyPerksButtonClicked = OnMyPerksButtonClicked;
            if (onMyPerksButtonClicked == null) return;
            onMyPerksButtonClicked();
        }

        public event Action<BrowseViewModel> OnBrowseProductControlModelChanged;

        private static void OnBrowseProductControlModelPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var browseViewModel = d as BrowseViewModel;
            if (browseViewModel != null && browseViewModel.OnBrowseProductControlModelChanged != null)
                browseViewModel.OnBrowseProductControlModelChanged(browseViewModel);
        }
    }
}