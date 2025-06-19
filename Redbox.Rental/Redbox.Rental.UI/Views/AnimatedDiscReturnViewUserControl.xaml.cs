using System;
using System.Windows.Input;
using System.Windows.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "AnimatedDiscReturnViewUserControl")]
    public partial class AnimatedDiscReturnViewUserControl : TextToSpeechUserControl
    {
        private readonly object _lockObject = new object();

        private DispatcherTimer _dispatcherTimer;

        private int _timeout;

        private bool _timerDisabled;

        public AnimatedDiscReturnViewUserControl()
        {
            InitializeComponent();
            ApplyTheme();
            RegisterRoutedCommand("GotItButton", AnimatedDiscReturnCommands.GotItButton);
        }

        private Guid? ViewFrameId { get; set; }

        public int Timeout
        {
            get => _timeout;
            set
            {
                _timeout = value;
                var dispatcherTimer = _dispatcherTimer;
                if (dispatcherTimer != null) dispatcherTimer.Stop();
                _dispatcherTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(_timeout)
                };
                _dispatcherTimer.Tick += TimerTick;
                _dispatcherTimer.Start();
            }
        }

        private AnimatedDiscReturnViewModel Model => DataContext as AnimatedDiscReturnViewModel;

        public override ISpeechControl GetSpeechControl()
        {
            var model = Model;
            if (model == null) return null;
            return model.OnGetSpeechControl();
        }

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme == null) return;
            theme.SetStyle(GotItButton);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            StopTimer(true);
        }

        private void StopTimer(bool executeTimeoutEvent = false)
        {
            var flag = false;
            var lockObject = _lockObject;
            lock (lockObject)
            {
                var dispatcherTimer = _dispatcherTimer;
                if (dispatcherTimer != null) dispatcherTimer.Stop();
                if (!_timerDisabled)
                {
                    _timerDisabled = true;
                    if (executeTimeoutEvent) flag = true;
                }
            }

            if (flag)
            {
                var model = Model;
                if (model == null) return;
                model.ProcessOnTimeout();
            }
        }

        private void GotItButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StopTimer();
            var model = Model;
            if (model != null) model.ProcessOnGotItButtonClicked();
            HandleWPFHit();
        }
    }
}