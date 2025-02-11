using System.Text;

namespace Redbox.Core
{
    internal class Error
    {
        public string Code { get; set; }

        public string Description { get; set; }

        public string ExceptionType { get; set; }

        public string ExceptionMessage { get; set; }

        public string ExceptionStackTrace { get; set; }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.Append(this.Code).Append(" - ").Append(this.Description);
            Error.AddExceptionPart(b, this.ExceptionType);
            Error.AddExceptionPart(b, this.ExceptionMessage);
            Error.AddExceptionPart(b, this.ExceptionStackTrace);
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
