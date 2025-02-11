using System;

namespace Redbox.HAL.Script.Framework
{
    internal class ScriptMessage
    {
        internal readonly DateTime EventTime;
        internal readonly string Message;

        internal ScriptMessage(DateTime time, string msg)
        {
            EventTime = time;
            Message = msg;
        }

        public override string ToString()
        {
            return string.Format("{0}|{1}", EventTime, Message);
        }

        internal static ScriptMessage Parse(string message)
        {
            var strArray = message.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            return new ScriptMessage(DateTime.Parse(strArray[0]), strArray[1]);
        }
    }
}