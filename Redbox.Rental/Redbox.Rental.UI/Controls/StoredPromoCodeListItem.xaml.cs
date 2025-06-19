using System.Windows;
using System.Windows.Media;

namespace Redbox.Rental.UI.Controls
{
    public partial class StoredPromoCodeListItem : BaseUserControl
    {
        private static readonly DependencyProperty PromoCodeTextProperty = DependencyProperty.Register("PromoCodeText",
            typeof(string), typeof(StoredPromoCodeListItem), new PropertyMetadata(null));

        private static readonly DependencyProperty IsAddedTextProperty = DependencyProperty.Register("IsAddedText",
            typeof(string), typeof(StoredPromoCodeListItem), new PropertyMetadata(null));

        private static readonly DependencyProperty PromoDescriptionTextProperty =
            DependencyProperty.Register("PromoDescriptionText", typeof(string), typeof(StoredPromoCodeListItem),
                new PropertyMetadata(null));

        private static readonly DependencyProperty PromoExpirationTextProperty =
            DependencyProperty.Register("PromoExpirationText", typeof(string), typeof(StoredPromoCodeListItem),
                new PropertyMetadata(null));

        private static readonly DependencyProperty AdaButtonNumberProperty =
            DependencyProperty.Register("AdaButtonNumber", typeof(int), typeof(StoredPromoCodeListItem),
                new PropertyMetadata(0));

        private static readonly DependencyProperty IsAdaModeProperty = DependencyProperty.Register("IsAdaMode",
            typeof(bool), typeof(StoredPromoCodeListItem), new PropertyMetadata(false, IsAdaChanged));

        private static readonly DependencyProperty AddRemoveButtonCommandProperty =
            DependencyProperty.Register("AddRemoveButtonCommand", typeof(DynamicRoutedCommand),
                typeof(StoredPromoCodeListItem), new PropertyMetadata(null));

        private static readonly DependencyPropertyKey IsAddedTextVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly("IsAddedTextVisibility", typeof(Visibility),
                typeof(StoredPromoCodeListItem),
                new FrameworkPropertyMetadata(Visibility.Collapsed,
                    FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public static readonly DependencyProperty IsAddedTextVisibilityProperty =
            IsAddedTextVisibilityPropertyKey.DependencyProperty;

        private static readonly DependencyProperty IsAddedProperty = DependencyProperty.Register("IsAdded",
            typeof(bool), typeof(StoredPromoCodeListItem),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange, IsAddedChanged));

        private static readonly DependencyPropertyKey MainButtonStylePropertyKey =
            DependencyProperty.RegisterReadOnly("MainButtonStyle", typeof(Style), typeof(StoredPromoCodeListItem),
                new PropertyMetadata(null));

        public static readonly DependencyProperty MainButtonStyleProperty =
            MainButtonStylePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey AddRemoveButtonStylePropertyKey =
            DependencyProperty.RegisterReadOnly("AddRemoveButtonStyle", typeof(Style), typeof(StoredPromoCodeListItem),
                new PropertyMetadata(null));

        public static readonly DependencyProperty AddRemoveButtonStyleProperty =
            AddRemoveButtonStylePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey AddRemoveButtonXVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly("AddRemoveButtonXVisibility", typeof(Visibility),
                typeof(StoredPromoCodeListItem), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty AddRemoveButtonXVisibilityProperty =
            AddRemoveButtonXVisibilityPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey AddRemoveButtonPlusVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly("AddRemoveButtonPlusVisibility", typeof(Visibility),
                typeof(StoredPromoCodeListItem), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty AddRemoveButtonPlusVisibilityProperty =
            AddRemoveButtonPlusVisibilityPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey PromoTextBrushPropertyKey =
            DependencyProperty.RegisterReadOnly("PromoTextBrush", typeof(Brush), typeof(StoredPromoCodeListItem),
                new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty PromoTextBrushProperty = PromoTextBrushPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ExpirationTextBrushPropertyKey =
            DependencyProperty.RegisterReadOnly("ExpirationTextBrush", typeof(Brush), typeof(StoredPromoCodeListItem),
                new PropertyMetadata(Brushes.Red));

        public static readonly DependencyProperty ExpirationTextBrushProperty =
            ExpirationTextBrushPropertyKey.DependencyProperty;

        public StoredPromoCodeListItem()
        {
            InitializeComponent();
            SetStylesForAddedStatus(false);
        }

        public string PromoCodeText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PromoCodeTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PromoCodeTextProperty, value); }); }
        }

        public string IsAddedText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(IsAddedTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(IsAddedTextProperty, value); }); }
        }

        public string PromoDescriptionText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PromoDescriptionTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PromoDescriptionTextProperty, value); }); }
        }

        public string PromoExpirationText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PromoExpirationTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PromoExpirationTextProperty, value); }); }
        }

        public int AdaButtonNumber
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(AdaButtonNumberProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AdaButtonNumberProperty, value); }); }
        }

        public bool IsAdaMode
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(IsAdaModeProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(IsAdaModeProperty, value); }); }
        }

        public DynamicRoutedCommand AddRemoveButtonCommand
        {
            get { return Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(AddRemoveButtonCommandProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AddRemoveButtonCommandProperty, value); }); }
        }

        public Visibility IsAddedTextVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(IsAddedTextVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(IsAddedTextVisibilityPropertyKey, value); }); }
        }

        public bool IsAdded
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(IsAddedProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(IsAddedProperty, value); }); }
        }

        public Style MainButtonStyle
        {
            get { return Dispatcher.Invoke(() => (Style)GetValue(MainButtonStyleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MainButtonStylePropertyKey, value); }); }
        }

        public Style AddRemoveButtonStyle
        {
            get { return Dispatcher.Invoke(() => (Style)GetValue(AddRemoveButtonStyleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AddRemoveButtonStylePropertyKey, value); }); }
        }

        public Visibility AddRemoveButtonXVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(AddRemoveButtonXVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AddRemoveButtonXVisibilityPropertyKey, value); }); }
        }

        public Visibility AddRemoveButtonPlusVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(AddRemoveButtonPlusVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AddRemoveButtonPlusVisibilityPropertyKey, value); }); }
        }

        public Brush PromoTextBrush
        {
            get { return Dispatcher.Invoke(() => (Brush)GetValue(PromoTextBrushProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PromoTextBrushPropertyKey, value); }); }
        }

        public Brush ExpirationTextBrush
        {
            get { return Dispatcher.Invoke(() => (Brush)GetValue(ExpirationTextBrushProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ExpirationTextBrushPropertyKey, value); }); }
        }

        private static void IsAdaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var storedPromoCodeListItem = d as StoredPromoCodeListItem;
            if (storedPromoCodeListItem != null)
                storedPromoCodeListItem.SetStylesForAddedStatus(storedPromoCodeListItem.IsAdded);
        }

        private static void IsAddedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var storedPromoCodeListItem = d as StoredPromoCodeListItem;
            if (storedPromoCodeListItem != null) storedPromoCodeListItem.SetStylesForAddedStatus((bool)e.NewValue);
        }

        private void SetStylesForAddedStatus(bool isAdded)
        {
            if (isAdded)
            {
                MainButtonStyle = FindResource("style_flat_selectedpurple_button") as Style;
                AddRemoveButtonXVisibility = Visibility.Visible;
                IsAddedTextVisibility = Visibility.Visible;
                AddRemoveButtonPlusVisibility = Visibility.Collapsed;
                PromoTextBrush = Brushes.White;
                ExpirationTextBrush = Brushes.White;
            }
            else
            {
                MainButtonStyle = FindResource("style_flat_warmpurple_button") as Style;
                AddRemoveButtonXVisibility = Visibility.Collapsed;
                IsAddedTextVisibility = Visibility.Collapsed;
                AddRemoveButtonPlusVisibility = Visibility.Visible;
            }

            if (IsAdaMode)
            {
                AddRemoveButtonPlusVisibility = Visibility.Collapsed;
                AddRemoveButtonXVisibility = Visibility.Collapsed;
                AddRemoveButtonStyle = FindResource("style_rb_rubine_white_border_with_outlineshadow_button") as Style;
                return;
            }

            AddRemoveButtonStyle = FindResource("style_white_no_border_button") as Style;
        }
    }
}