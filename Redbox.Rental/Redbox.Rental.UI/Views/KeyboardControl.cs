using Redbox.Controls;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    public class KeyboardControl : ViewUserControl
    {
        public KeyboardMode KeyboardMode { get; protected set; }

        protected KeyboardMode LastKeyMode { get; set; }

        protected KeyboardUserControl KeyboardUserControl { get; set; }

        protected BorderButton BorderControlElem { get; set; }

        protected KeyboardModel Model => DataContext as KeyboardModel;

        protected void InitKeyboardUserControl(KeyboardUserControl keyboardUserControl, BorderButton borderControlElem)
        {
            KeyboardUserControl = keyboardUserControl;
            BorderControlElem = borderControlElem;
            keyboardUserControl.KeyTouched += KeyTouched;
            keyboardUserControl.KeyPressed += KeyPressed;
        }

        protected virtual void KeyTouched()
        {
            if (BorderControlElem.ButtonState != 0) BorderControlElem.ButtonState = 0;
        }

        protected virtual void KeyPressed(SymbolMode mode)
        {
            var lastKeyMode = LastKeyMode;
            LastKeyMode = KeyboardUserControl.KeyboardMode;
            if (mode == SymbolMode.EMAIL_LETTER_KEY_PRESSED)
            {
                KeyboardUserControl.KeyboardMode = lastKeyMode;
                return;
            }

            if (mode != SymbolMode.EMAIL_SYMBOL_KEY_PRESSED) return;
            KeyboardUserControl.KeyboardMode = KeyboardMode.EMAIL_SYMBOLS;
        }

        protected virtual void ContinueTouched()
        {
            if (Model.ContinueButtonCommand.CanExecute(null)) Model.ContinueButtonCommand.Execute(null);
            var model = Model;
            if (!string.IsNullOrEmpty(model != null ? model.KeyboardError : null)) BorderControlElem.ButtonState = 1;
        }
    }
}