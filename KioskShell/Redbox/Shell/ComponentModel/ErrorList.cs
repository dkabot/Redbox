using System;
using System.Collections.Generic;

namespace Redbox.Shell.ComponentModel
{
    public class ErrorList : List<Error>, ICloneable
    {
        public int ErrorCount => FindAll(each => !each.IsWarning).Count;

        public int WarningCount => FindAll(each => each.IsWarning).Count;

        public object Clone()
        {
            var errorList = new ErrorList();
            errorList.AddRange(this);
            return errorList;
        }

        public static ErrorList NewFromStrings(string[] errors)
        {
            var errorList = new ErrorList();
            if (errors == null)
                return errorList;
            foreach (var error in errors)
                errorList.Add(Error.Parse(error));
            return errorList;
        }

        public bool ContainsCode(string code)
        {
            return Find(each => each.Code == code) != null;
        }

        public bool ContainsError()
        {
            return Find(each => !each.IsWarning) != null;
        }

        public int RemoveCode(string code)
        {
            return RemoveAll(each => each.Code == code);
        }
    }
}