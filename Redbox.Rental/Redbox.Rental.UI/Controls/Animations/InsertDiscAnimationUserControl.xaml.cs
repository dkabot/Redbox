using System.Windows.Controls;

namespace Redbox.Rental.UI.Controls.Animations
{
    public partial class InsertDiscAnimationUserControl : UserControl
    {
        private bool m_isAnimated;

        public InsertDiscAnimationUserControl()
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
            RedChevron.IsAnimated = true;
        }

        public void StopAnimation()
        {
            RedChevron.IsAnimated = false;
        }
    }
}