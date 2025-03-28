using Redbox.Controls.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Redbox.Controls
{
    [Category("ProgressBar")]
    public class SectionalBar : ProgressBar
    {
        private static Dictionary<string, List<SectionalBar>> groupSections =
            new Dictionary<string, List<SectionalBar>>();

        public static readonly RoutedEvent ThresholdReachedEvent = EventManager.RegisterRoutedEvent("ThresholdReached",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SectionalBar));

        public static readonly DependencyProperty IsThresholdReachedProperty =
            Dependency<SectionalBar>.CreateDependencyProperty(nameof(IsThresholdReached), typeof(bool), (object)false,
                new PropertyChangedCallback(OnThresholdReachedPropertyChanged));

        public static readonly DependencyProperty ThresholdProperty =
            Dependency<SectionalBar>.CreateDependencyProperty(nameof(Threshold), typeof(double),
                (object)double.MaxValue, new PropertyChangedCallback(OnThresholdPropertyChanged));

        public static readonly DependencyProperty MaxValueProperty =
            Dependency<SectionalBar>.CreateDependencyProperty(nameof(MaxValue), typeof(double),
                (object)double.MaxValue);

        public static readonly DependencyProperty GroupCountProperty =
            Dependency<SectionalBar>.CreateDependencyProperty(nameof(GroupCount), typeof(int), (object)1,
                new PropertyChangedCallback(OnGroupCountPropertyChanged));

        public static readonly DependencyProperty GroupIndexProperty =
            Dependency<SectionalBar>.CreateDependencyProperty(nameof(GroupIndex), typeof(int), (object)0);

        public static readonly DependencyProperty IsFrontProperty =
            Dependency<SectionalBar>.CreateDependencyProperty(nameof(IsFront), typeof(bool), (object)false);

        public static readonly DependencyProperty IsBackProperty =
            Dependency<SectionalBar>.CreateDependencyProperty(nameof(IsBack), typeof(bool), (object)false);

        public static readonly DependencyProperty FrontCornerRadiusProperty =
            Dependency<SectionalBar>.CreateDependencyProperty(nameof(FrontCornerRadius), typeof(CornerRadius),
                (object)new CornerRadius(0.0));

        public static readonly DependencyProperty BackCornerRadiusProperty =
            Dependency<SectionalBar>.CreateDependencyProperty(nameof(BackCornerRadius), typeof(CornerRadius),
                (object)new CornerRadius(0.0));

        public static readonly DependencyProperty AutoCornerRadiusProperty =
            Dependency<SectionalBar>.CreateDependencyProperty(nameof(AutoCornerRadius), typeof(bool), (object)true,
                new PropertyChangedCallback(OnAutoCornerRadiusPropertyChanged));

        public static readonly DependencyProperty GroupNameProperty =
            Dependency<SectionalBar>.CreateDependencyProperty(nameof(GroupName), typeof(string), (object)string.Empty,
                new PropertyChangedCallback(OnGroupNameChanged));

        public static readonly DependencyProperty DisposeProperty =
            Dependency<SectionalBar>.CreateDependencyProperty(nameof(Dispose), typeof(bool), (object)false,
                new PropertyChangedCallback(OnDisposePropertyChanged));

        public static readonly DependencyProperty AutomationValueProperty =
            Dependency<SectionalBar>.CreateDependencyProperty(nameof(AutomationValue), typeof(double),
                (object)double.MaxValue, new PropertyChangedCallback(OnAutomationValueChanged));

        private static List<RangeLimit> RangeLimits { get; set; }

        private static bool running { get; set; }

        private bool canSetGroupName { get; set; } = true;

        protected double ExpConstant { get; set; }

        private SectionFlag AutomationFlag { get; set; }

        public SectionalBar()
        {
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(IsVisiblePropertyChanged);
        }

        public void SetInitState(double value, RoutedEventHandler routedEventHandler = null, bool visible = true)
        {
            if (routedEventHandler != null)
                ThresholdReached += routedEventHandler;
            Value = value;
            Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        static SectionalBar()
        {
            Dependency<SectionalBar>.DefaultOverrideMetadata(
                new Action<Type, PropertyMetadata>(DefaultStyleKeyProperty.OverrideMetadata));
            HeightProperty.OverrideMetadata(typeof(SectionalBar),
                Dependency<SectionalBar>.CreatePropertyMetadata(new PropertyChangedCallback(OnHeightPropertyChanged)));
            MaximumProperty.OverrideMetadata(typeof(SectionalBar),
                (PropertyMetadata)Dependency<SectionalBar>.CreatePropertyMetadata((object)double.MaxValue,
                    new PropertyChangedCallback(OnMaximumPropertyChanged)));
            MinimumProperty.OverrideMetadata(typeof(SectionalBar),
                (PropertyMetadata)Dependency<SectionalBar>.CreatePropertyMetadata((object)double.MaxValue,
                    new PropertyChangedCallback(OnMinimumPropertyChanged)));
        }

        public event RoutedEventHandler ThresholdReached
        {
            add => AddHandler(ThresholdReachedEvent, (Delegate)value);
            remove => RemoveHandler(ThresholdReachedEvent, (Delegate)value);
        }

        private void RaiseThresholdReachedEvent()
        {
            RaiseEvent(new RoutedEventArgs(ThresholdReachedEvent, (object)this));
        }

        private static List<SectionalBar> GetSections(string groupName)
        {
            if (groupSections.ContainsKey(groupName))
            {
                var groupSection = groupSections[groupName];
                if (groupSection.Count > 0)
                    return groupSection;
            }

            return (List<SectionalBar>)null;
        }

        public List<SectionalBar> Sections => GetSections(GroupName);

        public bool IsThresholdReached
        {
            get => (bool)GetValue(IsThresholdReachedProperty);
            private set => SetValue(IsThresholdReachedProperty, (object)value);
        }

        public double Threshold
        {
            get => (double)GetValue(ThresholdProperty);
            set => SetValue(ThresholdProperty, (object)value);
        }

        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, (object)value);
        }

        public int GroupCount
        {
            get => (int)GetValue(GroupCountProperty);
            set => SetValue(GroupCountProperty, (object)value);
        }

        public int GroupIndex
        {
            get => (int)GetValue(GroupIndexProperty);
            private set => SetValue(GroupIndexProperty, (object)value);
        }

        public bool IsFront
        {
            get => (bool)GetValue(IsFrontProperty);
            private set => SetValue(IsFrontProperty, (object)value);
        }

        public bool IsBack
        {
            get => (bool)GetValue(IsBackProperty);
            private set => SetValue(IsBackProperty, (object)value);
        }

        public CornerRadius FrontCornerRadius
        {
            get => (CornerRadius)GetValue(FrontCornerRadiusProperty);
            set => SetValue(FrontCornerRadiusProperty, (object)value);
        }

        public CornerRadius BackCornerRadius
        {
            get => (CornerRadius)GetValue(BackCornerRadiusProperty);
            set => SetValue(BackCornerRadiusProperty, (object)value);
        }

        public bool AutoCornerRadius
        {
            get => (bool)GetValue(AutoCornerRadiusProperty);
            set
            {
                if (value)
                    UpdateCornerRadius();
                SetValue(AutoCornerRadiusProperty, (object)value);
            }
        }

        public bool Dispose
        {
            get => (bool)GetValue(DisposeProperty);
            set => SetValue(DisposeProperty, (object)value);
        }

        public string GroupName
        {
            get => (string)GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, (object)value);
        }

        public double AutomationValue
        {
            get => (double)GetValue(AutomationValueProperty);
            set => SetValue(AutomationValueProperty, (object)value);
        }

        protected virtual void ExecutedRipple(double value)
        {
            UpdateValue(value);
        }

        protected virtual void UpdateValue(double value)
        {
            Value = value;
        }

        protected virtual void SetThresholdReached()
        {
            var sections = Sections;
            if (sections == null)
                return;
            foreach (var sectionalBar in sections)
                if (sectionalBar.GroupIndex == GroupIndex)
                    sectionalBar.IsThresholdReached = true;
            foreach (var sectionalBar in sections)
                if (sectionalBar.GroupIndex != GroupIndex)
                    sectionalBar.IsThresholdReached = false;
        }

        protected void UpdateCornerRadius()
        {
            if (!AutoCornerRadius)
                return;
            var num = Height > 2.0 ? Height / 2.0 : 0.0;
            FrontCornerRadius = new CornerRadius(num, 0.0, 0.0, num);
            BackCornerRadius = new CornerRadius(0.0, num, num, 0.0);
        }

        protected static void IndexSections(List<SectionalBar> sections)
        {
            var num = 0;
            foreach (var section in sections)
            {
                section.GroupIndex = num++;
                section.IsFront = false;
                section.IsBack = false;
            }

            sections[0].IsFront = true;
            sections[sections.Count - 1].IsBack = true;
        }

        protected static void Register(string groupName, SectionalBar sectionBar)
        {
            List<SectionalBar> sections;
            if (!groupSections.ContainsKey(groupName))
            {
                sections = new List<SectionalBar>();
                sections.Add(sectionBar);
                groupSections.Add(groupName, sections);
            }
            else
            {
                sections = groupSections[groupName];
                if (!sections.Contains(sectionBar))
                    sections.Add(sectionBar);
            }

            var count = sections?.Count;
            var groupCount = sectionBar.GroupCount;
            if (!((count.GetValueOrDefault() == groupCount) & count.HasValue))
                return;
            IndexSections(sections);
            sectionBar.InvalidateProperty(ValueProperty);
        }

        protected static void Register(SectionalBar sectionBar)
        {
            Register(sectionBar.GroupName, sectionBar);
        }

        protected static void Unregister(string groupName, SectionalBar sectionBar)
        {
            var sections = sectionBar.Sections;
            if (sections == null)
                return;
            if (sections.Contains(sectionBar))
                sections.Remove(sectionBar);
            if (sections.Count == 0)
                groupSections.Remove(groupName);
            else
                IndexSections(sections);
        }

        protected static void Unregister(SectionalBar sectionBar)
        {
            Unregister(sectionBar.GroupName, sectionBar);
        }

        protected static void CheckRegister(SectionalBar sectionBar)
        {
            var sections = sectionBar.Sections;
            var count = sections?.Count;
            var groupCount = sectionBar.GroupCount;
            if (!((count.GetValueOrDefault() == groupCount) & count.HasValue))
                return;
            sections.Clear();
            groupSections.Remove(sectionBar.GroupName);
        }

        private static bool IsValidDoubleValue(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static bool IsValidChangeValue(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0.0;
        }

        private static void OnGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sectionBar = (SectionalBar)d;
            if (!sectionBar.canSetGroupName)
                return;
            sectionBar.canSetGroupName = false;
            CheckRegister(sectionBar);
            Register(sectionBar);
        }

        private static void OnThresholdReachedPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            (d as SectionalBar).RaiseThresholdReachedEvent();
        }

        private static void OnHeightPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            (d as SectionalBar).UpdateCornerRadius();
        }

        private static void OnAutoCornerRadiusPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            (d as SectionalBar).UpdateCornerRadius();
        }

        private static void OnDisposePropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            Unregister(d as SectionalBar);
        }

        private static void OnMinimumPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            (d as SectionalBar).AutomationFlag |= SectionFlag.Minimum;
        }

        private static void OnMaximumPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            (d as SectionalBar).AutomationFlag |= SectionFlag.Maximum;
        }

        private static void OnThresholdPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
        }

        private static void OnGroupCountPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            (d as SectionalBar).AutomationFlag |= SectionFlag.Count;
        }

        private static void OnAutomationValueChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            (d as SectionalBar).AutomationFlag |= SectionFlag.AutoValue;
        }

        private void IsVisiblePropertyChanged(object s, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                OnIsVisiblePropertyChanged(this);
            else
                UpdateValue(Minimum);
        }

        private double GetMaxValue()
        {
            if (MaxValue != double.MaxValue && IsBack && MaxValue > Maximum && MaxValue > Threshold)
                ExpConstant = Maximum - Math.Log10(MaxValue - Threshold) / Math.Log10(2.0);
            return MaxValue;
        }

        private static void OnIsVisiblePropertyChanged(SectionalBar sectionBar)
        {
            if (running)
                return;
            var sections = sectionBar.Sections;
            if (sections == null || sections.Count != sectionBar.GroupCount)
                return;
            var flag = true;
            foreach (var sectionalBar in sections)
                if ((sectionalBar.AutomationFlag & SectionFlag.All) != SectionFlag.All)
                {
                    flag = false;
                    break;
                }

            if (!flag)
                return;
            running = true;
            RangeLimits = new List<RangeLimit>();
            foreach (var section in sections)
            {
                RangeLimits.Add(new RangeLimit(section)
                {
                    Value = section.AutomationValue,
                    MaxValue = section.GetMaxValue(),
                    Minimum = section.Minimum,
                    Maximum = section.Maximum,
                    Threshold = section.Threshold
                });
                section.IsVisibleChanged -= new DependencyPropertyChangedEventHandler(section.IsVisiblePropertyChanged);
            }

            for (var index = 1; index < RangeLimits.Count; ++index)
                RangeLimits[index - 1].Nextlimit = RangeLimits[index];
            RangeLimit.Run(RangeLimits[0]);
        }

        [Flags]
        private enum SectionFlag
        {
            None = 0,
            Minimum = 1,
            Maximum = 2,
            AutoValue = 4,
            Count = 8,
            All = Count | AutoValue | Maximum | Minimum // 0x0000000F
        }

        public class RangeLimit : IDisposable
        {
            private static BackgroundWorker BgWorker = new BackgroundWorker();
            private static RangeLimit ActiveItem;

            public SectionalBar Section { get; private set; }

            public double Value { get; set; }

            public double MaxValue { get; set; }

            public double Minimum { get; set; }

            public double Maximum { get; set; }

            public double Threshold { get; set; }

            public RangeLimit Nextlimit { get; set; }

            public bool canDispatch { get; set; } = true;

            public RangeLimit(SectionalBar section)
            {
                Section = section;
            }

            public void Dispose()
            {
                DisposeOfWorker();
            }

            public void UpdateThresholdReached(bool isThresholdReached)
            {
                if (!isThresholdReached || !canDispatch)
                    return;
                canDispatch = false;
                Application.Current.Dispatcher.Invoke((Action)(() => Section.SetThresholdReached()));
            }

            public void UpdateValue(double value)
            {
                Application.Current.Dispatcher.Invoke((Action)(() => Section.UpdateValue(value)));
            }

            public void ExecutedRipple(double value)
            {
                Application.Current.Dispatcher.Invoke((Action)(() => Section.ExecutedRipple(value)));
            }

            public static void DisposeOfWorker()
            {
                BgWorker.DoWork -= new DoWorkEventHandler(DoWorkEvent);
                BgWorker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(DoWorkEventCompleted);
                BgWorker.Dispose();
            }

            private static void DoWorkEventCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
                DisposeOfWorker();
                Application.Current.Dispatcher.Invoke((Action)(() => running = false));
            }

            public static void Run(RangeLimit rangeLimit)
            {
                ActiveItem = rangeLimit;
                BgWorker.DoWork += new DoWorkEventHandler(DoWorkEvent);
                BgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DoWorkEventCompleted);
                BgWorker.RunWorkerAsync();
            }

            private double GetValue(double index)
            {
                if (Section.ExpConstant > 0.0 && index > Threshold && Threshold != -1.0)
                {
                    if (Value > MaxValue)
                        Value = MaxValue;
                    if (Maximum != MaxValue)
                        Maximum = MaxValue;
                    index = Section.ExpConstant + Math.Log10(index - Threshold) / Math.Log10(2.0);
                }

                return index;
            }

            private static void DoWorkEvent(object sender, DoWorkEventArgs e)
            {
                for (; ActiveItem != null; ActiveItem = ActiveItem.Nextlimit)
                for (var minimum = ActiveItem.Minimum; minimum <= ActiveItem.Maximum; ++minimum)
                {
                    var num = ActiveItem.GetValue(minimum);
                    Thread.Sleep(20);
                    var isThresholdReached = num >= ActiveItem.Threshold || ActiveItem.Threshold == -1.0;
                    if (minimum >= ActiveItem.Value)
                    {
                        ActiveItem.ExecutedRipple(num);
                        ActiveItem.UpdateThresholdReached(isThresholdReached);
                        break;
                    }

                    ActiveItem.UpdateValue(num);
                    ActiveItem.UpdateThresholdReached(isThresholdReached);
                }
            }
        }
    }
}