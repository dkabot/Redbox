using System;

namespace Redbox.Rental.UI.Models
{
    public class WelcomeViewModel
    {
        public event Action NavigateHelp;

        public event Action<bool, bool> NavigateStart;

        public void ProcessNavigateHelp()
        {
            var navigateHelp = NavigateHelp;
            if (navigateHelp == null) return;
            navigateHelp();
        }

        public void ProcessNavigateStart(bool isSpanish = false, bool isAda = false)
        {
            var navigateStart = NavigateStart;
            if (navigateStart == null) return;
            navigateStart(isSpanish, isAda);
        }
    }
}