using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;
using Outerwall.Shell.Interop;

namespace Outerwall.Shell
{
    internal class LowLevelKeyboardHook : WindowsHook
    {
        public LowLevelKeyboardHook()
            : base(13)
        {
        }

        public event EventHandler<KeyPressEventEventArgs> KeyPressEvent;

        protected override bool HookFunction(int code, IntPtr wParam, IntPtr lParam)
        {
            var keyPressEvent = KeyPressEvent;
            if (keyPressEvent != null)
            {
                var structure = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                int num;
                switch (wParam.ToInt32())
                {
                    case 256:
                    case 260:
                        num = 1;
                        break;
                    case 257:
                    case 261:
                        num = 2;
                        break;
                    default:
                        num = 0;
                        break;
                }

                var keyState = (KeyState)num;
                if (keyState != KeyState.Unknown)
                {
                    var int32 = Convert.ToInt32(structure.scanCode);
                    var vkCode = (Keys)structure.vkCode;
                    var key = structure.vkCode < int.MaxValue
                        ? KeyInterop.KeyFromVirtualKey(Convert.ToInt32(structure.vkCode))
                        : Key.None;
                    switch (key)
                    {
                        case Key.LWin:
                        case Key.RWin:
                        case Key.LeftShift:
                        case Key.RightShift:
                        case Key.LeftCtrl:
                        case Key.RightCtrl:
                        case Key.LeftAlt:
                        case Key.RightAlt:
                            key = Key.None;
                            break;
                    }

                    var modifierKeys = ModifierKeys.None | TestAlt(ref vkCode) | TestCtrl(ref vkCode) |
                                       TestShift(ref vkCode) | TestWindows(ref vkCode);
                    var e = new KeyPressEventEventArgs(keyState, int32, key, modifierKeys);
                    keyPressEvent(this, e);
                    return !e.Handled;
                }
            }

            return true;
        }

        private static ModifierKeys TestAlt(ref Keys keys)
        {
            if ((keys & Keys.Alt) == Keys.Alt)
            {
                keys &= ~Keys.Alt;
                return ModifierKeys.Alt;
            }

            if (keys != Keys.Menu && keys != Keys.LMenu && keys != Keys.RMenu)
                return ModifierKeys.None;
            keys = Keys.None;
            return ModifierKeys.Alt;
        }

        private static ModifierKeys TestCtrl(ref Keys keys)
        {
            if ((keys & Keys.Control) == Keys.Control)
            {
                keys &= ~Keys.Control;
                return ModifierKeys.Control;
            }

            if (keys != Keys.ControlKey && keys != Keys.LControlKey && keys != Keys.RControlKey)
                return ModifierKeys.None;
            keys = Keys.None;
            return ModifierKeys.Control;
        }

        private static ModifierKeys TestShift(ref Keys keys)
        {
            if ((keys & Keys.Shift) == Keys.Shift)
            {
                keys &= ~Keys.Shift;
                return ModifierKeys.Shift;
            }

            if (keys != Keys.ShiftKey && keys != Keys.LShiftKey && keys != Keys.RShiftKey)
                return ModifierKeys.None;
            keys = Keys.None;
            return ModifierKeys.Shift;
        }

        private static ModifierKeys TestWindows(ref Keys keys)
        {
            if (keys != Keys.LWin && keys != Keys.RWin)
                return ModifierKeys.None;
            keys = Keys.None;
            return ModifierKeys.Windows;
        }
    }
}