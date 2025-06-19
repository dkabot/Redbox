using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "MessagePopupViewUserControl")]
    public partial class MessagePopupViewUserControl : TextToSpeechUserControl
    {
        private readonly object m_lockObject = new object();

        private DispatcherTimer m_dispatcherTimer;

        private int m_timeout;

        private bool m_timerDisabled;

        public MessagePopupViewUserControl()
        {
            InitializeComponent();
            ApplyTheme();
            RegisterRoutedCommand("Button1", MessagePopupViewCommands.Button1);
            RegisterRoutedCommand("Button2", MessagePopupViewCommands.Button2);
            var service = ServiceLocator.Instance.GetService<IViewService>();
            var viewFrameInstance = service != null ? service.PeekViewFrame() : null;
            ViewFrameId = viewFrameInstance != null ? new Guid?(viewFrameInstance.Id) : null;
        }

        private Guid? ViewFrameId { get; }

        private int Timeout
        {
            get => m_timeout;
            set
            {
                m_timeout = value;
                if (m_dispatcherTimer != null) m_dispatcherTimer.Stop();
                m_dispatcherTimer = new DispatcherTimer();
                m_dispatcherTimer.Interval = new TimeSpan(m_timeout * 10000);
                m_dispatcherTimer.Tick += TimerTick;
                if (m_timeout > 0) m_dispatcherTimer.Start();
            }
        }

        private MessagePopupViewModel Model => DataContext as MessagePopupViewModel;

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme != null) theme.SetStyle(Button1);
            if (theme != null) theme.SetStyle(FloatingButton1);
            if (theme != null) theme.SetStyle(Button2);
            if (theme == null) return;
            theme.SetStyle(FloatingButton2);
        }

        public void StopTimer()
        {
            Model.OnTimeOut = null;
            StopTimer(false);
        }

        public void ReenableTimer()
        {
            m_timerDisabled = false;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            var service = ServiceLocator.Instance.GetService<IViewService>();
            var viewFrameInstance = service != null ? service.PeekViewFrame() : null;
            var flag = (viewFrameInstance != null ? new Guid?(viewFrameInstance.Id) : null) == ViewFrameId;
            if (!flag)
            {
                string text;
                if (viewFrameInstance == null)
                {
                    text = null;
                }
                else
                {
                    var viewFrame = viewFrameInstance.ViewFrame;
                    text = viewFrame != null ? viewFrame.ViewName : null;
                }

                var text2 = text ?? "Unknown View";
                LogHelper.Instance.Log(string.Format("message_popup_view time out when on {0}", text2));
            }

            StopTimer(flag);
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
                var model = Model;
                if ((model != null ? model.OnTimeOut : null) != null) Model.OnTimeOut();
            }
        }

        private void Button1CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RunCommandOnce(delegate
            {
                try
                {
                    if (!IsMultiClickBlocked)
                    {
                        IsMultiClickBlocked = true;
                        StopTimer();
                        var model = Model;
                        if ((model != null ? model.OnButton1Clicked : null) != null) Model.OnButton1Clicked();
                    }
                }
                finally
                {
                    CompleteRunOnce();
                }
            });
        }

        private void Button2CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RunCommandOnce(delegate
            {
                try
                {
                    if (!IsMultiClickBlocked)
                    {
                        IsMultiClickBlocked = true;
                        StopTimer();
                        var model = Model;
                        if ((model != null ? model.OnButton2Clicked : null) != null) Model.OnButton2Clicked();
                    }
                }
                finally
                {
                    CompleteRunOnce();
                }
            });
        }

        private void TextToSpeechUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Model != null)
            {
                Timeout = Model.TimeOut;
                if (Model.ContentUserControl != null)
                {
                    if (Model.AutoSizedGridVisibility == Visibility.Visible)
                    {
                        AutoSizeGrid_UserControlContainer.Children.Clear();
                        AutoSizeGrid_UserControlContainer.Children.Add(Model.ContentUserControl);
                    }
                    else
                    {
                        UserControlContainer.Children.Clear();
                        UserControlContainer.Children.Add(Model.ContentUserControl);
                    }
                }

                if (string.IsNullOrEmpty(Model.Button1Text))
                {
                    ButtonGrid.ColumnDefinitions[0].Width = new GridLength(0.0);
                    FloatingButtonGrid.ColumnDefinitions[0].Width = new GridLength(0.0);
                    AutoSizeGrid_ButtonGrid.ColumnDefinitions[0].Width = new GridLength(0.0);
                }

                if (string.IsNullOrEmpty(Model.Button2Text))
                {
                    ButtonGrid.ColumnDefinitions[1].Width = new GridLength(0.0);
                    FloatingButtonGrid.ColumnDefinitions[1].Width = new GridLength(0.0);
                    AutoSizeGrid_ButtonGrid.ColumnDefinitions[1].Width = new GridLength(0.0);
                }
            }
        }

        public override ISpeechControl GetSpeechControl()
        {
            var model = Model;
            if (model == null) return null;
            return model.OnGetSpeechControl();
        }
    }
}