using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Redbox.Rental.UI.Controls.Animations
{
    public partial class EmvInsertAndTapUserControl : UserControl
    {
        public enum CardReadAnimations
        {
            ChipAndContactless,
            ChipOnly,
            ContactlessOnly
        }

        private AnimationInfo _firstAnimationInfo;

        private AnimationInfo _secondAnimationInfo;

        private bool m_isAnimated;

        public EmvInsertAndTapUserControl()
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

        public CardReadAnimations ShowAnimations { get; set; }

        public bool OnlyShowChipAnimation { get; set; }

        private AnimationInfo FirstAnimationInfo
        {
            get => _firstAnimationInfo;
            set
            {
                _firstAnimationInfo = value;
                if (_firstAnimationInfo != null) _firstAnimationInfo.IsVisable = true;
            }
        }

        private AnimationInfo SecondAnimationInfo
        {
            get => _secondAnimationInfo;
            set
            {
                _secondAnimationInfo = value;
                if (_secondAnimationInfo != null) _secondAnimationInfo.IsVisable = true;
            }
        }

        public void BeginAnimation()
        {
            var storyboard = new Storyboard();
            storyboard.RepeatBehavior = RepeatBehavior.Forever;
            var translateTransform = new TranslateTransform();
            var rotateTransform = new RotateTransform();
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(translateTransform);
            transformGroup.Children.Add(rotateTransform);
            CardAndHand.RenderTransform = transformGroup;
            var animationInfo = CreatePhoneAndHandAnimationInfo();
            var animationInfo2 = CreateInsertChipAnimationInfo();
            FirstAnimationInfo = null;
            SecondAnimationInfo = null;
            switch (ShowAnimations)
            {
                case CardReadAnimations.ChipAndContactless:
                    FirstAnimationInfo = animationInfo2;
                    SecondAnimationInfo = animationInfo;
                    break;
                case CardReadAnimations.ChipOnly:
                    FirstAnimationInfo = animationInfo2;
                    break;
                case CardReadAnimations.ContactlessOnly:
                    FirstAnimationInfo = animationInfo;
                    animationInfo.IsVisable = true;
                    break;
            }

            if (FirstAnimationInfo != null && SecondAnimationInfo != null)
            {
                var list = new List<string>();
                foreach (var keyValuePair in SecondAnimationInfo.KeyTimes) list.Add(keyValuePair.Key);
                list.ForEach(delegate(string key)
                {
                    var keyTimes = SecondAnimationInfo.KeyTimes;
                    keyTimes[key] += FirstAnimationInfo.TotalDuration;
                });
            }

            CreatePhoneAndHandAnimation(storyboard, animationInfo);
            CreateInsertChipAnimation(storyboard, animationInfo2);
            Timeline timeline = storyboard;
            var firstAnimationInfo = FirstAnimationInfo;
            var timeSpan = firstAnimationInfo != null ? firstAnimationInfo.TotalDuration : TimeSpan.Zero;
            var secondAnimationInfo = SecondAnimationInfo;
            timeline.Duration =
                timeSpan + (secondAnimationInfo != null ? secondAnimationInfo.TotalDuration : TimeSpan.Zero);
            storyboard.Begin();
        }

        private void CreateInsertChipAnimation(Storyboard storyboard, AnimationInfo insertChipAnimationInfo)
        {
            var doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                Value = 0.0
            });
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["StartTime"]),
                Value = 0.0
            });
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["EndOfInitialDelay"]),
                Value = 0.0
            });
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["EndOfFirstMotion"]),
                Value = -48.0
            });
            var doubleAnimationUsingKeyFrames2 = new DoubleAnimationUsingKeyFrames();
            doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                Value = 30.0
            });
            doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["StartTime"]),
                Value = 30.0
            });
            doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["EndOfInitialDelay"]),
                Value = 30.0
            });
            doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["EndOfFirstMotion"]),
                Value = -110.0
            });
            var doubleAnimationUsingKeyFrames3 = new DoubleAnimationUsingKeyFrames();
            doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                Value = 0.0
            });
            if (insertChipAnimationInfo.IsVisable)
            {
                doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["StartTime"] > TimeSpan.Zero
                        ? insertChipAnimationInfo.KeyTimes["StartTime"] - TimeSpan.FromMilliseconds(10.0)
                        : TimeSpan.Zero),
                    Value = 0.0
                });
                doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["StartTime"]),
                    Value = 1.0
                });
                doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["EndOfPauseDelay"]),
                    Value = 1.0
                });
                doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["EndOfFade"]),
                    Value = 0.0
                });
            }
            else
            {
                doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                    Value = 0.0
                });
            }

            Storyboard.SetTarget(doubleAnimationUsingKeyFrames3, CardAndHand);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames3, new PropertyPath("(Opacity)"));
            storyboard.Children.Add(doubleAnimationUsingKeyFrames3);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrames, CardAndHand);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));
            storyboard.Children.Add(doubleAnimationUsingKeyFrames);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrames2, CardAndHand);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames2,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)"));
            storyboard.Children.Add(doubleAnimationUsingKeyFrames2);
        }

        private static AnimationInfo CreateInsertChipAnimationInfo()
        {
            var animationInfo = new AnimationInfo();
            animationInfo.Durations["InitialDelay"] = TimeSpan.FromSeconds(0.75);
            animationInfo.Durations["FirstMotion"] = TimeSpan.FromSeconds(0.5);
            animationInfo.Durations["PauseDelay"] = TimeSpan.FromSeconds(2.0);
            animationInfo.Durations["FadeDelay"] = TimeSpan.FromSeconds(0.25);
            animationInfo.Durations["EndDelay"] = TimeSpan.FromSeconds(0.25);
            animationInfo.KeyTimes["StartTime"] = TimeSpan.Zero;
            animationInfo.KeyTimes["EndOfInitialDelay"] =
                animationInfo.KeyTimes["StartTime"] + animationInfo.Durations["InitialDelay"];
            animationInfo.KeyTimes["EndOfFirstMotion"] =
                animationInfo.KeyTimes["EndOfInitialDelay"] + animationInfo.Durations["FirstMotion"];
            animationInfo.KeyTimes["EndOfPauseDelay"] =
                animationInfo.KeyTimes["EndOfFirstMotion"] + animationInfo.Durations["PauseDelay"];
            animationInfo.KeyTimes["EndOfFade"] =
                animationInfo.KeyTimes["EndOfPauseDelay"] + animationInfo.Durations["FadeDelay"];
            animationInfo.KeyTimes["EndOfAnimation"] =
                animationInfo.KeyTimes["EndOfFade"] + animationInfo.Durations["EndDelay"];
            return animationInfo;
        }

        private AnimationInfo CreatePhoneAndHandAnimationInfo()
        {
            var animationInfo = new AnimationInfo();
            animationInfo.Durations["InitialDelay"] = TimeSpan.FromSeconds(0.75);
            animationInfo.Durations["FirstMotion"] = TimeSpan.FromSeconds(0.5);
            animationInfo.Durations["PauseDelay"] = TimeSpan.FromSeconds(2.0);
            animationInfo.Durations["FadeDelay"] = TimeSpan.FromSeconds(0.25);
            animationInfo.Durations["EndDelay"] = TimeSpan.FromSeconds(0.25);
            animationInfo.KeyTimes["StartTime"] = TimeSpan.Zero;
            animationInfo.KeyTimes["EndOfInitialDelay"] =
                animationInfo.KeyTimes["StartTime"] + animationInfo.Durations["InitialDelay"];
            animationInfo.KeyTimes["EndOfFirstMotion"] =
                animationInfo.KeyTimes["EndOfInitialDelay"] + animationInfo.Durations["FirstMotion"];
            animationInfo.KeyTimes["EndOfPauseDelay"] =
                animationInfo.KeyTimes["EndOfFirstMotion"] + animationInfo.Durations["PauseDelay"];
            animationInfo.KeyTimes["EndOfFade"] =
                animationInfo.KeyTimes["EndOfPauseDelay"] + animationInfo.Durations["FadeDelay"];
            animationInfo.KeyTimes["EndOfAnimation"] =
                animationInfo.KeyTimes["EndOfFade"] + animationInfo.Durations["EndDelay"];
            return animationInfo;
        }

        private void CreatePhoneAndHandAnimation(Storyboard storyboard, AnimationInfo phoneAndHandAnimationInfo)
        {
            var translateTransform = new TranslateTransform();
            var rotateTransform = new RotateTransform();
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(translateTransform);
            transformGroup.Children.Add(rotateTransform);
            PhoneAndHand.RenderTransform = transformGroup;
            var doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["StartTime"]),
                Value = 80.0
            });
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["EndOfInitialDelay"]),
                Value = 80.0
            });
            doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["EndOfFirstMotion"]),
                Value = -205.0
            });
            var doubleAnimationUsingKeyFrames2 = new DoubleAnimationUsingKeyFrames();
            doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["StartTime"]),
                Value = 100.0
            });
            doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["EndOfInitialDelay"]),
                Value = 100.0
            });
            doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["EndOfFirstMotion"]),
                Value = 28.0
            });
            var doubleAnimationUsingKeyFrames3 = new DoubleAnimationUsingKeyFrames();
            doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                Value = 0.0
            });
            if (phoneAndHandAnimationInfo.IsVisable)
            {
                doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["StartTime"] > TimeSpan.Zero
                        ? phoneAndHandAnimationInfo.KeyTimes["StartTime"] - TimeSpan.FromMilliseconds(10.0)
                        : TimeSpan.Zero),
                    Value = 0.0
                });
                doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["StartTime"]),
                    Value = 1.0
                });
                doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["EndOfPauseDelay"]),
                    Value = 1.0
                });
                doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["EndOfFade"]),
                    Value = 0.0
                });
            }

            var doubleAnimationUsingKeyFrames4 = new DoubleAnimationUsingKeyFrames();
            doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["StartTime"]),
                Value = 30.0
            });
            doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["EndOfInitialDelay"]),
                Value = 30.0
            });
            doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["EndOfFirstMotion"]),
                Value = 8.0
            });
            Storyboard.SetTarget(doubleAnimationUsingKeyFrames3, PhoneAndHand);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames3, new PropertyPath("(Opacity)"));
            storyboard.Children.Add(doubleAnimationUsingKeyFrames3);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrames, PhoneAndHand);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));
            storyboard.Children.Add(doubleAnimationUsingKeyFrames);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrames2, PhoneAndHand);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames2,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)"));
            storyboard.Children.Add(doubleAnimationUsingKeyFrames2);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrames4, PhoneAndHand);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames4,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(RotateTransform.Angle)"));
            storyboard.Children.Add(doubleAnimationUsingKeyFrames4);
        }

        public void StopAnimation()
        {
            CardAndHand.RenderTransform = null;
        }

        private class DurationConstants
        {
            public const string InitialDelay = "InitialDelay";

            public const string FirstMotion = "FirstMotion";

            public const string PauseDelay = "PauseDelay";

            public const string FadeDelay = "FadeDelay";

            public const string EndDelay = "EndDelay";
        }

        private class KeyTimeConstants
        {
            public const string StartTime = "StartTime";

            public const string EndOfInitialDelay = "EndOfInitialDelay";

            public const string EndOfFirstMotion = "EndOfFirstMotion";

            public const string EndOfPauseDelay = "EndOfPauseDelay";

            public const string EndOfFade = "EndOfFade";

            public const string EndOfAnimation = "EndOfAnimation";
        }

        private class AnimationInfo
        {
            public bool IsVisable { get; set; }

            public Dictionary<string, TimeSpan> Durations { get; } = new Dictionary<string, TimeSpan>();

            public Dictionary<string, TimeSpan> KeyTimes { get; } = new Dictionary<string, TimeSpan>();

            public TimeSpan TotalDuration
            {
                get
                {
                    var timeSpan = TimeSpan.Zero;
                    if (Durations.Count > 0)
                        foreach (var keyValuePair in Durations)
                            timeSpan += keyValuePair.Value;

                    return timeSpan;
                }
            }
        }
    }
}