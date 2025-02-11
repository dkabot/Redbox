using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace Redbox.UpdateManager.Remoting
{
    internal static class Extensions
    {
        public static void ToLogHelper(this ErrorList errors)
        {
            if (errors.Count <= 0)
                return;
            LogHelper.Instance.Log(errors.ToSingleString());
        }

        public static string ToSingleString(this ErrorList errors)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Redbox.UpdateManager.ComponentModel.Error error in (List<Redbox.UpdateManager.ComponentModel.Error>)errors)
            {
                stringBuilder.AppendLine(error.ToString());
                stringBuilder.AppendLine(error.Details);
            }
            return stringBuilder.ToString();
        }
    }
}
