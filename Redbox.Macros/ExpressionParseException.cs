using System;
using System.Runtime.Serialization;

namespace Redbox.Macros
{
    [Serializable]
    public class ExpressionParseException : Exception
    {
        public ExpressionParseException()
        {
        }

        public ExpressionParseException(string message)
            : base(message, null)
        {
        }

        public ExpressionParseException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public ExpressionParseException(string message, int pos)
            : base(message, null)
        {
            StartPos = pos;
            EndPos = -1;
        }

        public ExpressionParseException(string message, int startPos, int endPos)
            : base(message, null)
        {
            StartPos = startPos;
            EndPos = endPos;
        }

        public ExpressionParseException(string message, int startPos, int endPos, Exception inner)
            : base(message, inner)
        {
            StartPos = startPos;
            EndPos = endPos;
        }

        protected ExpressionParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            StartPos = (int)info.GetValue("startPos", typeof(int));
            EndPos = (int)info.GetValue("endPos", typeof(int));
        }

        public int StartPos { get; } = -1;

        public int EndPos { get; } = -1;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("startPos", StartPos);
            info.AddValue("endPos", EndPos);
            base.GetObjectData(info, context);
        }
    }
}