using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Redbox.Rental.UI.Controls.Animations
{
    public partial class HackRVA : UserControl
    {
        public enum CardReadAnimations
        {
            InsertChipAndTapPhone
        }

        private bool m_isAnimated;

        public HackRVA()
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
            /*var storyboard = new Storyboard();
            storyboard.RepeatBehavior = RepeatBehavior.Forever;
            var translateTransform = new TranslateTransform();
            var rotateTransform = new RotateTransform();
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(translateTransform);
            transformGroup.Children.Add(rotateTransform);
            InsertChip.RenderTransform = transformGroup;
            var animationInfo = CreateInsertChipAnimationInfo();
            var animationInfo2 = CreateTapPhoneAnimationInfo();
            var animationInfo3 = CreateTapPhoneCheckmarkAnimationInfo();
            FirstAnimationInfo = null;
            SecondAnimationInfo = null;
            ThirdAnimationInfo = null;
            if (ShowAnimations == CardReadAnimations.InsertChipAndTapPhone)
            {
                FirstAnimationInfo = animationInfo;
                SecondAnimationInfo = animationInfo2;
                ThirdAnimationInfo = animationInfo3;
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

            if (SecondAnimationInfo != null && ThirdAnimationInfo != null)
            {
                var list2 = new List<string>();
                foreach (var keyValuePair2 in ThirdAnimationInfo.KeyTimes) list2.Add(keyValuePair2.Key);
                list2.ForEach(delegate(string key)
                {
                    var keyTimes2 = ThirdAnimationInfo.KeyTimes;
                    keyTimes2[key] += FirstAnimationInfo.TotalDuration + SecondAnimationInfo.TotalDuration;
                });
            }

            CreateInsertChipAnimation(storyboard, animationInfo);
            CreateTapPhoneAnimation(storyboard, animationInfo2);
            CreateTapPhoneCheckmarkAnimation(storyboard, animationInfo3);
            Timeline timeline = storyboard;
            var firstAnimationInfo = FirstAnimationInfo;
            var timeSpan = firstAnimationInfo != null ? firstAnimationInfo.TotalDuration : TimeSpan.Zero;
            var secondAnimationInfo = SecondAnimationInfo;
            var timeSpan2 =
                timeSpan + (secondAnimationInfo != null ? secondAnimationInfo.TotalDuration : TimeSpan.Zero);
            var thirdAnimationInfo = ThirdAnimationInfo;
            timeline.Duration =
                timeSpan2 + (thirdAnimationInfo != null ? thirdAnimationInfo.TotalDuration : TimeSpan.Zero);
            storyboard.Begin();*/
        }

        public void StopAnimation()
        {
            //InsertChip.RenderTransform = null;
        }
    }
}