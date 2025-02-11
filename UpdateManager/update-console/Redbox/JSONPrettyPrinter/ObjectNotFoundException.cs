using System;
using System.Runtime.Serialization;

namespace Redbox.JSONPrettyPrinter
{
    [Serializable]
    internal sealed class ObjectNotFoundException : Exception
    {
        public ObjectNotFoundException()
          : this((string)null)
        {
        }

        public ObjectNotFoundException(string message)
          : this(message, (Exception)null)
        {
        }

        public ObjectNotFoundException(string message, Exception innerException)
          : base(message ?? "Object not found.", innerException)
        {
        }

        private ObjectNotFoundException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }
    }
}
