using System.Windows.Controls;
using System.Windows.Input;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.Controls.Animations
{
    public partial class EMVTapSignInUserControl : UserControl, ISwipeAnimationControl
    {
        private bool m_isAnimated;

        public EMVTapSignInUserControl()
        {
            InitializeComponent();
            Phone.ShowAnimations = EMVTapPhoneUserControl.CardReadAnimations.TapPhone;
            IsAnimated = true;
        }

        public bool IsAnimated
        {
            get => m_isAnimated;
            set
            {
                if (m_isAnimated != value)
                {
                    m_isAnimated = value;
                    if (value)
                    {
                        BeginAnimation();
                        return;
                    }

                    StopAnimation();
                }
            }
        }

        private SwipeViewModel SwipeViewModel => DataContext as SwipeViewModel;

        public void BeginAnimation()
        {
            Phone.IsAnimated = true;
        }

        public void StopAnimation()
        {
            Phone.RenderTransform = null;
        }

        private void MessageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var swipeViewModel = SwipeViewModel;
            if (swipeViewModel == null) return;
            swipeViewModel.ProcessOnMessageButtonClicked();
        }
    }
}