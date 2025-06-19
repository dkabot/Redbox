using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Controls
{
    public class TextToSpeechUserControl : RentalUserControl, ITextToSpeechControl
    {
        protected Dictionary<string, RoutedCommand> RoutedCommands { get; } = new Dictionary<string, RoutedCommand>();

        public virtual ISpeechControl GetSpeechControl()
        {
            return null;
        }

        public virtual string ControlText(string controlName)
        {
            return null;
        }

        public virtual void ExecuteCommand(string commandName, string parameter)
        {
            RoutedCommand routedCommand = null;
            if (RoutedCommands.TryGetValue(commandName, out routedCommand)) routedCommand.Execute(parameter, this);
        }

        public virtual bool IsControlEnabled(string controlName)
        {
            var uielement = GetUIElement(controlName);
            return uielement != null && uielement.IsEnabled;
        }

        public virtual bool IsControlVisible(string controlName)
        {
            var uielement = GetUIElement(controlName);
            return uielement != null && uielement.IsVisible;
        }

        public virtual void ToggleADALine()
        {
            var uielement = GetUIElement("adaLine");
            if (uielement == null)
            {
                uielement = new Line
                {
                    X1 = 0.0,
                    Y1 = 286.0,
                    Height = 3.0,
                    Width = 1024.0,
                    Stroke = new SolidColorBrush
                    {
                        Color = Colors.GreenYellow
                    },
                    StrokeThickness = 3.0,
                    Visibility = Visibility.Visible
                };
                AddChild(uielement);
                return;
            }

            uielement.Visibility = uielement.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
        }

        protected void RegisterRoutedCommand(string commandName, RoutedCommand command)
        {
            if (!RoutedCommands.Keys.Contains(commandName)) RoutedCommands.Add(commandName, command);
        }

        private UIElement GetUIElement(string controlName)
        {
            UIElement uielement = null;
            var obj = FindName(controlName);
            if (obj != null)
            {
                var uielement2 = obj as UIElement;
                if (uielement2 != null) uielement = uielement2;
            }

            return uielement;
        }
    }
}