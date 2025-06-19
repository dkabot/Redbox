using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models
{
    public class FilterBarModel : DependencyObject
    {
        public static readonly DependencyProperty FilterBarTextProperty = DependencyProperty.Register("FilterBarText",
            typeof(string), typeof(FilterBarModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty FilterBarTextVisibilityProperty =
            DependencyProperty.Register("FilterBarTextVisibility", typeof(Visibility), typeof(FilterBarModel),
                new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty StartOverButtonTextProperty =
            DependencyProperty.Register("StartOverButtonText", typeof(string), typeof(FilterBarModel),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty StartOverButtonVisibilityProperty =
            DependencyProperty.Register("StartOverButtonVisibility", typeof(Visibility), typeof(FilterBarModel),
                new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty HomeButtonTextProperty = DependencyProperty.Register("HomeButtonText",
            typeof(string), typeof(FilterBarModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty HomeButtonVisibilityProperty =
            DependencyProperty.Register("HomeButtonVisibility", typeof(Visibility), typeof(FilterBarModel),
                new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty BackButtonVisibilityProperty =
            DependencyProperty.Register("BackButtonVisibility", typeof(Visibility), typeof(FilterBarModel),
                new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty FormatFilterButtonVisibilityProperty =
            DependencyProperty.Register("FormatFilterButtonVisibility", typeof(Visibility), typeof(FilterBarModel),
                new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty FormatFilterButtonCheckmarkVisibilityProperty =
            DependencyProperty.Register("FormatFilterButtonCheckmarkVisibility", typeof(Visibility),
                typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty FormatFilterButtonTextProperty =
            DependencyProperty.Register("FormatFilterButtonText", typeof(string), typeof(FilterBarModel),
                new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty FormatFilterButtonStyleProperty =
            DependencyProperty.Register("FormatFilterButtonStyle", typeof(Style), typeof(FilterBarModel),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SelectedFormatProperty = DependencyProperty.Register("SelectedFormat",
            typeof(TitleType?), typeof(FilterBarModel), new FrameworkPropertyMetadata(null, SelectedFormatChanged));

        public static readonly DependencyProperty PriceRangeFilterButtonVisibilityProperty =
            DependencyProperty.Register("PriceRangeFilterButtonVisibility", typeof(Visibility), typeof(FilterBarModel),
                new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty PriceRangeFilterButtonCheckmarkVisibilityProperty =
            DependencyProperty.Register("PriceRangeFilterButtonCheckmarkVisibility", typeof(Visibility),
                typeof(FilterBarModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty PriceRangeFilterButtonTextProperty =
            DependencyProperty.Register("PriceRangeFilterButtonText", typeof(string), typeof(FilterBarModel),
                new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty PriceRangeFilterButtonStyleProperty =
            DependencyProperty.Register("PriceRangeFilterButtonStyle", typeof(Style), typeof(FilterBarModel),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SelectedPriceRangeProperty =
            DependencyProperty.Register("SelectedPriceRange", typeof(BrowsePriceRangeParameter), typeof(FilterBarModel),
                new FrameworkPropertyMetadata(new BrowsePriceRangeParameter(), SelectedPriceRangeChanged));

        public string BackButtonText { get; set; }

        public string PriceRangeButtonDefaultText { get; set; }

        public List<BrowseFilter> Filters { get; set; }

        public Thickness FilterMargin { get; set; }

        public Dictionary<TitleType, string> FormatFilterButtonStrings { get; set; }

        public Style FilterButtonUnselectedStyle { get; set; }

        public Style FilterButtonSelectedStyle { get; set; }

        public CheckMarkButtonModel BottomRedboxPlusMoviesButtonModel { get; set; }

        public string StartOverButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(StartOverButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(StartOverButtonTextProperty, value); }); }
        }

        public Visibility StartOverButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(StartOverButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(StartOverButtonVisibilityProperty, value); }); }
        }

        public string HomeButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(HomeButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(HomeButtonTextProperty, value); }); }
        }

        public Visibility HomeButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(HomeButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(HomeButtonVisibilityProperty, value); }); }
        }

        public Visibility BackButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(BackButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BackButtonVisibilityProperty, value); }); }
        }

        public Visibility FormatFilterButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(FormatFilterButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(FormatFilterButtonVisibilityProperty, value); }); }
        }

        public Visibility FormatFilterButtonCheckmarkVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(FormatFilterButtonCheckmarkVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(FormatFilterButtonCheckmarkVisibilityProperty, value); }); }
        }

        public Visibility FilterBarTextVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(FilterBarTextVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(FilterBarTextVisibilityProperty, value); }); }
        }

        public string FilterBarText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(FilterBarTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(FilterBarTextProperty, value); }); }
        }

        public string FormatFilterButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(FormatFilterButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(FormatFilterButtonTextProperty, value); }); }
        }

        public TitleType SelectedFormat
        {
            get { return Dispatcher.Invoke(() => (TitleType)GetValue(SelectedFormatProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(SelectedFormatProperty, value); }); }
        }

        public Style FormatFilterButtonStyle
        {
            get { return Dispatcher.Invoke(() => (Style)GetValue(FormatFilterButtonStyleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(FormatFilterButtonStyleProperty, value); }); }
        }

        public Visibility PriceRangeFilterButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(PriceRangeFilterButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PriceRangeFilterButtonVisibilityProperty, value); }); }
        }

        public Visibility PriceRangeFilterButtonCheckmarkVisibility
        {
            get
            {
                return Dispatcher.Invoke(() => (Visibility)GetValue(PriceRangeFilterButtonCheckmarkVisibilityProperty));
            }
            set { Dispatcher.Invoke(delegate { SetValue(PriceRangeFilterButtonCheckmarkVisibilityProperty, value); }); }
        }

        public Style PriceRangeFilterButtonStyle
        {
            get { return Dispatcher.Invoke(() => (Style)GetValue(PriceRangeFilterButtonStyleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PriceRangeFilterButtonStyleProperty, value); }); }
        }

        public string PriceRangeFilterButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PriceRangeFilterButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PriceRangeFilterButtonTextProperty, value); }); }
        }

        public BrowsePriceRangeParameter SelectedPriceRange
        {
            get { return Dispatcher.Invoke(() => (BrowsePriceRangeParameter)GetValue(SelectedPriceRangeProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(SelectedPriceRangeProperty, value); }); }
        }

        private static bool IsADAMode
        {
            get
            {
                var service = ServiceLocator.Instance.GetService<IApplicationState>();
                return service != null && service.IsADAAccessible;
            }
        }

        public event Action OnStartOverButtonClicked;

        public event Action OnHomeButtonClicked;

        public event Action OnBackButtonClicked;

        public event Action<BrowseFilter> OnBrowseFilterButtonClicked;

        public event Action OnFormatFilterButtonClicked;

        public event Action OnPriceRangeFilterButtonClicked;

        public void ProcessOnStartOverButtonClicked()
        {
            if (OnStartOverButtonClicked != null) OnStartOverButtonClicked();
        }

        public void ProcessOnHomeButtonClicked()
        {
            var onHomeButtonClicked = OnHomeButtonClicked;
            if (onHomeButtonClicked == null) return;
            onHomeButtonClicked();
        }

        public void ProcessOnFormatFilterButtonClicked()
        {
            if (OnFormatFilterButtonClicked != null) OnFormatFilterButtonClicked();
        }

        public void ProcessOnPriceRangeFilterButtonClicked()
        {
            if (OnFormatFilterButtonClicked != null) OnPriceRangeFilterButtonClicked();
        }

        public void ProcessOnBrowseFilterButtonClicked(BrowseFilter browseFilter)
        {
            if (OnBrowseFilterButtonClicked != null) OnBrowseFilterButtonClicked(browseFilter);
        }

        public void ProcessOnBackButtonClicked()
        {
            if (OnBackButtonClicked != null) OnBackButtonClicked();
        }

        private static void SelectedPriceRangeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj != null && obj is FilterBarModel)
            {
                var filterBarModel = (FilterBarModel)obj;
                var browsePriceRangeParameter = args.NewValue == null ? null : (BrowsePriceRangeParameter)args.NewValue;
                if (browsePriceRangeParameter != null)
                {
                    filterBarModel.PriceRangeFilterButtonText = browsePriceRangeParameter.buttonText;
                    filterBarModel.PriceRangeFilterButtonStyle = filterBarModel.FilterButtonSelectedStyle;
                    filterBarModel.PriceRangeFilterButtonCheckmarkVisibility =
                        IsADAMode ? Visibility.Visible : Visibility.Collapsed;
                    return;
                }

                filterBarModel.PriceRangeFilterButtonText = filterBarModel.PriceRangeButtonDefaultText;
                filterBarModel.PriceRangeFilterButtonStyle = filterBarModel.FilterButtonUnselectedStyle;
                filterBarModel.PriceRangeFilterButtonCheckmarkVisibility = Visibility.Collapsed;
            }
        }

        private static void SelectedFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj != null && obj is FilterBarModel)
            {
                var filterBarModel = (FilterBarModel)obj;
                var titleType = args.NewValue == null ? TitleType.All : ((TitleType?)args.NewValue).Value;
                if (filterBarModel.FormatFilterButtonStrings.ContainsKey(titleType))
                {
                    filterBarModel.FormatFilterButtonText = filterBarModel.FormatFilterButtonStrings[titleType];
                    filterBarModel.FormatFilterButtonStyle = titleType == TitleType.All
                        ? filterBarModel.FilterButtonUnselectedStyle
                        : filterBarModel.FilterButtonSelectedStyle;
                    filterBarModel.FormatFilterButtonCheckmarkVisibility =
                        filterBarModel.FormatFilterButtonVisibility == Visibility.Visible && titleType != TitleType.All
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                }
            }
        }
    }
}