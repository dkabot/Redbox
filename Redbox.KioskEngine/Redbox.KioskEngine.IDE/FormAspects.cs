using Redbox.KioskEngine.ComponentModel;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.KioskEngine.IDE
{
    public static class FormAspects
    {
        public static void ActsAsPersistent(Form form, IUserSettingsStore store)
        {
            if (form == null || store == null)
                return;
            form.Load += (EventHandler)((_param1, _param2) =>
            {
                var str = store.GetValue<string>("Shell", string.Format("{0}_State", (object)form.GetType().FullName));
                if (str == null)
                    return;
                var strArray = str.Split("|".ToCharArray());
                if (strArray.Length != 3)
                    return;
                if (strArray[0] == "Maximized")
                {
                    form.WindowState = FormWindowState.Maximized;
                }
                else
                {
                    var converter = TypeDescriptor.GetConverter(typeof(Size));
                    var rect = new Rectangle(
                        (Point)TypeDescriptor.GetConverter(typeof(Point)).ConvertFromString(strArray[1]),
                        (Size)converter.ConvertFromString(strArray[2]));
                    var flag = false;
                    foreach (var allScreen in Screen.AllScreens)
                        if (allScreen.WorkingArea.IntersectsWith(rect))
                        {
                            flag = true;
                            break;
                        }

                    form.Bounds = flag ? rect : Screen.PrimaryScreen.Bounds;
                }
            });
            form.FormClosing += (FormClosingEventHandler)((_param1, _param2) => store.SetValue<string>("Shell",
                string.Format("{0}_State", (object)form.GetType().FullName),
                string.Format("{0}|{1}, {2}|{3}, {4}", (object)form.WindowState, (object)form.Location.X,
                    (object)form.Location.Y, (object)form.Size.Width, (object)form.Size.Height)));
        }
    }
}