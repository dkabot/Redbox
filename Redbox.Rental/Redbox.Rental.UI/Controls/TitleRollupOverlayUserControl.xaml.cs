using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public partial class TitleRollupOverlayUserControl : UserControl
    {
        private readonly object _lockObject = new object();

        private DispatcherTimer _dispatcherTimer;

        private bool _timerDisabled;

        public TitleRollupOverlayUserControl()
        {
            InitializeComponent();
            ApplyStyle();
        }

        private TimeSpan Timeout { get; set; }

        public void ApplyStyle()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme != null) theme.SetStyle(DVDButton);
            if (theme != null) theme.SetStyle(BlurayButton);
            if (theme != null) theme.SetStyle(_4kUhdButton);
            if (theme == null) return;
            theme.SetStyle(CancelButton);
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            StopTimer(false);
            var titleRollupOverlayModel = DataContext as TitleRollupOverlayModel;
            if (titleRollupOverlayModel != null) Timeout = titleRollupOverlayModel.Timeout;
        }

        private void StartTimer()
        {
            LogHelper.Instance.Log("Starting TitleRollupOverlay timer");
            if (Timeout != TimeSpan.Zero)
            {
                var lockObject = _lockObject;
                lock (lockObject)
                {
                    if (_dispatcherTimer != null) _dispatcherTimer.Stop();
                    _timerDisabled = false;
                    _dispatcherTimer = new DispatcherTimer();
                    _dispatcherTimer.Interval = Timeout;
                    _dispatcherTimer.Tick += TimerTick;
                    _dispatcherTimer.Start();
                }
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            StopTimer(true);
        }

        public void StopTimer(bool executeTimeoutEvent)
        {
            var flag = false;
            var lockObject = _lockObject;
            lock (lockObject)
            {
                var dispatcherTimer = _dispatcherTimer;
                if (dispatcherTimer != null) dispatcherTimer.Stop();
                if (!_timerDisabled)
                {
                    LogHelper.Instance.Log("Stopping TitleRollupOverlay timer");
                    _timerDisabled = true;
                    if (executeTimeoutEvent) flag = true;
                }
            }

            if (flag)
            {
                LogHelper.Instance.Log("TitleRollupOverlay timed out");
                var titleRollupOverlayModel = DataContext as TitleRollupOverlayModel;
                if (titleRollupOverlayModel != null)
                {
                    titleRollupOverlayModel.Visibility = Visibility.Collapsed;
                    titleRollupOverlayModel.ProcessTitleRollupOverlayTimeout(titleRollupOverlayModel.BrowseItemModel,
                        null);
                }
            }
        }

        private void CancelTitleRollupOverlayCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StopTimer(false);
            var titleRollupOverlayModel = DataContext as TitleRollupOverlayModel;
            if (titleRollupOverlayModel != null)
            {
                titleRollupOverlayModel.Visibility = Visibility.Collapsed;
                titleRollupOverlayModel.ProcessTitleRollupOverlayCancel(titleRollupOverlayModel.BrowseItemModel,
                    e.Parameter);
            }
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                StartTimer();
                Opacity = 0.0;
                AnimateOpacityChange();
            }
            else
            {
                StopTimer(false);
                Opacity = 1.0;
            }

            var titleRollupOverlayModel = DataContext as TitleRollupOverlayModel;
            if (titleRollupOverlayModel != null) titleRollupOverlayModel.ProcessTitleRollupVisibilityChange(Visibility);
        }

        private void AnimateOpacityChange()
        {
            var doubleAnimation = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(0.1)));
            BeginAnimation(OpacityProperty, doubleAnimation);
        }

        private void AddDVDCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StopTimer(false);
            if (CanExecuteAddDVDCommand())
            {
                var titleRollupOverlayModel = DataContext as TitleRollupOverlayModel;
                if (titleRollupOverlayModel != null)
                {
                    titleRollupOverlayModel.Visibility = Visibility.Collapsed;
                    titleRollupOverlayModel.ProcessTitleRollupOverlayAddDVD(titleRollupOverlayModel.BrowseItemModel,
                        e.Parameter);
                    Commands.ResetIdleTimerCommand.Execute(null, this);
                }
            }
        }

        private void AddDVDCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanExecuteAddDVDCommand();
        }

        private bool CanExecuteAddDVDCommand()
        {
            var flag = false;
            var titleRollupOverlayModel = DataContext as TitleRollupOverlayModel;
            if (titleRollupOverlayModel != null && titleRollupOverlayModel.DVDButtonEnabled) flag = true;
            return flag;
        }

        private void AddBlurayCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanExecuteAddBlurayCommand();
        }

        private bool CanExecuteAddBlurayCommand()
        {
            var flag = false;
            var titleRollupOverlayModel = DataContext as TitleRollupOverlayModel;
            if (titleRollupOverlayModel != null && titleRollupOverlayModel.BlurayButtonEnabled) flag = true;
            return flag;
        }

        private void AddBlurayCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StopTimer(false);
            if (CanExecuteAddBlurayCommand())
            {
                var titleRollupOverlayModel = DataContext as TitleRollupOverlayModel;
                if (titleRollupOverlayModel != null)
                {
                    titleRollupOverlayModel.Visibility = Visibility.Collapsed;
                    titleRollupOverlayModel.ProcessTitleRollupOverlayAddBluray(titleRollupOverlayModel.BrowseItemModel,
                        e.Parameter);
                    Commands.ResetIdleTimerCommand.Execute(null, this);
                }
            }
        }

        private void Add4kUhdCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanExecuteAdd4kUhdCommand();
        }

        private bool CanExecuteAdd4kUhdCommand()
        {
            var flag = false;
            var titleRollupOverlayModel = DataContext as TitleRollupOverlayModel;
            if (titleRollupOverlayModel != null && titleRollupOverlayModel._4kUhdButtonEnabled) flag = true;
            return flag;
        }

        private void Add4kUhdCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StopTimer(false);
            if (CanExecuteAdd4kUhdCommand())
            {
                var titleRollupOverlayModel = DataContext as TitleRollupOverlayModel;
                if (titleRollupOverlayModel != null)
                {
                    titleRollupOverlayModel.Visibility = Visibility.Collapsed;
                    titleRollupOverlayModel.ProcessTitleRollupOverlayAdd4kUhd(titleRollupOverlayModel.BrowseItemModel,
                        e.Parameter);
                    Commands.ResetIdleTimerCommand.Execute(null, this);
                }
            }
        }
    }
}