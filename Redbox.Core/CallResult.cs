using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.Core
{
    public class CallResult
    {
        public CallResult()
        {
            Errors = new List<Error>();
        }

        public bool Success { get; set; }

        public List<Error> Errors { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("CallResult - Success: {0}", Success);
            sb.AppendErrors(Errors);
            return sb.ToString();
        }

        public CallResult ErrorAndLog(string code, string description)
        {
            Errors.Add(LocalExtensions.MakeError(code, description, null));
            LogHelper.Instance.LogError(LogHelper.CallingFunction, code, description);
            return this;
        }

        public CallResult ErrorAndLog(string code, string description, Exception e)
        {
            Errors.Add(LocalExtensions.MakeError(code, description, e));
            LogHelper.Instance.LogError(LogHelper.CallingFunction, code, description);
            LogHelper.Instance.LogException(LogHelper.CallingFunction, string.Format("{0} - {1}", code, description),
                e);
            return this;
        }

        public CallResult ErrorAndLog(List<Error> errors)
        {
            Errors.AddRange(errors);
            errors.ForEach(e => LogHelper.Instance.LogError(LogHelper.CallingFunction, e.Code, e.Description));
            return this;
        }
    }
}