using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Redbox.Macros
{
    [Serializable]
    internal class EvaluatorException : ApplicationException
    {
        private Location _location = Location.UnknownLocation;

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
            this._location = location;
        }

        public EvaluatorException(string message, Location location, Exception innerException)
          : base(message, innerException)
        {
            this._location = location;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Location", (object)this._location);
        }

        public override string ToString()
        {
            return string.Format((IFormatProvider)CultureInfo.InvariantCulture, "{0}:{1}{2}", (object)this.Message, (object)Environment.NewLine, (object)base.ToString());
        }

        public override string Message
        {
            get
            {
                string message = base.Message;
                string empty = string.Empty;
                if (this._location != null)
                    empty = this._location.ToString();
                if (!string.IsNullOrEmpty(empty))
                    message = empty + Environment.NewLine + message;
                return message;
            }
        }

        public string RawMessage => base.Message;

        public Location Location => this._location;

        protected EvaluatorException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
            this._location = info.GetValue(nameof(Location), this._location.GetType()) as Location;
        }
    }
}
