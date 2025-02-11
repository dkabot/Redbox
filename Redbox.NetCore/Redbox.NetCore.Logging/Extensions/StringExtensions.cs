namespace Redbox.NetCore.Logging.Extensions
{
    public static class StringExtensions
    {
        public static string MaskLogValue(this string source, int visibleChars)
        {
            if (source == null)
                return null;
            if (source.Equals(string.Empty))
                return string.Empty;
            return source.Length <= visibleChars
                ? new string('X', source.Length)
                : new string('X', source.Length - visibleChars) + source.Substring(source.Length - visibleChars);
        }
    }
}