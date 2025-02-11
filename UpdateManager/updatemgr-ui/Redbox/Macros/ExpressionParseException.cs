using System;
using System.Runtime.Serialization;

namespace Redbox.Macros
{
    [Serializable]
    internal class ExpressionParseException : Exception
    {
        private int _endPos = -1;
        private int _startPos = -1;

        public ExpressionParseException()
        {
        }

        public ExpressionParseException(string message)
          : base(message, (Exception)null)
        {
        }

        public ExpressionParseException(string message, Exception inner)
          : base(message, inner)
        {
        }

        public ExpressionParseException(string message, int pos)
          : base(message, (Exception)null)
        {
            this._startPos = pos;
            this._endPos = -1;
        }

        public ExpressionParseException(string message, int startPos, int endPos)
          : base(message, (Exception)null)
        {
            this._startPos = startPos;
            this._endPos = endPos;
        }

        public ExpressionParseException(string message, int startPos, int endPos, Exception inner)
          : base(message, inner)
        {
            this._startPos = startPos;
            this._endPos = endPos;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("startPos", this._startPos);
            info.AddValue("endPos", this._endPos);
            base.GetObjectData(info, context);
        }

        public int StartPos => this._startPos;

        public int EndPos => this._endPos;

        protected ExpressionParseException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
            this._startPos = (int)info.GetValue("startPos", typeof(int));
            this._endPos = (int)info.GetValue("endPos", typeof(int));
        }
    }
}
