using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Controls.Animations;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "SwipeView")]
    public partial class SwipeViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        private readonly object m_lockObject = new object();

        private Dictionary<SwipeViewType, Type> _swipeViewTypeAnimationControls;

        private DispatcherTimer m_dispatcherTimer;

        private int m_timeout;

        private bool m_timerDisabled;

        public SwipeViewUserControl()
        {
            InitializeComponent();
            InitializeSwipeViewTypeAnimationControls();
            RegisterRoutedCommand("GoBack", SwipeViewCommands.GoBack);
        }

        public SwipeViewType SwipeType
        {
            get
            {
                var swipeViewType = SwipeViewType.RegularCheckOut;
                if (AnimationContainer.Children.Count > 0)
                    foreach (var keyValuePair in _swipeViewTypeAnimationControls)
                        if (keyValuePair.Value == AnimationContainer.Children[0].GetType())
                            swipeViewType = keyValuePair.Key;

                return swipeViewType;
            }
            set
            {
                AnimationContainer.Children.Clear();
                var obj = Activator.CreateInstance(_swipeViewTypeAnimationControls[value]);
                if (obj == null)
                {
                    LogHelper.Instance.Log("Error: Unable to instanciate instance of {0}",
                        _swipeViewTypeAnimationControls[value]);
                    return;
                }

                var frameworkElement = obj as FrameworkElement;
                if (frameworkElement != null)
                {
                    frameworkElement.VerticalAlignment = VerticalAlignment.Center;
                    AnimationContainer.Children.Add(frameworkElement);
                    return;
                }

                LogHelper.Instance.Log("Error: {0} is not a UIElement", _swipeViewTypeAnimationControls[value]);
            }
        }

        public int Timeout
        {
            get => m_timeout;
            set
            {
                m_timeout = value;
                if (m_dispatcherTimer != null) m_dispatcherTimer.Stop();
                m_dispatcherTimer = new DispatcherTimer();
                m_dispatcherTimer.Interval = new TimeSpan(m_timeout * 10000);
                m_dispatcherTimer.Tick += TimerTick;
                m_dispatcherTimer.Start();
            }
        }

        private ISwipeAnimationControl SwipeAnimationControl
        {
            get
            {
                ISwipeAnimationControl swipeAnimationControl = null;
                if (AnimationContainer.Children.Count > 0)
                    swipeAnimationControl = AnimationContainer.Children[0] as ISwipeAnimationControl;
                return swipeAnimationControl;
            }
        }

        private SwipeViewModel SwipeViewModel => DataContext as SwipeViewModel;

        public override ISpeechControl GetSpeechControl()
        {
            var swipeViewModel = SwipeViewModel;
            if (swipeViewModel == null) return null;
            return swipeViewModel.OnGetSpeechControl();
        }

        public void StopTimer()
        {
            StopTimer(false);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            StopTimer(true);
        }

        private void StopTimer(bool executeTimeoutEvent)
        {
            var flag = false;
            var lockObject = m_lockObject;
            lock (lockObject)
            {
                var dispatcherTimer = m_dispatcherTimer;
                if (dispatcherTimer != null) dispatcherTimer.Stop();
                if (!m_timerDisabled)
                {
                    m_timerDisabled = true;
                    if (executeTimeoutEvent) flag = true;
                }
            }

            if (flag)
            {
                var swipeViewModel = SwipeViewModel;
                if (swipeViewModel == null) return;
                var onTimeout = swipeViewModel.OnTimeout;
                if (onTimeout == null) return;
                onTimeout();
            }
        }

        private void GoBackCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StopTimer(false);
            var swipeViewModel = SwipeViewModel;
            if (swipeViewModel != null) swipeViewModel.ProcessOnBackButtonClicked();
            HandleWPFHit();
        }

        private void InitializeSwipeViewTypeAnimationControls()
        {
            _swipeViewTypeAnimationControls = new Dictionary<SwipeViewType, Type>
            {
                {
                    SwipeViewType.RegularCheckOut,
                    typeof(SwipeRegularCheckOutUserControl)
                },
                {
                    SwipeViewType.ReservationPickup,
                    typeof(SwipeRegularCheckOutUserControl)
                },
                {
                    SwipeViewType.EncryptedSwipeReservationPickup,
                    typeof(EncryptedSwipeReservationPickupUserControl)
                },
                {
                    SwipeViewType.EncryptedSwipeRegularCheckOut,
                    typeof(EncryptedSwipeRegularCheckoutUserControl)
                },
                {
                    SwipeViewType.EMVInsertTapRegularCheckout,
                    typeof(EMVInsertAndTapRegularCheckoutUserControl)
                },
                {
                    SwipeViewType.EMVInsertTapHideMobileRegularCheckout,
                    typeof(EMVInsertAndTapHideMobileRegularCheckoutUserControl)
                },
                {
                    SwipeViewType.EMVInsertTapReservationPickup,
                    typeof(EMVInsertAndTapHideMobilePickupUserControl)
                },
                {
                    SwipeViewType.EMVTapRegularCheckout,
                    typeof(EMVTapRegularCheckoutUserControl)
                },
                {
                    SwipeViewType.EMVTapReservationPickup,
                    typeof(EMVTapRegularCheckoutUserControl)
                },
                {
                    SwipeViewType.EMVInsertRegularCheckout,
                    typeof(EMVInsertRegularCheckoutUserControl)
                },
                {
                    SwipeViewType.EMVInsertReservationPickup,
                    typeof(EMVInsertRegularCheckoutUserControl)
                },
                {
                    SwipeViewType.EMVTapToLogin,
                    typeof(EMVTapSignInUserControl)
                }
            };
        }
    }
}