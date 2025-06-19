using System.Windows.Controls;

namespace Redbox.Rental.UI.Controls.Animations
{
    public partial class EncryptedSwipeRegularCheckoutUserControl : UserControl, ISwipeAnimationControl
    {
        private bool m_isAnimated;

        public EncryptedSwipeRegularCheckoutUserControl()
        {
            InitializeComponent();
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

        public void BeginAnimation()
        {
            Card.IsAnimated = true;
        }

        public void StopAnimation()
        {
            Card.RenderTransform = null;
        }
    }
}