using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models
{
    public class FormatSelectionPopupViewModel : BaseModel<FormatSelectionPopupViewModel>
    {
        public static readonly DependencyProperty PurchaseButtonMarginProperty =
            DependencyProperty.Register("PurchaseButtonMargin", typeof(Thickness),
                typeof(FormatSelectionPopupViewModel),
                new FrameworkPropertyMetadata(new Thickness(12.0, 24.0, 12.0, 0.0)));

        public static readonly DependencyProperty PurchaseButtonVisibilityProperty =
            DependencyProperty.Register("PurchaseButtonVisibility", typeof(Visibility),
                typeof(FormatSelectionPopupViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public Action OnCancelButtonClicked;

        public Action OnDigitalCodeInfoButtonClicked;

        public Action<TitleType> OnFormatButtonClicked;

        public Func<ISpeechControl> OnGetSpeechControl;

        public Action OnPurchaseButtonClicked;

        public FormatSelectionPopupViewModel()
        {
            ButtonData = new ObservableCollection<FormatButtonData>();
        }

        public string HeaderText { get; set; }

        public string DescriptionText { get; set; }

        public string DigitalCodeInfoText { get; set; }

        public ObservableCollection<FormatButtonData> ButtonData { get; set; }

        public string PurchaseButtonText { get; set; }

        public string PurchaseButtonLine2Text { get; set; }

        public string CancelButtonText { get; set; }

        public int DialogHeight { get; set; }

        public int DialogWidth { get; set; }

        public Thickness PurchaseButtonMargin
        {
            get { return Dispatcher.Invoke(() => (Thickness)GetValue(PurchaseButtonMarginProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PurchaseButtonMarginProperty, value); }); }
        }

        public Visibility PurchaseButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(PurchaseButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PurchaseButtonVisibilityProperty, value); }); }
        }

        public Visibility DigitalCodeInfoVisibility
        {
            get
            {
                if (!ButtonData.Any(b =>
                        b.Format == TitleType.DigitalCode && b.IsEnabled && !string.IsNullOrEmpty(DigitalCodeInfoText)))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public void ProcessFormatButtonClicked(TitleType tag)
        {
            var onFormatButtonClicked = OnFormatButtonClicked;
            if (onFormatButtonClicked == null) return;
            onFormatButtonClicked(tag);
        }

        public void ProcessPurchaseButtonClicked()
        {
            var onPurchaseButtonClicked = OnPurchaseButtonClicked;
            if (onPurchaseButtonClicked == null) return;
            onPurchaseButtonClicked();
        }

        public void ProcessCancelButtonClicked()
        {
            var onCancelButtonClicked = OnCancelButtonClicked;
            if (onCancelButtonClicked == null) return;
            onCancelButtonClicked();
        }

        public void ProcessDigitalCodeInfoButtonClicked()
        {
            var onDigitalCodeInfoButtonClicked = OnDigitalCodeInfoButtonClicked;
            if (onDigitalCodeInfoButtonClicked == null) return;
            onDigitalCodeInfoButtonClicked();
        }
    }
}