using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.Core
{
    internal class CallResult<T>
    {
        public CallResult() => this.Errors = new List<Error>();

        public bool Success { get; set; }

        public T Result { get; set; }

        public List<Error> Errors { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("CallResult - Success: {0}, Result: {1}", (object)this.Success, (object)this.Result);
            sb.AppendErrors(this.Errors);
            return sb.ToString();
        }

        public CallResult<T> ErrorAndLog(string code, string description)
        {
            this.Errors.Add(LocalExtensions.MakeError(code, description, (Exception)null));
            LogHelper.Instance.LogError(LogHelper.CallingFunction, code, description);
            return this;
        }

        public CallResult<T> ErrorAndLog(string code, string description, Exception e)
        {
            this.Errors.Add(LocalExtensions.MakeError(code, description, e));
            LogHelper.Instance.LogError(LogHelper.CallingFunction, code, description);
            LogHelper.Instance.LogException(LogHelper.CallingFunction, string.Format("{0} - {1}", (object)code, (object)description), e);
            return this;
        }

        public CallResult<T> ErrorAndLog(List<Error> errors)
        {
            this.Errors.AddRange((IEnumerable<Error>)errors);
            errors.ForEach((Action<Error>)(e => LogHelper.Instance.LogError(LogHelper.CallingFunction, e.Code, e.Description)));
            return this;
        }
    }
}
