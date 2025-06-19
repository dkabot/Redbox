using System;
using System.Windows;
using System.Windows.Threading;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "RemoveCardView")]
    public partial class RemoveCardViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        private const int _checkCardRemovedTimerIntervalMax = 5000;

        private readonly DispatcherTimer _checkCardRemovedTimer;

        private int _checkCardRemovedTimerInterval = 1000;

        private DispatcherTimer _timeOutTimer;

        public RemoveCardViewUserControl()
        {
            InitializeComponent();
            _checkCardRemovedTimer = new DispatcherTimer();
            SetCheckCardRemovedTimerInterval();
            _checkCardRemovedTimer.Tick += CheckCardRemovedTimerTick;
            StartCheckCardRemovedTimer();
        }

        private RemoveCardViewModel RemoveCardViewModel => DataContext as RemoveCardViewModel;

        public override ISpeechControl GetSpeechControl()
        {
            var removeCardViewModel = RemoveCardViewModel;
            if (removeCardViewModel == null) return null;
            return removeCardViewModel.OnGetSpeechControl();
        }

        private void SetCheckCardRemovedTimerInterval()
        {
            if (_checkCardRemovedTimer != null)
                _checkCardRemovedTimer.Interval = TimeSpan.FromMilliseconds(_checkCardRemovedTimerInterval);
        }

        private void StartCheckCardRemovedTimer()
        {
            if (_checkCardRemovedTimer != null) _checkCardRemovedTimer.Start();
        }

        private void StopCheckCardRemovedTimer()
        {
            if (_checkCardRemovedTimer != null) _checkCardRemovedTimer.Stop();
        }

        private void CheckCardRemovedTimerTick(object sender, EventArgs e)
        {
            StopCheckCardRemovedTimer();
            var removeCardViewModel = RemoveCardViewModel;
            if (removeCardViewModel == null) return;
            removeCardViewModel.StartCardRemovedYetCheck(RemoveCardViewModel);
        }

        private void ProcessGetCardRemovedCheckResult(bool cardWasRemoved)
        {
            if (!cardWasRemoved)
            {
                _checkCardRemovedTimerInterval = Math.Min(_checkCardRemovedTimerInterval + 1000, 5000);
                SetCheckCardRemovedTimerInterval();
                StartCheckCardRemovedTimer();
            }
        }

        private void TextToSpeechUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var removeCardViewModel = e.NewValue as RemoveCardViewModel;
            if (removeCardViewModel != null)
            {
                removeCardViewModel.ProcessCardRemovedCheckResult = ProcessGetCardRemovedCheckResult;
                removeCardViewModel.StopTimeOutTimerAction = StopTimeOutTimer;
                if (removeCardViewModel.Timeout > 0 && _timeOutTimer == null)
                {
                    _timeOutTimer = new DispatcherTimer();
                    _timeOutTimer.Interval = TimeSpan.FromMilliseconds(removeCardViewModel.Timeout);
                    _timeOutTimer.Tick += _timeOutTimer_Tick;
                    _timeOutTimer.Start();
                }
            }
        }

        private void _timeOutTimer_Tick(object sender, EventArgs e)
        {
            if (_timeOutTimer != null)
            {
                StopCheckCardRemovedTimer();
                StopTimeOutTimer();
                var removeCardViewModel = RemoveCardViewModel;
                if (removeCardViewModel == null) return;
                removeCardViewModel.CardNeverRemoved();
            }
        }

        private void StopTimeOutTimer()
        {
            if (_timeOutTimer != null) _timeOutTimer.Stop();
        }
    }
}