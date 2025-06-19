using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Redbox.Rental.UI.Controls.Animations
{
    public partial class RedboxDiscUserControl : UserControl
    {
        public enum ScrollDirection
        {
            Right,
            Left
        }

        private bool m_isAnimated;

        public RedboxDiscUserControl()
        {
            InitializeComponent();
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

        public ScrollDirection AnimationScrollDirection { get; set; }

        public void BeginAnimation()
        {
            if (AnimationScrollDirection == ScrollDirection.Left)
            {
                var doubleAnimation = new DoubleAnimation(600.0, 0.0, new Duration(TimeSpan.FromSeconds(4.0)));
                var translateTransform = new TranslateTransform();
                RenderTransform = translateTransform;
                translateTransform.BeginAnimation(TranslateTransform.XProperty, doubleAnimation);
                return;
            }

            var doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                Value = 0.0
            });
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.5)),
                Value = 0.0
            });
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(5.5)),
                Value = 600.0
            });
            var translateTransform2 = new TranslateTransform();
            RenderTransform = translateTransform2;
            translateTransform2.BeginAnimation(TranslateTransform.XProperty, doubleAnimationUsingKeyFrames);
        }

        public void StopAnimation()
        {
            RenderTransform = null;
        }
    }
}