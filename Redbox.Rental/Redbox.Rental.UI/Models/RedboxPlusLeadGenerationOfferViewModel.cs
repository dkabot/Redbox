using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Redbox.Rental.UI.Models
{
    public class RedboxPlusLeadGenerationOfferViewModel : DependencyObject
    {
        public Action OnEmailButtonClicked;

        public Action OnNoButtonClicked;

        public Action OnQRCodeButtonClicked;
        public BitmapImage BoxArt1 { get; set; }

        public BitmapImage BoxArt2 { get; set; }

        public BitmapImage BoxArt3 { get; set; }

        public BitmapImage BoxArt4 { get; set; }

        public BitmapImage BoxArt5 { get; set; }

        public BitmapImage BoxArt6 { get; set; }

        public BitmapImage BoxArt7 { get; set; }

        public BitmapImage BoxArt8 { get; set; }

        public BitmapImage BoxArt9 { get; set; }

        public string EmailText { get; set; }

        public string NoButtonText { get; set; }

        public Visibility EmailMessageTextVisibility { get; set; }

        public Visibility QRCodeStackPanelVisibility { get; set; }

        public void ProcessOnNoButtonClicked()
        {
            var onNoButtonClicked = OnNoButtonClicked;
            if (onNoButtonClicked == null) return;
            onNoButtonClicked();
        }

        public void ProcessOnEmailButtonClicked()
        {
            var onEmailButtonClicked = OnEmailButtonClicked;
            if (onEmailButtonClicked == null) return;
            onEmailButtonClicked();
        }

        public void ProcessOnQRCodeButtonClicked()
        {
            var onQRCodeButtonClicked = OnQRCodeButtonClicked;
            if (onQRCodeButtonClicked == null) return;
            onQRCodeButtonClicked();
        }
    }
}