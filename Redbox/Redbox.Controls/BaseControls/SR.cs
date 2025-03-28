using System;
using System.Globalization;
using System.Resources;

namespace Redbox.Controls.BaseControls
{
    internal static class SR
    {
        private static ResourceManager _resourceManager =
            new ResourceManager("ExceptionStringTable", typeof(SR).Assembly);

        internal static string Get(string id)
        {
            return _resourceManager.GetString(id) ?? _resourceManager.GetString("Unavailable");
        }

        internal static string Get(string id, params object[] args)
        {
            var format = _resourceManager.GetString(id);
            if (format == null)
                format = _resourceManager.GetString("Unavailable");
            else if (args != null && args.Length != 0)
                format = string.Format((IFormatProvider)CultureInfo.CurrentCulture, format, args);
            return format;
        }

        internal static ResourceManager ResourceManager => _resourceManager;
    }
}