using System;
using System.Windows.Media.Imaging;

namespace Redbox.Rental.UI.Models
{
    public class EmailOptInViewModel
    {
        public BitmapImage Image { get; set; }

        public string YesButtonText { get; set; }

        public string NoThanksButtonText { get; set; }

        public string MessageText { get; set; }

        public bool HasActionExecuted { get; set; }
        public event Action OnYesButtonClicked;

        public event Action OnNoThanksButtonClicked;

        public void ProcessOnYesButtonClicked()
        {
            var onYesButtonClicked = OnYesButtonClicked;
            if (onYesButtonClicked == null) return;
            onYesButtonClicked();
        }

        public void ProcessOnNoThanksButtonClicked()
        {
            var onNoThanksButtonClicked = OnNoThanksButtonClicked;
            if (onNoThanksButtonClicked == null) return;
            onNoThanksButtonClicked();
        }
    }
}