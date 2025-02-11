using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Redbox.Macros
{
    [Serializable]
    public class EvaluatorException : ApplicationException
    {
        public EvaluatorException()
        {
        }

        public EvaluatorException(string message)
            : base(message)
        {
        }

        public EvaluatorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public EvaluatorException(string message, Location location)
            : base(message)
        {
            Location = location;
        }

        public EvaluatorException(string message, Location location, Exception innerException)
            : base(message, innerException)
        {
            Location = location;
        }

        protected EvaluatorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Location = info.GetValue(nameof(Location), Location.GetType()) as Location;
        }

        public override string Message
        {
            get
            {
                var message = base.Message;
                var empty = string.Empty;
                if (Location != null)
                    empty = Location.ToString();
                if (!string.IsNullOrEmpty(empty))
                    message = empty + Environment.NewLine + message;
                return message;
            }
        }

        public string RawMessage => base.Message;

        public Location Location { get; } = Location.UnknownLocation;

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Location", Location);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}:{1}{2}", Message, Environment.NewLine,
                base.ToString());
        }
    }
}