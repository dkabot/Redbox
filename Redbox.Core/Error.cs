using System.Text;

namespace Redbox.Core
{
    public class Error
    {
        public string Code { get; set; }

        public string Description { get; set; }

        public string ExceptionType { get; set; }

        public string ExceptionMessage { get; set; }

        public string ExceptionStackTrace { get; set; }

        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append(Code).Append(" - ").Append(Description);
            AddExceptionPart(b, ExceptionType);
            AddExceptionPart(b, ExceptionMessage);
            AddExceptionPart(b, ExceptionStackTrace);
            return b.ToString();
        }

        private static void AddExceptionPart(StringBuilder b, string s)
        {
            if (string.IsNullOrEmpty(s))
                return;
            b.Append("; ").Append(s);
        }
    }
}