using System.Timers;
using System.Windows;
using System.Windows.Controls;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.Controls.Animations
{
    public partial class VendDiscAnimationUserControl : UserControl, IVendDiscAnimationControl
    {
        private bool _isAnimated;

        public VendDiscAnimationUserControl()
        {
            InitializeComponent();
            TurnOffArrows();
        }

        public VendViewType VendType { get; set; }

        public bool IsSignedIn { get; set; }

        public bool IsPickupDisc
        {
            get => VendDiscPickup.Visibility == Visibility.Visible;
            set
            {
                if (value)
                {
                    VendDiscPickup.Visibility = Visibility.Visible;
                    VendDiscServe.Visibility = Visibility.Collapsed;
                    TurnOffArrows();
                    return;
                }

                VendDiscPickup.Visibility = Visibility.Collapsed;
                VendDiscServe.Visibility = Visibility.Visible;
                MessageGridBlock3.Visibility = IsSignedIn ? Visibility.Visible : Visibility.Collapsed;
                TurnOnArrows();
            }
        }

        public bool IsAnimated
        {
            get => _isAnimated;
            set
            {
                if (_isAnimated != value)
                {
                    _isAnimated = value;
                    if (value)
                    {
                        BeginAnimation();
                        return;
                    }

                    StopAnimation();
                }
            }
        }

        public string TitleText1
        {
            get => TitleTextBlock1.Text;
            set => TitleTextBlock1.Text = value;
        }

        public string MessageText1
        {
            get => MessageTextBlock1.Text;
            set => MessageTextBlock1.Text = value;
        }

        public string MessageText2
        {
            get => MessageTextBlock2.Text;
            set => MessageTextBlock2.Text = value;
        }

        public string MessageText3
        {
            get => MessageTextBlock3.Text;
            set => MessageTextBlock3.Text = value;
        }

        public void TurnOffArrows()
        {
            MessageCheck2.Visibility = Visibility.Hidden;
            MessageCheck3.Visibility = Visibility.Hidden;
        }

        public void TurnOnArrows()
        {
            MessageCheck2.Visibility = Visibility.Visible;
            var timer = new Timer(1000.0);
            timer.Elapsed += delegate
            {
                Application.Current.Dispatcher.Invoke(() => MessageCheck3.Visibility = Visibility.Visible);
            };
            timer.Start();
        }

        public void BeginAnimation()
        {
            RedChevron.IsAnimated = true;
        }

        public void StopAnimation()
        {
            RedChevron.RenderTransform = null;
        }
    }
}