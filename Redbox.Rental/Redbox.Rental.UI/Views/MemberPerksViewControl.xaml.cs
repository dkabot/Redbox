using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Redbox.Controls;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "MemberPerksView")]
    public partial class MemberPerksViewControl : ViewUserControl
    {
        private readonly List<TierControl> PanelList = new List<TierControl>();

        public MemberPerksViewControl()
        {
            InitializeComponent();
            ApplyTheme<Control>(MainControl);
        }

        private MemberPerksModel Model => DataContext as MemberPerksModel;

        private Ellipse CreateBackgroundDots(StackPanel dots)
        {
            var thickness = new Thickness(1.0, 0.0, 1.0, 0.0);
            var solidColorBrush = new SolidColorBrush();
            solidColorBrush.Color = (Color)FindResource("RubineRed");
            for (var i = 0; i < 12; i++)
                dots.Children.Add(new Ellipse
                {
                    Width = 5.0,
                    Height = 5.0,
                    Fill = solidColorBrush,
                    Margin = thickness,
                    Opacity = 0.35
                });
            var ellipse = new Ellipse
            {
                Width = 10.0,
                Height = 10.0,
                Fill = solidColorBrush,
                Margin = thickness,
                Opacity = 0.35
            };
            dots.Children.Add(ellipse);
            for (var j = 0; j < 12; j++)
                dots.Children.Add(new Ellipse
                {
                    Width = 5.0,
                    Height = 5.0,
                    Fill = solidColorBrush,
                    Margin = thickness,
                    Opacity = 0.35
                });
            return ellipse;
        }

        protected override void DataContextHasChanged()
        {
        }

        private void SectionalBar_ThresholdReached(object sender, RoutedEventArgs e)
        {
            var sectionalBar = sender as SectionalBar;
            if (sectionalBar != null && sectionalBar.GroupIndex > 0 && DataContext is MemberPerksModel)
            {
                var memberPerksModel = DataContext as MemberPerksModel;
                var num = sectionalBar.GroupIndex - 1;
                var flag = true;
                var memberTier = memberPerksModel.MemberTier;
                if (memberTier != TierType.Member && memberTier - TierType.Star <= 2 && sectionalBar.Threshold == -1.0)
                {
                    flag = false;
                    if (sectionalBar.IsThresholdReached && num == memberPerksModel.MemberTier - TierType.Star)
                    {
                        var tierControl = PanelList[num];
                        tierControl.ImageElem.Opacity = 1.0;
                        tierControl.NameElem.Opacity = 1.0;
                    }
                }

                if (flag)
                {
                    var tierControl2 = PanelList[num];
                    if (sectionalBar.IsThresholdReached)
                    {
                        tierControl2.ImageElem.Opacity = 1.0;
                        tierControl2.NameElem.Opacity = 1.0;
                        tierControl2.TextElem.Opacity = 1.0;
                        tierControl2.EllipseElem.Opacity = 1.0;
                        return;
                    }

                    tierControl2.ImageElem.Opacity = 0.35;
                    tierControl2.NameElem.Opacity = 0.35;
                    tierControl2.TextElem.Opacity = 0.35;
                }
            }
        }

        private UIElement InitOpacity(UIElement element)
        {
            if (element != null) element.Opacity = 0.35;
            return element;
        }

        private void TierControl_Loaded(object sender, RoutedEventArgs e)
        {
            var children = (sender as GridExt).Children;
            var tierControl = new TierControl(this)
            {
                ImageElem = (Image)InitOpacity(children[0]),
                NameElem = (TextBlock)InitOpacity(children[1]),
                TextElem = (TextBlock)InitOpacity(children[3]),
                PanelElem = (StackPanel)children[2]
            };
            tierControl.EllipseElem.Opacity = 0.35;
            PanelList.Add(tierControl);
        }

        private void BarControl_Loaded(object sender, RoutedEventArgs e)
        {
            ((sender as GridExt).Children[0] as SectionalBar).SetInitState(0.0, SectionalBar_ThresholdReached);
        }

        private void SnapshotButton_Pressed(object sender, RoutedEventArgs e)
        {
            HandleWPFHit();
            if (Model != null) InvokeRoutedCommand(Model.SelectedButtonCommand, "SnapshotButton");
        }

        private void RedboxPlusButton_Pressed(object sender, RoutedEventArgs e)
        {
            HandleWPFHit();
            if (Model != null) InvokeRoutedCommand(Model.SelectedButtonCommand, "RedboxPlusButton");
        }

        private void PerksButton_Pressed(object sender, RoutedEventArgs e)
        {
            HandleWPFHit();
            if (Model != null) InvokeRoutedCommand(Model.SelectedButtonCommand, "PerksButton");
        }

        private void PromosButton_Pressed(object sender, RoutedEventArgs e)
        {
            HandleWPFHit();
            if (Model != null) InvokeRoutedCommand(Model.SelectedButtonCommand, "PromosButton");
        }

        public sealed class TierControl
        {
            private StackPanel _panelElem;

            public TierControl(MemberPerksViewControl perksView)
            {
                PerksView = perksView;
            }

            private MemberPerksViewControl PerksView { get; }

            public Image ImageElem { get; set; }

            public TextBlock NameElem { get; set; }

            public TextBlock TextElem { get; set; }

            public Ellipse EllipseElem { get; set; }

            public StackPanel PanelElem
            {
                get => _panelElem;
                set
                {
                    if (value != _panelElem)
                    {
                        var perksView = PerksView;
                        _panelElem = value;
                        EllipseElem = perksView.CreateBackgroundDots(value);
                    }
                }
            }
        }
    }
}