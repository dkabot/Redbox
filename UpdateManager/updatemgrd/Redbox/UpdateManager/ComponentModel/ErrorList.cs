using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.ComponentModel
{
    internal class ErrorList : List<Error>, ICloneable
    {
        public static ErrorList NewFromStrings(string[] errors)
        {
            ErrorList errorList = new ErrorList();
            if (errors == null)
                return errorList;
            foreach (string error in errors)
                errorList.Add(Error.Parse(error));
            return errorList;
        }

        public bool ContainsCode(string code)
        {
            return this.Find((Predicate<Error>)(each => each.Code == code)) != null;
        }

        public bool ContainsError() => this.Find((Predicate<Error>)(each => !each.IsWarning)) != null;

        public int RemoveCode(string code)
        {
            return this.RemoveAll((Predicate<Error>)(each => each.Code == code));
        }

        public object Clone()
        {
            ErrorList errorList = new ErrorList();
            errorList.AddRange((IEnumerable<Error>)this);
            return (object)errorList;
        }

        public int ErrorCount => this.FindAll((Predicate<Error>)(each => !each.IsWarning)).Count;

        public int WarningCount => this.FindAll((Predicate<Error>)(each => each.IsWarning)).Count;
    }
}
