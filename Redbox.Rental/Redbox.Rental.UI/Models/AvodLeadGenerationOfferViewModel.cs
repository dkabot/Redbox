using System;
using System.Windows;

namespace Redbox.Rental.UI.Models
{
    public class AvodLeadGenerationOfferViewModel : DependencyObject
    {
        public Action OnEmailButtonClicked;

        public Action OnNoButtonClicked;

        public Action OnQRCodeButtonClicked;
        public string GridBackgroundImageSource { get; set; }

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