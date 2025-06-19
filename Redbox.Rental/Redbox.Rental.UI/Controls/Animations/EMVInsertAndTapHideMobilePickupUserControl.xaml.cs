using System.Windows.Controls;

namespace Redbox.Rental.UI.Controls.Animations
{
    public partial class EMVInsertAndTapHideMobilePickupUserControl : UserControl, ISwipeAnimationControl
    {
        private bool m_isAnimated;

        public EMVInsertAndTapHideMobilePickupUserControl()
        {
            InitializeComponent();
            Card.ShowAnimations = EMVInsertAndTapCardUserControl.CardReadAnimations.InsertChipAndTapCard;
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