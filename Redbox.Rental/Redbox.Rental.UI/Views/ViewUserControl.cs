using System.Windows;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    public class ViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public ViewUserControl()
        {
            DataContextChanged += DataContextHasChanged;
        }

        protected string SpeechViewName { get; set; }

        protected string SimpleSpeechPart => "SimpleSpeechPart";

        protected bool NeedKeyboardFocus { get; set; }

        protected void ApplyTheme<T>(UIElement mainControl) where T : UIElement
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            var enumerable = FindLogicalChildren<T>(mainControl);
            if (enumerable != null)
                foreach (var t in enumerable)
                    if (theme != null)
                        theme.SetStyle(t as FrameworkElement);
        }

        protected TModel GetModel<TModel>()
        {
            return (TModel)DataContext;
        }

        private void DataContextHasChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DataContextHasChanged();
        }

        protected virtual void DataContextHasChanged()
        {
        }

        protected virtual void VisibleHasChanged()
        {
        }

        protected void InitVisibilityHasChanged()
        {
            IsVisibleChanged += delegate
            {
                VisibleHasChanged();
                if (NeedKeyboardFocus)
                {
                    Focusable = true;
                    Keyboard.Focus(this);
                }
            };
        }

        protected void InitKeyboardFocus(KeyEventHandler keyboardAction)
        {
            InitVisibilityHasChanged();
            if (keyboardAction != null)
            {
                KeyDown += keyboardAction;
                NeedKeyboardFocus = true;
            }
        }

        protected virtual void InitSpeechPart(ISpeechPart speechPart)
        {
        }

        protected virtual ISpeechControl CreateSimpleSpeechControl()
        {
            var speechControl = new SpeechControl();
            ((ISpeechControl)speechControl).Name = string.Format("tts_{0}", SpeechViewName);
            ISpeechPart speechPart = new SpeechPart
            {
                Sequence = 1,
                Name = SimpleSpeechPart,
                StartPause = 100
            };
            speechPart.Refresh = delegate
            {
                speechPart.Clear();
                InitSpeechPart(speechPart);
            };
            ((ISpeechControl)speechControl).SpeechParts.Add(speechPart);
            return speechControl;
        }

        public override ISpeechControl GetSpeechControl()
        {
            ISpeechControl speechControl;
            if (SpeechViewName != null)
            {
                speechControl = CreateSimpleSpeechControl();
            }
            else
            {
                var baseModel = DataContext as IBaseModel;
                speechControl = baseModel != null && baseModel.ProcessGetSpeechControl != null
                    ? baseModel.ProcessGetSpeechControl()
                    : null;
            }

            return speechControl;
        }

        protected static void InvokeRoutedCommand(DynamicRoutedCommand routeCommand, object parameter = null)
        {
            if (routeCommand != null && routeCommand.CanExecute(parameter)) routeCommand.Execute(parameter);
        }

        protected static void InvokeRoutedCommand(RoutedCommand routeCommand, IInputElement target,
            object parameter = null)
        {
            if (routeCommand != null && routeCommand.CanExecute(parameter, target))
                routeCommand.Execute(parameter, target);
        }
    }
}