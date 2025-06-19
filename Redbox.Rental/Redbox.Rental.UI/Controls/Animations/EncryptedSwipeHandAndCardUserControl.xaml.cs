using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Redbox.Rental.UI.Controls.Animations
{
    public partial class EncryptedSwipeHandAndCardUserControl : UserControl
    {
        private bool m_isAnimated;

        public EncryptedSwipeHandAndCardUserControl()
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

        public void BeginAnimation()
        {
            var storyboard = new Storyboard();
            storyboard.AutoReverse = true;
            storyboard.RepeatBehavior = RepeatBehavior.Forever;
            var translateTransform = new TranslateTransform();
            var rotateTransform = new RotateTransform();
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(translateTransform);
            transformGroup.Children.Add(rotateTransform);
            CardAndHand.RenderTransform = transformGroup;
            var timeSpan = TimeSpan.FromSeconds(0.25);
            var timeSpan2 = TimeSpan.FromSeconds(0.5);
            var timeSpan3 = timeSpan + timeSpan2;
            var timeSpan4 = TimeSpan.FromSeconds(0.5);
            var timeSpan5 = timeSpan3 + timeSpan4;
            var timeSpan6 = TimeSpan.FromSeconds(0.5);
            var timeSpan7 = timeSpan5 + timeSpan6;
            var timeSpan8 = TimeSpan.FromSeconds(0.25);
            var timeSpan9 = timeSpan7 + timeSpan8;
            storyboard.Duration = timeSpan9;
            var doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                Value = 10.0
            });
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(timeSpan),
                Value = 10.0
            });
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(timeSpan3),
                Value = 0.0
            });
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                Value = 0.0
            });
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                Value = 10.0
            });
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(timeSpan9),
                Value = 10.0
            });
            var doubleAnimationUsingKeyFrames2 = new DoubleAnimationUsingKeyFrames();
            doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                Value = 100.0
            });
            doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(timeSpan),
                Value = 100.0
            });
            doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(timeSpan3),
                Value = 0.0
            });
            doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                Value = 30.0
            });
            doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                Value = 160.0
            });
            doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(timeSpan9),
                Value = 160.0
            });
            var doubleAnimationUsingKeyFrames3 = new DoubleAnimationUsingKeyFrames();
            doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                Value = 20.0
            });
            doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(timeSpan),
                Value = 20.0
            });
            doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(timeSpan3),
                Value = -20.0
            });
            doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(timeSpan5),
                Value = 320.0
            });
            doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(timeSpan7),
                Value = 240.0
            });
            doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(timeSpan9),
                Value = 240.0
            });
            Storyboard.SetTarget(doubleAnimationUsingKeyFrames2, CardAndHand);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames2,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));
            storyboard.Children.Add(doubleAnimationUsingKeyFrames2);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrames3, CardAndHand);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames3,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)"));
            storyboard.Children.Add(doubleAnimationUsingKeyFrames3);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrames, CardAndHand);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(RotateTransform.Angle)"));
            storyboard.Children.Add(doubleAnimationUsingKeyFrames);
            storyboard.Begin();
        }

        public void StopAnimation()
        {
            CardAndHand.RenderTransform = null;
        }
    }
}