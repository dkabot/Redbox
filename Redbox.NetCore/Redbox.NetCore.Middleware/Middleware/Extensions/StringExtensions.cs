using System.Linq;

namespace Redbox.NetCore.Middleware.Extensions
{
    public static class StringExtensions
    {
        public static string ServiceName(this string callerLocation)
        {
            if (!callerLocation.Contains("\\"))
            {
                var text = callerLocation.Split('/').LastOrDefault();
                if (text == null) return null;
                return text.TrimEnd('.', 'c', 's');
            }

            var text2 = callerLocation.Split('\\').LastOrDefault();
            if (text2 == null) return null;
            return text2.TrimEnd('.', 'c', 's');
        }
    }
}